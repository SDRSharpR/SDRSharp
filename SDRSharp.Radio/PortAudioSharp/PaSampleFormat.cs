namespace PortAudioSharp
{
	internal enum PaSampleFormat : uint
	{
		PaFloat32 = 1u,
		PaInt32,
		PaInt24 = 4u,
		PaInt16 = 8u,
		PaInt8 = 0x10,
		PaUInt8 = 0x20,
		PaCustomFormat = 0x10000,
		PaNonInterleaved = 0x80000000
	}
}
