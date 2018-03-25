namespace SDRSharp.Radio
{
	public interface IRealProcessor : IStreamProcessor, IBaseProcessor
	{
		unsafe void Process(float* buffer, int length);
	}
}
