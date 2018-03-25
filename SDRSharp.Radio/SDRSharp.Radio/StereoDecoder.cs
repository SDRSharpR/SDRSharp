using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	[StructLayout(LayoutKind.Sequential, Pack = 16)]
	public sealed class StereoDecoder
	{
		private const int DefaultPilotFrequency = 19000;

		private const int PllRange = 20;

		private const int PllBandwith = 10;

		private const float PllThreshold = 1f;

		private const float PllLockTime = 0.5f;

		private const float PllZeta = 0.707f;

		private const float AudioGain = 0.2f;

		private static readonly float _deemphasisTime = (float)Utils.GetDoubleSetting("deemphasisTime", 50.0) * 1E-06f;

		private static readonly float _pllPhaseAdjM = (float)Utils.GetDoubleSetting("pllPhaseAdjM", 0.0);

		private static readonly float _pllPhaseAdjB = (float)Utils.GetDoubleSetting("pllPhaseAdjB", -1.75);

		private unsafe readonly Pll* _pll;

		private readonly UnsafeBuffer _pllBuffer;

		private unsafe IirFilter* _pilotFilter;

		private UnsafeBuffer _pilotFilterBuffer;

		private UnsafeBuffer _channelABuffer;

		private UnsafeBuffer _channelBBuffer;

		private unsafe float* _channelAPtr;

		private unsafe float* _channelBPtr;

		private ComplexFilter _channelAFilter;

		private ComplexFilter _channelBFilter;

		private FloatDecimator _channelADecimator;

		private FloatDecimator _channelBDecimator;

		private double _sampleRate;

		private int _audioDecimationRatio;

		private float _deemphasisAlpha;

		private float _deemphasisAvgL;

		private float _deemphasisAvgR;

		private bool _forceMono;

		public bool ForceMono
		{
			get
			{
				return this._forceMono;
			}
			set
			{
				this._forceMono = value;
			}
		}

		public unsafe bool IsPllLocked
		{
			get
			{
				return this._pll->IsLocked;
			}
		}

		public unsafe StereoDecoder()
		{
			this._pllBuffer = UnsafeBuffer.Create(sizeof(Pll));
			this._pll = (Pll*)(void*)this._pllBuffer;
		}

		public unsafe void Process(float* baseBand, float* interleavedStereo, int length)
		{
			if (this._forceMono)
			{
				this.ProcessMono(baseBand, interleavedStereo, length);
			}
			else
			{
				this.ProcessStereo(baseBand, interleavedStereo, length);
			}
		}

		private unsafe void ProcessMono(float* baseBand, float* interleavedStereo, int length)
		{
			if (this._channelABuffer == null || this._channelABuffer.Length != length)
			{
				this._channelABuffer = UnsafeBuffer.Create(length, 4);
				this._channelAPtr = (float*)(void*)this._channelABuffer;
			}
			Utils.Memcpy(this._channelAPtr, baseBand, length * 4);
			this._channelADecimator.Process(this._channelAPtr, length);
			length /= this._audioDecimationRatio;
			this._channelAFilter.Process(this._channelAPtr, length, 1);
			for (int i = 0; i < length; i++)
			{
				this._deemphasisAvgL += this._deemphasisAlpha * (this._channelAPtr[i] - this._deemphasisAvgL);
				this._channelAPtr[i] = this._deemphasisAvgL;
			}
			for (int j = 0; j < length; j++)
			{
				interleavedStereo[j * 2 + 1] = (interleavedStereo[j * 2] = this._channelAPtr[j] * 0.2f);
			}
		}

		private unsafe void ProcessStereo(float* baseBand, float* interleavedStereo, int length)
		{
			if (this._channelABuffer == null || this._channelABuffer.Length != length)
			{
				this._channelABuffer = UnsafeBuffer.Create(length, 4);
				this._channelAPtr = (float*)(void*)this._channelABuffer;
			}
			if (this._channelBBuffer == null || this._channelBBuffer.Length != length)
			{
				this._channelBBuffer = UnsafeBuffer.Create(length, 4);
				this._channelBPtr = (float*)(void*)this._channelBBuffer;
			}
			int num = length / this._audioDecimationRatio;
			Utils.Memcpy(this._channelAPtr, baseBand, length * 4);
			this._channelADecimator.Process(this._channelAPtr, length);
			this._channelAFilter.Process(this._channelAPtr, num, 1);
			for (int i = 0; i < length; i++)
			{
				float sample = this._pilotFilter->Process(baseBand[i]);
				this._pll->Process(sample);
				this._channelBPtr[i] = baseBand[i] * Trig.Sin((float)((double)this._pll->AdjustedPhase * 2.0));
			}
			if (!this._pll->IsLocked)
			{
				for (int j = 0; j < num; j++)
				{
					this._deemphasisAvgL += this._deemphasisAlpha * (this._channelAPtr[j] - this._deemphasisAvgL);
					this._channelAPtr[j] = this._deemphasisAvgL;
				}
				for (int k = 0; k < num; k++)
				{
					interleavedStereo[k * 2 + 1] = (interleavedStereo[k * 2] = this._channelAPtr[k] * 0.2f);
				}
			}
			else
			{
				this._channelBDecimator.Process(this._channelBPtr, length);
				this._channelBFilter.Process(this._channelBPtr, num, 1);
				for (int l = 0; l < num; l++)
				{
					float num2 = this._channelAPtr[l];
					float num3 = 2f * this._channelBPtr[l];
					interleavedStereo[l * 2] = (num2 + num3) * 0.2f;
					interleavedStereo[l * 2 + 1] = (num2 - num3) * 0.2f;
				}
				for (int m = 0; m < num; m++)
				{
					this._deemphasisAvgL += this._deemphasisAlpha * (interleavedStereo[m * 2] - this._deemphasisAvgL);
					interleavedStereo[m * 2] = this._deemphasisAvgL;
					this._deemphasisAvgR += this._deemphasisAlpha * (interleavedStereo[m * 2 + 1] - this._deemphasisAvgR);
					interleavedStereo[m * 2 + 1] = this._deemphasisAvgR;
				}
			}
		}

		public unsafe void Configure(double sampleRate, int decimationStageCount)
		{
			this._audioDecimationRatio = 1 << decimationStageCount;
			if (this._sampleRate != sampleRate)
			{
				this._sampleRate = sampleRate;
				this._pilotFilterBuffer = UnsafeBuffer.Create(sizeof(IirFilter));
				this._pilotFilter = (IirFilter*)(void*)this._pilotFilterBuffer;
				this._pilotFilter->Init(IirFilterType.BandPass, 19000.0, this._sampleRate, 500.0);
				this._pll->SampleRate = (float)this._sampleRate;
				this._pll->DefaultFrequency = 19000f;
				this._pll->Range = 20f;
				this._pll->Bandwidth = 10f;
				this._pll->Zeta = 0.707f;
				this._pll->PhaseAdjM = StereoDecoder._pllPhaseAdjM;
				this._pll->PhaseAdjB = StereoDecoder._pllPhaseAdjB;
				this._pll->LockTime = 0.5f;
				this._pll->LockThreshold = 1f;
				double num = sampleRate / (double)this._audioDecimationRatio;
				Complex[] kernel = FilterBuilder.MakeComplexKernel(num, 250, Math.Min(0.9 * num, 30000.0), 0.0, WindowType.BlackmanHarris4);
				this._channelAFilter = new ComplexFilter(kernel);
				this._channelBFilter = new ComplexFilter(kernel);
				this._deemphasisAlpha = (float)(1.0 - Math.Exp(-1.0 / (num * (double)StereoDecoder._deemphasisTime)));
				this._deemphasisAvgL = 0f;
				this._deemphasisAvgR = 0f;
			}
			if (this._channelADecimator != null && this._channelBDecimator != null && this._audioDecimationRatio == this._channelADecimator.DecimationRatio)
			{
				return;
			}
			this._channelADecimator = new FloatDecimator(this._audioDecimationRatio);
			this._channelBDecimator = new FloatDecimator(this._audioDecimationRatio);
		}
	}
}
