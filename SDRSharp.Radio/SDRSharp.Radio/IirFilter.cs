using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	[StructLayout(LayoutKind.Sequential, Pack = 16)]
	public struct IirFilter
	{
		private float _a0;

		private float _a1;

		private float _a2;

		private float _b0;

		private float _b1;

		private float _b2;

		private float _x1;

		private float _x2;

		private float _y1;

		private float _y2;

		public void Init(IirFilterType filterType, double frequency, double sampleRate, double qualityFactor)
		{
			double num = 6.2831853071795862 * frequency / sampleRate;
			double num2 = Math.Sin(num) / (2.0 * qualityFactor);
			switch (filterType)
			{
			case IirFilterType.LowPass:
				this._b0 = (float)((1.0 - Math.Cos(num)) / 2.0);
				this._b1 = (float)(1.0 - Math.Cos(num));
				this._b2 = (float)((1.0 - Math.Cos(num)) / 2.0);
				this._a0 = (float)(1.0 + num2);
				this._a1 = (float)(-2.0 * Math.Cos(num));
				this._a2 = (float)(1.0 - num2);
				break;
			case IirFilterType.HighPass:
				this._b0 = (float)((1.0 + Math.Cos(num)) / 2.0);
				this._b1 = (float)(0.0 - (1.0 + Math.Cos(num)));
				this._b2 = (float)((1.0 + Math.Cos(num)) / 2.0);
				this._a0 = (float)(1.0 + num2);
				this._a1 = (float)(-2.0 * Math.Cos(num));
				this._a2 = (float)(1.0 - num2);
				break;
			default:
				this._b0 = (float)num2;
				this._b1 = 0f;
				this._b2 = (float)(0.0 - num2);
				this._a0 = (float)(1.0 + num2);
				this._a1 = (float)(-2.0 * Math.Cos(num));
				this._a2 = (float)(1.0 - num2);
				break;
			case IirFilterType.Notch:
				this._b0 = 1f;
				this._b1 = (float)(-2.0 * Math.Cos(num));
				this._b2 = 1f;
				this._a0 = (float)(1.0 + num2);
				this._a1 = (float)(-2.0 * Math.Cos(num));
				this._a2 = (float)(1.0 - num2);
				break;
			}
			this._b0 /= this._a0;
			this._b1 /= this._a0;
			this._b2 /= this._a0;
			this._a1 /= this._a0;
			this._a2 /= this._a0;
			this._x1 = 0f;
			this._x2 = 0f;
			this._y1 = 0f;
			this._y2 = 0f;
		}

		public void Reset()
		{
			this._x1 = 0f;
			this._x2 = 0f;
			this._y1 = 0f;
			this._y2 = 0f;
		}

		public float Process(float sample)
		{
			float num = this._b0 * sample + this._b1 * this._x1 + this._b2 * this._x2 - this._a1 * this._y1 - this._a2 * this._y2;
			this._x2 = this._x1;
			this._x1 = sample;
			this._y2 = this._y1;
			this._y1 = num;
			return num;
		}

		public unsafe void Process(float* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				buffer[i] = this.Process(buffer[i]);
			}
		}
	}
}
