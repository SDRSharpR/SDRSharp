namespace SDRSharp.FrontEnds.SpyServer
{
	public enum StreamingMode : uint
	{
		STREAM_MODE_IQ_ONLY = 1u,
		STREAM_MODE_AF_ONLY,
		STREAM_MODE_FFT_ONLY = 4u,
		STREAM_MODE_FFT_IQ,
		STREAM_MODE_FFT_AF
	}
}
