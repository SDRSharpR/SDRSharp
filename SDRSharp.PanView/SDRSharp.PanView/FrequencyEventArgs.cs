using System;

namespace SDRSharp.PanView
{
	public class FrequencyEventArgs : EventArgs
	{
		public long Frequency
		{
			get;
			set;
		}

		public FrequencyChangeSource Source
		{
			get;
			set;
		}

		public bool Cancel
		{
			get;
			set;
		}

		public FrequencyEventArgs(long frequency, FrequencyChangeSource source)
		{
			this.Frequency = frequency;
			this.Source = source;
		}
	}
}
