namespace SDRSharp.Radio.PortAudio
{
	public struct Int24
	{
		public byte C;

		public byte B;

		public sbyte A;

		public static implicit operator float(Int24 i)
		{
			return (float)((i.C << 8 | i.B << 16 | i.A << 24) >> 8);
		}
	}
}
