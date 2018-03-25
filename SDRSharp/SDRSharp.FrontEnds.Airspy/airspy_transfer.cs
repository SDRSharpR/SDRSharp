using System;

namespace SDRSharp.FrontEnds.Airspy
{
	public struct airspy_transfer
	{
		public IntPtr device;

		public IntPtr ctx;

		public unsafe void* samples;

		public int sample_count;

		public ulong dropped_samples;

		public airspy_sample_type sample_type;
	}
}
