using System;
using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	internal static class PortAudioAPI
	{
		public const int PaFormatIsSupported = 0;

		public const int PaFramesPerBufferUnspecified = 0;

		private const string PortAudioLibrary = "portaudio";

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetVersion();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Pa_GetVersionText")]
		private static extern IntPtr IntPtr_Pa_GetVersionText();

		public static string Pa_GetVersionText()
		{
			return Marshal.PtrToStringAnsi(PortAudioAPI.IntPtr_Pa_GetVersionText());
		}

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Pa_GetErrorText")]
		public static extern IntPtr IntPtr_Pa_GetErrorText(PaError errorCode);

		public static string Pa_GetErrorText(PaError errorCode)
		{
			return Marshal.PtrToStringAnsi(PortAudioAPI.IntPtr_Pa_GetErrorText(errorCode));
		}

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_Initialize();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_Terminate();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetHostApiCount();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetDefaultHostApi();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Pa_GetHostApiInfo")]
		public static extern IntPtr IntPtr_Pa_GetHostApiInfo(int hostApi);

		public static PaHostApiInfo Pa_GetHostApiInfo(int hostApi)
		{
			return (PaHostApiInfo)Marshal.PtrToStructure(PortAudioAPI.IntPtr_Pa_GetHostApiInfo(hostApi), typeof(PaHostApiInfo));
		}

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_HostApiTypeIdToHostApiIndex(PaHostApiTypeId type);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_HostApiDeviceIndexToDeviceIndex(int hostApi, int hostApiDeviceIndex);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Pa_GetLastHostErrorInfo")]
		public static extern IntPtr IntPtr_Pa_GetLastHostErrorInfo();

		public static PaHostErrorInfo Pa_GetLastHostErrorInfo()
		{
			return (PaHostErrorInfo)Marshal.PtrToStructure(PortAudioAPI.IntPtr_Pa_GetLastHostErrorInfo(), typeof(PaHostErrorInfo));
		}

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetDeviceCount();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetDefaultInputDevice();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetDefaultOutputDevice();

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Pa_GetDeviceInfo")]
		public static extern IntPtr IntPtr_Pa_GetDeviceInfo(int device);

		public static PaDeviceInfo Pa_GetDeviceInfo(int device)
		{
			return (PaDeviceInfo)Marshal.PtrToStructure(PortAudioAPI.IntPtr_Pa_GetDeviceInfo(device), typeof(PaDeviceInfo));
		}

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_IsFormatSupported(ref PaStreamParameters inputParameters, ref PaStreamParameters outputParameters, double sampleRate);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_IsFormatSupported(IntPtr inputParameters, ref PaStreamParameters outputParameters, double sampleRate);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_IsFormatSupported(ref PaStreamParameters inputParameters, IntPtr outputParameters, double sampleRate);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_OpenStream(out IntPtr stream, ref PaStreamParameters inputParameters, ref PaStreamParameters outputParameters, double sampleRate, uint framesPerBuffer, PaStreamFlags streamFlags, PaStreamCallbackDelegate streamCallback, IntPtr userData);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_OpenStream(out IntPtr stream, IntPtr inputParameters, ref PaStreamParameters outputParameters, double sampleRate, uint framesPerBuffer, PaStreamFlags streamFlags, PaStreamCallbackDelegate streamCallback, IntPtr userData);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_OpenStream(out IntPtr stream, ref PaStreamParameters inputParameters, IntPtr outputParameters, double sampleRate, uint framesPerBuffer, PaStreamFlags streamFlags, PaStreamCallbackDelegate streamCallback, IntPtr userData);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_OpenDefaultStream(out IntPtr stream, int numInputChannels, int numOutputChannels, uint sampleFormat, double sampleRate, uint framesPerBuffer, PaStreamCallbackDelegate streamCallback, IntPtr userData);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_CloseStream(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_SetStreamFinishedCallback(ref IntPtr stream, [MarshalAs(UnmanagedType.FunctionPtr)] PaStreamFinishedCallbackDelegate streamFinishedCallback);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_StartStream(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_StopStream(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_AbortStream(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_IsStreamStopped(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_IsStreamActive(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Pa_GetStreamInfo")]
		public static extern IntPtr IntPtr_Pa_GetStreamInfo(IntPtr stream);

		public static PaStreamInfo Pa_GetStreamInfo(IntPtr stream)
		{
			return (PaStreamInfo)Marshal.PtrToStructure(PortAudioAPI.IntPtr_Pa_GetStreamInfo(stream), typeof(PaStreamInfo));
		}

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern double Pa_GetStreamTime(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern double Pa_GetStreamCpuLoad(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] float[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] byte[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] sbyte[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] ushort[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] short[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] uint[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, [Out] int[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_ReadStream(IntPtr stream, IntPtr buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] float[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] byte[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] sbyte[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] ushort[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] short[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] uint[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_WriteStream(IntPtr stream, [In] int[] buffer, uint frames);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetStreamReadAvailable(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern int Pa_GetStreamWriteAvailable(IntPtr stream);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern PaError Pa_GetSampleSize(PaSampleFormat format);

		[DllImport("portaudio", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Pa_Sleep(int msec);

		static PortAudioAPI()
		{
			PortAudioAPI.Pa_Initialize();
		}
	}
}
