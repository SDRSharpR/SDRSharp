using System;
using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void PaStreamFinishedCallbackDelegate(IntPtr userData);
}
