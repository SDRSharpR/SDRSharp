using System.Runtime.InteropServices;

namespace SDRSharp.FrontEnds.AirspyHF
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate int airspyhf_sample_cb(airspyhf_transfer* ptr);
}
