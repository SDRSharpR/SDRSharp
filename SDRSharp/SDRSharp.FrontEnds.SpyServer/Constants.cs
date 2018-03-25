namespace SDRSharp.FrontEnds.SpyServer
{
	public static class Constants
	{
		public const uint SPYSERVER_PROTOCOL_VERSION = 33556024u;

		public const uint SPYSERVER_MAX_COMMAND_BODY_SIZE = 256u;

		public const uint SPYSERVER_MAX_MESSAGE_BODY_SIZE = 1048576u;

		public const uint SPYSERVER_MAX_DISPLAY_PIXELS = 32768u;

		public const uint SPYSERVER_MIN_DISPLAY_PIXELS = 100u;

		public const uint SPYSERVER_MAX_FFT_DB_RANGE = 150u;

		public const uint SPYSERVER_MIN_FFT_DB_RANGE = 10u;

		public const uint SPYSERVER_MAX_FFT_DB_OFFSET = 100u;

		public const uint SPYSERVER_DIGITAL_GAIN_AUTO = uint.MaxValue;

		public const int SPYSERVER_MESSAGE_TYPE_BITS = 16;

		public const uint SPYSERVER_MESSAGE_TYPE_MASK = 65535u;

		public static string GetDeviceName(DeviceType deviceID)
		{
			switch (deviceID)
			{
			case DeviceType.DEVICE_AIRSPY_ONE:
				return "Airspy One";
			case DeviceType.DEVICE_AIRSPY_HF:
				return "Airspy HF+";
			case DeviceType.DEVICE_RTLSDR:
				return "RTL-SDR";
			default:
				return "Unknown";
			}
		}
	}
}
