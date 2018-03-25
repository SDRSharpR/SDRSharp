using System;

namespace SDRSharp.Radio
{
	public static class Trig
	{
		private const int ResolutionInBits = 16;

		private static readonly int _mask;

		private static readonly float _indexScale;

		private static readonly UnsafeBuffer _sinBuffer;

		private static readonly UnsafeBuffer _cosBuffer;

		private unsafe static readonly float* _sinPtr;

		private unsafe static readonly float* _cosPtr;

		unsafe static Trig()
		{
			Trig._mask = 65535;
			int num = Trig._mask + 1;
			Trig._sinBuffer = UnsafeBuffer.Create(num, 4);
			Trig._cosBuffer = UnsafeBuffer.Create(num, 4);
			Trig._sinPtr = (float*)(void*)Trig._sinBuffer;
			Trig._cosPtr = (float*)(void*)Trig._cosBuffer;
			Trig._indexScale = (float)num / 6.28318548f;
			for (int i = 0; i < num; i++)
			{
				Trig._sinPtr[i] = (float)Math.Sin((double)(((float)i + 0.5f) / (float)num * 6.28318548f));
				Trig._cosPtr[i] = (float)Math.Cos((double)(((float)i + 0.5f) / (float)num * 6.28318548f));
			}
			for (float num2 = 0f; num2 < 6.28318548f; num2 += 1.57079637f)
			{
				Trig._sinPtr[(int)(num2 * Trig._indexScale) & Trig._mask] = (float)Math.Sin((double)num2);
				Trig._cosPtr[(int)(num2 * Trig._indexScale) & Trig._mask] = (float)Math.Cos((double)num2);
			}
		}

		public unsafe static float Sin(float angle)
		{
			return Trig._sinPtr[(int)(angle * Trig._indexScale) & Trig._mask];
		}

		public unsafe static float Cos(float angle)
		{
			return Trig._cosPtr[(int)(angle * Trig._indexScale) & Trig._mask];
		}

		public unsafe static Complex SinCos(float rad)
		{
			int num = (int)(rad * Trig._indexScale) & Trig._mask;
			Complex result = default(Complex);
			result.Real = Trig._cosPtr[num];
			result.Imag = Trig._sinPtr[num];
			return result;
		}

		public static float Atan2(float y, float x)
		{
			if ((double)x == 0.0)
			{
				if ((double)y > 0.0)
				{
					return 1.57079637f;
				}
				if ((double)y == 0.0)
				{
					return 0f;
				}
				return -1.57079637f;
			}
			float num = y / x;
			float num2;
			if ((double)Math.Abs(num) < 1.0)
			{
				num2 = num / (1f + 0.2854f * num * num);
				if ((double)x < 0.0)
				{
					if ((double)y < 0.0)
					{
						return num2 - 3.14159274f;
					}
					return num2 + 3.14159274f;
				}
			}
			else
			{
				num2 = 1.57079637f - num / (num * num + 0.2854f);
				if ((double)y < 0.0)
				{
					return num2 - 3.14159274f;
				}
			}
			return num2;
		}
	}
}
