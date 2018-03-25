namespace SDRSharp.Radio
{
	public interface IIQProcessor : IStreamProcessor, IBaseProcessor
	{
		unsafe void Process(Complex* buffer, int length);
	}
}
