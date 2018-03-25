using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public class FrequencyTranslator : IDisposable
	{
		private IntPtr _nco;

		private double _sampleRate;

		private double _frequency;

		public double SampleRate
		{
			get
			{
				return this._sampleRate;
			}
		}

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
					FrequencyTranslator.nco_tune(this._nco, this._frequency);
				}
			}
		}

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr nco_create(double sample_rate);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void nco_destroy(IntPtr instance);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private static extern void nco_tune(IntPtr instance, double frequency);

		[DllImport("shark", CallingConvention = CallingConvention.Cdecl)]
		private unsafe static extern void nco_process(IntPtr instance, Complex* buffer, int length);

		public FrequencyTranslator(double sampleRate)
		{
			this._sampleRate = sampleRate;
			this._nco = FrequencyTranslator.nco_create(sampleRate);
		}

		public void Dispose()
		{
			if (this._nco != IntPtr.Zero)
			{
				FrequencyTranslator.nco_destroy(this._nco);
				this._nco = IntPtr.Zero;
			}
		}

		public unsafe void Process(Complex* buffer, int length)
		{
			FrequencyTranslator.nco_process(this._nco, buffer, length);
		}
	}
}
