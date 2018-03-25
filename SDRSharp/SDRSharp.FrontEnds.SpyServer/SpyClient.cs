using SDRSharp.Common;
using SDRSharp.Radio;
using SDRSharp.Radio.PortAudio;
using System;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.SpyServer
{
	public class SpyClient : IDisposable
	{
		private enum ParserPhase
		{
			AcquiringHeader,
			ReadingData
		}

		private const float TimeConst = 0.05f;

		private const int BufferSize = 65536;

		private const int DefaultDisplayPixels = 1000;

		private const int DefaultFFTRange = 127;

		private Socket _s;

		private Thread _receiveThread;

		private bool _terminated;

		private bool _streaming;

		private StreamingMode _streamingMode = StreamingMode.STREAM_MODE_FFT_IQ;

		private StreamFormat _streamFormat = StreamFormat.STREAM_FORMAT_UINT8;

		private bool _optimizePropertyChanges = true;

		private uint _channelCenterFrequency;

		private uint _displayCenterFrequency;

		private uint _deviceCenterFrequency;

		private int _displayDecimationStageCount;

		private int _channelDecimationStageCount;

		private uint _minimumTunableFrequency;

		private uint _maximumTunableFrequency;

		private uint _autoGainDecibels;

		private int _gain;

		private int _fftOffset;

		private int _fftRange = 127;

		private int _displayPixels = 1000;

		private int _messageSize;

		private uint _lastSequenceNumber = uint.MaxValue;

		private uint _droppedBuffers;

		private long _down_stream_bytes;

		private bool _gotDeviceInfo;

		private bool _gotSyncInfo;

		private bool _canControl;

		private Exception _error;

		private DeviceInfo _deviceInfo;

		private ParserPhase _parserPhase;

		private int _parserPosition;

		private unsafe byte[] _headerData = new byte[sizeof(MessageHeader)];

		private MessageHeader _header;

		private UnsafeBuffer _bodyBuffer;

		private UnsafeBuffer _uncompressedBuffer;

		private UnsafeBuffer _messageBuffer;

		private UnsafeBuffer _iqBuffer;

		private string _serverVersion;

		public string ServerVersion
		{
			get
			{
				return this._serverVersion;
			}
		}

		public bool OptimizePropertyChanges
		{
			get
			{
				return this._optimizePropertyChanges;
			}
			set
			{
				this._optimizePropertyChanges = value;
			}
		}

		public bool IsConnected
		{
			get
			{
				if (!this._terminated && this._s != null)
				{
					return this._s.Connected;
				}
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				if (this._gotDeviceInfo)
				{
					return this._gotSyncInfo;
				}
				return false;
			}
		}

		public string DeviceName
		{
			get
			{
				return Constants.GetDeviceName(this._deviceInfo.DeviceType);
			}
		}

		public uint DeviceSerial
		{
			get
			{
				return this._deviceInfo.DeviceSerial;
			}
		}

		public uint MaximumDecimationStageCount
		{
			get
			{
				return this._deviceInfo.DecimationStageCount;
			}
		}

		public uint MaximumBandwidth
		{
			get
			{
				return this._deviceInfo.MaximumBandwidth;
			}
		}

		public uint MaximumSampleRate
		{
			get
			{
				return this._deviceInfo.MaximumSampleRate;
			}
		}

		public uint MaximumGainIndex
		{
			get
			{
				return this._deviceInfo.MaximumGainIndex;
			}
		}

		public double ChannelSamplerate
		{
			get
			{
				return (double)this._deviceInfo.MaximumSampleRate / (double)(1 << this._channelDecimationStageCount);
			}
		}

		public int DisplayBandwidth
		{
			get
			{
				return (int)((double)this._deviceInfo.MaximumBandwidth / (double)(1 << this._displayDecimationStageCount));
			}
		}

		public int ChannelBandwidth
		{
			get
			{
				return (int)((double)this._deviceInfo.MaximumBandwidth / (double)(1 << this._channelDecimationStageCount));
			}
		}

		public bool Is8bitForced
		{
			get
			{
				return this._deviceInfo.ForcedIQFormat == 1;
			}
		}

		public int MinimumIQDecimation
		{
			get
			{
				return (int)this._deviceInfo.MinimumIQDecimation;
			}
		}

		public bool CanControl
		{
			get
			{
				return this._canControl;
			}
		}

		public uint MaximumTunableFrequency
		{
			get
			{
				return this._maximumTunableFrequency;
			}
		}

		public uint MinimumTunableFrequency
		{
			get
			{
				return this._minimumTunableFrequency;
			}
		}

		public uint DeviceCenterFrequency
		{
			get
			{
				return this._deviceCenterFrequency;
			}
		}

		public StreamingMode StreamingMode
		{
			get
			{
				return this._streamingMode;
			}
			set
			{
				if (this._streamingMode == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._streamingMode = value;
				this.SetSetting(SettingType.SETTING_STREAMING_MODE, (uint)this._streamingMode);
			}
		}

		public StreamFormat StreamFormat
		{
			get
			{
				return this._streamFormat;
			}
			set
			{
				if (this._streamFormat == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._streamFormat = value;
				this.UpdateIQFormat();
			}
		}

		public uint DisplayCenterFrequency
		{
			get
			{
				return this._displayCenterFrequency;
			}
			set
			{
				if (this._canControl)
				{
					this._deviceCenterFrequency = value;
				}
				if (this._displayCenterFrequency == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._displayCenterFrequency = value;
				this.SetSetting(SettingType.SETTING_FFT_FREQUENCY, this._displayCenterFrequency);
			}
		}

		public uint ChannelCenterFrequency
		{
			get
			{
				return this._channelCenterFrequency;
			}
			set
			{
				if (this._channelCenterFrequency == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._channelCenterFrequency = value;
				this.SetSetting(SettingType.SETTING_IQ_FREQUENCY, this._channelCenterFrequency);
			}
		}

		public int Gain
		{
			get
			{
				return this._gain;
			}
			set
			{
				if (this._gain == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._gain = value;
				this.SetSetting(SettingType.SETTING_GAIN, (uint)this._gain);
			}
		}

		public int DisplayDecimationStageCount
		{
			get
			{
				return this._displayDecimationStageCount;
			}
			set
			{
				if (this._displayDecimationStageCount == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._displayDecimationStageCount = value;
				this.SetSetting(SettingType.SETTING_FFT_DECIMATION, (uint)this._displayDecimationStageCount);
			}
		}

		public int ChannelDecimationStageCount
		{
			get
			{
				return this._channelDecimationStageCount;
			}
			set
			{
				if (this._channelDecimationStageCount == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._channelDecimationStageCount = value;
				this.SetSetting(SettingType.SETTING_IQ_DECIMATION, (uint)this._channelDecimationStageCount);
				this.UpdateIQFormat();
			}
		}

		public int FFTRange
		{
			get
			{
				return this._fftRange;
			}
			set
			{
				if (this._fftRange == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._fftRange = value;
				this.SetSetting(SettingType.SETTING_FFT_DB_RANGE, (uint)this._fftRange);
			}
		}

		public int FFTOffset
		{
			get
			{
				return this._fftOffset;
			}
			set
			{
				if (this._fftOffset == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._fftOffset = value;
				this.SetSetting(SettingType.SETTING_FFT_DB_OFFSET, (uint)this._fftOffset);
			}
		}

		public int DisplayPixels
		{
			get
			{
				return this._displayPixels;
			}
			set
			{
				if (this._displayPixels == value && this._optimizePropertyChanges)
				{
					return;
				}
				this._displayPixels = value;
				this.SetSetting(SettingType.SETTING_FFT_DISPLAY_PIXELS, (uint)this._displayPixels);
			}
		}

		public event EventHandler Connected;

		public event EventHandler Disconnected;

		public event EventHandler Synchronized;

		public event SamplesAvailableDelegate<ComplexSamplesEventArgs> SamplesAvailable;

		public event SamplesAvailableDelegate<ByteSamplesEventArgs> FFTAvailable;

		public void Dispose()
		{
			this.Disconnect();
		}

		public void Connect(string host, int port)
		{
			if (this._receiveThread == null)
			{
				this._s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
				{
					NoDelay = true
				};
				this._s.Connect(host, port);
				this._s.Blocking = false;
				this.SayHello();
				this.Cleanup();
				this._error = null;
				this._terminated = false;
				this._receiveThread = new Thread(this.ThreadProc)
				{
					Name = "Spy Server Receive Thread"
				};
				this._receiveThread.Start();
				for (int i = 0; i < 2000; i++)
				{
					if (this._error != null)
					{
						break;
					}
					if (this._gotDeviceInfo)
					{
						if (this._deviceInfo.DeviceType == DeviceType.DEVICE_INVALID)
						{
							this._error = new ApplicationException("Server is up but no device is available.");
							break;
						}
						if (this._gotSyncInfo)
						{
							this.OnConnect();
							return;
						}
					}
					Thread.Sleep(1);
					Application.DoEvents();
				}
				this.Disconnect();
				if (this._error != null)
				{
					Exception error = this._error;
					this._error = null;
					throw error;
				}
				throw new ApplicationException("Server didn't send the device capability and synchronization info.");
			}
		}

		public void Disconnect()
		{
			this._terminated = true;
			if (this._s != null)
			{
				this._s.Close();
				this._s = null;
			}
			Thread receiveThread = this._receiveThread;
			if (receiveThread != null)
			{
				receiveThread.Join();
				this._receiveThread = null;
			}
			this.Cleanup();
		}

		private void OnConnect()
		{
			this.SetSetting(SettingType.SETTING_STREAMING_MODE, (uint)this._streamingMode);
			this.SetSetting(SettingType.SETTING_FFT_DISPLAY_PIXELS, (uint)this._displayPixels);
			this.SetSetting(SettingType.SETTING_FFT_DB_OFFSET, (uint)this._fftOffset);
			this.SetSetting(SettingType.SETTING_FFT_DB_RANGE, (uint)this._fftRange);
			if (!Utils.GetBooleanSetting("spyserver.disableAutoScaling"))
			{
				this.SetSetting(SettingType.SETTING_IQ_DIGITAL_GAIN, uint.MaxValue);
			}
			this.UpdateFFTFormat();
			this.UpdateIQFormat();
		}

		public void StartStreaming()
		{
			if (!this._streaming)
			{
				this._streaming = true;
				this._down_stream_bytes = 0L;
				this.SetStreamState();
			}
		}

		public void StopStreaming()
		{
			if (this._streaming)
			{
				this._streaming = false;
				this._down_stream_bytes = 0L;
				this.SetStreamState();
			}
		}

		private void Cleanup()
		{
			this._deviceInfo.DeviceType = DeviceType.DEVICE_INVALID;
			this._deviceInfo.DeviceSerial = 0u;
			this._deviceInfo.DecimationStageCount = 0u;
			this._deviceInfo.GainStageCount = 0u;
			this._deviceInfo.MaximumSampleRate = 0u;
			this._deviceInfo.MaximumBandwidth = 0u;
			this._deviceInfo.MaximumGainIndex = 0u;
			this._deviceInfo.MinimumFrequency = 0u;
			this._deviceInfo.MaximumFrequency = 0u;
			this._deviceInfo.Resolution = 0u;
			this._deviceInfo.MinimumIQDecimation = 0u;
			this._deviceInfo.ForcedIQFormat = 0u;
			this._gain = 0;
			this._displayCenterFrequency = 0u;
			this._deviceCenterFrequency = 0u;
			this._displayDecimationStageCount = 0;
			this._channelDecimationStageCount = 0;
			this._minimumTunableFrequency = 0u;
			this._maximumTunableFrequency = 0u;
			this._canControl = false;
			this._gotDeviceInfo = false;
			this._gotSyncInfo = false;
			this._lastSequenceNumber = uint.MaxValue;
			this._droppedBuffers = 0u;
			this._down_stream_bytes = 0L;
			this._parserPhase = ParserPhase.AcquiringHeader;
			this._parserPosition = 0;
			this._streaming = false;
			this._terminated = true;
		}

		private void UpdateFFTFormat()
		{
			this.SetSetting(SettingType.SETTING_FFT_FORMAT, 1u);
		}

		private void UpdateIQFormat()
		{
			this.SetSetting(SettingType.SETTING_IQ_FORMAT, (uint)this._streamFormat);
		}

		private bool SetStreamState()
		{
			return this.SetSetting(SettingType.SETTING_STREAMING_ENABLED, (uint)(this._streaming ? 1 : 0));
		}

		private unsafe bool SetSetting(SettingType setting, params uint[] args)
		{
			byte[] array;
			if (args != null && args.Length != 0)
			{
				array = new byte[4 + args.Length * 4];
				byte* ptr = (byte*)(&setting);
				for (int i = 0; i < 4; i++)
				{
					array[i] = ptr[i];
				}
				Buffer.BlockCopy(args, 0, array, 4, args.Length * 4);
			}
			else
			{
				array = null;
			}
			return this.SendCommand(CommandType.CMD_SET_SETTING, array);
		}

		private bool SayHello()
		{
			byte[] bytes = BitConverter.GetBytes(33556024u);
			string s = string.Format("SDR# v{0} on {1}", Assembly.GetEntryAssembly().GetName().Version, Environment.OSVersion);
			byte[] bytes2 = Encoding.ASCII.GetBytes(s);
			byte[] array = new byte[bytes.Length + bytes2.Length];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			Buffer.BlockCopy(bytes2, 0, array, bytes.Length, bytes2.Length);
			return this.SendCommand(CommandType.CMD_HELLO, array);
		}

		private unsafe bool SendCommand(CommandType cmd, byte[] args)
		{
			if (this._s == null)
			{
				return false;
			}
			int num = sizeof(CommandHeader);
			int num2 = (args != null) ? args.Length : 0;
			byte[] array = new byte[num + num2];
			CommandHeader commandHeader = default(CommandHeader);
			commandHeader.CommandType = cmd;
			commandHeader.BodySize = (ushort)num2;
			byte* ptr = (byte*)(&commandHeader);
			for (int i = 0; i < sizeof(CommandHeader); i++)
			{
				array[i] = ptr[i];
			}
			if (args != null)
			{
				byte[] array2 = array;
				fixed (byte* ptr2 = array2)
				{
					byte* ptr3 = ptr2 + num;
					for (int j = 0; j < args.Length; j++)
					{
						ptr3[j] = args[j];
					}
				}
			}
			try
			{
				this._s.Send(array);
			}
			catch
			{
				return false;
			}
			return true;
		}

		private unsafe void ThreadProc()
		{
			EventHandler connected = this.Connected;
			if (connected != null)
			{
				connected(this, EventArgs.Empty);
			}
			this._parserPhase = ParserPhase.AcquiringHeader;
			this._parserPosition = 0;
			byte[] array = new byte[65536];
			byte[] array2 = array;
			fixed (byte* buffer = array2)
			{
				try
				{
					while (!this._terminated)
					{
						Socket s = this._s;
						if (s != null && s.Poll(1000000, SelectMode.SelectRead))
						{
							if (this._terminated)
							{
								break;
							}
							int num = s.Receive(array, 0, array.Length, SocketFlags.None);
							if (num > 0)
							{
								this.ParseMessage(buffer, num);
								continue;
							}
							throw new ApplicationException("Device got disconnected");
						}
					}
				}
				catch (Exception error)
				{
					Exception ex = this._error = error;
				}
			}
			if (this._bodyBuffer != null)
			{
				this._bodyBuffer.Dispose();
				this._bodyBuffer = null;
			}
			if (this._uncompressedBuffer != null)
			{
				this._uncompressedBuffer.Dispose();
				this._uncompressedBuffer = null;
			}
			this._messageBuffer = null;
			this.Cleanup();
			this._receiveThread = null;
			EventHandler disconnected = this.Disconnected;
			if (disconnected != null)
			{
				disconnected(this, EventArgs.Empty);
			}
		}

		private unsafe int ParseHeader(byte* buffer, int length)
		{
			int num = 0;
			byte[] headerData = this._headerData;
			fixed (byte* ptr = headerData)
			{
				while (length > 0)
				{
					int num2 = Math.Min(sizeof(MessageHeader) - this._parserPosition, length);
					Utils.Memcpy(ptr + this._parserPosition, buffer, num2);
					length -= num2;
					buffer += num2;
					this._parserPosition += num2;
					num += num2;
					if (this._parserPosition == sizeof(MessageHeader))
					{
						this._parserPosition = 0;
						MessageHeader messageHeader = default(MessageHeader);
						Utils.Memcpy(&messageHeader, ptr, sizeof(MessageHeader));
						this._header = messageHeader;
						if (messageHeader.BodySize != 0)
						{
							this._parserPhase = ParserPhase.ReadingData;
						}
						return num;
					}
				}
			}
			return num;
		}

		private unsafe int ParseBody(byte* buffer, int length)
		{
			int num = 0;
			byte* ptr = (byte*)(void*)this._bodyBuffer;
			while (length > 0)
			{
				int num2 = Math.Min((int)this._header.BodySize - this._parserPosition, length);
				Utils.Memcpy(ptr + this._parserPosition, buffer, num2);
				length -= num2;
				buffer += num2;
				this._parserPosition += num2;
				num += num2;
				if (this._parserPosition == this._header.BodySize)
				{
					this._parserPosition = 0;
					this._parserPhase = ParserPhase.AcquiringHeader;
					return num;
				}
			}
			return num;
		}

		public long GetDownstreamBytes()
		{
			return Interlocked.Exchange(ref this._down_stream_bytes, 0L);
		}

		private unsafe void ParseMessage(byte* buffer, int len)
		{
			Interlocked.Add(ref this._down_stream_bytes, len);
			while (true)
			{
				if (len > 0 && !this._terminated)
				{
					if (this._parserPhase == ParserPhase.AcquiringHeader)
					{
						while (this._parserPhase == ParserPhase.AcquiringHeader && len > 0)
						{
							int num = this.ParseHeader(buffer, len);
							buffer += num;
							len -= num;
						}
						if (this._parserPhase == ParserPhase.ReadingData)
						{
							byte b = 2;
							byte b2 = 0;
							ushort num2 = 1592;
							byte b3 = (byte)(this._header.ProtocolID >> 24);
							byte b4 = (byte)(this._header.ProtocolID >> 16 & 0xFF);
							ushort num3 = (ushort)(this._header.ProtocolID & 0xFFFF);
							this._serverVersion = string.Format("{0}.{1}.{2}", b3, b4, num3);
							if (b == b3 && b2 == b4)
							{
								if (this._header.BodySize <= 1048576)
								{
									if (this._bodyBuffer == null || this._bodyBuffer.Length < this._header.BodySize)
									{
										if (this._bodyBuffer != null)
										{
											this._bodyBuffer.Dispose();
										}
										this._bodyBuffer = UnsafeBuffer.Create((int)this._header.BodySize);
									}
									goto IL_017b;
								}
								break;
							}
							string message = string.Format("Server is running an unsupported protocol version.\r\nExpected {0}.{1}.* but got {3}.{4}.{5}.", b, b2, num2, b3, b4, num3);
							throw new ApplicationException(message);
						}
					}
					goto IL_017b;
				}
				return;
				IL_017b:
				if (this._parserPhase == ParserPhase.ReadingData)
				{
					int num = this.ParseBody(buffer, len);
					buffer += num;
					len -= num;
					if (this._parserPhase == ParserPhase.AcquiringHeader)
					{
						if (this._header.StreamType == StreamType.STREAM_TYPE_IQ)
						{
							uint num4 = this._header.SequenceNumber - this._lastSequenceNumber - 1;
							this._lastSequenceNumber = this._header.SequenceNumber;
							this._droppedBuffers += num4;
						}
						this.HandleNewMessage();
					}
				}
			}
			throw new ApplicationException("The server is probably buggy");
		}

		private void HandleNewMessage()
		{
			if (!this._terminated)
			{
				this._messageBuffer = this._bodyBuffer;
				this._messageSize = (int)this._header.BodySize;
				switch (this._header.MessageType & (MessageType)65535u)
				{
				case MessageType.MSG_TYPE_DEVICE_INFO:
					this.ProcessDeviceInfo();
					break;
				case MessageType.MSG_TYPE_CLIENT_SYNC:
					this.ProcessClientSync();
					break;
				case MessageType.MSG_TYPE_UINT8_IQ:
					this._autoGainDecibels = (uint)this._header.MessageType >> 16;
					this.ProcessUInt8Samples();
					break;
				case MessageType.MSG_TYPE_INT16_IQ:
					this._autoGainDecibels = (uint)this._header.MessageType >> 16;
					this.ProcessInt16Samples();
					break;
				case MessageType.MSG_TYPE_INT24_IQ:
					this._autoGainDecibels = (uint)this._header.MessageType >> 16;
					this.ProcessInt24Samples();
					break;
				case MessageType.MSG_TYPE_FLOAT_IQ:
					this._autoGainDecibels = (uint)this._header.MessageType >> 16;
					this.ProcessFloatSamples();
					break;
				case MessageType.MSG_TYPE_UINT8_FFT:
					this.ProcessUInt8FFT();
					break;
				}
			}
		}

		private unsafe void ProcessDeviceInfo()
		{
			DeviceInfo deviceInfo = default(DeviceInfo);
			Utils.Memcpy(&deviceInfo, this._messageBuffer, Math.Min(this._messageSize, sizeof(DeviceInfo)));
			this._deviceInfo = deviceInfo;
			if (this._deviceInfo.Resolution == 0)
			{
				switch (this._deviceInfo.DeviceType)
				{
				case DeviceType.DEVICE_AIRSPY_HF:
					this._deviceInfo.Resolution = 16u;
					break;
				case DeviceType.DEVICE_AIRSPY_ONE:
					this._deviceInfo.Resolution = 12u;
					break;
				case DeviceType.DEVICE_RTLSDR:
					this._deviceInfo.Resolution = 8u;
					break;
				}
			}
			this._minimumTunableFrequency = this._deviceInfo.MinimumFrequency;
			this._maximumTunableFrequency = this._deviceInfo.MaximumFrequency;
			this._gotDeviceInfo = true;
		}

		private unsafe void ProcessClientSync()
		{
			ClientSync clientSync = default(ClientSync);
			Utils.Memcpy(&clientSync, this._messageBuffer, Math.Min(this._messageSize, sizeof(ClientSync)));
			this._canControl = (clientSync.CanControl != 0);
			this._gain = (int)clientSync.Gain;
			this._deviceCenterFrequency = clientSync.DeviceCenterFrequency;
			this._channelCenterFrequency = clientSync.IQCenterFrequency;
			this._displayCenterFrequency = clientSync.FFTCenterFrequency;
			switch (this._streamingMode)
			{
			case StreamingMode.STREAM_MODE_FFT_ONLY:
			case StreamingMode.STREAM_MODE_FFT_IQ:
				this._minimumTunableFrequency = clientSync.MinimumFFTCenterFrequency;
				this._maximumTunableFrequency = clientSync.MaximumFFTCenterFrequency;
				break;
			case StreamingMode.STREAM_MODE_IQ_ONLY:
				this._minimumTunableFrequency = clientSync.MinimumIQCenterFrequency;
				this._maximumTunableFrequency = clientSync.MaximumIQCenterFrequency;
				break;
			}
			this._gotSyncInfo = true;
			EventHandler synchronized = this.Synchronized;
			if (synchronized != null)
			{
				synchronized(this, EventArgs.Empty);
			}
		}

		private unsafe void ProcessUInt8Samples()
		{
			int num = this._messageSize / 2;
			if (this._iqBuffer == null || this._iqBuffer.Length != num)
			{
				this._iqBuffer = UnsafeBuffer.Create(num, sizeof(Complex));
			}
			byte* ptr = (byte*)(void*)this._messageBuffer;
			Complex* ptr2 = (Complex*)(void*)this._iqBuffer;
			for (int i = 0; i < num; i++)
			{
				Complex* intPtr = ptr2 + i;
				byte* intPtr2 = ptr;
				ptr = intPtr2 + 1;
				intPtr->Real = ((float)(int)(*intPtr2) - 128f) * 0.0078125f;
				Complex* intPtr3 = ptr2 + i;
				byte* intPtr4 = ptr;
				ptr = intPtr4 + 1;
				intPtr3->Imag = ((float)(int)(*intPtr4) - 128f) * 0.0078125f;
			}
			this.PushIQData(ptr2, num);
		}

		private unsafe void ProcessInt16Samples()
		{
			int num = this._messageSize / 4;
			if (this._iqBuffer == null || this._iqBuffer.Length != num)
			{
				this._iqBuffer = UnsafeBuffer.Create(num, sizeof(Complex));
			}
			short* ptr = (short*)(void*)this._messageBuffer;
			Complex* ptr2 = (Complex*)(void*)this._iqBuffer;
			for (int i = 0; i < num; i++)
			{
				Complex* intPtr = ptr2 + i;
				short* intPtr2 = ptr;
				ptr = intPtr2 + 1;
				intPtr->Real = (float)(*intPtr2) * 3.05175781E-05f;
				Complex* intPtr3 = ptr2 + i;
				short* intPtr4 = ptr;
				ptr = intPtr4 + 1;
				intPtr3->Imag = (float)(*intPtr4) * 3.05175781E-05f;
			}
			this.PushIQData(ptr2, num);
		}

		private unsafe void ProcessInt24Samples()
		{
			int num = this._messageSize / 6;
			if (this._iqBuffer == null || this._iqBuffer.Length != num)
			{
				this._iqBuffer = UnsafeBuffer.Create(num, sizeof(Complex));
			}
			Int24* ptr = (Int24*)(void*)this._messageBuffer;
			Complex* ptr2 = (Complex*)(void*)this._iqBuffer;
			for (int i = 0; i < num; i++)
			{
				Complex* intPtr = ptr2 + i;
				Int24* ptr3 = ptr;
				ptr = ptr3 + 1;
				intPtr->Real = (float)(*ptr3) * 1.1920929E-07f;
				Complex* intPtr2 = ptr2 + i;
				ptr3 = ptr;
				ptr = ptr3 + 1;
				intPtr2->Imag = (float)(*ptr3) * 1.1920929E-07f;
			}
			this.PushIQData(ptr2, num);
		}

		private unsafe void ProcessFloatSamples()
		{
			int count = this._messageSize / 8;
			Complex* samples = (Complex*)(void*)this._messageBuffer;
			this.PushIQData(samples, count);
		}

		private unsafe void PushIQData(Complex* samples, int count)
		{
			ComplexSamplesEventArgs e = new ComplexSamplesEventArgs
			{
				Buffer = samples,
				Length = count,
				DroppedSamples = (uint)((int)this._droppedBuffers * count)
			};
			this._droppedBuffers = 0u;
			if (this._autoGainDecibels != 0)
			{
				float b = (float)Math.Pow(10.0, (double)((float)(double)this._autoGainDecibels * -0.05f));
				for (int i = 0; i < count; i++)
				{
					Complex* intPtr = samples + i;
					*intPtr *= b;
				}
			}
			SamplesAvailableDelegate<ComplexSamplesEventArgs> samplesAvailable = this.SamplesAvailable;
			if (samplesAvailable != null)
			{
				samplesAvailable(this, e);
			}
		}

		private unsafe void ProcessUInt8FFT()
		{
			ByteSamplesEventArgs e = new ByteSamplesEventArgs
			{
				Buffer = (byte*)(void*)this._messageBuffer,
				Length = this._messageSize
			};
			SamplesAvailableDelegate<ByteSamplesEventArgs> fFTAvailable = this.FFTAvailable;
			if (fFTAvailable != null)
			{
				fFTAvailable(this, e);
			}
		}

		public unsafe SpyClient()
		{
		}
	}
}
