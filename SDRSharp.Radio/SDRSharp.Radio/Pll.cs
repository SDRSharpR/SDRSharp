using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	[StructLayout(LayoutKind.Sequential, Pack = 16)]
	public struct Pll
	{
		private float _sampleRate;

		private float _phase;

		private float _frequencyRadian;

		private float _minFrequencyRadian;

		private float _maxFrequencyRadian;

		private float _defaultFrequency;

		private float _range;

		private float _bandwidth;

		private float _alpha;

		private float _beta;

		private float _zeta;

		private float _phaseAdj;

		private float _phaseAdjM;

		private float _phaseAdjB;

		private float _lockAlpha;

		private float _lockOneMinusAlpha;

		private float _lockTime;

		private float _phaseErrorAvg;

		private float _adjustedPhase;

		private float _lockThreshold;

		private bool _stickOnFrequencyIfNotLocked;

		public float AdjustedPhase
		{
			get
			{
				return this._adjustedPhase;
			}
		}

		public float Phase
		{
			get
			{
				return this._phase;
			}
		}

		public float SampleRate
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
					this.Configure();
				}
			}
		}

		public float DefaultFrequency
		{
			get
			{
				return this._defaultFrequency;
			}
			set
			{
				if (this._defaultFrequency != value)
				{
					this._defaultFrequency = value;
					this.Configure();
				}
			}
		}

		public float Range
		{
			get
			{
				return this._range;
			}
			set
			{
				if (this._range != value)
				{
					this._range = value;
					this.Configure();
				}
			}
		}

		public float Bandwidth
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
					this.Configure();
				}
			}
		}

		public float LockTime
		{
			get
			{
				return this._lockTime;
			}
			set
			{
				if (this._lockTime != value)
				{
					this._lockTime = value;
					this.Configure();
				}
			}
		}

		public float LockThreshold
		{
			get
			{
				return this._lockThreshold;
			}
			set
			{
				if (this._lockThreshold != value)
				{
					this._lockThreshold = value;
					this.Configure();
				}
			}
		}

		public float Zeta
		{
			get
			{
				return this._zeta;
			}
			set
			{
				if (this._zeta != value)
				{
					this._zeta = value;
					this.Configure();
				}
			}
		}

		public float PhaseAdjM
		{
			get
			{
				return this._phaseAdjM;
			}
			set
			{
				if (this._phaseAdjM != value)
				{
					this._phaseAdjM = value;
					this.Configure();
				}
			}
		}

		public float PhaseAdjB
		{
			get
			{
				return this._phaseAdjB;
			}
			set
			{
				if (this._phaseAdjB != value)
				{
					this._phaseAdjB = value;
					this.Configure();
				}
			}
		}

		public bool StickOnFrequencyIfNotLocked
		{
			get
			{
				return this._stickOnFrequencyIfNotLocked;
			}
			set
			{
				this._stickOnFrequencyIfNotLocked = value;
			}
		}

		public bool IsLocked
		{
			get
			{
				return this._phaseErrorAvg < this._lockThreshold;
			}
		}

		private void Configure()
		{
			this._phase = 0f;
			float num = (float)(6.2831853071795862 / (double)this._sampleRate);
			this._frequencyRadian = this._defaultFrequency * num;
			this._minFrequencyRadian = (this._defaultFrequency - this._range) * num;
			this._maxFrequencyRadian = (this._defaultFrequency + this._range) * num;
			this._alpha = 2f * this._zeta * this._bandwidth * num;
			this._beta = this._alpha * this._alpha / (4f * this._zeta * this._zeta);
			this._phaseAdj = this._phaseAdjM * this._sampleRate + this._phaseAdjB;
			this._lockAlpha = (float)(1.0 - Math.Exp(-1.0 / (double)(this._sampleRate * this._lockTime)));
			this._lockOneMinusAlpha = 1f - this._lockAlpha;
		}

		public Complex Process(float sample)
		{
			Complex complex = Trig.SinCos(this._phase);
			complex *= sample;
			float phaseError = 0f - complex.ArgumentFast();
			this.ProcessPhaseError(phaseError);
			return complex;
		}

		public Complex Process(Complex sample)
		{
			Complex complex = Trig.SinCos(this._phase);
			complex *= sample;
			float phaseError = 0f - complex.ArgumentFast();
			this.ProcessPhaseError(phaseError);
			return complex;
		}

		private void ProcessPhaseError(float phaseError)
		{
			this._phaseErrorAvg = this._lockOneMinusAlpha * this._phaseErrorAvg + this._lockAlpha * phaseError * phaseError;
			if (this._stickOnFrequencyIfNotLocked && !this.IsLocked)
			{
				this._phase += this._frequencyRadian;
			}
			else
			{
				this._frequencyRadian += this._beta * phaseError;
				if (this._frequencyRadian > this._maxFrequencyRadian)
				{
					this._frequencyRadian = this._maxFrequencyRadian;
				}
				else if (this._frequencyRadian < this._minFrequencyRadian)
				{
					this._frequencyRadian = this._minFrequencyRadian;
				}
				this._phase += this._frequencyRadian + this._alpha * phaseError;
			}
			this._phase %= 6.28318548f;
			this._adjustedPhase = this._phase + this._phaseAdj;
		}

		public void Reset()
		{
			this._phase = 0f;
			this._frequencyRadian = 0f;
			this._phaseErrorAvg = 10f;
			this._stickOnFrequencyIfNotLocked = false;
		}
	}
}
