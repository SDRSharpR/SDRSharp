using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.Runtime.InteropServices;

namespace SDRSharp.FrontEnds.AirspyHF
{
	public class AirspyHFDevice : IDisposable
	{
		public const string DeviceName = "AIRSPY HF+";

		public const uint DefaultFrequency = 7200000u;

		public const uint DefaultSampleRate = 768000u;

		public const uint DefaultIFShift = 192000u;

		private uint _deviceSampleRate = 768000u;

		private int _decimationStages;

		private IntPtr _dev;

		private uint _centerFrequency;

		private bool _isStreaming;

		private double _ifShift;

		private GCHandle _gcHandle;

		private DownConverter _ddc;

		private unsafe static readonly airspyhf_sample_cb _airspyhfCallback = AirspyHFDevice.AirSpyHFSamplesAvailable;

		public int DecimationStages
		{
			get
			{
				return this._decimationStages;
			}
			set
			{
				this._decimationStages = value;
				this.SetDeviceFrequency();
			}
		}

		public int CalibrationPPB
		{
			get
			{
				int result;
				NativeMethods.airspyhf_get_calibration(this._dev, out result);
				return result;
			}
			set
			{
				NativeMethods.airspyhf_set_calibration(this._dev, value);
			}
		}

		public double SampleRate
		{
			get
			{
				return (double)this._deviceSampleRate / (double)(1 << this._decimationStages);
			}
		}

		public uint Frequency
		{
			get
			{
				return this._centerFrequency;
			}
			set
			{
				this._centerFrequency = value;
				this.SetDeviceFrequency();
			}
		}

		public bool IsStreaming
		{
			get
			{
				return this._isStreaming;
			}
		}

		public bool IsHung
		{
			get
			{
				if (this._isStreaming)
				{
					return !NativeMethods.airspyhf_is_streaming(this._dev);
				}
				return false;
			}
		}

		public event SamplesAvailableDelegate<ComplexSamplesEventArgs> SamplesAvailable;

		public unsafe AirspyHFDevice()
		{
			if (NativeMethods.airspyhf_open(out this._dev) != 0)
			{
				throw new ApplicationException("Cannot open AIRSPY HF+ device");
			}
			uint num = default(uint);
			if (NativeMethods.airspyhf_get_samplerates(this._dev, &num, 0u) == airspyhf_error.SUCCESS && num != 0)
			{
				uint[] array = new uint[num];
				uint[] array2 = array;
				airspyhf_error airspyhf_error;
				fixed (uint* buffer = array2)
				{
					airspyhf_error = NativeMethods.airspyhf_get_samplerates(this._dev, buffer, num);
				}
				if (airspyhf_error == airspyhf_error.SUCCESS)
				{
					this._deviceSampleRate = array[0];
					NativeMethods.airspyhf_set_samplerate(this._dev, this._deviceSampleRate);
				}
			}
			this._gcHandle = GCHandle.Alloc(this);
		}

		~AirspyHFDevice()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (this._dev != IntPtr.Zero)
			{
				this.Stop();
				NativeMethods.airspyhf_close(this._dev);
				if (this._gcHandle.IsAllocated)
				{
					this._gcHandle.Free();
				}
				this._dev = IntPtr.Zero;
				GC.SuppressFinalize(this);
			}
		}

		internal void FlashCalibration()
		{
			NativeMethods.airspyhf_flash_calibration(this._dev);
		}

		public unsafe void Start()
		{
			if (!this._isStreaming)
			{
				if (NativeMethods.airspyhf_start(this._dev, AirspyHFDevice._airspyhfCallback, (IntPtr)this._gcHandle) != 0)
				{
					throw new ApplicationException("airspy_start_rx() error");
				}
				this._isStreaming = true;
			}
		}

		public void Stop()
		{
			if (this._isStreaming)
			{
				NativeMethods.airspyhf_stop(this._dev);
				this._isStreaming = false;
			}
		}

		private void SetDeviceFrequency()
		{
			if (!(this._dev == IntPtr.Zero))
			{
				uint num = this._centerFrequency;
				if (this._decimationStages > 1)
				{
					this._ifShift = -192000.0;
					num += 192000;
				}
				else
				{
					this._ifShift = 0.0;
				}
				NativeMethods.airspyhf_set_freq(this._dev, num);
			}
		}

		protected unsafe virtual void OnComplexSamplesAvailable(Complex* buffer, int length, ulong droppedSamples)
		{
			SamplesAvailableDelegate<ComplexSamplesEventArgs> samplesAvailable = this.SamplesAvailable;
			if (samplesAvailable != null)
			{
				ComplexSamplesEventArgs complexSamplesEventArgs = new ComplexSamplesEventArgs();
				complexSamplesEventArgs.Buffer = buffer;
				complexSamplesEventArgs.Length = length;
				complexSamplesEventArgs.DroppedSamples = droppedSamples;
				samplesAvailable(this, complexSamplesEventArgs);
			}
		}

		private unsafe static int AirSpyHFSamplesAvailable(airspyhf_transfer* data)
		{
			int length = data->sample_count;
			Complex* samples = data->samples;
			ulong num = data->dropped_samples;
			IntPtr ctx = data->ctx;
			GCHandle gCHandle = GCHandle.FromIntPtr(ctx);
			if (!gCHandle.IsAllocated)
			{
				return -1;
			}
			AirspyHFDevice airspyHFDevice = (AirspyHFDevice)gCHandle.Target;
			if (airspyHFDevice._decimationStages > 0)
			{
				int num2 = 1 << airspyHFDevice._decimationStages;
				if (airspyHFDevice._ddc == null || airspyHFDevice._ddc.DecimationRatio != num2)
				{
					airspyHFDevice._ddc = new DownConverter((double)airspyHFDevice._deviceSampleRate, num2);
				}
				airspyHFDevice._ddc.Frequency = airspyHFDevice._ifShift;
				length = airspyHFDevice._ddc.Process(samples, length);
				num >>= airspyHFDevice._decimationStages;
			}
			airspyHFDevice.OnComplexSamplesAvailable(samples, length, num);
			return 0;
		}

		public unsafe airspyhf_error TunerRead(uint address, byte* data)
		{
			return NativeMethods.airspyhf_tuner_read(this._dev, address, data);
		}

		public airspyhf_error TunerWrite(uint address, uint value)
		{
			return NativeMethods.airspyhf_tuner_write(this._dev, address, value);
		}
	}
}
