using System;

namespace SDRSharp.Radio
{
	public sealed class Vfo
	{
		private const float NFMDeemphasisTime = 0.000149999993f;

		public const int DefaultCwSideTone = 600;

		public const int DefaultSSBBandwidth = 2400;

		public const int DefaultWFMBandwidth = 250000;

		public const int MinSSBAudioFrequency = 100;

		public const int MinBCAudioFrequency = 20;

		public const int MaxBCAudioFrequency = 15000;

		public const int MaxNFMBandwidth = 15000;

		public const int MinNFMAudioFrequency = 300;

		public const int MaxNFMAudioFrequency = 3800;

		private readonly double _minThreadedSampleRate = Utils.GetDoubleSetting("minThreadedSampleRate", 1000000.0);

		private readonly AutomaticGainControl _agc = new AutomaticGainControl();

		private readonly AmDetector _amDetector = new AmDetector();

		private readonly FmDetector _fmDetector = new FmDetector();

		private readonly SideBandDetector _sideBandDetector = new SideBandDetector();

		private readonly CwDetector _cwDetector = new CwDetector();

		private readonly StereoDecoder _stereoDecoder = new StereoDecoder();

		private readonly RdsDecoder _rdsDecoder = new RdsDecoder();

		private readonly CarrierLocker _carrierLocker = new CarrierLocker();

		private readonly AmAntiFading _amAntiFading = new AmAntiFading();

		private readonly HookManager _hookManager;

		private DownConverter _mainDownConverter;

		private FrequencyTranslator _ifOffsetTranslator;

		private ComplexFilter _iqFilter;

		private ComplexFilter _audioFIR;

		private IirFilter _audioIIR;

		private DetectorType _detectorType;

		private DetectorType _actualDetectorType;

		private WindowType _windowType;

		private double _sampleRate;

		private int _bandwidth;

		private int _frequency;

		private int _ifOffset;

		private int _filterOrder;

		private int _decimationStageCount;

		private int _baseBandDecimationStageCount;

		private int _audioDecimationStageCount;

		private int _cwToneShift;

		private bool _needConfigure;

		private bool _lockCarrier;

		private bool _useAgc;

		private float _agcThreshold;

		private float _agcDecay;

		private float _agcSlope;

		private bool _agcUseHang;

		private bool _useAntiFading;

		private float _deemphasisAlpha;

		private float _deemphasisState;

		private int _squelchThreshold;

		private bool _fmStereo;

		private bool _filterAudio;

		private bool _bypassDemodulation;

		private bool _muted;

		private bool _hooksEnabled;

		private UnsafeBuffer _rawAudioBuffer;

		private unsafe float* _rawAudioPtr;

		public DetectorType DetectorType
		{
			get
			{
				return this._detectorType;
			}
			set
			{
				if (value != this._detectorType)
				{
					this._detectorType = value;
					this._needConfigure = true;
				}
			}
		}

		public int Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				if (this._frequency != value)
				{
					this._frequency = value;
					this._needConfigure = true;
					this._carrierLocker.Reset();
				}
			}
		}

		public int IFOffset
		{
			get
			{
				return this._ifOffset;
			}
			set
			{
				if (this._ifOffset != value)
				{
					this._ifOffset = value;
					this._needConfigure = true;
					this._carrierLocker.Reset();
				}
			}
		}

		public int FilterOrder
		{
			get
			{
				return this._filterOrder;
			}
			set
			{
				if (this._filterOrder != value)
				{
					this._filterOrder = value;
					this._needConfigure = true;
				}
			}
		}

		public double SampleRate
		{
			get
			{
				return this._sampleRate;
			}
			set
			{
				if (this._sampleRate != value)
				{
					this._sampleRate = value;
					this._needConfigure = true;
				}
			}
		}

		public WindowType WindowType
		{
			get
			{
				return this._windowType;
			}
			set
			{
				if (this._windowType != value)
				{
					this._windowType = value;
					this._needConfigure = true;
				}
			}
		}

		public int Bandwidth
		{
			get
			{
				return this._bandwidth;
			}
			set
			{
				if (this._bandwidth != value)
				{
					this._bandwidth = value;
					this._needConfigure = true;
				}
			}
		}

		public bool UseAGC
		{
			get
			{
				return this._useAgc;
			}
			set
			{
				this._useAgc = value;
			}
		}

		public float AgcThreshold
		{
			get
			{
				return this._agcThreshold;
			}
			set
			{
				if (this._agcThreshold != value)
				{
					this._agcThreshold = value;
					this._needConfigure = true;
				}
			}
		}

		public float AgcDecay
		{
			get
			{
				return this._agcDecay;
			}
			set
			{
				if (this._agcDecay != value)
				{
					this._agcDecay = value;
					this._needConfigure = true;
				}
			}
		}

		public float AgcSlope
		{
			get
			{
				return this._agcSlope;
			}
			set
			{
				if (this._agcSlope != value)
				{
					this._agcSlope = value;
					this._needConfigure = true;
				}
			}
		}

		public bool AgcHang
		{
			get
			{
				return this._agcUseHang;
			}
			set
			{
				if (this._agcUseHang != value)
				{
					this._agcUseHang = value;
					this._needConfigure = true;
				}
			}
		}

		public bool UseAntiFading
		{
			get
			{
				return this._useAntiFading;
			}
			set
			{
				this._useAntiFading = value;
			}
		}

		public int SquelchThreshold
		{
			get
			{
				return this._squelchThreshold;
			}
			set
			{
				if (this._squelchThreshold != value)
				{
					this._squelchThreshold = value;
					this._needConfigure = true;
				}
			}
		}

		public bool IsSquelchOpen
		{
			get
			{
				if (this._actualDetectorType == DetectorType.NFM && this._fmDetector.IsSquelchOpen)
				{
					return true;
				}
				if (this._actualDetectorType == DetectorType.AM)
				{
					return this._amDetector.IsSquelchOpen;
				}
				return false;
			}
		}

		public int DecimationStageCount
		{
			get
			{
				return this._decimationStageCount;
			}
			set
			{
				if (this._decimationStageCount != value)
				{
					this._decimationStageCount = value;
					this._needConfigure = true;
				}
			}
		}

		public int CWToneShift
		{
			get
			{
				return this._cwToneShift;
			}
			set
			{
				if (this._cwToneShift != value)
				{
					this._cwToneShift = value;
					this._needConfigure = true;
				}
			}
		}

		public bool LockCarrier
		{
			get
			{
				return this._lockCarrier;
			}
			set
			{
				if (this._lockCarrier != value)
				{
					this._lockCarrier = value;
					this._needConfigure = true;
				}
			}
		}

		public bool FmStereo
		{
			get
			{
				return this._fmStereo;
			}
			set
			{
				if (this._fmStereo != value)
				{
					this._fmStereo = value;
					this._needConfigure = true;
				}
			}
		}

		public bool Muted
		{
			get
			{
				return this._muted;
			}
			set
			{
				this._muted = value;
			}
		}

		public bool HookdEnabled
		{
			get
			{
				return this._hooksEnabled;
			}
			set
			{
				this._hooksEnabled = value;
			}
		}

		public bool SignalIsStereo
		{
			get
			{
				if (this._actualDetectorType == DetectorType.WFM && this._fmStereo)
				{
					return this._stereoDecoder.IsPllLocked;
				}
				return false;
			}
		}

		public string RdsStationName
		{
			get
			{
				return this._rdsDecoder.ProgramService;
			}
		}

		public string RdsStationText
		{
			get
			{
				return this._rdsDecoder.RadioText;
			}
		}

		public ushort RdsPICode
		{
			get
			{
				return this._rdsDecoder.PICode;
			}
		}

		public bool RdsUseFEC
		{
			get
			{
				return this._rdsDecoder.UseFEC;
			}
			set
			{
				this._rdsDecoder.UseFEC = value;
			}
		}

		public bool FilterAudio
		{
			get
			{
				return this._filterAudio;
			}
			set
			{
				this._filterAudio = value;
			}
		}

		public bool BypassDemodulation
		{
			get
			{
				return this._bypassDemodulation;
			}
			set
			{
				this._bypassDemodulation = value;
			}
		}

		public double BasebandSampleRate
		{
			get
			{
				return this._sampleRate / (double)(1 << this._baseBandDecimationStageCount);
			}
		}

		public Vfo(HookManager hookManager = null)
		{
			this._hookManager = hookManager;
			this._bandwidth = 2400;
			this._filterOrder = 500;
			this._rdsDecoder.RdsFrameAvailable += this.RdsFrameAvailableHandler;
			this._needConfigure = true;
		}

		public void RdsReset()
		{
			this._rdsDecoder.Reset();
		}

		public void CarrierLockerReset()
		{
			this._carrierLocker.Reset();
		}

		private void ConfigureHookSampleRates()
		{
			if (this._hookManager != null)
			{
				this._hookManager.SetProcessorSampleRate(ProcessorType.RawIQ, this._sampleRate);
				this._hookManager.SetProcessorSampleRate(ProcessorType.DecimatedAndFilteredIQ, this._sampleRate / (double)(1 << this._baseBandDecimationStageCount));
				this._hookManager.SetProcessorSampleRate(ProcessorType.DemodulatorOutput, this._sampleRate / (double)(1 << this._baseBandDecimationStageCount));
				this._hookManager.SetProcessorSampleRate(ProcessorType.FMMPX, this._sampleRate / (double)(1 << this._baseBandDecimationStageCount));
				this._hookManager.SetProcessorSampleRate(ProcessorType.FilteredAudioOutput, this._sampleRate / (double)(1 << this._decimationStageCount));
			}
		}

		public void Init()
		{
			this.Configure(false);
		}

		private void Configure(bool refreshOnly = true)
		{
			if (this._sampleRate != 0.0)
			{
				this._actualDetectorType = this._detectorType;
				bool refresh = false;
				this._baseBandDecimationStageCount = StreamControl.GetDecimationStageCount(this._sampleRate, this._actualDetectorType);
				this._audioDecimationStageCount = this._decimationStageCount - this._baseBandDecimationStageCount;
				int num = 1 << this._baseBandDecimationStageCount;
				double num2 = this._sampleRate / (double)num;
				if (!refreshOnly || this._mainDownConverter == null || this._mainDownConverter.SampleRate != this._sampleRate || this._mainDownConverter.DecimationRatio != num)
				{
					this._mainDownConverter = new DownConverter(this._sampleRate, num);
					refresh = true;
					this.ConfigureHookSampleRates();
				}
				this._mainDownConverter.Frequency = (double)this._frequency;
				if (!refreshOnly || this._ifOffsetTranslator == null || this._ifOffsetTranslator.SampleRate != num2)
				{
					this._ifOffsetTranslator = new FrequencyTranslator(num2);
				}
				this._ifOffsetTranslator.Frequency = (double)(-this._ifOffset);
				this.UpdateFilters(refresh);
				this._carrierLocker.SampleRate = num2;
				this._cwDetector.SampleRate = num2;
				this._fmDetector.SampleRate = num2;
				this._fmDetector.SquelchThreshold = this._squelchThreshold;
				this._amDetector.SquelchThreshold = this._squelchThreshold;
				this._stereoDecoder.Configure(this._fmDetector.SampleRate, this._audioDecimationStageCount);
				this._rdsDecoder.SampleRate = this._fmDetector.SampleRate;
				this._stereoDecoder.ForceMono = !this._fmStereo;
				switch (this._actualDetectorType)
				{
				case DetectorType.CW:
					this._cwDetector.BfoFrequency = this._cwToneShift;
					break;
				case DetectorType.NFM:
					this._fmDetector.Mode = FmMode.Narrow;
					break;
				case DetectorType.WFM:
					this._fmDetector.Mode = FmMode.Wide;
					break;
				}
				this._agc.SampleRate = this._sampleRate / (double)(1 << this._decimationStageCount);
				this._agc.Decay = this._agcDecay;
				this._agc.Slope = this._agcSlope;
				this._agc.Threshold = this._agcThreshold;
				this._agc.UseHang = this._agcUseHang;
				this._needConfigure = false;
			}
		}

		private void UpdateFilters(bool refresh)
		{
			int num = 0;
			int num2 = 15000;
			int num3 = 0;
			switch (this._actualDetectorType)
			{
			case DetectorType.WFM:
			case DetectorType.AM:
				num = 20;
				num2 = Math.Min(this._bandwidth / 2, 15000);
				break;
			case DetectorType.CW:
				num = Math.Abs(this._cwToneShift) - this._bandwidth / 2;
				num2 = Math.Abs(this._cwToneShift) + this._bandwidth / 2;
				break;
			case DetectorType.USB:
				num = (this._lockCarrier ? 20 : 100);
				num2 = this._bandwidth;
				num3 = this._bandwidth / 2;
				break;
			case DetectorType.LSB:
				num = (this._lockCarrier ? 20 : 100);
				num2 = this._bandwidth;
				num3 = -this._bandwidth / 2;
				break;
			case DetectorType.DSB:
				num = 20;
				num2 = this._bandwidth / 2;
				break;
			case DetectorType.NFM:
				num = 300;
				num2 = Math.Min(this._bandwidth / 2, 3800);
				break;
			}
			Complex[] array = FilterBuilder.MakeComplexKernel(this._sampleRate / (double)(1 << this._baseBandDecimationStageCount), this._filterOrder, (double)this._bandwidth, (double)num3, this._windowType);
			if ((this._iqFilter == null | refresh) || this._iqFilter.KernelSize != array.Length)
			{
				this._iqFilter = new ComplexFilter(array);
			}
			else
			{
				this._iqFilter.SetKernel(array);
			}
			double num4 = this._sampleRate / (double)(1 << this._decimationStageCount);
			if (refresh)
			{
				if (this._actualDetectorType == DetectorType.CW)
				{
					this._audioIIR.Init(IirFilterType.BandPass, (double)Math.Abs(this._cwToneShift), num4, 3.0);
				}
				else if (this._actualDetectorType == DetectorType.WFM)
				{
					double sampleRate = this._sampleRate / (double)(1 << this._baseBandDecimationStageCount);
					this._audioIIR.Init(IirFilterType.HighPass, (double)num, sampleRate, 1.0);
				}
				else
				{
					this._audioIIR.Init(IirFilterType.HighPass, (double)num, num4, 1.0);
				}
			}
			Complex[] array2 = FilterBuilder.MakeComplexKernel(num4, this._filterOrder, (double)(num2 - num), (double)(num2 + num) * 0.5, this._windowType);
			if ((this._audioFIR == null | refresh) || this._audioFIR.KernelSize != array2.Length)
			{
				this._audioFIR = new ComplexFilter(array2);
			}
			else
			{
				this._audioFIR.SetKernel(array2);
			}
			this._deemphasisAlpha = (float)(1.0 - Math.Exp(-1.0 / (num4 * 0.00014999999257270247)));
			this._deemphasisState = 0f;
		}

		public unsafe void ProcessBuffer(Complex* iqBuffer, float* audioBuffer, int length)
		{
			if (this._needConfigure)
			{
				this.Configure(true);
			}
			length = this._mainDownConverter.Process(iqBuffer, length);
			this._ifOffsetTranslator.Process(iqBuffer, length);
			if (this._lockCarrier && (this._actualDetectorType == DetectorType.LSB || this._actualDetectorType == DetectorType.USB))
			{
				this._carrierLocker.Process(iqBuffer, length);
			}
			this._iqFilter.Process(iqBuffer, length);
			if (this._hookManager != null && this._hooksEnabled)
			{
				this._hookManager.ProcessDecimatedAndFilteredIQ(iqBuffer, length);
			}
			if (this._lockCarrier && (this._actualDetectorType == DetectorType.DSB || this._actualDetectorType == DetectorType.AM))
			{
				this._carrierLocker.Process(iqBuffer, length);
			}
			if (this._lockCarrier && this._useAntiFading && this._carrierLocker.IsLocked && (this._actualDetectorType == DetectorType.DSB || this._actualDetectorType == DetectorType.AM))
			{
				this._amAntiFading.Process(iqBuffer, length);
			}
			if (this._actualDetectorType == DetectorType.RAW)
			{
				Utils.Memcpy(audioBuffer, iqBuffer, length * sizeof(Complex));
				if (this._hookManager != null && this._hooksEnabled)
				{
					this._hookManager.ProcessFilteredAudioOutput(audioBuffer, length * 2);
				}
				if (this._muted)
				{
					Vfo.MuteAudio(audioBuffer, length * 2);
				}
			}
			else
			{
				if (this._rawAudioBuffer == null || this._rawAudioBuffer.Length != length)
				{
					this._rawAudioBuffer = UnsafeBuffer.Create(length, 4);
					this._rawAudioPtr = (float*)(void*)this._rawAudioBuffer;
				}
				if (this._actualDetectorType != DetectorType.WFM)
				{
					Vfo.ScaleIQ(iqBuffer, length);
				}
				if (this._bypassDemodulation)
				{
					if (this._actualDetectorType == DetectorType.WFM)
					{
						length >>= this._audioDecimationStageCount;
					}
					length <<= 1;
					Vfo.MuteAudio(audioBuffer, length);
				}
				else
				{
					this.Demodulate(iqBuffer, this._rawAudioPtr, length);
					if (this._hookManager != null && this._hooksEnabled)
					{
						this._hookManager.ProcessDemodulatorOutput(this._rawAudioPtr, length);
					}
					switch (this._actualDetectorType)
					{
					case DetectorType.WFM:
						if (this._filterAudio)
						{
							this._audioIIR.Process(this._rawAudioPtr, length);
						}
						if (this._hookManager != null && this._hooksEnabled)
						{
							this._hookManager.ProcessFMMPX(this._rawAudioPtr, length);
						}
						this._rdsDecoder.Process(this._rawAudioPtr, length);
						this._stereoDecoder.Process(this._rawAudioPtr, audioBuffer, length);
						length >>= this._audioDecimationStageCount;
						break;
					case DetectorType.NFM:
						if (this._filterAudio)
						{
							this._audioIIR.Process(this._rawAudioPtr, length);
							this._audioFIR.Process(this._rawAudioPtr, length, 1);
							this.Deemphasis(this._rawAudioPtr, length);
						}
						if (this._useAgc)
						{
							this._agc.Process(this._rawAudioPtr, length);
						}
						Vfo.MonoToStereo(this._rawAudioPtr, audioBuffer, length);
						break;
					default:
						if (this._useAgc)
						{
							this._agc.Process(this._rawAudioPtr, length);
						}
						if (this._filterAudio)
						{
							this._audioIIR.Process(this._rawAudioPtr, length);
							this._audioFIR.Process(this._rawAudioPtr, length, 1);
						}
						Vfo.MonoToStereo(this._rawAudioPtr, audioBuffer, length);
						break;
					}
					length <<= 1;
					if (this._hookManager != null && this._hooksEnabled)
					{
						this._hookManager.ProcessFilteredAudioOutput(audioBuffer, length);
					}
					if (this._muted)
					{
						Vfo.MuteAudio(audioBuffer, length);
					}
				}
			}
		}

		private unsafe static void MuteAudio(float* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				buffer[i] = 0f;
			}
		}

		private unsafe static void ScaleIQ(Complex* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				buffer[i].Real *= 0.01f;
				buffer[i].Imag *= 0.01f;
			}
		}

		private unsafe static void MonoToStereo(float* input, float* output, int inputLength)
		{
			for (int i = 0; i < inputLength; i++)
			{
				float* intPtr = output;
				output = intPtr + 1;
				*intPtr = *input;
				float* intPtr2 = output;
				output = intPtr2 + 1;
				*intPtr2 = *input;
				input++;
			}
		}

		private unsafe void Deemphasis(float* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				this._deemphasisState += this._deemphasisAlpha * (buffer[i] - this._deemphasisState);
				buffer[i] = this._deemphasisState;
			}
		}

		private unsafe void Demodulate(Complex* iq, float* audio, int length)
		{
			switch (this._actualDetectorType)
			{
			case DetectorType.NFM:
			case DetectorType.WFM:
				this._fmDetector.Demodulate(iq, audio, length);
				break;
			case DetectorType.AM:
				this._amDetector.Demodulate(iq, audio, length);
				break;
			case DetectorType.DSB:
			case DetectorType.LSB:
			case DetectorType.USB:
				this._sideBandDetector.Demodulate(iq, audio, length);
				break;
			case DetectorType.CW:
				this._cwDetector.Demodulate(iq, audio, length);
				break;
			}
		}

		private void RdsFrameAvailableHandler(ref RdsFrame frame)
		{
			if (this._hookManager != null)
			{
				this._hookManager.ProcessRdsBitStream(ref frame);
			}
		}
	}
}
