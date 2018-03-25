using System;

namespace SDRSharp.PanView
{
	public class LineInsertEventArgs : EventArgs
	{
		public unsafe int* RgbBuffer
		{
			get;
			private set;
		}

		public int Length
		{
			get;
			private set;
		}

		public unsafe LineInsertEventArgs(int* rgbBuffer, int length)
		{
			rgbBuffer = this.RgbBuffer;
			this.Length = length;
		}
	}
}
