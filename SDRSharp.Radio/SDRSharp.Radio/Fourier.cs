using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public static class Fourier
	{
		private const int MaxLutBits = 16;

		private const int MaxLutBins = 65536;

		private const int LutSize = 32768;

		private const double TwoPi = 6.2831853071795862;

		private static UnsafeBuffer _lutBuffer;

		private unsafe static Complex* _lut;

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern void FourierForwardTransformLut(Complex* buffer, int length, Complex* lut, int lutbits);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern void FourierForwardTransformSinCos(Complex* buffer, int length);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern void FourierForwardTransformRotator(Complex* buffer, int length);

		unsafe static Fourier()
		{
			Fourier._lutBuffer = UnsafeBuffer.Create(32768, sizeof(Complex));
			Fourier._lut = (Complex*)(void*)Fourier._lutBuffer;
			for (int i = 0; i < 32768; i++)
			{
				Fourier._lut[i] = Complex.FromAngle(9.5873799242852573E-05 * (double)i).Conjugate();
			}
		}

		public static float DecibelToRatio(float db)
		{
			return (float)Math.Pow(10.0, (double)db * 0.1);
		}

		public unsafe static void SpectrumPower(Complex* buffer, float* power, int length, float offset = 0f)
		{
			for (int i = 0; i < length; i++)
			{
				float num = buffer[i].ModulusSquared();
				float num2 = power[i] = (float)(10.0 * Math.Log10(1E-60 + (double)num)) + offset;
			}
		}

		public unsafe static void ScaleFFT(float* src, byte* dest, int length, float minPower, float maxPower)
		{
			float num = 255f / (maxPower - minPower);
			for (int i = 0; i < length; i++)
			{
				float num2 = src[i];
				if (num2 < minPower)
				{
					num2 = minPower;
				}
				else if (num2 > maxPower)
				{
					num2 = maxPower;
				}
				dest[i] = (byte)((num2 - minPower) * num);
			}
		}

		public unsafe static void SmoothMaxCopy(float* srcPtr, float* dstPtr, int sourceLength, int destinationLength, float zoom = 1f, float offset = 0f)
		{
			if (zoom < 1f)
			{
				zoom = 1f;
			}
			float num = (float)sourceLength / (zoom * (float)destinationLength);
			float num2 = (float)sourceLength * (offset + 0.5f * (1f - 1f / zoom));
			if (num > 1f)
			{
				int num3 = (int)Math.Ceiling((double)num * 0.5);
				int num4 = -1;
				for (int i = 0; i < destinationLength; i++)
				{
					float num5 = -600f;
					for (int j = -num3; j <= num3; j++)
					{
						int num6 = (int)Math.Round((double)(num2 + num * (float)i + (float)j));
						if (num6 > num4 && num6 >= 0 && num6 < sourceLength && num5 < srcPtr[num6])
						{
							num5 = srcPtr[num6];
						}
						num4 = num6;
					}
					dstPtr[i] = num5;
				}
			}
			else
			{
				int num7 = (int)Math.Ceiling((double)(1f / num));
				float num8 = 1f / (float)num7;
				int num9 = 0;
				int num10 = (int)num2;
				int num11 = num10 + 1;
				int num12 = sourceLength - 1;
				for (int k = 0; k < destinationLength; k++)
				{
					int num13 = (int)(num2 + (float)k * num);
					if (num13 > num10)
					{
						num9 = 0;
						if (num13 >= num12)
						{
							num10 = num12;
							num11 = num12;
						}
						else
						{
							num10 = num13;
							num11 = num13 + 1;
						}
					}
					dstPtr[k] = (srcPtr[num10] * (float)(num7 - num9) + srcPtr[num11] * (float)num9) * num8;
					num9++;
				}
			}
		}

		public unsafe static void SmoothMinCopy(float* srcPtr, float* dstPtr, int sourceLength, int destinationLength, float zoom = 1f, float offset = 0f)
		{
			if (zoom < 1f)
			{
				zoom = 1f;
			}
			float num = (float)sourceLength / (zoom * (float)destinationLength);
			float num2 = (float)sourceLength * (offset + 0.5f * (1f - 1f / zoom));
			if (num > 1f)
			{
				int num3 = (int)Math.Ceiling((double)num * 0.5);
				int num4 = -1;
				for (int i = 0; i < destinationLength; i++)
				{
					float num5 = 600f;
					for (int j = -num3; j <= num3; j++)
					{
						int num6 = (int)Math.Round((double)(num2 + num * (float)i + (float)j));
						if (num6 > num4 && num6 >= 0 && num6 < sourceLength && num5 > srcPtr[num6])
						{
							num5 = srcPtr[num6];
						}
						num4 = num6;
					}
					dstPtr[i] = num5;
				}
			}
			else
			{
				for (int k = 0; k < destinationLength; k++)
				{
					int num7 = (int)(num * (float)k + num2);
					if (num7 >= 0 && num7 < sourceLength)
					{
						dstPtr[k] = srcPtr[num7];
					}
				}
			}
		}

		public unsafe static void ApplyFFTWindow(Complex* buffer, float* window, int length)
		{
			for (int i = 0; i < length; i++)
			{
				buffer[i].Real *= window[i];
				buffer[i].Imag *= window[i];
			}
		}

		public unsafe static void ForwardTransform(Complex* buffer, int length, bool rearrange = true)
		{
			if (length <= 128)
			{
				Fourier.FourierForwardTransformRotator(buffer, length);
			}
			else if (length <= 65536)
			{
				Fourier.FourierForwardTransformLut(buffer, length, Fourier._lut, 16);
			}
			else if (rearrange)
			{
				Fourier.FourierForwardTransformRotator(buffer, length);
			}
			else
			{
				Fourier.FourierForwardTransformSinCos(buffer, length);
			}
			if (rearrange)
			{
				int num = length / 2;
				for (int i = 0; i < num; i++)
				{
					int num2 = num + i;
					Complex complex = buffer[i];
					buffer[i] = buffer[num2];
					buffer[num2] = complex;
				}
			}
		}

		public unsafe static void InverseTransform(Complex* samples, int length)
		{
			for (int i = 0; i < length; i++)
			{
				samples[i].Imag = 0f - samples[i].Imag;
			}
			Fourier.ForwardTransform(samples, length, false);
			float num = 1f / (float)length;
			for (int j = 0; j < length; j++)
			{
				samples[j].Real *= num;
				samples[j].Imag = (0f - samples[j].Imag) * num;
			}
		}
	}
}
