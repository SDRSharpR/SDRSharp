using System;

namespace SDRSharp.FrequencyEdit
{
	public class FrequencyChangingEventArgs : EventArgs
	{
		public long Frequency { get; set; }

		public bool Accept { get; set; }

		public FrequencyChangingEventArgs()
		{
		}
	}
}
