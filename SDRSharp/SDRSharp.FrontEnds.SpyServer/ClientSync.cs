namespace SDRSharp.FrontEnds.SpyServer
{
	public struct ClientSync
	{
		public uint CanControl;

		public uint Gain;

		public uint DeviceCenterFrequency;

		public uint IQCenterFrequency;

		public uint FFTCenterFrequency;

		public uint MinimumIQCenterFrequency;

		public uint MaximumIQCenterFrequency;

		public uint MinimumFFTCenterFrequency;

		public uint MaximumFFTCenterFrequency;
	}
}
