using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SDRSharp.FrontEnds.Airspy
{
	public class AirspyDevice : IDisposable
	{
		private enum CalibrationState
		{
			UnCalibrated,
			PrologSent,
			EpilogueSent
		}

		private class AnalogFilterConfig
		{
			public byte LPF
			{
				get;
				set;
			}

			public byte HPF
			{
				get;
				set;
			}

			public int Shift
			{
				get;
				set;
			}
		}

		private class AnalogFilterSet
		{
			public uint SampleRate
			{
				get;
				set;
			}

			public AnalogFilterConfig[] Filters
			{
				get;
				set;
			}
		}

		public const float TimeConst = 0.05f;

		public const uint DefaultFrequency = 103000000u;

		public const uint DefaultSpyVerterLO = 120000000u;

		public const uint DefaultSpyVerterThreshold = 35000000u;

		public const uint CalibrationDelay = 500u;

		public const uint CalibrationDuration = 100u;

		public const string DeviceName = "AIRSPY";

		private static readonly AnalogFilterSet[] _analogDecimationFilters = new AnalogFilterSet[4]
		{
			new AnalogFilterSet
			{
				SampleRate = 10000000u,
				Filters = new AnalogFilterConfig[6]
				{
					new AnalogFilterConfig
					{
						LPF = 60,
						HPF = 2,
						Shift = 0
					},
					new AnalogFilterConfig
					{
						LPF = 38,
						HPF = 7,
						Shift = 1250000
					},
					new AnalogFilterConfig
					{
						LPF = 28,
						HPF = 4,
						Shift = 2750000
					},
					new AnalogFilterConfig
					{
						LPF = 15,
						HPF = 5,
						Shift = 3080000
					},
					new AnalogFilterConfig
					{
						LPF = 6,
						HPF = 5,
						Shift = 3200000
					},
					new AnalogFilterConfig
					{
						LPF = 2,
						HPF = 6,
						Shift = 3250000
					}
				}
			},
			new AnalogFilterSet
			{
				SampleRate = 2500000u,
				Filters = new AnalogFilterConfig[4]
				{
					new AnalogFilterConfig
					{
						LPF = 4,
						HPF = 0,
						Shift = 0
					},
					new AnalogFilterConfig
					{
						LPF = 8,
						HPF = 3,
						Shift = -280000
					},
					new AnalogFilterConfig
					{
						LPF = 5,
						HPF = 5,
						Shift = -500000
					},
					new AnalogFilterConfig
					{
						LPF = 3,
						HPF = 6,
						Shift = -550000
					}
				}
			},
			new AnalogFilterSet
			{
				SampleRate = 6000000u,
				Filters = new AnalogFilterConfig[5]
				{
					new AnalogFilterConfig
					{
						LPF = 33,
						HPF = 2,
						Shift = 0
					},
					new AnalogFilterConfig
					{
						LPF = 26,
						HPF = 2,
						Shift = 1000000
					},
					new AnalogFilterConfig
					{
						LPF = 17,
						HPF = 4,
						Shift = 1100000
					},
					new AnalogFilterConfig
					{
						LPF = 7,
						HPF = 5,
						Shift = 1200000
					},
					new AnalogFilterConfig
					{
						LPF = 2,
						HPF = 6,
						Shift = 1250000
					}
				}
			},
			new AnalogFilterSet
			{
				SampleRate = 3000000u,
				Filters = new AnalogFilterConfig[3]
				{
					new AnalogFilterConfig
					{
						LPF = 8,
						HPF = 0,
						Shift = 0
					},
					new AnalogFilterConfig
					{
						LPF = 7,
						HPF = 4,
						Shift = -250000
					},
					new AnalogFilterConfig
					{
						LPF = 2,
						HPF = 4,
						Shift = -300000
					}
				}
			}
		};

		private IntPtr _dev;

		private uint _sampleRate;

		private uint _centerFrequency;

		private uint _frequencySet;

		private int _decimationStages;

		private byte _vgaGain;

		private byte _mixerGain;

		private byte _lnaGain;

		private byte _linearityGain;

		private byte _sensitivityGain;

		private bool _isStreaming;

		private bool _lnaGainAuto;

		private bool _mixerGainAuto;

		private bool _biasTeeEnabled;

		private bool _biasTeeState;

		private bool _spyVerterEnabled;

		private bool _usePacking;

		private bool _bypassTrackingFilter;

		private bool _bypassTrackingFilterState;

		private uint[] _supportedSampleRates;

		private float _spyVerterPPM;

		private float _iavg;

		private float _qavg;

		private float _alpha;

		private byte _old_0x0f_value;

		private byte _old_0x0b_value;

		private CalibrationState _calibrationState;

		private Timer _calibrationTimer;

		private AnalogFilterConfig _analogFilterConfig;

		private DownConverter _ddc;

		private AirspyGainMode _gainMode;

		private GCHandle _gcHandle;

		private bool _useDynamicRangeEnhancements = Utils.GetBooleanSetting("airspy.useDynamicRangeEnhancements", true);

		private unsafe static readonly airspy_sample_block_cb_fn _airspyCallback = AirspyDevice.AirSpySamplesAvailable;

		public uint[] SupportedSampleRates
		{
			get
			{
				return this._supportedSampleRates;
			}
		}

		public uint SampleRate
		{
			get
			{
				return this._sampleRate;
			}
			set
			{
				if (value != this._sampleRate)
				{
					if (NativeMethods.airspy_set_samplerate(this._dev, value) != 0)
					{
						throw new ApplicationException("Sample rate is not supported");
					}
					this._sampleRate = value;
					this.OnSampleRateChanged();
				}
			}
		}

		public bool UseDynamicRangeEnhancements
		{
			get
			{
				return this._useDynamicRangeEnhancements;
			}
			set
			{
				this._useDynamicRangeEnhancements = value;
			}
		}

		public uint DecimatedSampleRate
		{
			get
			{
				return this._sampleRate >> this._decimationStages;
			}
		}

		public bool UsePacking
		{
			get
			{
				return this._usePacking;
			}
			set
			{
				this._usePacking = value;
				NativeMethods.airspy_set_packing(this._dev, this._usePacking);
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
				this.UpdateFrequency();
			}
		}

		public byte VgaGain
		{
			get
			{
				return this._vgaGain;
			}
			set
			{
				this._vgaGain = value;
				this.UpdateGains();
			}
		}

		public byte MixerGain
		{
			get
			{
				return this._mixerGain;
			}
			set
			{
				this._mixerGain = value;
				this.UpdateGains();
			}
		}

		public byte LnaGain
		{
			get
			{
				return this._lnaGain;
			}
			set
			{
				this._lnaGain = value;
				this.UpdateGains();
			}
		}

		public bool MixerGainAuto
		{
			get
			{
				return this._mixerGainAuto;
			}
			set
			{
				this._mixerGainAuto = value;
				NativeMethods.airspy_set_mixer_agc(this._dev, value);
			}
		}

		public bool LnaGainAuto
		{
			get
			{
				return this._lnaGainAuto;
			}
			set
			{
				this._lnaGainAuto = value;
				NativeMethods.airspy_set_lna_agc(this._dev, value);
			}
		}

		public byte LinearityGain
		{
			get
			{
				return this._linearityGain;
			}
			set
			{
				this._linearityGain = value;
				this.UpdateGains();
			}
		}

		public byte SensitivityGain
		{
			get
			{
				return this._sensitivityGain;
			}
			set
			{
				this._sensitivityGain = value;
				this.UpdateGains();
			}
		}

		public AirspyGainMode GainMode
		{
			get
			{
				return this._gainMode;
			}
			set
			{
				this._gainMode = value;
				this.UpdateGains();
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
				if (this._decimationStages != value)
				{
					this._decimationStages = value;
					this._alpha = (float)(1.0 - Math.Exp(-1.0 / (double)((float)(double)this.DecimatedSampleRate * 0.05f)));
					this.OnSampleRateChanged();
				}
			}
		}

		public bool BiasTeeEnabled
		{
			get
			{
				return this._biasTeeEnabled;
			}
			set
			{
				this._biasTeeEnabled = value;
				this.UpdateFrequency();
			}
		}

		public bool BypassTrackingFilter
		{
			get
			{
				return this._bypassTrackingFilter;
			}
			set
			{
				this._bypassTrackingFilter = value;
				this.UpdateTrackingFilter();
			}
		}

		public bool SpyVerterEnabled
		{
			get
			{
				return this._spyVerterEnabled;
			}
			set
			{
				this._spyVerterEnabled = value;
				this.UpdateFrequency();
			}
		}

		public float SpyVerterPPM
		{
			get
			{
				return this._spyVerterPPM;
			}
			set
			{
				this._spyVerterPPM = value;
				this.UpdateFrequency();
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
					return NativeMethods.airspy_is_streaming(this._dev) != airspy_error.AIRSPY_TRUE;
				}
				return false;
			}
		}

		public event SamplesAvailableDelegate<ComplexSamplesEventArgs> ComplexSamplesAvailable;

		public event SamplesAvailableDelegate<RealSamplesEventArgs> RealSamplesAvailable;

		public event EventHandler SampleRateChanged;

		public unsafe AirspyDevice(bool useRealSamples = false)
		{
			if (NativeMethods.airspy_open(out this._dev) != 0)
			{
				throw new ApplicationException("Cannot open AirSpy device");
			}
			if (useRealSamples)
			{
				NativeMethods.airspy_set_sample_type(this._dev, airspy_sample_type.AIRSPY_SAMPLE_FLOAT32_REAL);
			}
			else
			{
				NativeMethods.airspy_set_sample_type(this._dev, airspy_sample_type.AIRSPY_SAMPLE_FLOAT32_IQ);
			}
			uint num = default(uint);
			NativeMethods.airspy_get_samplerates(this._dev, &num, 0u);
			this._supportedSampleRates = new uint[num];
			uint[] supportedSampleRates = this._supportedSampleRates;
			fixed (uint* buffer = supportedSampleRates)
			{
				NativeMethods.airspy_get_samplerates(this._dev, buffer, num);
			}
			this._sampleRate = this._supportedSampleRates[0];
			this._alpha = (float)(1.0 - Math.Exp(-1.0 / (double)((float)(double)this.DecimatedSampleRate * 0.05f)));
			NativeMethods.airspy_set_samplerate(this._dev, this._sampleRate);
			NativeMethods.airspy_set_rf_bias(this._dev, false);
			NativeMethods.airspy_set_packing(this._dev, false);
			this.UpdateGains();
			this._calibrationTimer = new Timer(this.CalibrationHandler, null, 500L, -1L);
			this._gcHandle = GCHandle.Alloc(this);
		}

		~AirspyDevice()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (this._dev != IntPtr.Zero)
			{
				try
				{
					this.Stop();
					NativeMethods.airspy_close(this._dev);
				}
				catch (AccessViolationException)
				{
				}
				if (this._gcHandle.IsAllocated)
				{
					this._gcHandle.Free();
				}
				this._dev = IntPtr.Zero;
				GC.SuppressFinalize(this);
			}
		}

		private void SendCalibrationProlog()
		{
			if (this._calibrationState == CalibrationState.UnCalibrated)
			{
				airspy_error airspy_error = NativeMethods.airspy_r820t_read(this._dev, (byte)15, out this._old_0x0f_value);
				if (airspy_error >= airspy_error.AIRSPY_SUCCESS)
				{
					airspy_error = NativeMethods.airspy_r820t_read(this._dev, (byte)11, out this._old_0x0b_value);
					if (airspy_error >= airspy_error.AIRSPY_SUCCESS)
					{
						NativeMethods.airspy_r820t_write(this._dev, 15, (byte)((this._old_0x0f_value & -5) | 4));
						NativeMethods.airspy_r820t_write(this._dev, 11, (byte)((this._old_0x0b_value & -17) | 0x10));
						this._calibrationState = CalibrationState.PrologSent;
					}
				}
			}
		}

		private void SendCalibrationEpilogue()
		{
			if (this._calibrationState == CalibrationState.PrologSent)
			{
				NativeMethods.airspy_r820t_write(this._dev, 11, this._old_0x0b_value);
				NativeMethods.airspy_r820t_write(this._dev, 15, this._old_0x0f_value);
				this._old_0x0b_value = 0;
				this._old_0x0f_value = 0;
				this._calibrationState = CalibrationState.EpilogueSent;
			}
		}

		private void AbortCalibration()
		{
			if (this._calibrationState == CalibrationState.PrologSent)
			{
				this.SendCalibrationEpilogue();
			}
			this._calibrationState = CalibrationState.UnCalibrated;
		}

		public void CalibrateIF()
		{
			lock (this)
			{
				this.AbortCalibration();
				this._calibrationTimer.Change(500L, -1L);
			}
		}

		private void CalibrationHandler(object state)
		{
			if (this._isStreaming)
			{
				lock (this)
				{
					switch (this._calibrationState)
					{
					case CalibrationState.UnCalibrated:
						this.SendCalibrationProlog();
						this._calibrationTimer.Change(100L, -1L);
						break;
					case CalibrationState.PrologSent:
						this.SendCalibrationEpilogue();
						break;
					}
				}
			}
		}

		public unsafe void Start()
		{
			if (!this._isStreaming)
			{
				this._iavg = 0f;
				this._qavg = 0f;
				int num = (!this._useDynamicRangeEnhancements) ? Math.Min(this._decimationStages, ConversionFilters.FirKernels100dB.Length - 1) : 0;
				float[] array = ConversionFilters.FirKernels100dB[num];
				float[] array2 = array;
				fixed (float* kernel = array2)
				{
					NativeMethods.airspy_set_conversion_filter_float32(this._dev, kernel, array.Length);
				}
				if (NativeMethods.airspy_start_rx(this._dev, AirspyDevice._airspyCallback, (IntPtr)this._gcHandle) != 0)
				{
					throw new ApplicationException("airspy_start_rx() error");
				}
				this.UpdateTrackingFilter();
				this.UpdateAnalogIFFilters();
				this._isStreaming = true;
			}
		}

		public void Stop()
		{
			if (this._isStreaming)
			{
				NativeMethods.airspy_stop_rx(this._dev);
				this._isStreaming = false;
			}
		}

		private void UpdateGains()
		{
			switch (this._gainMode)
			{
			case AirspyGainMode.Custom:
				NativeMethods.airspy_set_lna_gain(this._dev, this._lnaGain);
				NativeMethods.airspy_set_mixer_gain(this._dev, this._mixerGain);
				NativeMethods.airspy_set_vga_gain(this._dev, this._vgaGain);
				NativeMethods.airspy_set_lna_agc(this._dev, this._lnaGainAuto);
				NativeMethods.airspy_set_mixer_agc(this._dev, this._mixerGainAuto);
				break;
			case AirspyGainMode.Linearity:
				NativeMethods.airspy_set_linearity_gain(this._dev, this._linearityGain);
				break;
			case AirspyGainMode.Sensitivity:
				NativeMethods.airspy_set_sensitivity_gain(this._dev, this._sensitivityGain);
				break;
			}
		}

		private void UpdateFrequency()
		{
			uint num = this._centerFrequency;
			if (this._spyVerterEnabled && num < 35000000)
			{
				num += (uint)(120000000.0 * (1.0 + (double)this._spyVerterPPM * 1E-06));
				if (!this._biasTeeState)
				{
					NativeMethods.airspy_set_rf_bias(this._dev, true);
					this._biasTeeState = true;
				}
			}
			else if (this._biasTeeState != this._biasTeeEnabled)
			{
				NativeMethods.airspy_set_rf_bias(this._dev, this._biasTeeEnabled);
				this._biasTeeState = this._biasTeeEnabled;
			}
			if (this._analogFilterConfig != null)
			{
				num = (uint)((int)num - this._analogFilterConfig.Shift);
			}
			if (this._frequencySet != num)
			{
				this._frequencySet = num;
				this.SetDeviceCenterFrequency();
				this._bypassTrackingFilterState = false;
				this.UpdateTrackingFilter();
			}
		}

		private void UpdateTrackingFilter()
		{
			if (this._bypassTrackingFilter)
			{
				if (!this._bypassTrackingFilterState)
				{
					byte b;
					NativeMethods.airspy_r820t_read(this._dev, (byte)26, out b);
					b = (byte)((b & 0x3F) | 0x40);
					NativeMethods.airspy_r820t_write(this._dev, 26, b);
					this._bypassTrackingFilterState = true;
				}
			}
			else if (this._bypassTrackingFilterState)
			{
				this.SetDeviceCenterFrequency();
				this._bypassTrackingFilterState = false;
			}
		}

		private void SetDeviceCenterFrequency()
		{
			lock (this)
			{
				this.AbortCalibration();
				NativeMethods.airspy_set_freq(this._dev, this._frequencySet);
				this.CalibrateIF();
			}
		}

		private void UpdateAnalogIFFilters()
		{
			this._analogFilterConfig = null;
			if (this._useDynamicRangeEnhancements)
			{
				AnalogFilterSet analogFilterSet = Array.Find(AirspyDevice._analogDecimationFilters, (AnalogFilterSet item) => item.SampleRate == this._sampleRate);
				if (analogFilterSet != null)
				{
					int num = Math.Min(this._decimationStages, analogFilterSet.Filters.Length - 1);
					this._analogFilterConfig = analogFilterSet.Filters[num];
					this.SetAnalogIFFilters(this._analogFilterConfig.LPF, this._analogFilterConfig.HPF);
				}
			}
		}

		private void UpdateDDC()
		{
			int num = 1 << this._decimationStages;
			if (this._ddc != null && this._ddc.DecimationRatio == num && this._ddc.SampleRate == (double)this._sampleRate)
			{
				return;
			}
			this._ddc = new DownConverter((double)this._sampleRate, num);
			AnalogFilterConfig analogFilterConfig = this._analogFilterConfig;
			if (analogFilterConfig != null)
			{
				this._ddc.Frequency = (double)analogFilterConfig.Shift;
			}
		}

		public virtual void OnSampleRateChanged()
		{
			this._alpha = (float)(1.0 - Math.Exp(-1.0 / (double)((float)(double)this.DecimatedSampleRate * 0.05f)));
			this.UpdateAnalogIFFilters();
			EventHandler sampleRateChanged = this.SampleRateChanged;
			if (sampleRateChanged != null)
			{
				sampleRateChanged(this, EventArgs.Empty);
			}
		}

		protected unsafe virtual void OnComplexSamplesAvailable(Complex* buffer, int length, ulong droppedSamples)
		{
			SamplesAvailableDelegate<ComplexSamplesEventArgs> complexSamplesAvailable = this.ComplexSamplesAvailable;
			if (complexSamplesAvailable != null)
			{
				ComplexSamplesEventArgs complexSamplesEventArgs = new ComplexSamplesEventArgs();
				complexSamplesEventArgs.Buffer = buffer;
				complexSamplesEventArgs.Length = length;
				complexSamplesEventArgs.DroppedSamples = droppedSamples;
				complexSamplesAvailable(this, complexSamplesEventArgs);
			}
		}

		protected unsafe virtual void OnRealSamplesAvailable(float* buffer, int length, ulong droppedSamples)
		{
			SamplesAvailableDelegate<RealSamplesEventArgs> realSamplesAvailable = this.RealSamplesAvailable;
			if (realSamplesAvailable != null)
			{
				RealSamplesEventArgs realSamplesEventArgs = new RealSamplesEventArgs();
				realSamplesEventArgs.Buffer = buffer;
				realSamplesEventArgs.Length = length;
				realSamplesEventArgs.DroppedSamples = droppedSamples;
				realSamplesAvailable(this, realSamplesEventArgs);
			}
		}

		private unsafe static int AirSpySamplesAvailable(airspy_transfer* data)
		{
			int num = data->sample_count;
			ulong dropped_samples = data->dropped_samples;
			IntPtr ctx = data->ctx;
			GCHandle gCHandle = GCHandle.FromIntPtr(ctx);
			if (!gCHandle.IsAllocated)
			{
				return -1;
			}
			AirspyDevice airspyDevice = (AirspyDevice)gCHandle.Target;
			if (data->sample_type == airspy_sample_type.AIRSPY_SAMPLE_FLOAT32_REAL)
			{
				float* samples = (float*)data->samples;
				airspyDevice.OnRealSamplesAvailable(samples, num, dropped_samples);
			}
			else
			{
				Complex* samples2 = (Complex*)data->samples;
				bool flag = airspyDevice._analogFilterConfig != null && airspyDevice._analogFilterConfig.Shift != 0;
				if (airspyDevice._decimationStages > 0)
				{
					airspyDevice.UpdateDDC();
					num = airspyDevice._ddc.Process(samples2, num);
				}
				if (!flag)
				{
					float num2 = airspyDevice._iavg;
					float num3 = airspyDevice._qavg;
					float alpha = airspyDevice._alpha;
					for (int i = 0; i < num; i++)
					{
						num2 += alpha * (samples2[i].Real - num2);
						num3 += alpha * (samples2[i].Imag - num3);
						samples2[i].Real -= num2;
						samples2[i].Imag -= num3;
					}
					airspyDevice._iavg = num2;
					airspyDevice._qavg = num3;
				}
				airspyDevice.OnComplexSamplesAvailable(samples2, num, dropped_samples);
			}
			return 0;
		}

		public void Dump(string baseFileName = "")
		{
			string str = "airspy_dump_" + DateTime.Now.ToString("yyyy_MM_dd__hh_mm_ss__");
			this.DumpFile(268435456, 131072, Path.Combine(baseFileName, str + "local_sram0_128K.bin"));
			this.DumpFile(268959744, 73728, Path.Combine(baseFileName, str + "local_sram1_72K.bin"));
			this.DumpFile(402653184, 18432, Path.Combine(baseFileName, str + "m0sub_sram_18K.bin"));
			this.DumpFile(536870912, 32768, Path.Combine(baseFileName, str + "ahb1_sram_32K.bin"));
			this.DumpFile(671088640, 32768, Path.Combine(baseFileName, str + "ahb2_sram_32K.bin"));
			this.DumpFile(1074724884, 812, Path.Combine(baseFileName, str + "periph_adchs.bin"));
			this.DumpFile(1074728716, 4, Path.Combine(baseFileName, str + "periph_adchs_status0.bin"));
			this.DumpFile(1074728748, 4, Path.Combine(baseFileName, str + "periph_adchs_status1.bin"));
			this.DumpFile(1074069524, 184, Path.Combine(baseFileName, str + "periph_cgu.bin"));
			this.DumpTunerRegs(Path.Combine(baseFileName, str + "r820t.txt"));
		}

		private void DumpTunerRegs(string filename)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Address\tValue");
			for (int i = 0; i < 32; i++)
			{
				byte b;
				NativeMethods.airspy_r820t_read(this._dev, (byte)i, out b);
				stringBuilder.AppendLine("0x" + i.ToString("x2") + "\t0x" + b.ToString("x2"));
			}
			File.WriteAllText(filename, stringBuilder.ToString());
		}

		private unsafe void DumpFile(int address, int length, string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.CreateNew);
			try
			{
				byte[] array = new byte[256];
				try
				{
					byte[] array2 = array;
					fixed (byte* data = array2)
					{
						while (length > 0)
						{
							ushort num = (ushort)Math.Min(array.Length, length);
							if (NativeMethods.airspy_spiflash_read(this._dev, (uint)address, num, data) != 0)
							{
								break;
							}
							fileStream.Write(array, 0, num);
							length -= num;
							address += num;
						}
					}
				}
				finally
				{
				}
			}
			finally
			{
				fileStream.Close();
			}
		}

		public byte GetR820TRegister(byte reg)
		{
			byte result;
			NativeMethods.airspy_r820t_read(this._dev, reg, out result);
			return result;
		}

		public void SetR820TRegister(byte reg, byte value)
		{
			NativeMethods.airspy_r820t_write(this._dev, reg, value);
		}

		public byte GetSi5351CRegister(byte reg)
		{
			byte result;
			NativeMethods.airspy_si5351c_read(this._dev, reg, out result);
			return result;
		}

		public void SetSi5351CRegister(byte reg, byte value)
		{
			NativeMethods.airspy_si5351c_write(this._dev, reg, value);
		}

		public void SetGPIO(airspy_gpio_port_t port, airspy_gpio_pin_t pin, bool value)
		{
			NativeMethods.airspy_gpio_write(this._dev, port, pin, value);
		}

		public bool GetGPIO(airspy_gpio_port_t port, airspy_gpio_pin_t pin)
		{
			bool result;
			NativeMethods.airspy_gpio_read(this._dev, port, pin, out result);
			return result;
		}

		public unsafe uint GetMemory(uint address)
		{
			uint result = 0u;
			NativeMethods.airspy_spiflash_read(this._dev, address, 4, (byte*)(&result));
			return result;
		}

		public void SetIFBandwidth(byte bw)
		{
			this.SetAnalogIFFilters(bw, 0);
		}

		public void SetAnalogIFFilters(byte lpf, byte hpf)
		{
			byte[] array = new byte[4]
			{
				224,
				128,
				96,
				0
			};
			byte[] array2 = new byte[16]
			{
				15,
				14,
				13,
				12,
				11,
				10,
				9,
				8,
				7,
				6,
				5,
				4,
				3,
				2,
				1,
				0
			};
			int num = 0xF0 | array2[lpf & 0xF];
			int num2 = array[lpf >> 4] | array2[hpf & 0xF];
			NativeMethods.airspy_r820t_write(this._dev, 10, (byte)num);
			NativeMethods.airspy_r820t_write(this._dev, 11, (byte)num2);
		}

		public unsafe void WriteFlash(uint address, ushort length, byte* data)
		{
			NativeMethods.airspy_spiflash_write(this._dev, address, length, data);
		}

		public unsafe void ReadFlash(uint address, ushort length, byte* data)
		{
			NativeMethods.airspy_spiflash_read(this._dev, address, length, data);
		}

		public void EraseFlashSector(ushort sector_num)
		{
			NativeMethods.airspy_spiflash_erase_sector(this._dev, sector_num);
		}
	}
}
