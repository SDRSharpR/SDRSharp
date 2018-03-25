using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.AirspyHF
{
	public class AirspyHFIO : IFrontendController, IIQStreamController, ITunableSource, IFrontendOffset, IControlAwareObject, ISpectrumProvider, IConfigurationPanelProvider, ISampleRateChangeSource, IDisposable
	{
		private const float AliasFreeRatio = 1f;

		private bool _disposed;

		private ControllerPanel _gui;

		private AirspyHFDevice _device;

		private ISharpControl _control;

		private long _frequency = 7200000L;

		private int _decimationStages;

		private SamplesAvailableDelegate _callback;

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
				return 1700000000L;
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
				return 768000.0 / Math.Pow(2.0, (double)this._decimationStages);
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

		public bool IsDeviceHung
		{
			get
			{
				if (this._device != null)
				{
					return this._device.IsHung;
				}
				return false;
			}
		}

		public int Offset
		{
			get
			{
				return 0;
			}
		}

		public int DecimationStages
		{
			get
			{
				return this._decimationStages;
			}
			set
			{
				this._decimationStages = value;
				if (this._device != null)
				{
					this._device.DecimationStages = value;
				}
				if (this._gui != null)
				{
					this.NotifySampleRateChanged();
				}
			}
		}

		public event EventHandler SampleRateChanged;

		public AirspyHFIO()
		{
			this._gui = new ControllerPanel(this);
		}

		~AirspyHFIO()
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
			this._device = new AirspyHFDevice();
			this._device.DecimationStages = this._decimationStages;
			this._device.Frequency = (uint)this._frequency;
			this._gui.Device = this._device;
			this._device.SamplesAvailable += this.Device_SamplesAvailable;
		}

		public void Close()
		{
			if (this._device != null)
			{
				this._gui.Device = null;
				this._device.SamplesAvailable -= this.Device_SamplesAvailable;
				this._device.Dispose();
				this._device = null;
			}
		}

		public unsafe void Start(SamplesAvailableDelegate callback)
		{
			this._callback = callback;
			try
			{
				if (this._device == null)
				{
					this.Open();
				}
				this._device.Start();
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
			if (this._device != null)
			{
				this._device.Stop();
			}
		}

		public void ShowSettingGUI(IWin32Window parent)
		{
		}

		public void HideSettingGUI()
		{
		}

		public void SetControl(object control)
		{
			this._control = (ISharpControl)control;
		}

		private void SetDeviceFrequency()
		{
			if (this._device != null)
			{
				this._device.Frequency = (uint)this._frequency;
			}
		}

		private unsafe void Device_SamplesAvailable(object sender, ComplexSamplesEventArgs e)
		{
			this._callback(this, e.Buffer, e.Length);
		}

		public void NotifySampleRateChanged()
		{
			this._gui.BeginInvoke((Action)delegate
			{
				EventHandler sampleRateChanged = this.SampleRateChanged;
				if (sampleRateChanged != null)
				{
					sampleRateChanged(this, EventArgs.Empty);
				}
			});
		}
	}
}
