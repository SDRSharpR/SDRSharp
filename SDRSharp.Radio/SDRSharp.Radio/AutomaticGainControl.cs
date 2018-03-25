using System;

namespace SDRSharp.Radio
{
	public sealed class AutomaticGainControl
	{
		private const float DelayTimeconst = 0.015f;

		private const float WindowTimeconst = 0.018f;

		private const float AttackRiseTimeconst = 0.002f;

		private const float AttackFallTimeconst = 0.005f;

		private const float DecayRisefallRatio = 0.3f;

		private const float ReleaseTimeconst = 0.05f;

		private const float AGCOutscale = 0.7f;

		private UnsafeBuffer _sigDelayBuf;

		private unsafe float* _sigDelayBufPtr;

		private UnsafeBuffer _magBuf;

		private unsafe float* _magBufPtr;

		private float _decayAve;

		private float _attackAve;

		private float _attackRiseAlpha;

		private float _attackFallAlpha;

		private float _decayRiseAlpha;

		private float _decayFallAlpha;

		private float _fixedGain;

		private float _knee;

		private float _gainSlope;

		private float _peak;

		private int _sigDelayPtr;

		private int _magBufPos;

		private int _delaySamples;

		private int _windowSamples;

		private int _hangTime;

		private int _hangTimer;

		private float _threshold;

		private float _slopeFactor;

		private float _decay;

		private double _sampleRate;

		private bool _useHang;

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
					this.Configure(true);
				}
			}
		}

		public bool UseHang
		{
			get
			{
				return this._useHang;
			}
			set
			{
				if (this._useHang != value)
				{
					this._useHang = value;
					this.Configure(false);
				}
			}
		}

		public float Threshold
		{
			get
			{
				return this._threshold;
			}
			set
			{
				if (this._threshold != value)
				{
					this._threshold = value;
					this.Configure(false);
				}
			}
		}

		public float Slope
		{
			get
			{
				return this._slopeFactor;
			}
			set
			{
				if (this._slopeFactor != value)
				{
					this._slopeFactor = value;
					this.Configure(false);
				}
			}
		}

		public float Decay
		{
			get
			{
				return this._decay;
			}
			set
			{
				if (this._decay != value)
				{
					this._decay = value;
					this.Configure(false);
				}
			}
		}

		private unsafe void Configure(bool resetBuffers)
		{
			if (resetBuffers)
			{
				this._sigDelayPtr = 0;
				this._hangTimer = 0;
				this._peak = -16f;
				this._decayAve = -5f;
				this._attackAve = -5f;
				this._magBufPos = 0;
				if (this._sampleRate > 0.0)
				{
					this._delaySamples = (int)(this._sampleRate * 0.014999999664723873);
					this._windowSamples = (int)(this._sampleRate * 0.017999999225139618);
					this._sigDelayBuf = UnsafeBuffer.Create(this._delaySamples, 4);
					this._sigDelayBufPtr = (float*)(void*)this._sigDelayBuf;
					this._magBuf = UnsafeBuffer.Create(this._windowSamples, 4);
					this._magBufPtr = (float*)(void*)this._magBuf;
					for (int i = 0; i < this._windowSamples; i++)
					{
						this._magBufPtr[i] = -16f;
					}
					if (this._delaySamples >= this._sigDelayBuf.Length - 1)
					{
						this._delaySamples = this._sigDelayBuf.Length - 1;
					}
				}
			}
			this._knee = this._threshold / 20f;
			this._gainSlope = this._slopeFactor / 100f;
			this._fixedGain = 0.7f * (float)Math.Pow(10.0, (double)this._knee * ((double)this._gainSlope - 1.0));
			this._attackRiseAlpha = 1f - (float)Math.Exp(-1.0 / (this._sampleRate * 0.0020000000949949026));
			this._attackFallAlpha = 1f - (float)Math.Exp(-1.0 / (this._sampleRate * 0.004999999888241291));
			this._decayRiseAlpha = 1f - (float)Math.Exp(-1.0 / (this._sampleRate * (double)this.Decay * 0.001 * 0.30000001192092896));
			this._hangTime = (int)(this._sampleRate * (double)this.Decay * 0.001);
			if (this._useHang)
			{
				this._decayFallAlpha = 1f - (float)Math.Exp(-1.0 / (this._sampleRate * 0.05000000074505806));
			}
			else
			{
				this._decayFallAlpha = 1f - (float)Math.Exp(-1.0 / (this._sampleRate * (double)this.Decay * 0.001));
			}
		}

		public unsafe void Process(float* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				float num = buffer[i];
				if ((double)num != 0.0)
				{
					num *= 1000f;
					float num2 = this._sigDelayBufPtr[this._sigDelayPtr];
					this._sigDelayBufPtr[this._sigDelayPtr++] = num;
					if (this._sigDelayPtr >= this._delaySamples)
					{
						this._sigDelayPtr = 0;
					}
					float num3 = (float)Math.Log10((double)Math.Abs(num));
					if (float.IsNaN(num3) || float.IsInfinity(num3))
					{
						num3 = -8f;
					}
					float num4 = this._magBufPtr[this._magBufPos];
					this._magBufPtr[this._magBufPos++] = num3;
					if (this._magBufPos >= this._windowSamples)
					{
						this._magBufPos = 0;
					}
					if (num3 > this._peak)
					{
						this._peak = num3;
					}
					else if (num4 == this._peak)
					{
						this._peak = -8f;
						for (int j = 0; j < this._windowSamples; j++)
						{
							num4 = this._magBufPtr[j];
							if (num4 > this._peak)
							{
								this._peak = num4;
							}
						}
					}
					if (this.UseHang)
					{
						if (this._peak > this._attackAve)
						{
							this._attackAve = (1f - this._attackRiseAlpha) * this._attackAve + this._attackRiseAlpha * this._peak;
						}
						else
						{
							this._attackAve = (1f - this._attackFallAlpha) * this._attackAve + this._attackFallAlpha * this._peak;
						}
						if (this._peak > this._decayAve)
						{
							this._decayAve = (1f - this._decayRiseAlpha) * this._decayAve + this._decayRiseAlpha * this._peak;
							this._hangTimer = 0;
						}
						else if (this._hangTimer < this._hangTime)
						{
							this._hangTimer++;
						}
						else
						{
							this._decayAve = (1f - this._decayFallAlpha) * this._decayAve + this._decayFallAlpha * this._peak;
						}
					}
					else
					{
						if (this._peak > this._attackAve)
						{
							this._attackAve = (1f - this._attackRiseAlpha) * this._attackAve + this._attackRiseAlpha * this._peak;
						}
						else
						{
							this._attackAve = (1f - this._attackFallAlpha) * this._attackAve + this._attackFallAlpha * this._peak;
						}
						if (this._peak > this._decayAve)
						{
							this._decayAve = (1f - this._decayRiseAlpha) * this._decayAve + this._decayRiseAlpha * this._peak;
						}
						else
						{
							this._decayAve = (1f - this._decayFallAlpha) * this._decayAve + this._decayFallAlpha * this._peak;
						}
					}
					num3 = ((this._attackAve > this._decayAve) ? this._attackAve : this._decayAve);
					float num5 = (!(num3 <= this._knee)) ? (0.7f * (float)Math.Pow(10.0, (double)num3 * ((double)this._gainSlope - 1.0))) : this._fixedGain;
					buffer[i] = num2 * num5 * 1E-05f;
				}
			}
		}
	}
}
