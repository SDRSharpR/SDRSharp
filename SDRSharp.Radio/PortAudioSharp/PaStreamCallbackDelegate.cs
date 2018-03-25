using System;
using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate PaStreamCallbackResult PaStreamCallbackDelegate(float* input, float* output, uint frameCount, ref PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData);
}
