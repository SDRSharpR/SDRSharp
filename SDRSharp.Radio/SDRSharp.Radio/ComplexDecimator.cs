using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public sealed class ComplexDecimator : IDisposable
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
		private static extern IntPtr complex_decimator_create(int decimation);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void complex_decimator_destroy(IntPtr instance);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern int complex_decimator_process(IntPtr instance, Complex* buffer, int length);

		public ComplexDecimator(int decimationRatio)
		{
			this._decimationRatio = decimationRatio;
			this._dec = ComplexDecimator.complex_decimator_create(decimationRatio);
		}

		public void Dispose()
		{
			if (this._dec != IntPtr.Zero)
			{
				ComplexDecimator.complex_decimator_destroy(this._dec);
				this._dec = IntPtr.Zero;
			}
		}

		public unsafe int Process(Complex* buffer, int length)
		{
			return ComplexDecimator.complex_decimator_process(this._dec, buffer, length);
		}
	}
}
