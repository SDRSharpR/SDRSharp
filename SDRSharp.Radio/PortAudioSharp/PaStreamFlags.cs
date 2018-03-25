namespace PortAudioSharp
{
	internal enum PaStreamFlags : uint
	{
		PaNoFlag,
		PaClipOff,
		PaDitherOff,
		PaNeverDropInput = 4u,
		PaPrimeOutputBuffersUsingStreamCallback = 8u,
		PaPlatformSpecificFlags = 4294901760u
	}
}
