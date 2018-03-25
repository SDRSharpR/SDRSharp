namespace PortAudioSharp
{
	internal enum PaError
	{
		paNoError,
		paNotInitialized = -10000,
		paUnanticipatedHostError,
		paInvalidChannelCount,
		paInvalidSampleRate,
		paInvalidDevice,
		paInvalidFlag,
		paSampleFormatNotSupported,
		paBadIODeviceCombination,
		paInsufficientMemory,
		paBufferTooBig,
		paBufferTooSmall,
		paNullCallback,
		paBadStreamPtr,
		paTimedOut,
		paInternalError,
		paDeviceUnavailable,
		paIncompatibleHostApiSpecificStreamInfo,
		paStreamIsStopped,
		paStreamIsNotStopped,
		paInputOverflowed,
		paOutputUnderflowed,
		paHostApiNotFound,
		paInvalidHostApi,
		paCanNotReadFromACallbackStream,
		paCanNotWriteToACallbackStream,
		paCanNotReadFromAnOutputOnlyStream,
		paCanNotWriteToAnInputOnlyStream,
		paIncompatibleStreamHostApi,
		paBadBufferPtr
	}
}
