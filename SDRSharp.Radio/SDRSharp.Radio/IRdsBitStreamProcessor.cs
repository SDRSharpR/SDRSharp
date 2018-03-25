namespace SDRSharp.Radio
{
	public interface IRdsBitStreamProcessor : IBaseProcessor
	{
		void Process(ref RdsFrame frame);
	}
}
