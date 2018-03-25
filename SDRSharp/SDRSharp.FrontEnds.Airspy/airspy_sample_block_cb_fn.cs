using System.Runtime.InteropServices;

namespace SDRSharp.FrontEnds.Airspy
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public unsafe delegate int airspy_sample_block_cb_fn(airspy_transfer* ptr);
}
