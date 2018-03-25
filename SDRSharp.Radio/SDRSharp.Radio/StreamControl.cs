using SDRSharp.Radio.PortAudio;
using System;
using System.Threading;

namespace SDRSharp.Radio
{
	public sealed class StreamControl : IDisposable
	{
		private enum InputType
		{
			SoundCard,
			Plugin,
			WaveFile
		}

		private const int IQBufferingFactor = 6;

		private const int AudioBufferingFactor = 2;

		private const int WaveBufferSize = 65536;

		private const int MaxDecimationStageCount = 20;

		private static readonly int _bufferAlignment = 8 * Environment.ProcessorCount;

		private static readonly int _minOutputSampleRate = Utils.GetIntSetting("minOutputSampleRate", 24000);

		private static readonly int _minReducedNarrowBandwidth = Utils.GetIntSetting("minReducedNarrowBandwidth", 8000);

		private static readonly int _minReducedWideBandwidth = Utils.GetIntSetting("minReducedWideBandwidth", 120000);

		private unsafe float* _dspOutPtr;

		private UnsafeBuffer _dspOutBuffer;

		private unsafe Complex* _dspInPtr;

		private WavePlayer _wavePlayer;

		private WaveRecorder _waveRecorder;

		private WaveDuplex _waveDuplex;

		private WaveFile _waveFile;

		private ComplexCircularBuffer _iqCircularBuffer;

		private FloatCircularBuffer _audioCircularBuffer;

		private Thread _waveReadThread;

		private Thread _dspThread;

		private float _audioGain;

		private float _outputGain;

		private int _inputDevice;

		private double _inputSampleRate;

		private int _inputBufferSize;

		private double _bufferSizeInMs;

		private int _outputDevice;

		private double _outputSampleRate;

		private int _outputBufferSize;

		private int _decimationStageCount;

		private bool _swapIQ;

		private bool _isPlaying;

		private InputType _inputType;

		private IFrontendController _frontend;

		private HookManager _hookManager;

		public static bool ReducedBandwidth
		{
			get;
			set;
		}

		public float AudioGain
		{
			get
			{
				return this._audioGain;
			}
			set
			{
				this._audioGain = value;
				this._outputGain = (float)Math.Pow(10.0, (double)value / 10.0);
			}
		}

		public bool ScaleOutput
		{
			get;
			set;
		}

		public bool SwapIQ
		{
			get
			{
				return this._swapIQ;
			}
			set
			{
				this._swapIQ = value;
			}
		}

		public double SampleRate
		{
			get
			{
				return this._inputSampleRate;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this._isPlaying;
			}
		}

		public int BufferSize
		{
			get
			{
				return this._inputBufferSize;
			}
		}

		public double BufferSizeInMs
		{
			get
			{
				return this._bufferSizeInMs;
			}
		}

		public int DecimationStageCount
		{
			get
			{
				return this._decimationStageCount;
			}
		}

		public double AudioSampleRate
		{
			get
			{
				return this._outputSampleRate;
			}
		}

		public event BufferNeededDelegate BufferNeeded;

		public StreamControl(HookManager hookManager = null)
		{
			this._hookManager = hookManager;
			this.AudioGain = 10f;
			this.ScaleOutput = true;
		}

		~StreamControl()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			this.Stop();
			GC.SuppressFinalize(this);
		}

		private unsafe void DuplexFiller(float* buffer, int frameCount)
		{
			if (!this._isPlaying)
			{
				Utils.Memset(buffer, 0, this._outputBufferSize * 4);
			}
			else
			{
				this._dspInPtr = (Complex*)buffer;
				if (this._dspOutBuffer == null || this._dspOutBuffer.Length != frameCount * 2)
				{
					this._dspOutBuffer = UnsafeBuffer.Create(frameCount * 2, 4);
					this._dspOutPtr = (float*)(void*)this._dspOutBuffer;
				}
				if (this._hookManager != null)
				{
					this._hookManager.ProcessRawIQ(this._dspInPtr, frameCount);
				}
				this.ProcessIQ();
				this.ScaleBuffer(this._dspOutPtr, this._dspOutBuffer.Length);
				Utils.Memcpy(buffer, this._dspOutPtr, this._dspOutBuffer.Length * 4);
			}
		}

		private unsafe void PlayerFiller(float* buffer, int frameCount)
		{
			if (!this._isPlaying)
			{
				Utils.Memset(buffer, 0, this._outputBufferSize * 4);
			}
			else
			{
				float* ptr = this._audioCircularBuffer.Acquire();
				if (ptr != null)
				{
					Utils.Memcpy(buffer, ptr, this._outputBufferSize * 4);
					this._audioCircularBuffer.Release();
					this.ScaleBuffer(buffer, this._outputBufferSize);
				}
			}
		}

		private unsafe void RecorderFiller(float* buffer, int frameCount)
		{
			if (this._isPlaying)
			{
				Utils.Memcpy(buffer, buffer, frameCount * sizeof(Complex));
				if (this._hookManager != null)
				{
					this._hookManager.ProcessRawIQ((Complex*)buffer, frameCount);
				}
				this._iqCircularBuffer.Write((Complex*)buffer, frameCount);
			}
		}

		private unsafe void FrontendFiller(IFrontendController sender, Complex* samples, int len)
		{
			if (this._isPlaying)
			{
				if (this._hookManager != null)
				{
					this._hookManager.ProcessRawIQ(samples, len);
				}
				this._iqCircularBuffer.Write(samples, len, !(sender is INonBlockingController));
			}
		}

		private unsafe void WaveFileFiller()
		{
			Complex[] array = new Complex[65536];
			Complex[] array2 = array;
			fixed (Complex* ptr = array2)
			{
				while (this.IsPlaying)
				{
					this._waveFile.Read(ptr, array.Length);
					if (this._hookManager != null)
					{
						this._hookManager.ProcessRawIQ(ptr, array.Length);
					}
					this._iqCircularBuffer.Write(ptr, array.Length);
				}
			}
		}

		private unsafe void ScaleBuffer(float* buffer, int length)
		{
			if (this.ScaleOutput)
			{
				for (int i = 0; i < length; i++)
				{
					buffer[i] *= this._outputGain;
				}
			}
		}

		private unsafe void DSPProc()
		{
			if (this._dspOutBuffer == null || this._dspOutBuffer.Length != this._outputBufferSize)
			{
				this._dspOutBuffer = UnsafeBuffer.Create(this._outputBufferSize, 4);
				this._dspOutPtr = (float*)(void*)this._dspOutBuffer;
			}
			while (this._isPlaying)
			{
				this._dspInPtr = this._iqCircularBuffer.Acquire();
				if (this._dspInPtr == null)
				{
					break;
				}
				this.ProcessIQ();
				this._iqCircularBuffer.Release();
				this._audioCircularBuffer.Write(this._dspOutPtr, this._outputBufferSize);
			}
		}

		private unsafe void ProcessIQ()
		{
			BufferNeededDelegate bufferNeeded = this.BufferNeeded;
			if (bufferNeeded != null)
			{
				if (this._swapIQ)
				{
					this.SwapIQBuffer();
				}
				bufferNeeded(this._dspInPtr, this._dspOutPtr, this._inputBufferSize);
			}
		}

		private unsafe void SwapIQBuffer()
		{
			for (int i = 0; i < this._inputBufferSize; i++)
			{
				float real = this._dspInPtr[i].Real;
				this._dspInPtr[i].Real = this._dspInPtr[i].Imag;
				this._dspInPtr[i].Imag = real;
			}
		}

		public void Stop()
		{
			this._isPlaying = false;
			if (this._inputType == InputType.Plugin && this._frontend is IIQStreamController)
			{
				((IIQStreamController)this._frontend).Stop();
				this._frontend = null;
			}
			if (this._iqCircularBuffer != null)
			{
				this._iqCircularBuffer.Close();
			}
			if (this._audioCircularBuffer != null)
			{
				this._audioCircularBuffer.Close();
			}
			if (this._wavePlayer != null)
			{
				this._wavePlayer.Dispose();
				this._wavePlayer = null;
			}
			if (this._waveRecorder != null)
			{
				this._waveRecorder.Dispose();
				this._waveRecorder = null;
			}
			if (this._waveDuplex != null)
			{
				this._waveDuplex.Dispose();
				this._waveDuplex = null;
			}
			this._inputSampleRate = 0.0;
			if (this._waveReadThread != null)
			{
				this._waveReadThread.Join();
				this._waveReadThread = null;
			}
			if (this._dspThread != null)
			{
				this._dspThread.Join();
				this._dspThread = null;
			}
			if (this._waveFile != null)
			{
				this._waveFile.Dispose();
				this._waveFile = null;
			}
			this._iqCircularBuffer = null;
			this._audioCircularBuffer = null;
			this._dspOutBuffer = null;
		}

		public unsafe void Play()
		{
			if (this._wavePlayer == null && this._waveDuplex == null)
			{
				this._isPlaying = true;
				try
				{
					switch (this._inputType)
					{
					case InputType.SoundCard:
						if (this._inputDevice == this._outputDevice)
						{
							this._waveDuplex = new WaveDuplex(this._inputDevice, this._inputSampleRate, this._inputBufferSize, this.DuplexFiller);
						}
						else
						{
							this._iqCircularBuffer = new ComplexCircularBuffer(this._inputBufferSize, 6);
							this._audioCircularBuffer = new FloatCircularBuffer(this._outputBufferSize, 2);
							this._waveRecorder = new WaveRecorder(this._inputDevice, this._inputSampleRate, this._inputBufferSize, this.RecorderFiller);
							this._wavePlayer = new WavePlayer(this._outputDevice, this._outputSampleRate, this._outputBufferSize / 2, this.PlayerFiller);
							this._dspThread = new Thread(this.DSPProc);
							this._dspThread.Start();
						}
						break;
					case InputType.WaveFile:
						this._iqCircularBuffer = new ComplexCircularBuffer(this._inputBufferSize, 6);
						this._audioCircularBuffer = new FloatCircularBuffer(this._outputBufferSize, 2);
						this._wavePlayer = new WavePlayer(this._outputDevice, this._outputSampleRate, this._outputBufferSize / 2, this.PlayerFiller);
						this._waveReadThread = new Thread(this.WaveFileFiller);
						this._waveReadThread.Start();
						this._dspThread = new Thread(this.DSPProc);
						this._dspThread.Start();
						break;
					case InputType.Plugin:
						this._iqCircularBuffer = new ComplexCircularBuffer(this._inputBufferSize, 6);
						this._audioCircularBuffer = new FloatCircularBuffer(this._outputBufferSize, 2);
						this._wavePlayer = new WavePlayer(this._outputDevice, this._outputSampleRate, this._outputBufferSize / 2, this.PlayerFiller);
						if (this._frontend is IIQStreamController)
						{
							((IIQStreamController)this._frontend).Start(this.FrontendFiller);
						}
						this._dspThread = new Thread(this.DSPProc);
						this._dspThread.Start();
						break;
					}
					if (this._dspThread != null)
					{
						this._dspThread.Name = "DSP Thread";
					}
				}
				catch
				{
					this._isPlaying = false;
					throw;
				}
			}
		}

		public void OpenSoundDevice(int inputDevice, int outputDevice, double inputSampleRate, int bufferSizeInMs)
		{
			this.Stop();
			this._inputType = InputType.SoundCard;
			this._inputDevice = inputDevice;
			this._outputDevice = outputDevice;
			this._inputSampleRate = inputSampleRate;
			this._inputBufferSize = (int)((double)bufferSizeInMs * this._inputSampleRate / 1000.0);
			if (this._inputDevice == this._outputDevice)
			{
				this._decimationStageCount = 0;
				this._outputSampleRate = this._inputSampleRate;
				this._outputBufferSize = this._inputBufferSize * 2;
			}
			else
			{
				this._decimationStageCount = StreamControl.GetDecimationStageCount(this._inputSampleRate, DetectorType.AM);
				int num = 1 << this._decimationStageCount;
				int num2 = num * StreamControl._bufferAlignment;
				this._inputBufferSize = this._inputBufferSize / num2 * num2;
				this._outputSampleRate = this._inputSampleRate / (double)num;
				this._outputBufferSize = this._inputBufferSize / num * 2;
			}
			this._bufferSizeInMs = (double)this._inputBufferSize / this._inputSampleRate * 1000.0;
		}

		public void OpenFile(string filename, int outputDevice, int bufferSizeInMs)
		{
			this.Stop();
			try
			{
				this._inputType = InputType.WaveFile;
				this._waveFile = new WaveFile(filename);
				this._outputDevice = outputDevice;
				this._inputSampleRate = (double)this._waveFile.SampleRate;
				this._inputBufferSize = (int)((double)bufferSizeInMs * this._inputSampleRate / 1000.0);
				this._decimationStageCount = StreamControl.GetDecimationStageCount(this._inputSampleRate, DetectorType.AM);
				int num = 1 << this._decimationStageCount;
				int num2 = num * StreamControl._bufferAlignment;
				this._inputBufferSize = this._inputBufferSize / num2 * num2;
				this._outputSampleRate = this._inputSampleRate / (double)num;
				this._outputBufferSize = this._inputBufferSize / num * 2;
				this._bufferSizeInMs = (double)this._inputBufferSize / this._inputSampleRate * 1000.0;
			}
			catch
			{
				this.Stop();
				throw;
			}
		}

		public void OpenPlugin(IFrontendController frontend, int outputDevice, int bufferSizeInMs)
		{
			this.Stop();
			try
			{
				this._inputType = InputType.Plugin;
				this._frontend = frontend;
				if (frontend is IIQStreamController)
				{
					this._inputSampleRate = ((IIQStreamController)this._frontend).Samplerate;
					this._outputDevice = outputDevice;
					this._inputBufferSize = (int)((double)bufferSizeInMs * this._inputSampleRate / 1000.0);
					if (this._inputBufferSize == 0)
					{
						throw new ArgumentException("The source '" + this._frontend + "' is not ready");
					}
					this._decimationStageCount = StreamControl.GetDecimationStageCount(this._inputSampleRate, DetectorType.AM);
					int num = 1 << this._decimationStageCount;
					int num2 = num * StreamControl._bufferAlignment;
					this._inputBufferSize = this._inputBufferSize / num2 * num2;
					this._outputSampleRate = this._inputSampleRate / (double)num;
					this._outputBufferSize = this._inputBufferSize / num * 2;
					this._bufferSizeInMs = (double)this._inputBufferSize / this._inputSampleRate * 1000.0;
					goto end_IL_0006;
				}
				throw new ArgumentException("The source '" + this._frontend + "' is not ready");
				end_IL_0006:;
			}
			catch
			{
				this.Stop();
				throw;
			}
		}

		public static int GetDecimationStageCount(double inputSampleRate, DetectorType detector = DetectorType.AM)
		{
			int num = 20;
			int num2;
			if (StreamControl.ReducedBandwidth)
			{
				if (inputSampleRate <= (double)StreamControl._minReducedNarrowBandwidth)
				{
					return 0;
				}
				num2 = ((detector == DetectorType.WFM) ? StreamControl._minReducedWideBandwidth : StreamControl._minReducedNarrowBandwidth);
			}
			else
			{
				if (inputSampleRate <= (double)StreamControl._minOutputSampleRate)
				{
					return 0;
				}
				num2 = ((detector == DetectorType.WFM) ? 250000 : StreamControl._minOutputSampleRate);
			}
			while (inputSampleRate / (double)(1 << num) < (double)num2 && num > 0)
			{
				num--;
			}
			return num;
		}
	}
}
