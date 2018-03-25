using System;

namespace SDRSharp.Radio
{
	public class Resampler
	{
		public const double DefaultProtectedPassband = 0.45;

		public const int DefaultTapsPerPhase = 160;

		private int _phase;

		private readonly int _interpolationFactor;

		private readonly int _decimationFactor;

		private readonly int _tapsPerPhase;

		private readonly UnsafeBuffer _firKernelBuffer;

		private unsafe readonly float* _firKernel;

		private readonly UnsafeBuffer _firQueueBuffer;

		private unsafe readonly float* _firQueue;

		public unsafe Resampler(double inputSampleRate, double outputSampleRate, int tapsPerPhase = 160, double protectedPassband = 0.45)
		{
			Resampler.DoubleToFraction(outputSampleRate / inputSampleRate, out this._interpolationFactor, out this._decimationFactor);
			this._tapsPerPhase = tapsPerPhase;
			int num = tapsPerPhase * this._interpolationFactor;
			this._firKernelBuffer = UnsafeBuffer.Create(num, 4);
			this._firKernel = (float*)(void*)this._firKernelBuffer;
			double cutoffFrequency = Math.Min(inputSampleRate, outputSampleRate) * protectedPassband;
			float[] array = FilterBuilder.MakeLowPassKernel(inputSampleRate * (double)this._interpolationFactor, num - 1, cutoffFrequency, WindowType.BlackmanHarris4);
			float[] array2 = array;
			fixed (float* ptr = array2)
			{
				for (int i = 0; i < array.Length; i++)
				{
					ptr[i] *= (float)this._interpolationFactor;
				}
				Utils.Memcpy(this._firKernel, ptr, (num - 1) * 4);
				this._firKernel[num - 1] = 0f;
			}
			this._firQueueBuffer = UnsafeBuffer.Create(num, 4);
			this._firQueue = (float*)(void*)this._firQueueBuffer;
		}

		private static void DoubleToFraction(double value, out int num, out int den)
		{
			int num2 = 1;
			int num3 = 1;
			double num4 = 1.0;
			while (Math.Abs(num4 - value) > 1E-15)
			{
				if (num4 > value)
				{
					num3++;
				}
				else
				{
					num2++;
				}
				num4 = (double)num2 / (double)num3;
			}
			num = num2;
			den = num3;
		}

		public unsafe int Process(float* input, float* output, int inputLength)
		{
			int num = 0;
			while (inputLength > 0)
			{
				int num2 = 0;
				while (this._phase >= this._interpolationFactor)
				{
					this._phase -= this._interpolationFactor;
					num2++;
					if (--inputLength == 0)
					{
						break;
					}
				}
				if (num2 >= this._tapsPerPhase)
				{
					input += num2 - this._tapsPerPhase;
					num2 = this._tapsPerPhase;
				}
				for (int num3 = this._tapsPerPhase - 1; num3 >= num2; num3--)
				{
					this._firQueue[num3] = this._firQueue[num3 - num2];
				}
				for (int num4 = num2 - 1; num4 >= 0; num4--)
				{
					float* intPtr = this._firQueue + num4;
					float* intPtr2 = input;
					input = intPtr2 + 1;
					*intPtr = *intPtr2;
				}
				while (this._phase < this._interpolationFactor)
				{
					float* ptr = this._firKernel + this._phase;
					float num5 = 0f;
					for (int i = 0; i < this._tapsPerPhase; i++)
					{
						num5 += *ptr * this._firQueue[i];
						ptr += this._interpolationFactor;
					}
					float* intPtr3 = output;
					output = intPtr3 + 1;
					*intPtr3 = num5;
					num++;
					this._phase += this._decimationFactor;
				}
			}
			return num;
		}

		public unsafe int ProcessInterleaved(float* input, float* output, int inputLength)
		{
			int num = 0;
			while (inputLength > 0)
			{
				int num2 = 0;
				while (this._phase >= this._interpolationFactor)
				{
					this._phase -= this._interpolationFactor;
					num2++;
					if (--inputLength == 0)
					{
						break;
					}
				}
				if (num2 >= this._tapsPerPhase)
				{
					input += (num2 - this._tapsPerPhase) * 2;
					num2 = this._tapsPerPhase;
				}
				for (int num3 = this._tapsPerPhase - 1; num3 >= num2; num3--)
				{
					this._firQueue[num3] = this._firQueue[num3 - num2];
				}
				for (int num4 = num2 - 1; num4 >= 0; num4--)
				{
					this._firQueue[num4] = *input;
					input += 2;
				}
				while (this._phase < this._interpolationFactor)
				{
					float* ptr = this._firKernel + this._phase;
					float num5 = 0f;
					for (int i = 0; i < this._tapsPerPhase; i++)
					{
						num5 += *ptr * this._firQueue[i];
						ptr += this._interpolationFactor;
					}
					*output = num5;
					output += 2;
					num++;
					this._phase += this._decimationFactor;
				}
			}
			return num;
		}
	}
}
