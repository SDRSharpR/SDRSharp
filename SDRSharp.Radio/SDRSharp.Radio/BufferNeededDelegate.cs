namespace SDRSharp.Radio
{
	public unsafe delegate void BufferNeededDelegate(Complex* iqBuffer, float* audioBuffer, int length);
}
