using System;

namespace SDRSharp.Radio
{
	public static class FilterBuilder
	{
		public const int DefaultFilterOrder = 500;

		public static float[] MakeWindow(WindowType windowType, int length)
		{
			float[] array = new float[length];
			length--;
			for (int i = 0; i <= length; i++)
			{
				array[i] = 1f;
				switch (windowType)
				{
				case WindowType.Hamming:
				{
					float num = 0.54f;
					float num2 = 0.46f;
					float num3 = 0f;
					float num4 = 0f;
					array[i] *= num - num2 * (float)Math.Cos(6.2831853071795862 * (double)i / (double)length) + num3 * (float)Math.Cos(12.566370614359172 * (double)i / (double)length) - num4 * (float)Math.Cos(18.849555921538759 * (double)i / (double)length);
					break;
				}
				case WindowType.Blackman:
				{
					float num = 0.42f;
					float num2 = 0.5f;
					float num3 = 0.08f;
					float num4 = 0f;
					array[i] *= num - num2 * (float)Math.Cos(6.2831853071795862 * (double)i / (double)length) + num3 * (float)Math.Cos(12.566370614359172 * (double)i / (double)length) - num4 * (float)Math.Cos(18.849555921538759 * (double)i / (double)length);
					break;
				}
				case WindowType.BlackmanHarris4:
				{
					float num = 0.35875f;
					float num2 = 0.48829f;
					float num3 = 0.14128f;
					float num4 = 0.01168f;
					array[i] *= num - num2 * (float)Math.Cos(6.2831853071795862 * (double)i / (double)length) + num3 * (float)Math.Cos(12.566370614359172 * (double)i / (double)length) - num4 * (float)Math.Cos(18.849555921538759 * (double)i / (double)length);
					break;
				}
				case WindowType.BlackmanHarris7:
				{
					float num = 0.2710514f;
					float num2 = 0.433297932f;
					float num3 = 0.218123f;
					float num4 = 0.06592545f;
					float num6 = 0.0108117424f;
					float num7 = 0.000776584842f;
					float num8 = 1.38872174E-05f;
					array[i] *= num - num2 * (float)Math.Cos(6.2831853071795862 * (double)i / (double)length) + num3 * (float)Math.Cos(12.566370614359172 * (double)i / (double)length) - num4 * (float)Math.Cos(18.849555921538759 * (double)i / (double)length) + num6 * (float)Math.Cos(25.132741228718345 * (double)i / (double)length) - num7 * (float)Math.Cos(31.415926535897931 * (double)i / (double)length) + num8 * (float)Math.Cos(37.699111843077517 * (double)i / (double)length);
					break;
				}
				case WindowType.HannPoisson:
				{
					float value = (float)i - (float)length / 2f;
					float num5 = 0.005f;
					array[i] *= 0.5f * (float)((1.0 + Math.Cos(6.2831853071795862 * (double)value / (double)length)) * Math.Exp(-2.0 * (double)num5 * (double)Math.Abs(value) / (double)length));
					break;
				}
				case WindowType.Youssef:
				{
					float num = 0.35875f;
					float num2 = 0.48829f;
					float num3 = 0.14128f;
					float num4 = 0.01168f;
					float value = (float)i - (float)length / 2f;
					float num5 = 0.005f;
					array[i] *= num - num2 * (float)Math.Cos(6.2831853071795862 * (double)i / (double)length) + num3 * (float)Math.Cos(12.566370614359172 * (double)i / (double)length) - num4 * (float)Math.Cos(18.849555921538759 * (double)i / (double)length);
					array[i] *= (float)Math.Exp(-2.0 * (double)num5 * (double)Math.Abs(value) / (double)length);
					break;
				}
				}
			}
			return array;
		}

		public static float[] MakeSinc(double sampleRate, double frequency, int length)
		{
			if (length % 2 == 0)
			{
				throw new ArgumentException("Length should be odd", "length");
			}
			double num = 6.2831853071795862 * frequency / sampleRate;
			float[] array = new float[length];
			for (int i = 0; i < length; i++)
			{
				int num2 = i - length / 2;
				if (num2 == 0)
				{
					array[i] = (float)num;
				}
				else
				{
					array[i] = (float)(Math.Sin(num * (double)num2) / (double)num2);
				}
			}
			return array;
		}

		public static float[] MakeSin(double sampleRate, double frequency, int length)
		{
			if (length % 2 == 0)
			{
				throw new ArgumentException("Length should be odd", "length");
			}
			double num = 6.2831853071795862 * frequency / sampleRate;
			float[] array = new float[length];
			int num2 = length / 2;
			for (int i = 0; i <= num2; i++)
			{
				array[num2 - i] = 0f - (array[num2 + i] = (float)Math.Sin(num * (double)i));
			}
			return array;
		}

		public static float[] MakeLowPassKernel(double sampleRate, int filterOrder, double cutoffFrequency, WindowType windowType)
		{
			filterOrder |= 1;
			float[] array = FilterBuilder.MakeSinc(sampleRate, cutoffFrequency, filterOrder);
			float[] window = FilterBuilder.MakeWindow(windowType, filterOrder);
			FilterBuilder.ApplyWindow(array, window);
			FilterBuilder.Normalize(array);
			return array;
		}

		public static float[] MakeHighPassKernel(double sampleRate, int filterOrder, double cutoffFrequency, WindowType windowType)
		{
			return FilterBuilder.InvertSpectrum(FilterBuilder.MakeLowPassKernel(sampleRate, filterOrder, cutoffFrequency, windowType));
		}

		public static float[] MakeBandPassKernel(double sampleRate, int filterOrder, double cutoff1, double cutoff2, WindowType windowType)
		{
			double num = (cutoff2 - cutoff1) / 2.0;
			double num2 = cutoff2 - num;
			double num3 = 6.2831853071795862 * num2 / sampleRate;
			float[] array = FilterBuilder.MakeLowPassKernel(sampleRate, filterOrder, num, windowType);
			for (int i = 0; i < array.Length; i++)
			{
				int num4 = i - array.Length / 2;
				array[i] *= (float)(2.0 * Math.Cos(num3 * (double)num4));
			}
			return array;
		}

		public static Complex[] MakeComplexKernel(double sampleRate, int filterOrder, double bandwidth, double offset, WindowType windowType)
		{
			double num = -6.2831853071795862 * offset / sampleRate;
			float[] array = FilterBuilder.MakeLowPassKernel(sampleRate, filterOrder, bandwidth * 0.5, windowType);
			Complex[] array2 = new Complex[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				int num2 = i - array2.Length / 2;
				double num3 = num * (double)num2;
				array2[i].Real = (float)((double)array[i] * Math.Cos(num3));
				array2[i].Imag = (float)((double)(0f - array[i]) * Math.Sin(num3));
			}
			return array2;
		}

		public static Complex[] MakeComplexKernel(double sampleRate, double passband, double transition, double ripple, double attenuation, double offset)
		{
			double num = -6.2831853071795862 * offset / sampleRate;
			passband *= 0.5;
			float[] array = FilterBuilder.MakeLowPassKernel(sampleRate, passband, passband + transition, ripple, attenuation);
			if (array == null)
			{
				return null;
			}
			Complex[] array2 = new Complex[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				int num2 = i - array2.Length / 2;
				double num3 = num * (double)num2;
				array2[i].Real = (float)((double)array[i] * Math.Cos(num3));
				array2[i].Imag = (float)((double)(0f - array[i]) * Math.Sin(num3));
			}
			return array2;
		}

		private static double ACosh(double x)
		{
			return Math.Log(x + Math.Sqrt(x * x - 1.0));
		}

		private static double ChebychevPoly(int n, double x)
		{
			if (Math.Abs(x) <= 1.0)
			{
				return Math.Cos((double)n * Math.Acos(x));
			}
			return Math.Cosh((double)n * FilterBuilder.ACosh(x));
		}

		private static float[] MakeChebychevWindow(int length, double attenuation)
		{
			double num = Math.Pow(10.0, attenuation / 20.0);
			float[] array = new float[length];
			float num2 = 0f;
			double num3 = Math.Cosh(FilterBuilder.ACosh(num) / (double)(length - 1));
			double num4 = (double)((length - 1) / 2);
			if (length % 2 == 0)
			{
				num4 += 0.5;
			}
			for (int i = 0; i < length / 2 + 1; i++)
			{
				double num5 = (double)i - num4;
				double num6 = 0.0;
				for (int j = 1; (double)j <= num4; j++)
				{
					num6 += FilterBuilder.ChebychevPoly(length - 1, num3 * Math.Cos(3.1415926535897931 * (double)j / (double)length)) * Math.Cos(2.0 * num5 * 3.1415926535897931 * (double)j / (double)length);
				}
				array[i] = (float)(num + 2.0 * num6);
				array[length - i - 1] = array[i];
				if (array[i] > num2)
				{
					num2 = array[i];
				}
			}
			float num7 = 1f / num2;
			for (int k = 0; k < length; k++)
			{
				array[k] *= num7;
			}
			return array;
		}

		private static float[] MakeChebychevLowPass(int filterOrder, double normalizedCutoff, double attenuation)
		{
			float[] array = FilterBuilder.MakeChebychevWindow(filterOrder, attenuation);
			float[] array2 = FilterBuilder.MakeSinc(2.0, normalizedCutoff, array.Length);
			FilterBuilder.ApplyWindow(array2, array);
			FilterBuilder.Normalize(array2);
			return array2;
		}

		public static float[] MakeLowPassKernel(double sampleRate, double passband, double stopband, double ripple, double attenuation)
		{
			double num = 2.0 * passband / sampleRate;
			double num2 = 2.0 * stopband / sampleRate;
			int num3 = (int)((double)FilterBuilder.EstimateOrder(num, num2, ripple, attenuation) * 1.4) | 1;
			int num4 = Math.Max(1, num3 - 20);
			int num5 = num4 + 40;
			for (int i = num4; i < num5; i += 2)
			{
				int num6 = 50;
				double num7 = 0.0;
				for (int j = 0; j < 100; j++)
				{
					double normalizedCutoff = (num * (double)num6 + num2 * (double)(100 - num6)) / 100.0;
					float[] array = FilterBuilder.MakeChebychevLowPass(i, normalizedCutoff, attenuation + num7);
					double num8;
					double num9;
					FilterBuilder.GetFilterSpecs(array, num, num2, out num8, out num9);
					if (num8 <= ripple && num9 >= attenuation)
					{
						return array;
					}
					num6 += Math.Sign(ripple - num8);
					num7 += (double)Math.Sign(attenuation - num9) * 0.15 - (double)Math.Sign(ripple - num8) * 2.5;
					if (num7 < -20.0)
					{
						num7 = -20.0;
					}
					if (num7 > 20.0)
					{
						num7 = 20.0;
					}
					if (num6 < 0)
					{
						num6 = 0;
					}
					if (num6 > 100)
					{
						num6 = 100;
					}
				}
			}
			return null;
		}

		public static int EstimateOrder(double passband, double stopband, double ripple, double attenuation)
		{
			return (int)Math.Round(0.0010765282977552 * (52.0490855934073 * attenuation + 194.466145732802 / Math.Log10(1.0 + ripple)) / (stopband - passband) + 1.74881176954081);
		}

		public static double GetFilterError(float[] kernel, double passband, double stopband, double ripple, double attenuation)
		{
			double num;
			double num2;
			FilterBuilder.GetFilterSpecs(kernel, passband, stopband, out num, out num2);
			return ((num2 >= attenuation) ? 0.0 : (attenuation - num2)) + 100.0 * ((num <= ripple) ? 0.0 : (num - ripple));
		}

		public unsafe static void GetFilterSpecs(float[] kernel, double passband, double stopband, out double ripple, out double attenuation)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 2;
			while (true)
			{
				if (num3 >= kernel.Length * 2 && num >= 4)
				{
					break;
				}
				num = (int)Math.Round((double)num3 * passband);
				num2 = (int)Math.Ceiling((double)num3 * stopband);
				num3 *= 2;
			}
			Complex[] obj = new Complex[num3];
			float[] array = new float[num3 / 2];
			Complex[] array2 = obj;
			fixed (Complex* ptr = array2)
			{
				float[] array3 = array;
				fixed (float* ptr2 = array3)
				{
					for (int i = 0; i < kernel.Length; i++)
					{
						ptr[i] = kernel[i];
					}
					Fourier.ForwardTransform(ptr, num3, false);
					Fourier.SpectrumPower(ptr, ptr2, array.Length, 0f);
					float num4 = float.PositiveInfinity;
					float num5 = float.NegativeInfinity;
					for (int j = 0; j <= num; j++)
					{
						if (num4 > ptr2[j])
						{
							num4 = ptr2[j];
						}
						if (num5 < ptr2[j])
						{
							num5 = ptr2[j];
						}
					}
					ripple = (double)(num5 - num4);
					float num6 = float.NegativeInfinity;
					for (int k = num2; k < array.Length; k++)
					{
						if (num6 < ptr2[k])
						{
							num6 = ptr2[k];
						}
					}
					attenuation = Math.Max(0.0, (double)(num4 + num5) * 0.5 - (double)num6);
				}
			}
		}

		public static void Normalize(float[] h)
		{
			double num = 0.0;
			for (int i = 0; i < h.Length; i++)
			{
				num += (double)h[i];
			}
			double num2 = 1.0 / num;
			for (int j = 0; j < h.Length; j++)
			{
				h[j] = (float)((double)h[j] * num2);
			}
		}

		public static void ApplyWindow(float[] coefficients, float[] window)
		{
			for (int i = 0; i < coefficients.Length; i++)
			{
				coefficients[i] *= window[i];
			}
		}

		private static float[] InvertSpectrum(float[] h)
		{
			for (int i = 0; i < h.Length; i++)
			{
				h[i] = 0f - h[i];
			}
			h[(h.Length - 1) / 2] += 1f;
			return h;
		}
	}
}
