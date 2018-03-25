namespace PortAudioSharp
{
	internal enum PaStreamCallbackFlags : uint
	{
		PaInputUnderflow = 1u,
		PaInputOverflow,
		PaOutputUnderflow = 4u,
		PaOutputOverflow = 8u,
		PaPrimingOutput = 0x10
	}
}
