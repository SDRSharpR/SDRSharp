using SDRSharp.Common;
using SDRSharp.PanView;
using SDRSharp.Radio;
using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDRSharp
{
	public class SharpControlProxy : ISharpControl, INotifyPropertyChanged
	{
		private readonly MainForm _owner;

		public bool Enabled
		{
			get;
			set;
		}

		public DetectorType DetectorType
		{
			get
			{
				return this._owner.DetectorType;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.DetectorType = value;
						});
					}
					else
					{
						this._owner.DetectorType = value;
					}
				}
			}
		}

		public WindowType FilterType
		{
			get
			{
				return this._owner.FilterType;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FilterType = value;
						});
					}
					else
					{
						this._owner.FilterType = value;
					}
				}
			}
		}

		public int AudioGain
		{
			get
			{
				return this._owner.AudioGain;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.AudioGain = value;
						});
					}
					else
					{
						this._owner.AudioGain = value;
					}
				}
			}
		}

		public bool AudioIsMuted
		{
			get
			{
				return this._owner.AudioIsMuted;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.AudioIsMuted = value;
						});
					}
					else
					{
						this._owner.AudioIsMuted = value;
					}
				}
			}
		}

		public long CenterFrequency
		{
			get
			{
				return this._owner.CenterFrequency;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.CenterFrequency = value;
						});
					}
					else
					{
						this._owner.CenterFrequency = value;
					}
				}
			}
		}

		public int CWShift
		{
			get
			{
				return this._owner.CWShift;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.CWShift = value;
						});
					}
					else
					{
						this._owner.CWShift = value;
					}
				}
			}
		}

		public bool FilterAudio
		{
			get
			{
				return this._owner.FilterAudio;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FilterAudio = value;
						});
					}
					else
					{
						this._owner.FilterAudio = value;
					}
				}
			}
		}

		public bool UnityGain
		{
			get
			{
				return this._owner.UnityGain;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.UnityGain = value;
						});
					}
					else
					{
						this._owner.UnityGain = value;
					}
				}
			}
		}

		public int FilterBandwidth
		{
			get
			{
				return this._owner.FilterBandwidth;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FilterBandwidth = value;
						});
					}
					else
					{
						this._owner.FilterBandwidth = value;
					}
				}
			}
		}

		public int FilterOrder
		{
			get
			{
				return this._owner.FilterOrder;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FilterOrder = value;
						});
					}
					else
					{
						this._owner.FilterOrder = value;
					}
				}
			}
		}

		public bool FmStereo
		{
			get
			{
				return this._owner.FmStereo;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FmStereo = value;
						});
					}
					else
					{
						this._owner.FmStereo = value;
					}
				}
			}
		}

		public long Frequency
		{
			get
			{
				return this._owner.Frequency;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.Frequency = value;
						});
					}
					else
					{
						this._owner.Frequency = value;
					}
				}
			}
		}

		public long FrequencyShift
		{
			get
			{
				return this._owner.FrequencyShift;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FrequencyShift = value;
						});
					}
					else
					{
						this._owner.FrequencyShift = value;
					}
				}
			}
		}

		public bool FrequencyShiftEnabled
		{
			get
			{
				return this._owner.FrequencyShiftEnabled;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.FrequencyShiftEnabled = value;
						});
					}
					else
					{
						this._owner.FrequencyShiftEnabled = value;
					}
				}
			}
		}

		public bool UseAgc
		{
			get
			{
				return this._owner.UseAgc;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.UseAgc = value;
						});
					}
					else
					{
						this._owner.UseAgc = value;
					}
				}
			}
		}

		public bool AgcHang
		{
			get
			{
				return this._owner.AgcHang;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.AgcHang = value;
						});
					}
					else
					{
						this._owner.AgcHang = value;
					}
				}
			}
		}

		public int AgcThreshold
		{
			get
			{
				return this._owner.AgcThreshold;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.AgcThreshold = value;
						});
					}
					else
					{
						this._owner.AgcThreshold = value;
					}
				}
			}
		}

		public int AgcDecay
		{
			get
			{
				return this._owner.AgcDecay;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.AgcDecay = value;
						});
					}
					else
					{
						this._owner.AgcDecay = value;
					}
				}
			}
		}

		public int AgcSlope
		{
			get
			{
				return this._owner.AgcSlope;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.AgcSlope = value;
						});
					}
					else
					{
						this._owner.AgcSlope = value;
					}
				}
			}
		}

		public bool MarkPeaks
		{
			get
			{
				return this._owner.MarkPeaks;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.MarkPeaks = value;
						});
					}
					else
					{
						this._owner.MarkPeaks = value;
					}
				}
			}
		}

		public bool SnapToGrid
		{
			get
			{
				return this._owner.SnapToGrid;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.SnapToGrid = value;
						});
					}
					else
					{
						this._owner.SnapToGrid = value;
					}
				}
			}
		}

		public bool SquelchEnabled
		{
			get
			{
				return this._owner.SquelchEnabled;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.SquelchEnabled = value;
						});
					}
					else
					{
						this._owner.SquelchEnabled = value;
					}
				}
			}
		}

		public int SquelchThreshold
		{
			get
			{
				return this._owner.SquelchThreshold;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.SquelchThreshold = value;
						});
					}
					else
					{
						this._owner.SquelchThreshold = value;
					}
				}
			}
		}

		public bool SwapIq
		{
			get
			{
				return this._owner.SwapIq;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.SwapIq = value;
						});
					}
					else
					{
						this._owner.SwapIq = value;
					}
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this._owner.IsPlaying;
			}
		}

		public float WAttack
		{
			get
			{
				return this._owner.WAttack;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.WAttack = value;
						});
					}
					else
					{
						this._owner.WAttack = value;
					}
				}
			}
		}

		public float WDecay
		{
			get
			{
				return this._owner.WDecay;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.WDecay = value;
						});
					}
					else
					{
						this._owner.WDecay = value;
					}
				}
			}
		}

		public float SAttack
		{
			get
			{
				return this._owner.SAttack;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.SAttack = value;
						});
					}
					else
					{
						this._owner.SAttack = value;
					}
				}
			}
		}

		public float SDecay
		{
			get
			{
				return this._owner.SDecay;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.SDecay = value;
						});
					}
					else
					{
						this._owner.SDecay = value;
					}
				}
			}
		}

		public int Zoom
		{
			get
			{
				return this._owner.Zoom;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.Zoom = value;
						});
					}
					else
					{
						this._owner.Zoom = value;
					}
				}
			}
		}

		public bool UseTimeMarkers
		{
			get
			{
				return this._owner.UseTimeMarkers;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.UseTimeMarkers = value;
						});
					}
					else
					{
						this._owner.UseTimeMarkers = value;
					}
				}
			}
		}

		public string RdsProgramService
		{
			get
			{
				return this._owner.RdsProgramService;
			}
		}

		public string RdsRadioText
		{
			get
			{
				return this._owner.RdsRadioText;
			}
		}

		public bool RdsUseFEC
		{
			get
			{
				return this._owner.RdsUseFEC;
			}
			set
			{
				if (this.Enabled)
				{
					this._owner.RdsUseFEC = value;
				}
			}
		}

		public bool IsSquelchOpen
		{
			get
			{
				return this._owner.IsSquelchOpen;
			}
		}

		public int RFBandwidth
		{
			get
			{
				return this._owner.RFBandwidth;
			}
		}

		public int RFDisplayBandwidth
		{
			get
			{
				return this._owner.RFDisplayBandwidth;
			}
		}

		public int FFTResolution
		{
			get
			{
				return this._owner.FFTResolution;
			}
		}

		public float FFTRange
		{
			get
			{
				return this._owner.FFTRange;
			}
		}

		public float FFTOffset
		{
			get
			{
				return this._owner.FFTOffset;
			}
		}

		public int FFTContrast
		{
			get
			{
				return this._owner.FFTContrast;
			}
		}

		public ColorBlend Gradient
		{
			get
			{
				return this._owner.Gradient;
			}
		}

		public SpectrumStyle FFTSpectrumStyle
		{
			get
			{
				return this._owner.FFTSpectrumStyle;
			}
		}

		public int StepSize
		{
			get
			{
				return this._owner.StepSize;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.StepSize = value;
						});
					}
					else
					{
						this._owner.StepSize = value;
					}
				}
			}
		}

		public bool SourceIsWaveFile
		{
			get
			{
				return this._owner.SourceIsWaveFile;
			}
		}

		public string SourceName
		{
			get
			{
				return this._owner.SourceName;
			}
		}

		public Type SourceType
		{
			get
			{
				return this._owner.SourceType;
			}
		}

		public bool SourceIsSoundCard
		{
			get
			{
				return this._owner.SourceIsSoundCard;
			}
		}

		public bool SourceIsTunable
		{
			get
			{
				return this._owner.SourceIsTunable;
			}
		}

		public object Source
		{
			get
			{
				return this._owner.Source;
			}
		}

		public bool BypassDemodulation
		{
			get
			{
				return this._owner.BypassDemodulation;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.BypassDemodulation = value;
						});
					}
					else
					{
						this._owner.BypassDemodulation = value;
					}
				}
			}
		}

		public int TunableBandwidth
		{
			get
			{
				return this._owner.TunableBandwidth;
			}
		}

		public TuningStyle TuningStyle
		{
			get
			{
				return this._owner.TuningStyle;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.TuningStyle = value;
						});
					}
					else
					{
						this._owner.TuningStyle = value;
					}
				}
			}
		}

		public int IFOffset
		{
			get
			{
				return this._owner.IFOffset;
			}
		}

		public float TuningLimit
		{
			get
			{
				return this._owner.TuningLimit;
			}
			set
			{
				if (this.Enabled)
				{
					if (this._owner.InvokeRequired)
					{
						this._owner.Invoke((MethodInvoker)delegate
						{
							this._owner.TuningLimit = value;
						});
					}
					else
					{
						this._owner.TuningLimit = value;
					}
				}
			}
		}

		public float VisualSNR
		{
			get
			{
				return this._owner.VisualSNR;
			}
		}

		public bool TuningStyleFreezed
		{
			get
			{
				return this._owner.TuningStyleFreezed;
			}
			set
			{
				this._owner.TuningStyleFreezed = value;
			}
		}

		public double AudioSampleRate
		{
			get
			{
				return this._owner.AudioSampleRate;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event CustomPaintEventHandler WaterfallCustomPaint;

		public event CustomPaintEventHandler SpectrumAnalyzerCustomPaint;

		public event CustomPaintEventHandler SpectrumAnalyzerBackgroundCustomPaint;

		public SharpControlProxy(MainForm owner)
		{
			this._owner = owner;
			this._owner.PropertyChanged += this.PropertyChangedEventHandler;
			this._owner.WaterfallCustomPaint += this.waterfall_CustomPaint;
			this._owner.SpectrumAnalyzerCustomPaint += this.spectrumAnalyzer_CustomPaint;
			this._owner.SpectrumAnalyzerBackgroundCustomPaint += this.spectrumAnalyzer_BackgroundCustomPaint;
		}

		public void SetFrequency(long frequency, bool onlyMoveCenterFrequency)
		{
			if (this.Enabled)
			{
				if (this._owner.InvokeRequired)
				{
					this._owner.Invoke((MethodInvoker)delegate
					{
						this._owner.SetFrequency(frequency, onlyMoveCenterFrequency);
					});
				}
				else
				{
					this._owner.SetFrequency(frequency, onlyMoveCenterFrequency);
				}
			}
		}

		public void ResetFrequency(long frequency, long centerFrequency)
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.ResetFrequency(frequency, centerFrequency);
				});
			}
			else
			{
				this._owner.ResetFrequency(frequency, centerFrequency);
			}
		}

		public void ResetFrequency(long frequency)
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.ResetFrequency(frequency);
				});
			}
			else
			{
				this._owner.ResetFrequency(frequency);
			}
		}

		public void GetSpectrumSnapshot(byte[] destArray)
		{
			this._owner.GetSpectrumSnapshot(destArray);
		}

		public void GetSpectrumSnapshot(float[] destArray, float scale = 1f, float offset = 0f)
		{
			this._owner.GetSpectrumSnapshot(destArray, scale, offset);
		}

		public void StartRadio()
		{
			if (this.Enabled)
			{
				if (this._owner.InvokeRequired)
				{
					this._owner.Invoke((MethodInvoker)delegate
					{
						this._owner.StartRadio();
					});
				}
				else
				{
					this._owner.StartRadio();
				}
			}
		}

		public void StopRadio()
		{
			if (this.Enabled)
			{
				if (this._owner.InvokeRequired)
				{
					this._owner.BeginInvoke((MethodInvoker)delegate
					{
						this._owner.StopRadio();
					});
				}
				else
				{
					this._owner.StopRadio();
				}
			}
		}

		public void RegisterStreamHook(object streamHook, ProcessorType processorType)
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.RegisterStreamHook(streamHook, processorType);
				});
			}
			else
			{
				this._owner.RegisterStreamHook(streamHook, processorType);
			}
		}

		public void UnregisterStreamHook(object streamHook)
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.UnregisterStreamHook(streamHook);
				});
			}
			else
			{
				this._owner.UnregisterStreamHook(streamHook);
			}
		}

		public void RegisterFrontControl(UserControl control, PluginPosition position)
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.RegisterFrontControl(control, position);
				});
			}
			else
			{
				this._owner.RegisterFrontControl(control, position);
			}
		}

		public void Perform()
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.Perform();
				});
			}
			else
			{
				this._owner.Perform();
			}
		}

		public void RefreshSource(bool reload)
		{
			if (this._owner.InvokeRequired)
			{
				this._owner.Invoke((MethodInvoker)delegate
				{
					this._owner.RefreshSource(reload);
				});
			}
			else
			{
				this._owner.RefreshSource(reload);
			}
		}

		private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				propertyChanged(sender, new PropertyChangedEventArgs(e.PropertyName));
			}
		}

		private void spectrumAnalyzer_CustomPaint(object sender, CustomPaintEventArgs e)
		{
			CustomPaintEventHandler spectrumAnalyzerCustomPaint = this.SpectrumAnalyzerCustomPaint;
			if (spectrumAnalyzerCustomPaint != null)
			{
				spectrumAnalyzerCustomPaint(sender, e);
			}
		}

		private void spectrumAnalyzer_BackgroundCustomPaint(object sender, CustomPaintEventArgs e)
		{
			CustomPaintEventHandler spectrumAnalyzerBackgroundCustomPaint = this.SpectrumAnalyzerBackgroundCustomPaint;
			if (spectrumAnalyzerBackgroundCustomPaint != null)
			{
				spectrumAnalyzerBackgroundCustomPaint(sender, e);
			}
		}

		private void waterfall_CustomPaint(object sender, CustomPaintEventArgs e)
		{
			CustomPaintEventHandler waterfallCustomPaint = this.WaterfallCustomPaint;
			if (waterfallCustomPaint != null)
			{
				waterfallCustomPaint(sender, e);
			}
		}
	}
}
