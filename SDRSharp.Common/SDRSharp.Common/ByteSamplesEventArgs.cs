using System;

namespace SDRSharp.Common
{
	public sealed class ByteSamplesEventArgs : EventArgs
	{
		public int Length
		{
			get;
			set;
		}

		public unsafe byte* Buffer
		{
			get;
			set;
		}
	}
}
