namespace SDRSharp.Radio
{
	public interface ITunableSource
	{
		bool CanTune
		{
			get;
		}

		long Frequency
		{
			get;
			set;
		}

		long MinimumTunableFrequency
		{
			get;
		}

		long MaximumTunableFrequency
		{
			get;
		}
	}
}
