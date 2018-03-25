using SDRSharp.Radio;
using System;

namespace SDRSharp.Common
{
	public sealed class ComplexSamplesEventArgs : EventArgs
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

		public unsafe Complex* Buffer
		{
			get;
			set;
		}
	}
}
