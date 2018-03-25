namespace SDRSharp.FrontEnds.Airspy
{
	public static class ConversionFilters
	{
		private static readonly float[] Kernel_Dec16_110dB = new float[7]
		{
			-0.03183508f,
			0f,
			0.2818315f,
			0.5000073f,
			0.2818315f,
			0f,
			-0.03183508f
		};

		private static readonly float[] Kernel_Dec8_100dB = new float[11]
		{
			0.006633401f,
			0f,
			-0.0510355234f,
			0f,
			0.2944033f,
			0.4999975f,
			0.2944033f,
			0f,
			-0.0510355234f,
			0f,
			0.006633401f
		};

		private static readonly float[] Kernel_Dec4_90dB = new float[15]
		{
			-0.0024741888f,
			0f,
			0.0169657469f,
			0f,
			-0.0676806f,
			0f,
			0.303180575f,
			0.500017047f,
			0.303180575f,
			0f,
			-0.0676806f,
			0f,
			0.0169657469f,
			0f,
			-0.0024741888f
		};

		private static readonly float[] Kernel_Dec2_80dB = new float[47]
		{
			-0.00019800663f,
			0f,
			0.000576853752f,
			0f,
			-0.001352191f,
			0f,
			0.00272917747f,
			0f,
			-0.00498819351f,
			0f,
			0.008499503f,
			0f,
			-0.0137885809f,
			0f,
			0.0217131376f,
			0f,
			-0.0339800119f,
			0f,
			0.0549448729f,
			0f,
			-0.100657463f,
			0f,
			0.3164574f,
			0.5f,
			0.3164574f,
			0f,
			-0.100657463f,
			0f,
			0.0549448729f,
			0f,
			-0.0339800119f,
			0f,
			0.0217131376f,
			0f,
			-0.0137885809f,
			0f,
			0.008499503f,
			0f,
			-0.00498819351f,
			0f,
			0.00272917747f,
			0f,
			-0.001352191f,
			0f,
			0.000576853752f,
			0f,
			-0.00019800663f
		};

		public static readonly float[][] FirKernels100dB = new float[4][]
		{
			ConversionFilters.Kernel_Dec2_80dB,
			ConversionFilters.Kernel_Dec4_90dB,
			ConversionFilters.Kernel_Dec8_100dB,
			ConversionFilters.Kernel_Dec16_110dB
		};
	}
}
