using System;

namespace SDRSharp.PanView
{
	public class BandwidthEventArgs : EventArgs
	{
		public int Bandwidth
		{
			get;
			set;
		}

		public int Offset
		{
			get;
			set;
		}

		public bool Cancel
		{
			get;
			set;
		}

		public BandType Side
		{
			get;
			private set;
		}

		public BandwidthEventArgs(int bandwidth, int offset, BandType side)
		{
			this.Bandwidth = bandwidth;
			this.Offset = offset;
			this.Side = side;
		}
	}
}
