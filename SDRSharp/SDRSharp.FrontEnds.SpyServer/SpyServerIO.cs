using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.SpyServer
{
	public class SpyServerIO : IFrontendController, IIQStreamController, INonBlockingController, ITunableSource, ISampleRateChangeSource, IConnectableSource, IFrontendOffset, IControlAwareObject, ISpectrumProvider, IConfigurationPanelProvider, IFFTSource, IVFOSource, IDisposable
	{
		private bool _disposed;

		private ControllerPanel _gui;

		private ISharpControl _control;

		private SpyClient _client;

		private SamplesAvailableDelegate _callback;

		public ISharpControl Control
		{
			get
			{
				return this._control;
			}
		}

		public bool Connected
		{
			get
			{
				if (this._client != null)
				{
					return this._client.IsConnected;
				}
				return false;
			}
		}

		public float UsableSpectrumRatio
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return (float)(double)this._client.MaximumBandwidth / (float)(double)this._client.MaximumSampleRate;
				}
				return 1f;
			}
		}

		public UserControl Gui
		{
			get
			{
				return this._gui;
			}
		}

		public int Offset
		{
			get
			{
				return 0;
			}
		}

		public long Frequency
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return (this._client.StreamingMode == StreamingMode.STREAM_MODE_FFT_IQ) ? this._client.DisplayCenterFrequency : this._client.ChannelCenterFrequency;
				}
				return 0L;
			}
			set
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					if (this._client.StreamingMode == StreamingMode.STREAM_MODE_FFT_IQ)
					{
						if (this.CanTune)
						{
							this._client.DisplayCenterFrequency = (uint)value;
						}
					}
					else
					{
						this._client.ChannelCenterFrequency = (uint)value;
					}
				}
			}
		}

		public bool CanTune
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					if (!this._client.CanControl)
					{
						return this._client.MaximumTunableFrequency > this._client.MinimumTunableFrequency;
					}
					return true;
				}
				return false;
			}
		}

		public long MinimumTunableFrequency
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.MinimumTunableFrequency;
				}
				return 0L;
			}
		}

		public long MaximumTunableFrequency
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.MaximumTunableFrequency;
				}
				return 9223372036854775807L;
			}
		}

		public double Samplerate
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.ChannelSamplerate;
				}
				return 0.0;
			}
		}

		public int FFTRange
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.FFTRange;
				}
				return 0;
			}
			set
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					this._client.FFTRange = value;
				}
			}
		}

		public int FFTOffset
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.FFTOffset;
				}
				return 0;
			}
			set
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					this._client.FFTOffset = value;
				}
			}
		}

		public int DisplayPixels
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.DisplayPixels;
				}
				return 0;
			}
			set
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					this._client.DisplayPixels = value;
				}
			}
		}

		public int DisplayBandwidth
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.DisplayBandwidth;
				}
				return 0;
			}
		}

		public long VFOFrequency
		{
			get
			{
				return (this._client != null) ? this._client.ChannelCenterFrequency : 0;
			}
			set
			{
				if (this._client == null && this._client.IsSynchronized)
				{
					return;
				}
				this._client.ChannelCenterFrequency = (uint)value;
			}
		}

		public int VFODecimation
		{
			get
			{
				if (this._client != null)
				{
					return this._client.ChannelDecimationStageCount;
				}
				return 0;
			}
			set
			{
				if (this._client != null && this._client.IsSynchronized && this._client.ChannelDecimationStageCount != value)
				{
					this._client.ChannelDecimationStageCount = value;
				}
			}
		}

		public double VFOMaxSampleRate
		{
			get
			{
				if (this._client == null)
				{
					return 0.0;
				}
				return (double)this._client.MaximumSampleRate;
			}
		}

		public int VFOMinIQDecimation
		{
			get
			{
				if (this._client == null)
				{
					return 0;
				}
				return this._client.MinimumIQDecimation;
			}
		}

		public bool FFTEnabled
		{
			get
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					return this._client.StreamingMode == StreamingMode.STREAM_MODE_FFT_IQ;
				}
				return false;
			}
			set
			{
				if (this._client != null && this._client.IsSynchronized)
				{
					this._client.StreamingMode = ((!value) ? StreamingMode.STREAM_MODE_IQ_ONLY : StreamingMode.STREAM_MODE_FFT_IQ);
					this._client.OptimizePropertyChanges = false;
					long num = this._control.FrequencyShiftEnabled ? this._control.FrequencyShift : 0;
					switch (this._client.StreamingMode)
					{
					case StreamingMode.STREAM_MODE_FFT_IQ:
						this._client.DisplayCenterFrequency = (uint)(this._control.CenterFrequency - num);
						this._client.ChannelCenterFrequency = (uint)(this._control.Frequency - num);
						this._client.DisplayDecimationStageCount = this._gui.Decimation;
						this._client.ChannelDecimationStageCount = StreamControl.GetDecimationStageCount((double)this._client.MaximumSampleRate, this._control.DetectorType);
						break;
					case StreamingMode.STREAM_MODE_IQ_ONLY:
						this._client.ChannelCenterFrequency = (uint)(this._control.CenterFrequency - num);
						this._client.ChannelDecimationStageCount = this._gui.Decimation;
						break;
					default:
						throw new ApplicationException("Streaming Mode is not supported: " + value.ToString());
					}
					this._client.OptimizePropertyChanges = true;
				}
			}
		}

		public event EventHandler SampleRateChanged;

		public event SamplesAvailableDelegate<ByteSamplesEventArgs> FFTAvailable;

		public SpyServerIO()
		{
			this._gui = new ControllerPanel(this);
		}

		~SpyServerIO()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (!this._disposed)
			{
				this._disposed = true;
				this.DestroyClient();
				GC.SuppressFinalize(this);
			}
		}

		private void DestroyClient()
		{
			if (this._client != null)
			{
				this._client.Synchronized -= this.Client_Synchronized;
				this._client.Disconnected -= this.Client_Disconnected;
				this._client.SamplesAvailable -= this.Client_SamplesAvailable;
				this._client.FFTAvailable -= this.Client_FFTAvailable;
				this._client.Dispose();
				this._client = null;
			}
		}

		public void Connect()
		{
			if (this._client == null)
			{
				this._client = new SpyClient();
				this._client.Synchronized += this.Client_Synchronized;
				this._client.Disconnected += this.Client_Disconnected;
				this._client.SamplesAvailable += this.Client_SamplesAvailable;
				this._client.FFTAvailable += this.Client_FFTAvailable;
			}
			this._client.Connect(this._gui.Host, this._gui.Port);
			this._control.FrequencyShiftEnabled = false;
			this._control.ResetFrequency(this._client.ChannelCenterFrequency);
			this.UpdateTuningBoundaries();
			bool bandwidth = this._client.MaximumDecimationStageCount != 0;
			List<int> list = new List<int>();
			for (int i = 0; i <= this._client.MaximumDecimationStageCount; i++)
			{
				list.Add((int)this._client.MaximumBandwidth >> i);
			}
			string deviceName = this._client.DeviceName;
			string deviceSerial = this._client.DeviceSerial.ToString("X8");
			int maximumGainIndex = (int)this._client.MaximumGainIndex;
			bool flag = maximumGainIndex > 0;
			if (this._client.Is8bitForced)
			{
				this._gui.Force8bit();
			}
			this._gui.EnableURI(false);
			this._gui.UpdateControlOptions(deviceName, deviceSerial, this._client.ServerVersion, list.ToArray(), this._client.MinimumIQDecimation, maximumGainIndex);
			this._gui.UpdateDisplaySections(true, bandwidth, !this._client.Is8bitForced, flag);
			this.FFTEnabled = !this._gui.UseFullIQ;
			if (flag)
			{
				this._gui.UpdateGain(this._client.Gain, this._client.CanControl);
			}
		}

		private void Client_Disconnected(object sender, EventArgs e)
		{
			if (this._control.IsPlaying)
			{
				this._control.StopRadio();
			}
			if (this._gui.IsHandleCreated)
			{
				this._gui.BeginInvoke((Action)delegate
				{
					this._gui.EnableURI(true);
					this._gui.UpdateDisplaySections(false, false, false, false);
					this._gui.EnableFullIQ(true);
				});
			}
		}

		private void Client_Synchronized(object sender, EventArgs e)
		{
			if (this._client.IsConnected)
			{
				this.UpdateTuningStyle();
				this.UpdateTuningBoundaries();
				if (this._gui.IsHandleCreated)
				{
					this._gui.BeginInvoke((Action)delegate
					{
						this._gui.UpdateGain(this._client.Gain, this._client.CanControl);
					});
				}
			}
		}

		private unsafe void Client_SamplesAvailable(object sender, ComplexSamplesEventArgs e)
		{
			this._callback(this, e.Buffer, e.Length);
		}

		private void Client_FFTAvailable(object sender, ByteSamplesEventArgs e)
		{
			SamplesAvailableDelegate<ByteSamplesEventArgs> fFTAvailable = this.FFTAvailable;
			if (fFTAvailable != null)
			{
				fFTAvailable(sender, e);
			}
		}

		public void Disconnect()
		{
			this._client.Disconnect();
		}

		public void SetFormat(StreamFormat format)
		{
			if (this._client != null)
			{
				this._client.StreamFormat = format;
			}
		}

		public long GetDownstreamBytes()
		{
			if (this._client == null)
			{
				return 0L;
			}
			return this._client.GetDownstreamBytes();
		}

		public void SetControl(object control)
		{
			this._control = (ISharpControl)control;
			if (this._control != null)
			{
				this._control.PropertyChanged += this.Control_PropertyChanged;
			}
		}

		private void Control_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this._client != null)
			{
				string propertyName = e.PropertyName;
				if (!(propertyName == "StartRadio"))
				{
					if (propertyName == "StopRadio")
					{
						this._gui.EnableFullIQ(this._gui.Decimation >= this._client.MinimumIQDecimation);
					}
				}
				else
				{
					this._gui.EnableFullIQ(false);
				}
			}
		}

		public void Open()
		{
			this._gui.EnableURI(true);
			this._control.TuningStyle = TuningStyle.Free;
			this._control.TuningStyleFreezed = true;
		}

		public void Close()
		{
			if (this._client != null)
			{
				this._client.Dispose();
				this._client = null;
			}
			this._control.TuningStyleFreezed = false;
			this._gui.SaveSettings();
		}

		public unsafe void Start(SamplesAvailableDelegate callback)
		{
			if (this._client != null && this._client.IsSynchronized)
			{
				this._callback = callback;
				this._client.StartStreaming();
				return;
			}
			throw new ApplicationException("Not connected to a server");
		}

		public void Stop()
		{
			if (this._client != null)
			{
				this._client.StopStreaming();
			}
		}

		public void SetGain(int value)
		{
			this._client.Gain = value;
		}

		public void SetDecimation(int value)
		{
			switch (this._client.StreamingMode)
			{
			case StreamingMode.STREAM_MODE_IQ_ONLY:
				this._client.ChannelDecimationStageCount = value;
				break;
			case StreamingMode.STREAM_MODE_FFT_IQ:
				this._client.DisplayDecimationStageCount = value;
				break;
			}
			EventHandler sampleRateChanged = this.SampleRateChanged;
			if (sampleRateChanged != null)
			{
				sampleRateChanged(this, EventArgs.Empty);
			}
			if (this._client.StreamingMode == StreamingMode.STREAM_MODE_IQ_ONLY)
			{
				this._control.ResetFrequency(this._client.DeviceCenterFrequency);
			}
		}

		private void UpdateTuningStyle()
		{
			if (!this.CanTune)
			{
				this._control.TuningStyle = TuningStyle.Free;
				this._control.TuningStyleFreezed = true;
			}
			else
			{
				this._control.TuningStyleFreezed = false;
			}
		}

		private void UpdateTuningBoundaries()
		{
			if (this._client != null)
			{
				switch (this._client.StreamingMode)
				{
				case StreamingMode.STREAM_MODE_IQ_ONLY:
					if (!this._client.IsSynchronized)
					{
						this._control.ResetFrequency(this._client.ChannelCenterFrequency);
					}
					break;
				case StreamingMode.STREAM_MODE_FFT_IQ:
					this._control.ResetFrequency(this._client.ChannelCenterFrequency, this._client.DisplayCenterFrequency);
					break;
				}
			}
		}
	}
}
