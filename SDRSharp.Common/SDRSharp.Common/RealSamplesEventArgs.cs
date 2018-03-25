using System;

namespace SDRSharp.Common
{
	public sealed class RealSamplesEventArgs : EventArgs
	{
		public int Length
		{
			get;
			set;
		}

		public ulong DroppedSamples
		{
			get;
			set;
		}

		public unsafe float* Buffer
		{
			get;
			set;
		}
	}
}
