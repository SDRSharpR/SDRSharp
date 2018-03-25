using System;

namespace SDRSharp.Radio
{
	public sealed class FmDetector
	{
		private const float NarrowAFGain = 0.5f;

		private const float FMGain = 1E-05f;

		private const int MinHissFrequency = 4000;

		private const int MaxHissFrequency = 6000;

		private const int HissFilterOrder = 20;

		private const float HissFactor = 2E-05f;

		private unsafe float* _hissPtr;

		private UnsafeBuffer _hissBuffer;

		private FirFilter _hissFilter;

		private Complex _iqState;

		private float _noiseLevel;

		private double _sampleRate;

		private float _noiseAveragingRatio;

		private int _squelchThreshold;

		private bool _isSquelchOpen;

		private float _noiseThreshold;

		private FmMode _mode;

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
					this._noiseAveragingRatio = (float)(30.0 / this._sampleRate);
					float[] coefficients = FilterBuilder.MakeBandPassKernel(this._sampleRate, 20, 4000.0, 6000.0, WindowType.BlackmanHarris4);
					if (this._hissFilter != null)
					{
						this._hissFilter.Dispose();
					}
					this._hissFilter = new FirFilter(coefficients, 1);
				}
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
					this._noiseThreshold = (float)Math.Log10(2.0 - (double)this._squelchThreshold / 100.0) * 2E-05f;
				}
			}
		}

		public bool IsSquelchOpen
		{
			get
			{
				return this._isSquelchOpen;
			}
		}

		public FmMode Mode
		{
			get
			{
				return this._mode;
			}
			set
			{
				this._mode = value;
			}
		}

		public unsafe void Demodulate(Complex* iq, float* audio, int length)
		{
			for (int i = 0; i < length; i++)
			{
				Complex a = iq[i] * this._iqState.Conjugate();
				float num = a.Modulus();
				if (num > 0f)
				{
					a /= num;
				}
				float num2 = a.Argument();
				if (!float.IsNaN(num2))
				{
					audio[i] = num2 * 1E-05f;
				}
				else
				{
					audio[i] = 0f;
				}
				this._iqState = iq[i];
			}
			if (this._mode == FmMode.Narrow)
			{
				this.ProcessSquelch(audio, length);
				for (int j = 0; j < length; j++)
				{
					audio[j] *= 0.5f;
				}
			}
		}

		private unsafe void ProcessSquelch(float* audio, int length)
		{
			if (this._squelchThreshold > 0)
			{
				if (this._hissBuffer == null || this._hissBuffer.Length != length)
				{
					this._hissBuffer = UnsafeBuffer.Create(length, 4);
					this._hissPtr = (float*)(void*)this._hissBuffer;
				}
				Utils.Memcpy(this._hissPtr, audio, length * 4);
				this._hissFilter.Process(this._hissPtr, length);
				for (int i = 0; i < this._hissBuffer.Length; i++)
				{
					this._noiseLevel = (1f - this._noiseAveragingRatio) * this._noiseLevel + this._noiseAveragingRatio * Math.Abs(this._hissPtr[i]);
					if (this._noiseLevel > this._noiseThreshold)
					{
						audio[i] *= 1E-15f;
					}
				}
				this._isSquelchOpen = (this._noiseLevel < this._noiseThreshold);
			}
			else
			{
				this._isSquelchOpen = true;
			}
		}
	}
}
