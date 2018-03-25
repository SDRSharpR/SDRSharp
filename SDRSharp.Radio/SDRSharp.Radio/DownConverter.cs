using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public sealed class DownConverter : IDisposable
	{
		private IntPtr _dec;

		private int _decimationRatio;

		private double _sampleRate;

		private double _frequency;

		public double Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				if (this._frequency != value)
				{
					this._frequency = value;
					DownConverter.ddc_tune(this._dec, this._frequency);
				}
			}
		}

		public int DecimationRatio
		{
			get
			{
				return this._decimationRatio;
			}
		}

		public double SampleRate
		{
			get
			{
				return this._sampleRate;
			}
		}

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr ddc_create(double sample_rate, int decimation);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ddc_destroy(IntPtr instance);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void ddc_tune(IntPtr instance, double frequency);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern int ddc_process(IntPtr instance, Complex* buffer, int length);

		public DownConverter(double sampleRate, int decimationRatio)
		{
			this._sampleRate = sampleRate;
			this._decimationRatio = decimationRatio;
			this._dec = DownConverter.ddc_create(this._sampleRate, this._decimationRatio);
		}

		public void Dispose()
		{
			if (this._dec != IntPtr.Zero)
			{
				DownConverter.ddc_destroy(this._dec);
				this._dec = IntPtr.Zero;
			}
		}

		public unsafe int Process(Complex* buffer, int length)
		{
			return DownConverter.ddc_process(this._dec, buffer, length);
		}
	}
}
