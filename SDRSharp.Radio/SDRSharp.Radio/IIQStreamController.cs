namespace SDRSharp.Radio
{
	public interface IIQStreamController
	{
		double Samplerate
		{
			get;
		}

		void Start(SamplesAvailableDelegate callback);

		void Stop();
	}
}
