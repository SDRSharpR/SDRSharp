using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public sealed class IQFirFilter : IDisposable
	{
		private int _decimationRatio;

		private int _length;

		private IntPtr _fir;

		public int Length
		{
			get
			{
				return this._length;
			}
		}

		public int DecimationRatio
		{
			get
			{
				return this._decimationRatio;
			}
		}

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern IntPtr complex_fir_create(float* kernel, int length, int decimation);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void complex_fir_destroy(IntPtr instance);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern int complex_fir_process(IntPtr instance, Complex* buffer, int length);

		public unsafe IQFirFilter(float[] coefficients, int decimationRatio = 1)
		{
			if (decimationRatio <= 0)
			{
				throw new ArgumentException("The decimation factor must be greater than zero", "decimationRatio");
			}
			this._decimationRatio = decimationRatio;
			this._length = coefficients.Length;
			fixed (float* kernel = coefficients)
			{
				this._fir = IQFirFilter.complex_fir_create(kernel, this._length, decimationRatio);
			}
		}

		public void Dispose()
		{
			if (this._fir != IntPtr.Zero)
			{
				IQFirFilter.complex_fir_destroy(this._fir);
				this._fir = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}

		public unsafe int Process(Complex* buffer, int length)
		{
			return IQFirFilter.complex_fir_process(this._fir, buffer, length);
		}
	}
}
