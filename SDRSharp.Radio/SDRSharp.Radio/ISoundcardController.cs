namespace SDRSharp.Radio
{
	public interface ISoundcardController
	{
		string SoundCardHint
		{
			get;
		}

		double SampleRateHint
		{
			get;
		}
	}
}
