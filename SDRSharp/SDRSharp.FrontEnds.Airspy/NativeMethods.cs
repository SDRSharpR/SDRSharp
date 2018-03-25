using System;
using System.Runtime.InteropServices;

namespace SDRSharp.FrontEnds.Airspy
{
	public static class NativeMethods
	{
		private const string LibAirSpy = "airspy";

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_init();

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_exit();

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_open(out IntPtr dev);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_close(IntPtr dev);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_samplerate(IntPtr dev, uint samplerate);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspy_error airspy_set_conversion_filter_float32(IntPtr dev, float* kernel, int len);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspy_error airspy_set_conversion_filter_int16(IntPtr dev, short* kernel, int len);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspy_error airspy_get_samplerates(IntPtr dev, uint* buffer, uint len);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_start_rx(IntPtr dev, airspy_sample_block_cb_fn cb, IntPtr rx_ctx);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_stop_rx(IntPtr dev);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_is_streaming(IntPtr dev);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl, EntryPoint = "airspy_board_id_name")]
		private static extern IntPtr airspy_board_id_name_native(uint index);

		public static string airspy_board_id_name(uint index)
		{
			try
			{
				IntPtr ptr = NativeMethods.airspy_board_id_name_native(index);
				return Marshal.PtrToStringAnsi(ptr);
			}
			catch (EntryPointNotFoundException ex)
			{
				Console.WriteLine("{0}:\n   {1}", ex.GetType().Name, ex.Message);
				return "AirSpy";
			}
		}

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_sample_type(IntPtr dev, airspy_sample_type sample_type);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_freq(IntPtr dev, uint freq_hz);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_packing(IntPtr dev, [MarshalAs(UnmanagedType.U1)] bool value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_lna_gain(IntPtr dev, byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_mixer_gain(IntPtr dev, byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_vga_gain(IntPtr dev, byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_lna_agc(IntPtr dev, [MarshalAs(UnmanagedType.U1)] bool value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_mixer_agc(IntPtr dev, [MarshalAs(UnmanagedType.U1)] bool value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_linearity_gain(IntPtr dev, byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_sensitivity_gain(IntPtr dev, byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_r820t_write(IntPtr device, byte register_number, byte value);

		public static airspy_error airspy_r820t_write_mask(IntPtr device, byte reg, byte value, byte mask)
		{
			byte b;
			airspy_error airspy_error = NativeMethods.airspy_r820t_read(device, reg, out b);
			if (airspy_error < airspy_error.AIRSPY_SUCCESS)
			{
				return airspy_error;
			}
			value = (byte)((b & ~mask) | (value & mask));
			return NativeMethods.airspy_r820t_write(device, reg, value);
		}

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_r820t_read(IntPtr device, byte register_number, out byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_si5351c_write(IntPtr device, byte register_number, byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_si5351c_read(IntPtr device, byte register_number, out byte value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_set_rf_bias(IntPtr dev, [In] [MarshalAs(UnmanagedType.U1)] bool value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_gpio_read(IntPtr device, airspy_gpio_port_t port, airspy_gpio_pin_t pin, [MarshalAs(UnmanagedType.U1)] out bool value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_gpio_write(IntPtr device, airspy_gpio_port_t port, airspy_gpio_pin_t pin, [MarshalAs(UnmanagedType.U1)] bool value);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_spiflash_erase(IntPtr device);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspy_error airspy_spiflash_write(IntPtr device, uint address, ushort length, byte* data);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern airspy_error airspy_spiflash_read(IntPtr device, uint address, ushort length, byte* data);

		[DllImport("airspy", CallingConvention = CallingConvention.Cdecl)]
		public static extern airspy_error airspy_spiflash_erase_sector(IntPtr device, ushort sector_num);
	}
}
