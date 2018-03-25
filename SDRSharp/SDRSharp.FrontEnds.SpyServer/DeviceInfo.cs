namespace SDRSharp.FrontEnds.SpyServer
{
	public struct DeviceInfo
	{
		public DeviceType DeviceType;

		public uint DeviceSerial;

		public uint MaximumSampleRate;

		public uint MaximumBandwidth;

		public uint DecimationStageCount;

		public uint GainStageCount;

		public uint MaximumGainIndex;

		public uint MinimumFrequency;

		public uint MaximumFrequency;

		public uint Resolution;

		public uint MinimumIQDecimation;

		public uint ForcedIQFormat;
	}
}
