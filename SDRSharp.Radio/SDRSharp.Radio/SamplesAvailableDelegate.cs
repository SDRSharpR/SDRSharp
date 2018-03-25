namespace SDRSharp.Radio
{
	public unsafe delegate void SamplesAvailableDelegate(IFrontendController sender, Complex* data, int len);
}
