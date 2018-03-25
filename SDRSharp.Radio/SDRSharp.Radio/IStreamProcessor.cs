namespace SDRSharp.Radio
{
	public interface IStreamProcessor : IBaseProcessor
	{
		double SampleRate
		{
			set;
		}
	}
}
