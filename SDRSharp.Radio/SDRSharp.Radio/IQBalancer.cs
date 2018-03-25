using System;

namespace SDRSharp.Radio
{
	public class IQBalancer
	{
		private const int FFTBins = 128;

		private const int SkippedBuffers = 5;

		private const float DcTimeConst = 1E-05f;

		private const float MaximumStep = 0.01f;

		private const float MinimumStep = 1E-05f;

		private const float StepIncrement = 1.1f;

		private const float StepDecrement = 0.9090909f;

		private const float MaxPhaseCorrection = 0.2f;

		private const float PhaseAlpha = 0.01f;

		private const float GainAlpha = 0.01f;

		private bool _enabled;

		private float _phase;

		private float _lastPhase;

		private float _step = 1E-05f;

		private float _stepFactor = 0.9090909f;

		private double _gain;

		private double _iampavg;

		private double _qampavg;

		private unsafe readonly DcRemover* _dcRemoverI;

		private readonly UnsafeBuffer _dcRemoverIBuffer;

		private unsafe readonly DcRemover* _dcRemoverQ;

		private readonly UnsafeBuffer _dcRemoverQBuffer;

		private readonly bool _isMultithreaded;

		private readonly SharpEvent _event = new SharpEvent(false);

		private unsafe static readonly float* _windowPtr;

		private static readonly UnsafeBuffer _windowBuffer;

		public float Phase
		{
			get
			{
				return (float)Math.Asin((double)this._phase);
			}
		}

		public float Gain
		{
			get
			{
				return (float)this._gain;
			}
		}

		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
			set
			{
				this._enabled = value;
			}
		}

		public unsafe IQBalancer()
		{
			this._dcRemoverIBuffer = UnsafeBuffer.Create(sizeof(DcRemover));
			this._dcRemoverI = (DcRemover*)(void*)this._dcRemoverIBuffer;
			this._dcRemoverI->Init(1E-05f);
			this._dcRemoverQBuffer = UnsafeBuffer.Create(sizeof(DcRemover));
			this._dcRemoverQ = (DcRemover*)(void*)this._dcRemoverQBuffer;
			this._dcRemoverQ->Init(1E-05f);
			this._isMultithreaded = (Environment.ProcessorCount > 1);
		}

		unsafe static IQBalancer()
		{
			IQBalancer._windowBuffer = UnsafeBuffer.Create(FilterBuilder.MakeWindow(WindowType.BlackmanHarris7, 128));
			IQBalancer._windowPtr = (float*)(void*)IQBalancer._windowBuffer;
		}

		public unsafe void Reset()
		{
			this._phase = 0f;
			this._lastPhase = 0f;
			this._gain = 1.0;
			this._step = 1E-05f;
			this._stepFactor = 1.1f;
			this._iampavg = 1.0;
			this._qampavg = 1.0;
			this._dcRemoverI->Reset();
			this._dcRemoverQ->Reset();
		}

		public unsafe void Process(Complex* iq, int length)
		{
			if (this._enabled)
			{
				this.RemoveDC(iq, length);
				int num = 0;
				while (length >= 128)
				{
					if (num % 5 == 0)
					{
						this.EstimatePhaseImbalance(iq);
					}
					this.Adjust(iq, 128);
					iq += 128;
					length -= 128;
					num++;
				}
				this.Adjust(iq, length);
			}
		}

		private unsafe void RemoveDC(Complex* iq, int length)
		{
			float* buffer = (float*)((byte*)iq + 4);
			if (this._isMultithreaded)
			{
				DSPThreadPool.QueueUserWorkItem(delegate
				{
					this._dcRemoverI->ProcessInterleaved((float*)iq, length);
					this._event.Set();
				});
			}
			else
			{
				this._dcRemoverI->ProcessInterleaved((float*)iq, length);
			}
			this._dcRemoverQ->ProcessInterleaved(buffer, length);
			if (this._isMultithreaded)
			{
				this._event.WaitOne();
			}
		}

		private unsafe void EstimatePhaseImbalance(Complex* iq)
		{
			float num = this.Utility(iq, this._phase);
			float num2 = this._phase + this._step;
			if (num2 > 0.2f)
			{
				num2 = 0.2f;
			}
			if (num2 < -0.2f)
			{
				num2 = -0.2f;
			}
			if (this.Utility(iq, num2) > num)
			{
				this._phase += 0.01f * (num2 - this._phase);
			}
			else
			{
				if (Math.Abs(this._step) < 1E-05f)
				{
					this._stepFactor = -1.1f;
				}
				else if (Math.Abs(this._step) > 0.01f)
				{
					this._stepFactor = -0.9090909f;
				}
				this._step *= this._stepFactor;
			}
		}

		private unsafe float Utility(Complex* iq, float phase)
		{
			Complex* ptr = stackalloc Complex[128 * sizeof(Complex)];
			Utils.Memcpy(ptr, iq, 128 * sizeof(Complex));
			this.AdjustPhase(ptr, phase);
			Fourier.ApplyFFTWindow(ptr, IQBalancer._windowPtr, 128);
			Fourier.ForwardTransform(ptr, 128, false);
			float num = 0f;
			int num2 = 1;
			int num3 = 127;
			while (num2 < 64)
			{
				float num4 = Math.Abs(ptr[num2].Real) + Math.Abs(ptr[num2].Imag);
				float num5 = Math.Abs(ptr[num3].Real) + Math.Abs(ptr[num3].Imag);
				num += Math.Abs(num4 - num5);
				num2++;
				num3--;
			}
			return num;
		}

		private unsafe void AdjustPhase(Complex* iq, float phase)
		{
			float num = (float)this._gain;
			for (int i = 0; i < 128; i++)
			{
				iq[i].Real += phase * iq[i].Imag;
				iq[i].Imag *= num;
			}
		}

		private unsafe void Adjust(Complex* iq, int length)
		{
			float num = 1f / (float)(length - 1);
			for (int i = 0; i < length; i++)
			{
				float num2 = ((float)i * this._lastPhase + (float)(length - 1 - i) * this._phase) * num;
				iq[i].Real += num2 * iq[i].Imag;
				float num3 = iq[i].Real * iq[i].Real;
				float num4 = iq[i].Imag * iq[i].Imag;
				this._iampavg += 9.9999997473787516E-06 * ((double)num3 - this._iampavg);
				this._qampavg += 9.9999997473787516E-06 * ((double)num4 - this._qampavg);
				if (this._qampavg != 0.0)
				{
					double num5 = Math.Sqrt(this._iampavg / this._qampavg);
					this._gain += 0.0099999997764825821 * (num5 - this._gain);
				}
				iq[i].Imag *= (float)this._gain;
			}
			this._lastPhase = this._phase;
		}
	}
}
