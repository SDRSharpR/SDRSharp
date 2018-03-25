namespace SDRSharp.Common
{
	public interface IVFOSource
	{
		long VFOFrequency
		{
			get;
			set;
		}

		int VFODecimation
		{
			get;
			set;
		}

		int VFOMinIQDecimation
		{
			get;
		}

		double VFOMaxSampleRate
		{
			get;
		}
	}
}
