using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.Airspy
{
	public class AirspyIO : IFrontendController, IIQStreamController, ITunableSource, ISampleRateChangeSource, IControlAwareObject, ISpectrumProvider, IConfigurationPanelProvider, IFrontendOffset, IDisposable
	{
		private const int DefaultIFOffset = -1582;

		private const double DefaultSampleRate = 10000000.0;

		private bool _disposed;

		private ControllerPanel _gui;

		private AirspyDevice _airspyDevice;

		private ISharpControl _control;

		private long _frequency = 102998418L;

		private SamplesAvailableDelegate _callback;

		private static float _aliasFreeRatio = (float)Math.Min(Math.Max(Utils.GetDoubleSetting("airspy.aliasFreeRatio", 0.8), 0.1), 1.0);

		public bool CanTune
		{
			get
			{
				return true;
			}
		}

		public long MinimumTunableFrequency
		{
			get
			{
				return 0L;
			}
		}

		public long MaximumTunableFrequency
		{
			get
			{
				return 2000000000L;
			}
		}

		public ISharpControl SharpControl
		{
			get
			{
				return this._control;
			}
		}

		public double Samplerate
		{
			get
			{
				if (this._airspyDevice != null)
				{
					return (double)this._airspyDevice.DecimatedSampleRate;
				}
				return 10000000.0;
			}
		}

		public long Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				this._frequency = value;
				this.SetDeviceFrequency();
			}
		}

		public float UsableSpectrumRatio
		{
			get
			{
				return AirspyIO._aliasFreeRatio;
			}
		}

		public UserControl Gui
		{
			get
			{
				return this._gui;
			}
		}

		public bool IsDeviceHung
		{
			get
			{
				if (this._airspyDevice != null && this._airspyDevice.IsHung)
				{
					return true;
				}
				return false;
			}
		}

		public int Offset
		{
			get
			{
				return -1582;
			}
		}

		public event EventHandler SampleRateChanged;

		public AirspyIO()
		{
			this._gui = new ControllerPanel(this);
		}

		~AirspyIO()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (!this._disposed)
			{
				this._disposed = true;
				this.Close();
				GC.SuppressFinalize(this);
			}
		}

		public void Open()
		{
			this._airspyDevice = new AirspyDevice(false);
			this._airspyDevice.Frequency = (uint)this._frequency;
			this._gui.Device = this._airspyDevice;
			this._airspyDevice.ComplexSamplesAvailable += this.AirSpyDevice_SamplesAvailable;
			this._airspyDevice.SampleRateChanged += this.AirSpyDevice_SampleRateChanged;
			this._gui.RefreshTimerEnabled = true;
		}

		public void Close()
		{
			if (this._airspyDevice != null)
			{
				this._gui.RefreshTimerEnabled = false;
				this._gui.SaveSettings();
				this._gui.Device = null;
				this._airspyDevice.ComplexSamplesAvailable -= this.AirSpyDevice_SamplesAvailable;
				this._airspyDevice.SampleRateChanged -= this.AirSpyDevice_SampleRateChanged;
				this._airspyDevice.Dispose();
				this._airspyDevice = null;
			}
		}

		public unsafe void Start(SamplesAvailableDelegate callback)
		{
			this._callback = callback;
			try
			{
				if (this._airspyDevice == null)
				{
					this.Open();
				}
				this._airspyDevice.Start();
				this.SetDeviceFrequency();
			}
			catch
			{
				this.Close();
				throw;
			}
		}

		public void Stop()
		{
			if (this._airspyDevice != null)
			{
				this._airspyDevice.Stop();
			}
		}

		public void SetControl(object control)
		{
			this._control = (ISharpControl)control;
		}

		private void SetDeviceFrequency()
		{
			if (this._airspyDevice != null)
			{
				this._airspyDevice.Frequency = (uint)this._frequency;
			}
		}

		private unsafe void AirSpyDevice_SamplesAvailable(object sender, ComplexSamplesEventArgs e)
		{
			this._callback(this, e.Buffer, e.Length);
		}

		private void AirSpyDevice_SampleRateChanged(object sender, EventArgs e)
		{
			EventHandler evt = this.SampleRateChanged;
			this._gui.BeginInvoke((Action)delegate
			{
				if (evt != null)
				{
					evt(this, EventArgs.Empty);
				}
			});
		}
	}
}
