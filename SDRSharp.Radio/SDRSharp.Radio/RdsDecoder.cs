using System;

namespace SDRSharp.Radio
{
	public class RdsDecoder
	{
		private const int PllDefaultFrequency = 57000;

		private const int PllRange = 12;

		private const int PllBandwith = 1;

		private const float PllZeta = 0.707f;

		private const float PllLockTime = 0.5f;

		private const float PllLockThreshold = 3.2f;

		private const float RdsBitRate = 1187.5f;

		private readonly RdsDetectorBank _bitDecoder;

		private unsafe readonly Pll* _pll;

		private readonly UnsafeBuffer _pllBuffer;

		private readonly Oscillator _osc = new Oscillator();

		private unsafe readonly IirFilter* _syncFilter;

		private readonly UnsafeBuffer _syncFilterBuffer;

		private UnsafeBuffer _rawBuffer;

		private unsafe Complex* _rawPtr;

		private UnsafeBuffer _magBuffer;

		private unsafe float* _magPtr;

		private UnsafeBuffer _dataBuffer;

		private unsafe float* _dataPtr;

		private DownConverter _decimator;

		private FirFilter _matchedFilter;

		private IQFirFilter _baseBandFilter;

		private double _sampleRate;

		private double _demodulationSampleRate;

		private int _decimationFactor;

		private bool _configureNeeded;

		private float _lastSync;

		private float _lastData;

		private float _lastSyncSlope;

		private bool _lastBit;

		public double SampleRate
		{
			get
			{
				return this._sampleRate;
			}
			set
			{
				if (value != this._sampleRate)
				{
					this._sampleRate = value;
					this._configureNeeded = true;
				}
			}
		}

		public string RadioText
		{
			get
			{
				return this._bitDecoder.RadioText;
			}
		}

		public string ProgramService
		{
			get
			{
				return this._bitDecoder.ProgramService;
			}
		}

		public ushort PICode
		{
			get
			{
				return this._bitDecoder.PICode;
			}
		}

		public bool UseFEC
		{
			get
			{
				return this._bitDecoder.UseFEC;
			}
			set
			{
				this._bitDecoder.UseFEC = value;
			}
		}

		public event RdsFrameAvailableDelegate RdsFrameAvailable;

		public unsafe RdsDecoder()
		{
			this._pllBuffer = UnsafeBuffer.Create(sizeof(Pll));
			this._pll = (Pll*)(void*)this._pllBuffer;
			this._syncFilterBuffer = UnsafeBuffer.Create(sizeof(IirFilter));
			this._syncFilter = (IirFilter*)(void*)this._syncFilterBuffer;
			this._bitDecoder = new RdsDetectorBank();
			this._bitDecoder.FrameAvailable += this.FrameAvailableHandler;
		}

		private unsafe void Configure()
		{
			this._osc.SampleRate = this._sampleRate;
			this._osc.Frequency = 57000.0;
			int i;
			for (i = 0; this._sampleRate >= (double)(20000 << i); i++)
			{
			}
			this._decimationFactor = 1 << i;
			this._demodulationSampleRate = this._sampleRate / (double)this._decimationFactor;
			this._decimator = new DownConverter(this._demodulationSampleRate, this._decimationFactor);
			float[] coefficients = FilterBuilder.MakeLowPassKernel(this._demodulationSampleRate, 200, 2500.0, WindowType.BlackmanHarris4);
			this._baseBandFilter = new IQFirFilter(coefficients, 1);
			this._pll->SampleRate = (float)this._demodulationSampleRate;
			this._pll->DefaultFrequency = 0f;
			this._pll->Range = 12f;
			this._pll->Bandwidth = 1f;
			this._pll->Zeta = 0.707f;
			this._pll->LockTime = 0.5f;
			this._pll->LockThreshold = 3.2f;
			int length = (int)(this._demodulationSampleRate / 1187.5) | 1;
			coefficients = FilterBuilder.MakeSin(this._demodulationSampleRate, 1187.5, length);
			this._matchedFilter = new FirFilter(coefficients, 1);
			this._syncFilter->Init(IirFilterType.BandPass, 1187.5, this._demodulationSampleRate, 500.0);
		}

		public unsafe void Reset()
		{
			this._bitDecoder.Reset();
			this._syncFilter->Reset();
		}

		public unsafe void Process(float* baseBand, int length)
		{
			if (this._configureNeeded)
			{
				this.Configure();
				this._configureNeeded = false;
			}
			if (this._rawBuffer == null || this._rawBuffer.Length != length)
			{
				this._rawBuffer = UnsafeBuffer.Create(length, sizeof(Complex));
				this._rawPtr = (Complex*)(void*)this._rawBuffer;
			}
			if (this._magBuffer == null || this._magBuffer.Length != length)
			{
				this._magBuffer = UnsafeBuffer.Create(length, 4);
				this._magPtr = (float*)(void*)this._magBuffer;
			}
			if (this._dataBuffer == null || this._dataBuffer.Length != length)
			{
				this._dataBuffer = UnsafeBuffer.Create(length, 4);
				this._dataPtr = (float*)(void*)this._dataBuffer;
			}
			for (int i = 0; i < length; i++)
			{
				this._osc.Tick();
				this._rawPtr[i] = this._osc.Phase * baseBand[i];
			}
			this._decimator.Process(this._rawPtr, length);
			length /= this._decimationFactor;
			this._baseBandFilter.Process(this._rawPtr, length);
			for (int j = 0; j < length; j++)
			{
				this._dataPtr[j] = this._pll->Process(this._rawPtr[j]).Imag;
			}
			this._matchedFilter.Process(this._dataPtr, length);
			for (int k = 0; k < length; k++)
			{
				this._magPtr[k] = Math.Abs(this._dataPtr[k]);
			}
			this._syncFilter->Process(this._magPtr, length);
			for (int l = 0; l < length; l++)
			{
				float lastData = this._dataPtr[l];
				float num = this._magPtr[l];
				float num2 = num - this._lastSync;
				this._lastSync = num;
				if (num2 < 0f && this._lastSyncSlope * num2 < 0f)
				{
					bool flag = this._lastData > 0f;
					this._bitDecoder.Process(flag ^ this._lastBit);
					this._lastBit = flag;
				}
				this._lastData = lastData;
				this._lastSyncSlope = num2;
			}
		}

		private void FrameAvailableHandler(ref RdsFrame frame)
		{
			RdsFrameAvailableDelegate rdsFrameAvailable = this.RdsFrameAvailable;
			if (rdsFrameAvailable != null)
			{
				rdsFrameAvailable(ref frame);
			}
		}
	}
}
