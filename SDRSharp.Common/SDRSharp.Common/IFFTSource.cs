namespace SDRSharp.Common
{
	public interface IFFTSource
	{
		bool FFTEnabled
		{
			get;
			set;
		}

		int FFTRange
		{
			get;
			set;
		}

		int FFTOffset
		{
			get;
			set;
		}

		int DisplayBandwidth
		{
			get;
		}

		int DisplayPixels
		{
			get;
			set;
		}

		event SamplesAvailableDelegate<ByteSamplesEventArgs> FFTAvailable;
	}
}
