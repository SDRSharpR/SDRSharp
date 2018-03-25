using System;

namespace SDRSharp.Radio
{
	public interface ISampleRateChangeSource
	{
		event EventHandler SampleRateChanged;
	}
}
