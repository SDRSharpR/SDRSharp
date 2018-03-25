using SDRSharp.Radio;
using System;

namespace SDRSharp.FrontEnds.AirspyHF
{
	public struct airspyhf_transfer
	{
		public IntPtr device;

		public IntPtr ctx;

		public unsafe Complex* samples;

		public int sample_count;

		public ulong dropped_samples;
	}
}
