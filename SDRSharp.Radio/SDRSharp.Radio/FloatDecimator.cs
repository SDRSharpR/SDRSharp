using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public sealed class FloatDecimator : IDisposable
	{
		private IntPtr _dec;

		private int _decimationRatio;

		public int DecimationRatio
		{
			get
			{
				return this._decimationRatio;
			}
		}

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr float_decimator_create(int decimation);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void float_decimator_destroy(IntPtr instance);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern int float_decimator_process(IntPtr instance, float* buffer, int length);

		public FloatDecimator(int decimationRatio)
		{
			this._decimationRatio = decimationRatio;
			this._dec = FloatDecimator.float_decimator_create(decimationRatio);
		}

		public void Dispose()
		{
			if (this._dec != IntPtr.Zero)
			{
				FloatDecimator.float_decimator_destroy(this._dec);
				this._dec = IntPtr.Zero;
			}
		}

		public unsafe int Process(float* buffer, int length)
		{
			return FloatDecimator.float_decimator_process(this._dec, buffer, length);
		}
	}
}
