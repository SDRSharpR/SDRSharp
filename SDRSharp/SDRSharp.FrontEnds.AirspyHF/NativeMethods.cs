using System;
using System.Runtime.InteropServices;

namespace SDRSharp.FrontEnds.AirspyHF
{
	public static class NativeMethods
	{
		private const string LibAirspyHF = "airspyhf";

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_open(out IntPtr dev);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_close(IntPtr dev);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_start(IntPtr dev, airspyhf_sample_cb cb, IntPtr ctx);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_stop(IntPtr dev);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool airspyhf_is_streaming(IntPtr dev);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_set_freq(IntPtr dev, uint freq_hz);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_i2c_write(IntPtr device, byte register_number, byte value);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_i2c_read(IntPtr device, byte register_number, out byte value);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_tuner_write(IntPtr device, uint address, uint value);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspyhf_error airspyhf_tuner_read(IntPtr device, uint address, byte* data);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspyhf_error airspyhf_get_samplerates(IntPtr device, uint* buffer, uint len);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_set_samplerate(IntPtr device, uint samplerate);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_get_calibration(IntPtr device, out int ppb);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_set_calibration(IntPtr device, int ppb);

		[DllImport("airspyhf", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspyhf_error airspyhf_flash_calibration(IntPtr dev);
	}
}
