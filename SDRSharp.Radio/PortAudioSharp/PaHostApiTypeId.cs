namespace PortAudioSharp
{
	internal enum PaHostApiTypeId : uint
	{
		paInDevelopment,
		paDirectSound,
		paMME,
		paASIO,
		paSoundManager,
		paCoreAudio,
		paOSS = 7u,
		paALSA,
		paAL,
		paBeOS,
		paWDMKS,
		paJACK,
		paWASAPI,
		paAudioScienceHPI
	}
}
