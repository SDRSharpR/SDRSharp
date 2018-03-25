using System;

namespace SDRSharp.Radio
{
	public class CarrierLocker
	{
		private const int PllRange = 2000;

		private const int PllBandwith = 10;

		private const float PllThreshold = 2f;

		private const float PllLockTime = 0.5f;

		private const float PllResumeDelay = 0.5f;

		private const float PllZeta = 1.5f;

		private const float PllPhaseAdjM = 0f;

		private const float PllPhaseAdjB = 0f;

		private const float TimeConst = 0.003f;

		private Pll _pll;

		private float _iavg;

		private float _qavg;

		private float _alpha;

		private int _unlockedCount;

		private int _maxUnlockedTicks;

		private bool _resetNeeded;

		private double _sampleRate;

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
					this.Configure();
				}
			}
		}

		public bool IsLocked
		{
			get
			{
				return this._pll.IsLocked;
			}
		}

		private void Configure()
		{
			this._pll.SampleRate = (float)this._sampleRate;
			this._pll.DefaultFrequency = 0f;
			this._pll.Range = 2000f;
			this._pll.Bandwidth = 10f;
			this._pll.Zeta = 1.5f;
			this._pll.PhaseAdjM = 0f;
			this._pll.PhaseAdjB = 0f;
			this._pll.LockTime = 0.5f;
			this._pll.LockThreshold = 2f;
			this._alpha = (float)(1.0 - Math.Exp(-1.0 / (this._sampleRate * 0.0030000000260770321)));
			this._maxUnlockedTicks = (int)(this._sampleRate * 0.5);
		}

		public void Reset()
		{
			this._resetNeeded = true;
		}

		public unsafe void Process(Complex* buffer, int length)
		{
			if (this._resetNeeded)
			{
				this._pll.Reset();
				this._resetNeeded = false;
			}
			for (int i = 0; i < length; i++)
			{
				Complex b = Complex.FromAngleFast(this._pll.Phase);
				Complex* intPtr = buffer + i;
				*intPtr *= b;
				Complex complex = buffer[i];
				if (this._pll.StickOnFrequencyIfNotLocked || this._pll.IsLocked)
				{
					this._iavg += this._alpha * (complex.Real - this._iavg);
					this._qavg += this._alpha * (complex.Imag - this._qavg);
					complex.Real = this._iavg;
					complex.Imag = this._qavg;
					this._pll.StickOnFrequencyIfNotLocked = true;
					if (this._pll.IsLocked)
					{
						this._unlockedCount = 0;
					}
					else if (++this._unlockedCount > this._maxUnlockedTicks)
					{
						this._pll.StickOnFrequencyIfNotLocked = false;
						this._unlockedCount = 0;
					}
				}
				complex *= b.Conjugate();
				this._pll.Process(complex);
			}
		}
	}
}
