using SDRSharp.PanView;
using SDRSharp.Radio;
using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDRSharp.Common
{
	public interface ISharpControl
	{
		DetectorType DetectorType
		{
			get;
			set;
		}

		WindowType FilterType
		{
			get;
			set;
		}

		int AudioGain
		{
			get;
			set;
		}

		long CenterFrequency
		{
			get;
			set;
		}

		int CWShift
		{
			get;
			set;
		}

		bool FilterAudio
		{
			get;
			set;
		}

		bool UnityGain
		{
			get;
			set;
		}

		int FilterBandwidth
		{
			get;
			set;
		}

		int FilterOrder
		{
			get;
			set;
		}

		bool FmStereo
		{
			get;
			set;
		}

		long Frequency
		{
			get;
			set;
		}

		long FrequencyShift
		{
			get;
			set;
		}

		bool FrequencyShiftEnabled
		{
			get;
			set;
		}

		bool MarkPeaks
		{
			get;
			set;
		}

		bool SnapToGrid
		{
			get;
			set;
		}

		bool SquelchEnabled
		{
			get;
			set;
		}

		int SquelchThreshold
		{
			get;
			set;
		}

		bool IsSquelchOpen
		{
			get;
		}

		bool SwapIq
		{
			get;
			set;
		}

		bool UseAgc
		{
			get;
			set;
		}

		bool AgcHang
		{
			get;
			set;
		}

		int AgcThreshold
		{
			get;
			set;
		}

		int AgcDecay
		{
			get;
			set;
		}

		int AgcSlope
		{
			get;
			set;
		}

		int FFTResolution
		{
			get;
		}

		float FFTRange
		{
			get;
		}

		float FFTOffset
		{
			get;
		}

		int FFTContrast
		{
			get;
		}

		float VisualSNR
		{
			get;
		}

		int IFOffset
		{
			get;
		}

		ColorBlend Gradient
		{
			get;
		}

		SpectrumStyle FFTSpectrumStyle
		{
			get;
		}

		int StepSize
		{
			get;
			set;
		}

		int Zoom
		{
			get;
			set;
		}

		bool IsPlaying
		{
			get;
		}

		float SAttack
		{
			get;
			set;
		}

		float SDecay
		{
			get;
			set;
		}

		float WAttack
		{
			get;
			set;
		}

		float WDecay
		{
			get;
			set;
		}

		bool UseTimeMarkers
		{
			get;
			set;
		}

		string RdsProgramService
		{
			get;
		}

		string RdsRadioText
		{
			get;
		}

		bool RdsUseFEC
		{
			get;
			set;
		}

		int RFBandwidth
		{
			get;
		}

		int RFDisplayBandwidth
		{
			get;
		}

		int TunableBandwidth
		{
			get;
		}

		float TuningLimit
		{
			get;
			set;
		}

		TuningStyle TuningStyle
		{
			get;
			set;
		}

		bool TuningStyleFreezed
		{
			get;
			set;
		}

		bool SourceIsSoundCard
		{
			get;
		}

		bool SourceIsWaveFile
		{
			get;
		}

		bool SourceIsTunable
		{
			get;
		}

		object Source
		{
			get;
		}

		bool AudioIsMuted
		{
			get;
			set;
		}

		bool BypassDemodulation
		{
			get;
			set;
		}

		Type SourceType
		{
			get;
		}

		string SourceName
		{
			get;
		}

		double AudioSampleRate
		{
			get;
		}

		event PropertyChangedEventHandler PropertyChanged;

		event CustomPaintEventHandler WaterfallCustomPaint;

		event CustomPaintEventHandler SpectrumAnalyzerCustomPaint;

		event CustomPaintEventHandler SpectrumAnalyzerBackgroundCustomPaint;

		void SetFrequency(long frequency, bool onlyMoveCenterFrequency);

		void ResetFrequency(long frequency);

		void ResetFrequency(long frequency, long centerFrequency);

		[Obsolete("Use GetSpectrumSnapshot(float[], float, float) instead")]
		void GetSpectrumSnapshot(byte[] destArray);

		void GetSpectrumSnapshot(float[] destArray, float scale = 1f, float offset = 0f);

		void StartRadio();

		void StopRadio();

		void RegisterStreamHook(object streamHook, ProcessorType processorType);

		void UnregisterStreamHook(object streamHook);

		void RegisterFrontControl(UserControl control, PluginPosition preferredPosition);

		void Perform();

		void RefreshSource(bool reload);
	}
}
