using Properties;
using SDRSharp.CollapsiblePanel;
using SDRSharp.Common;
using SDRSharp.FrequencyEdit;
using SDRSharp.FrontEnds.Airspy;
using SDRSharp.FrontEnds.AirspyHF;
using SDRSharp.FrontEnds.SpyServer;
using SDRSharp.PanView;
//using SDRSharp.Properties;
using SDRSharp.Radio;
using SDRSharp.Radio.PortAudio;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp
{
	public class MainForm : Form, ISharpControl, INotifyPropertyChanged
	{
		private class IQCorrectionProcessor : IIQProcessor, IStreamProcessor, IBaseProcessor
		{
			private readonly IQBalancer _iqBalancer = new IQBalancer();

			public double SampleRate
			{
				set
				{
				}
			}

			public bool Enabled
			{
				get
				{
					return true;
				}
				set
				{
				}
			}

			public IQBalancer Engine
			{
				get
				{
					return this._iqBalancer;
				}
			}

			public unsafe void Process(Complex* buffer, int length)
			{
				this._iqBalancer.Process(buffer, length);
			}
		}

		private enum FrequencyInitType
		{
			None,
			Vfo,
			Device
		}

		private IContainer components;

		private Button playStopButton;

		private OpenFileDialog openDlg;

		private CheckBox agcCheckBox;

		private Label label4;

		private NumericUpDown agcThresholdNumericUpDown;

		private SpectrumAnalyzer spectrumAnalyzer;

		private Waterfall waterfall;

		private Label label10;

		private NumericUpDown agcDecayNumericUpDown;

		private Label label12;

		private ComboBox outputDeviceComboBox;

		private Label label11;

		private ComboBox inputDeviceComboBox;

		private Label label13;

		private ComboBox sampleRateComboBox;

		private Label label7;

		private ComboBox viewComboBox;

		private Label label8;

		private ComboBox fftWindowComboBox;

		private System.Windows.Forms.Timer iqTimer;

		private Button gradientButton;

		private Label label14;

		private TrackBar fftContrastTrackBar;

		private TrackBar fftZoomTrackBar;

		private Label label19;

		private Label label20;

		private Label label21;

		private ComboBox fftResolutionComboBox;

		private NumericUpDown agcSlopeNumericUpDown;

		private Label label22;

		private RadioButton nfmRadioButton;

		private RadioButton rawRadioButton;

		private RadioButton cwRadioButton;

		private RadioButton amRadioButton;

		private RadioButton dsbRadioButton;

		private RadioButton wfmRadioButton;

		private Button configureSourceButton;

		private RadioButton lsbRadioButton;

		private Label label18;

		private RadioButton usbRadioButton;

		private ComboBox stepSizeComboBox;

		private NumericUpDown filterBandwidthNumericUpDown;

		private Label label1;

		private NumericUpDown squelchNumericUpDown;

		private NumericUpDown filterOrderNumericUpDown;

		private Label label16;

		private Label label5;

		private ComboBox iqSourceComboBox;

		private ComboBox filterTypeComboBox;

		private CheckBox swapIQCheckBox;

		private CheckBox correctIQCheckBox;

		private SDRSharp.CollapsiblePanel.CollapsiblePanel radioCollapsiblePanel;

		private SDRSharp.CollapsiblePanel.CollapsiblePanel audioCollapsiblePanel;

		private SDRSharp.CollapsiblePanel.CollapsiblePanel agcCollapsiblePanel;

		private CheckBox agcUseHangCheckBox;

		private SDRSharp.CollapsiblePanel.CollapsiblePanel fftCollapsiblePanel;

		private NumericUpDown latencyNumericUpDown;

		private Label label6;

		private Label label15;

		private NumericUpDown cwShiftNumericUpDown;

		private Panel controlPanel;

		private Label label25;

		private Label label26;

		private Label label24;

		private Label label23;

		private TrackBar wDecayTrackBar;

		private TrackBar wAttackTrackBar;

		private TrackBar sDecayTrackBar;

		private TrackBar sAttackTrackBar;

		private CheckBox snapFrequencyCheckBox;

		private TrackBar audioGainTrackBar;

		private CheckBox fmStereoCheckBox;

		private CheckBox filterAudioCheckBox;

		private CheckBox useSquelchCheckBox;

		private CheckBox frequencyShiftCheckBox;

		private NumericUpDown frequencyShiftNumericUpDown;

		private CheckBox markPeaksCheckBox;

		private CheckBox useTimestampsCheckBox;

		private Label label17;

		private TrackBar fftSpeedTrackBar;

		private GroupBox groupBox1;

		private TrackBar fftOffsetTrackBar;

		private TrackBar fftRangeTrackBar;

		private Label label28;

		private GroupBox smoothingGroupBox;

		private Panel scrollPanel;

		private SDRSharp.FrequencyEdit.FrequencyEdit vfoFrequencyEdit;

		private CheckBox unityGainCheckBox;

		private TableLayoutPanel rightTableLayoutPanel;

		private TableLayoutPanel settingsTableLayoutPanel;

		private Panel centerPanel;

		private TableLayoutPanel leftPluginPanel;

		private TableLayoutPanel rightPluginPanel;

		private Splitter rightSplitter;

		private Splitter leftSplitter;

		private Panel spectrumPanel;

		private Splitter spectrumSplitter;

		private TableLayoutPanel radioTableLayoutPanel;

		private TableLayoutPanel tableLayoutPanel1;

		private TableLayoutPanel tableLayoutPanel2;

		private TableLayoutPanel tableLayoutPanel3;

		private TableLayoutPanel tableLayoutPanel5;

		private TableLayoutPanel tableLayoutPanel4;

		private Button muteButton;

		private Splitter bottomSplitter;

		private TableLayoutPanel bottomPluginPanel;

		private Splitter topSplitter;

		private TableLayoutPanel topPluginPanel;

		private SDRSharp.CollapsiblePanel.CollapsiblePanel sourceCollapsiblePanel;

		private TableLayoutPanel sourceTableLayoutPanel;

		private TableLayoutPanel tableLayoutPanel7;

		private Button toggleMenuButton;

		private Panel menuSpacerPanel;

		private Label label2;

		private CheckBox lockCarrierCheckBox;

		private CheckBox useAntiFadingCheckBox;

		private Button tuningStyleButton;

		private Label label3;

		private ComboBox spectrumStyleComboBox;

		private PictureBox logoPictureBox;

		private static readonly string _baseTitle = "SDR# v" + Assembly.GetExecutingAssembly().GetName().Version;

		private static readonly int[] _defaultNFMState = new int[12]
		{
			8000,
			1000,
			3,
			50,
			1,
			1000,
			1,
			12,
			0,
			0,
			0,
			0
		};

		private static readonly int[] _defaultWFMState = new int[12]
		{
			200000,
			250,
			3,
			50,
			0,
			1000,
			1,
			17,
			0,
			0,
			0,
			0
		};

		private static readonly int[] _defaultAMState = new int[12]
		{
			10000,
			1000,
			3,
			50,
			0,
			1000,
			1,
			4,
			0,
			1,
			0,
			0
		};

		private static readonly int[] _defaultSSBState = new int[12]
		{
			2400,
			1000,
			3,
			50,
			0,
			1000,
			1,
			1,
			0,
			1,
			0,
			0
		};

		private static readonly int[] _defaultDSBState = new int[12]
		{
			6000,
			1000,
			3,
			50,
			0,
			1000,
			1,
			1,
			0,
			1,
			0,
			0
		};

		private static readonly int[] _defaultCWState = new int[12]
		{
			300,
			1000,
			3,
			50,
			0,
			1000,
			1,
			1,
			0,
			1,
			0,
			0
		};

		private static readonly int[] _defaultRAWState = new int[12]
		{
			10000,
			1000,
			3,
			50,
			0,
			1000,
			1,
			4,
			1,
			0,
			0,
			0
		};

		private const long DefaultFrontEndFrequency = 103000000L;

		private const int DefaultFrontEndSpectrumWidth = 250000;

		private const float DefaultUsableSpectrumRatio = 0.9f;

		private const double DefaultSoundCardSampleRate = 48000.0;

		private const int FFTFrameQueueLength = 3;

		private const int SpectrumAnalyzerInterval = 20;

		private const int MaxLockTime = 300000;

		private WindowType _fftWindowType;

		private IFrontendController _frontendController;

		private readonly Dictionary<string, IFrontendController> _frontendControllers = new Dictionary<string, IFrontendController>();

		private readonly List<IFrontendController> _builtinControllers = new List<IFrontendController>();

		private readonly IQCorrectionProcessor _iqBalancerProcessor = new IQCorrectionProcessor();

		private readonly Vfo _vfo;

		private readonly HookManager _hookManager;

		private readonly StreamControl _streamControl;

		private readonly ComplexFifoStream _fftStream = new ComplexFifoStream(BlockMode.BlockingRead);

		private readonly SharpEvent _fftEvent = new SharpEvent(false);

		private readonly ReaderWriterLock _fftResolutionLock = new ReaderWriterLock();

		private FloatCircularBuffer _fftFrames;

		private UnsafeBuffer _iqBuffer;

		private unsafe Complex* _iqPtr;

		private UnsafeBuffer _fftBuffer;

		private unsafe Complex* _fftPtr;

		private UnsafeBuffer _fftWindow;

		private unsafe float* _fftWindowPtr;

		private UnsafeBuffer _fftSpectrum;

		private unsafe float* _fftSpectrumPtr;

		private unsafe float* _fftDisplayPtr;

		private int _fftDisplaySize;

		private UnsafeBuffer _scaledFFTSpectrum;

		private unsafe byte* _scaledFFTSpectrumPtr;

		private System.Windows.Forms.Timer _waterfallTimer;

		private System.Windows.Forms.Timer _spectrumAnalyzerTimer;

		private long _centerFrequency;

		private long _frequencySet;

		private long _frequencyShift;

		private int _fftFramesCount;

		private int _fftcorrectionFPS;

		private float _fftAverageFPS;

		private DateTime _lastFFTTick;

		private int _inputBufferLength;

		private int _fftBins;

		private int _stepSize;

		private int _usableSpectrumWidth;

		private volatile bool _fftIsRunning;

		private bool _changingStickySpot;

		private bool _changingCenterSpot;

		private bool _changingSampleRate;

		private bool _configuringSnap;

		private bool _configuringSquelch;

		private bool _terminated;

		private string _waveFile;

		private Point _lastLocation;

		private Size _lastSize;

		private string _lastSourceName;

		private bool _initializing;

		private int _oldTopSplitterPosition = 200;

		private int _oldBottomSplitterPosition = 200;

		private int _oldLeftSplitterPosition = 200;

		private int _oldRightSplitterPosition = 200;

		private int _sourcePanelHeight;

		private int _ifOffset;

		private float _usableSpectrumRatio = 0.9f;

		private TuningStyle _tuningStyle;

		private bool _tuningStyleFreezed;

		private float _tuningLimit = (float)Math.Min(0.5, Utils.GetDoubleSetting("tuningLimit", 0.4));

		private readonly ToolTip _tooltip = new ToolTip();

		private readonly float _fftOffset = (float)Utils.GetDoubleSetting("fftOffset", -40.0);

		private readonly int _minOutputSampleRate = Utils.GetIntSetting("minOutputSampleRate", 24000);

		private readonly Dictionary<string, ISharpPlugin> _sharpPlugins = new Dictionary<string, ISharpPlugin>();

		private readonly Dictionary<DetectorType, int[]> _modeStates = new Dictionary<DetectorType, int[]>();

		private SharpControlProxy _sharpControlProxy;

		public DetectorType DetectorType
		{
			get
			{
				return this._vfo.DetectorType;
			}
			set
			{
				switch (value)
				{
				case DetectorType.AM:
					this.amRadioButton.Checked = true;
					break;
				case DetectorType.CW:
					this.cwRadioButton.Checked = true;
					break;
				case DetectorType.DSB:
					this.dsbRadioButton.Checked = true;
					break;
				case DetectorType.LSB:
					this.lsbRadioButton.Checked = true;
					break;
				case DetectorType.USB:
					this.usbRadioButton.Checked = true;
					break;
				case DetectorType.NFM:
					this.nfmRadioButton.Checked = true;
					break;
				case DetectorType.WFM:
					this.wfmRadioButton.Checked = true;
					break;
				case DetectorType.RAW:
					this.rawRadioButton.Checked = true;
					break;
				}
			}
		}

		public WindowType FilterType
		{
			get
			{
				return (WindowType)(this.filterTypeComboBox.SelectedIndex + 1);
			}
			set
			{
				this.filterTypeComboBox.SelectedIndex = (int)(value - 1);
			}
		}

		public bool IsPlaying
		{
			get
			{
				return this._streamControl.IsPlaying;
			}
		}

		public long Frequency
		{
			get
			{
				return this.vfoFrequencyEdit.Frequency;
			}
			set
			{
				this.SetFrequency(value, false);
			}
		}

		public long CenterFrequency
		{
			get
			{
				return this._centerFrequency;
			}
			set
			{
				if (this._frontendController == null)
				{
					throw new ApplicationException("Cannot set the center frequency when no front end is connected");
				}
				this.SetCenterFrequency(value);
			}
		}

		public long FrequencyShift
		{
			get
			{
				return (long)this.frequencyShiftNumericUpDown.Value;
			}
			set
			{
				this.frequencyShiftNumericUpDown.Value = value;
			}
		}

		public bool FrequencyShiftEnabled
		{
			get
			{
				return this.frequencyShiftCheckBox.Checked;
			}
			set
			{
				this.frequencyShiftCheckBox.Checked = value;
			}
		}

		public int FilterBandwidth
		{
			get
			{
				return (int)this.filterBandwidthNumericUpDown.Value;
			}
			set
			{
				if ((decimal)value <= this.filterBandwidthNumericUpDown.Maximum)
				{
					this.filterBandwidthNumericUpDown.Value = value;
				}
				else
				{
					this.filterBandwidthNumericUpDown.Value = this.filterBandwidthNumericUpDown.Maximum;
				}
			}
		}

		public int FilterOrder
		{
			get
			{
				return (int)this.filterOrderNumericUpDown.Value;
			}
			set
			{
				this.filterOrderNumericUpDown.Value = value;
			}
		}

		public bool SquelchEnabled
		{
			get
			{
				return this.useSquelchCheckBox.Checked;
			}
			set
			{
				this.useSquelchCheckBox.Checked = value;
			}
		}

		public int SquelchThreshold
		{
			get
			{
				return (int)this.squelchNumericUpDown.Value;
			}
			set
			{
				this.squelchNumericUpDown.Value = value;
			}
		}

		public int CWShift
		{
			get
			{
				return (int)this.cwShiftNumericUpDown.Value;
			}
			set
			{
				this.cwShiftNumericUpDown.Value = value;
			}
		}

		public bool SnapToGrid
		{
			get
			{
				return this.snapFrequencyCheckBox.Checked;
			}
			set
			{
				this.snapFrequencyCheckBox.Checked = value;
			}
		}

		public bool SwapIq
		{
			get
			{
				return this.swapIQCheckBox.Checked;
			}
			set
			{
				this.swapIQCheckBox.Checked = value;
			}
		}

		public bool FmStereo
		{
			get
			{
				return this.fmStereoCheckBox.Checked;
			}
			set
			{
				this.fmStereoCheckBox.Checked = value;
			}
		}

		public bool MarkPeaks
		{
			get
			{
				return this.markPeaksCheckBox.Checked;
			}
			set
			{
				this.markPeaksCheckBox.Checked = value;
			}
		}

		public int AudioGain
		{
			get
			{
				return this.audioGainTrackBar.Value;
			}
			set
			{
				this.audioGainTrackBar.Value = value;
			}
		}

		public bool AudioIsMuted
		{
			get
			{
				return this._vfo.Muted;
			}
			set
			{
				this._vfo.Muted = value;
				this.UpdateMuteButton();
			}
		}

		public string SourceName
		{
			get
			{
				if (this.SourceIsWaveFile)
				{
					if (File.Exists(this._waveFile))
					{
						Uri uri = new Uri(this._waveFile);
						return uri.AbsolutePath;
					}
				}
				else
				{
					if (this.SourceIsSoundCard)
					{
						return "Sound Card";
					}
					if (this.SourceIsFrontEnd)
					{
						if (this._frontendController == null)
						{
							return string.Empty;
						}
						return this._frontendController.GetType().AssemblyQualifiedName;
					}
				}
				return string.Empty;
			}
		}

		public Type SourceType
		{
			get
			{
				if (this.SourceIsFrontEnd)
				{
					if (this._frontendController == null)
					{
						return null;
					}
					return this._frontendController.GetType();
				}
				return null;
			}
		}

		public object Source
		{
			get
			{
				if (this.SourceIsFrontEnd)
				{
					if (this._frontendController == null)
					{
						return null;
					}
					return this._frontendController;
				}
				return null;
			}
		}

		public bool FilterAudio
		{
			get
			{
				return this.filterAudioCheckBox.Checked;
			}
			set
			{
				this.filterAudioCheckBox.Checked = value;
			}
		}

		public bool UnityGain
		{
			get
			{
				return this.unityGainCheckBox.Checked;
			}
			set
			{
				this.unityGainCheckBox.Checked = value;
			}
		}

		public bool UseAgc
		{
			get
			{
				return this.agcCheckBox.Checked;
			}
			set
			{
				this.agcCheckBox.Checked = value;
			}
		}

		public bool AgcHang
		{
			get
			{
				return this.agcUseHangCheckBox.Checked;
			}
			set
			{
				this.agcUseHangCheckBox.Checked = value;
			}
		}

		public int AgcThreshold
		{
			get
			{
				return (int)this.agcThresholdNumericUpDown.Value;
			}
			set
			{
				this.agcThresholdNumericUpDown.Value = value;
			}
		}

		public int AgcDecay
		{
			get
			{
				return (int)this.agcDecayNumericUpDown.Value;
			}
			set
			{
				this.agcDecayNumericUpDown.Value = value;
			}
		}

		public int AgcSlope
		{
			get
			{
				return (int)this.agcSlopeNumericUpDown.Value;
			}
			set
			{
				this.agcSlopeNumericUpDown.Value = value;
			}
		}

		public float SAttack
		{
			get
			{
				return (float)this.sAttackTrackBar.Value / (float)this.sAttackTrackBar.Maximum;
			}
			set
			{
				this.sAttackTrackBar.Value = (int)(value * (float)this.sAttackTrackBar.Maximum);
			}
		}

		public float SDecay
		{
			get
			{
				return (float)this.sDecayTrackBar.Value / (float)this.sDecayTrackBar.Maximum;
			}
			set
			{
				this.sDecayTrackBar.Value = (int)(value * (float)this.sDecayTrackBar.Maximum);
			}
		}

		public float WAttack
		{
			get
			{
				return (float)this.wAttackTrackBar.Value / (float)this.wAttackTrackBar.Maximum;
			}
			set
			{
				this.wAttackTrackBar.Value = (int)(value * (float)this.wAttackTrackBar.Maximum);
			}
		}

		public float WDecay
		{
			get
			{
				return (float)this.wDecayTrackBar.Value / (float)this.wDecayTrackBar.Maximum;
			}
			set
			{
				this.wDecayTrackBar.Value = (int)(value * (float)this.wDecayTrackBar.Maximum);
			}
		}

		public int Zoom
		{
			get
			{
				return this.fftZoomTrackBar.Value;
			}
			set
			{
				this.fftZoomTrackBar.Value = value;
			}
		}

		public bool UseTimeMarkers
		{
			get
			{
				return this.useTimestampsCheckBox.Checked;
			}
			set
			{
				this.useTimestampsCheckBox.Checked = value;
			}
		}

		public string RdsProgramService
		{
			get
			{
				return this._vfo.RdsStationName;
			}
		}

		public string RdsRadioText
		{
			get
			{
				return this._vfo.RdsStationText;
			}
		}

		public ushort RdsPICode
		{
			get
			{
				return this._vfo.RdsPICode;
			}
		}

		public bool RdsUseFEC
		{
			get
			{
				return this._vfo.RdsUseFEC;
			}
			set
			{
				this._vfo.RdsUseFEC = value;
			}
		}

		public int RFBandwidth
		{
			get
			{
				return (int)this._vfo.SampleRate;
			}
		}

		public int RFDisplayBandwidth
		{
			get
			{
				return this._usableSpectrumWidth;
			}
		}

		public bool SourceIsWaveFile
		{
			get
			{
				return this.iqSourceComboBox.SelectedIndex == this.iqSourceComboBox.Items.Count - 2;
			}
		}

		public bool SourceIsSoundCard
		{
			get
			{
				return this.iqSourceComboBox.SelectedIndex == this.iqSourceComboBox.Items.Count - 1;
			}
		}

		public bool SourceIsFrontEnd
		{
			get
			{
				if (!this.SourceIsSoundCard)
				{
					return !this.SourceIsWaveFile;
				}
				return false;
			}
		}

		public bool SourceIsTunable
		{
			get
			{
				if (!this.SourceIsSoundCard && !this.SourceIsWaveFile)
				{
					if (this._frontendController == null)
					{
						return false;
					}
					if (this._frontendController is ITunableSource)
					{
						return ((ITunableSource)this._frontendController).CanTune;
					}
					return false;
				}
				return false;
			}
		}

		public bool IsSquelchOpen
		{
			get
			{
				return this._vfo.IsSquelchOpen;
			}
		}

		public int FFTResolution
		{
			get
			{
				return this._fftBins;
			}
		}

		public float FFTRange
		{
			get
			{
				return (float)this.spectrumAnalyzer.DisplayRange;
			}
		}

		public float FFTOffset
		{
			get
			{
				return (float)this.spectrumAnalyzer.DisplayOffset;
			}
		}

		public int FFTContrast
		{
			get
			{
				return this.spectrumAnalyzer.Contrast;
			}
		}

		public ColorBlend Gradient
		{
			get
			{
				return this.spectrumAnalyzer.VerticalLinesGradient;
			}
		}

		public SpectrumStyle FFTSpectrumStyle
		{
			get
			{
				return this.spectrumAnalyzer.SpectrumStyle;
			}
		}

		public int StepSize
		{
			get
			{
				return this._stepSize;
			}
			set
			{
				if (this.SetStepSize(value))
				{
					this._stepSize = value;
				}
			}
		}

		public bool BypassDemodulation
		{
			get
			{
				return this._vfo.BypassDemodulation;
			}
			set
			{
				this._vfo.BypassDemodulation = value;
			}
		}

		public int TunableBandwidth
		{
			get
			{
				return (int)Math.Ceiling((double)((float)this._usableSpectrumWidth * this._tuningLimit * 2f));
			}
		}

		public TuningStyle TuningStyle
		{
			get
			{
				return this._tuningStyle;
			}
			set
			{
				if (!this._tuningStyleFreezed)
				{
					this._tuningStyle = value;
				}
				this.UpdateTuningStyle();
			}
		}

		public int IFOffset
		{
			get
			{
				return this._ifOffset;
			}
		}

		public float TuningLimit
		{
			get
			{
				return this._tuningLimit;
			}
			set
			{
				if (value != this._tuningLimit)
				{
					this._tuningLimit = value;
					this._tuningLimit = Math.Min(0.5f, this._tuningLimit);
					this._tuningLimit = Math.Max(0.1f, this._tuningLimit);
					this.UpdateTunableBandwidth();
				}
			}
		}

		public float VisualSNR
		{
			get
			{
				return this.spectrumAnalyzer.VisualSNR;
			}
		}

		public bool TuningStyleFreezed
		{
			get
			{
				return this._tuningStyleFreezed;
			}
			set
			{
				this._tuningStyleFreezed = value;
				this.tuningStyleButton.Enabled = !this._tuningStyleFreezed;
			}
		}

		public bool UseFFTSource
		{
			get
			{
				if (this._frontendController is IFFTSource)
				{
					return (this._frontendController as IFFTSource).FFTEnabled;
				}
				return false;
			}
		}

		public double AudioSampleRate
		{
			get
			{
				return this._streamControl.AudioSampleRate;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event CustomPaintEventHandler WaterfallCustomPaint;

		public event CustomPaintEventHandler SpectrumAnalyzerCustomPaint;

		public event CustomPaintEventHandler SpectrumAnalyzerBackgroundCustomPaint;

		private void InitializeComponent()
		{
			this.components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MainForm));
			this.openDlg = new OpenFileDialog();
			this.iqTimer = new System.Windows.Forms.Timer(this.components);
			this.fftContrastTrackBar = new TrackBar();
			this.fftZoomTrackBar = new TrackBar();
			this.label19 = new Label();
			this.label20 = new Label();
			this.controlPanel = new Panel();
			this.sourceCollapsiblePanel = new SDRSharp.CollapsiblePanel.CollapsiblePanel();
			this.sourceTableLayoutPanel = new TableLayoutPanel();
			this.iqSourceComboBox = new ComboBox();
			this.radioCollapsiblePanel = new SDRSharp.CollapsiblePanel.CollapsiblePanel();
			this.radioTableLayoutPanel = new TableLayoutPanel();
			this.swapIQCheckBox = new CheckBox();
			this.frequencyShiftCheckBox = new CheckBox();
			this.tableLayoutPanel7 = new TableLayoutPanel();
			this.amRadioButton = new RadioButton();
			this.nfmRadioButton = new RadioButton();
			this.lsbRadioButton = new RadioButton();
			this.usbRadioButton = new RadioButton();
			this.wfmRadioButton = new RadioButton();
			this.dsbRadioButton = new RadioButton();
			this.cwRadioButton = new RadioButton();
			this.rawRadioButton = new RadioButton();
			this.useSquelchCheckBox = new CheckBox();
			this.stepSizeComboBox = new ComboBox();
			this.cwShiftNumericUpDown = new NumericUpDown();
			this.label18 = new Label();
			this.label15 = new Label();
			this.squelchNumericUpDown = new NumericUpDown();
			this.filterBandwidthNumericUpDown = new NumericUpDown();
			this.label1 = new Label();
			this.label5 = new Label();
			this.filterOrderNumericUpDown = new NumericUpDown();
			this.frequencyShiftNumericUpDown = new NumericUpDown();
			this.filterTypeComboBox = new ComboBox();
			this.label16 = new Label();
			this.correctIQCheckBox = new CheckBox();
			this.snapFrequencyCheckBox = new CheckBox();
			this.lockCarrierCheckBox = new CheckBox();
			this.fmStereoCheckBox = new CheckBox();
			this.useAntiFadingCheckBox = new CheckBox();
			this.audioCollapsiblePanel = new SDRSharp.CollapsiblePanel.CollapsiblePanel();
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.label13 = new Label();
			this.sampleRateComboBox = new ComboBox();
			this.label11 = new Label();
			this.inputDeviceComboBox = new ComboBox();
			this.label12 = new Label();
			this.outputDeviceComboBox = new ComboBox();
			this.latencyNumericUpDown = new NumericUpDown();
			this.label6 = new Label();
			this.unityGainCheckBox = new CheckBox();
			this.filterAudioCheckBox = new CheckBox();
			this.agcCollapsiblePanel = new SDRSharp.CollapsiblePanel.CollapsiblePanel();
			this.tableLayoutPanel2 = new TableLayoutPanel();
			this.agcCheckBox = new CheckBox();
			this.agcSlopeNumericUpDown = new NumericUpDown();
			this.agcUseHangCheckBox = new CheckBox();
			this.label22 = new Label();
			this.label4 = new Label();
			this.agcDecayNumericUpDown = new NumericUpDown();
			this.label10 = new Label();
			this.agcThresholdNumericUpDown = new NumericUpDown();
			this.fftCollapsiblePanel = new SDRSharp.CollapsiblePanel.CollapsiblePanel();
			this.tableLayoutPanel3 = new TableLayoutPanel();
			this.label7 = new Label();
			this.viewComboBox = new ComboBox();
			this.label8 = new Label();
			this.fftWindowComboBox = new ComboBox();
			this.label21 = new Label();
			this.fftResolutionComboBox = new ComboBox();
			this.groupBox1 = new GroupBox();
			this.tableLayoutPanel5 = new TableLayoutPanel();
			this.label28 = new Label();
			this.fftSpeedTrackBar = new TrackBar();
			this.smoothingGroupBox = new GroupBox();
			this.tableLayoutPanel4 = new TableLayoutPanel();
			this.label23 = new Label();
			this.wDecayTrackBar = new TrackBar();
			this.wAttackTrackBar = new TrackBar();
			this.label25 = new Label();
			this.sDecayTrackBar = new TrackBar();
			this.sAttackTrackBar = new TrackBar();
			this.label24 = new Label();
			this.label26 = new Label();
			this.markPeaksCheckBox = new CheckBox();
			this.useTimestampsCheckBox = new CheckBox();
			this.label14 = new Label();
			this.gradientButton = new Button();
			this.label3 = new Label();
			this.spectrumStyleComboBox = new ComboBox();
			this.fftOffsetTrackBar = new TrackBar();
			this.fftRangeTrackBar = new TrackBar();
			this.audioGainTrackBar = new TrackBar();
			this.label17 = new Label();
			this.scrollPanel = new Panel();
			this.rightTableLayoutPanel = new TableLayoutPanel();
			this.label2 = new Label();
			this.centerPanel = new Panel();
			this.spectrumPanel = new Panel();
			this.spectrumSplitter = new Splitter();
			this.waterfall = new Waterfall();
			this.spectrumAnalyzer = new SpectrumAnalyzer();
			this.bottomSplitter = new Splitter();
			this.bottomPluginPanel = new TableLayoutPanel();
			this.topSplitter = new Splitter();
			this.topPluginPanel = new TableLayoutPanel();
			this.rightSplitter = new Splitter();
			this.leftSplitter = new Splitter();
			this.leftPluginPanel = new TableLayoutPanel();
			this.rightPluginPanel = new TableLayoutPanel();
			this.settingsTableLayoutPanel = new TableLayoutPanel();
			this.playStopButton = new Button();
			this.configureSourceButton = new Button();
			this.toggleMenuButton = new Button();
			this.muteButton = new Button();
			this.vfoFrequencyEdit = new SDRSharp.FrequencyEdit.FrequencyEdit();
			this.tuningStyleButton = new Button();
			this.logoPictureBox = new PictureBox();
			this.menuSpacerPanel = new Panel();
			((ISupportInitialize)this.fftContrastTrackBar).BeginInit();
			((ISupportInitialize)this.fftZoomTrackBar).BeginInit();
			this.controlPanel.SuspendLayout();
			this.sourceCollapsiblePanel.Content.SuspendLayout();
			this.sourceCollapsiblePanel.SuspendLayout();
			this.sourceTableLayoutPanel.SuspendLayout();
			this.radioCollapsiblePanel.Content.SuspendLayout();
			this.radioCollapsiblePanel.SuspendLayout();
			this.radioTableLayoutPanel.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			((ISupportInitialize)this.cwShiftNumericUpDown).BeginInit();
			((ISupportInitialize)this.squelchNumericUpDown).BeginInit();
			((ISupportInitialize)this.filterBandwidthNumericUpDown).BeginInit();
			((ISupportInitialize)this.filterOrderNumericUpDown).BeginInit();
			((ISupportInitialize)this.frequencyShiftNumericUpDown).BeginInit();
			this.audioCollapsiblePanel.Content.SuspendLayout();
			this.audioCollapsiblePanel.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((ISupportInitialize)this.latencyNumericUpDown).BeginInit();
			this.agcCollapsiblePanel.Content.SuspendLayout();
			this.agcCollapsiblePanel.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((ISupportInitialize)this.agcSlopeNumericUpDown).BeginInit();
			((ISupportInitialize)this.agcDecayNumericUpDown).BeginInit();
			((ISupportInitialize)this.agcThresholdNumericUpDown).BeginInit();
			this.fftCollapsiblePanel.Content.SuspendLayout();
			this.fftCollapsiblePanel.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			((ISupportInitialize)this.fftSpeedTrackBar).BeginInit();
			this.smoothingGroupBox.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			((ISupportInitialize)this.wDecayTrackBar).BeginInit();
			((ISupportInitialize)this.wAttackTrackBar).BeginInit();
			((ISupportInitialize)this.sDecayTrackBar).BeginInit();
			((ISupportInitialize)this.sAttackTrackBar).BeginInit();
			((ISupportInitialize)this.fftOffsetTrackBar).BeginInit();
			((ISupportInitialize)this.fftRangeTrackBar).BeginInit();
			((ISupportInitialize)this.audioGainTrackBar).BeginInit();
			this.scrollPanel.SuspendLayout();
			this.rightTableLayoutPanel.SuspendLayout();
			this.centerPanel.SuspendLayout();
			this.spectrumPanel.SuspendLayout();
			this.settingsTableLayoutPanel.SuspendLayout();
			((ISupportInitialize)this.logoPictureBox).BeginInit();
			base.SuspendLayout();
			this.openDlg.DefaultExt = "wav";
			this.openDlg.Filter = "WAV files|*.wav";
			this.iqTimer.Enabled = true;
			this.iqTimer.Tick += this.iqTimer_Tick;
			this.fftContrastTrackBar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
			this.fftContrastTrackBar.Location = new Point(3, 186);
			this.fftContrastTrackBar.Maximum = 24;
			this.fftContrastTrackBar.Minimum = -24;
			this.fftContrastTrackBar.Name = "fftContrastTrackBar";
			this.fftContrastTrackBar.Orientation = Orientation.Vertical;
			this.fftContrastTrackBar.RightToLeftLayout = true;
			this.fftContrastTrackBar.Size = new Size(45, 151);
			this.fftContrastTrackBar.TabIndex = 1;
			this.fftContrastTrackBar.TickFrequency = 6;
			this.fftContrastTrackBar.TickStyle = TickStyle.Both;
			this.fftContrastTrackBar.ValueChanged += this.fftContrastTrackBar_Changed;
			this.fftZoomTrackBar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
			this.fftZoomTrackBar.Location = new Point(3, 16);
			this.fftZoomTrackBar.Maximum = 50;
			this.fftZoomTrackBar.Name = "fftZoomTrackBar";
			this.fftZoomTrackBar.Orientation = Orientation.Vertical;
			this.fftZoomTrackBar.RightToLeftLayout = true;
			this.fftZoomTrackBar.Size = new Size(45, 151);
			this.fftZoomTrackBar.TabIndex = 0;
			this.fftZoomTrackBar.TickFrequency = 5;
			this.fftZoomTrackBar.TickStyle = TickStyle.Both;
			this.fftZoomTrackBar.ValueChanged += this.fftZoomTrackBar_ValueChanged;
			this.label19.Anchor = AnchorStyles.None;
			this.label19.AutoSize = true;
			this.label19.Location = new Point(9, 0);
			this.label19.Name = "label19";
			this.label19.Size = new Size(34, 13);
			this.label19.TabIndex = 19;
			this.label19.Text = "Zoom";
			this.label19.TextAlign = ContentAlignment.MiddleCenter;
			this.label20.Anchor = AnchorStyles.None;
			this.label20.AutoSize = true;
			this.label20.Location = new Point(3, 170);
			this.label20.Name = "label20";
			this.label20.Size = new Size(46, 13);
			this.label20.TabIndex = 20;
			this.label20.Text = "Contrast";
			this.label20.TextAlign = ContentAlignment.MiddleCenter;
			this.controlPanel.AutoSize = true;
			this.controlPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.controlPanel.Controls.Add(this.sourceCollapsiblePanel);
			this.controlPanel.Controls.Add(this.radioCollapsiblePanel);
			this.controlPanel.Controls.Add(this.audioCollapsiblePanel);
			this.controlPanel.Controls.Add(this.fftCollapsiblePanel);
			this.controlPanel.Controls.Add(this.agcCollapsiblePanel);
			this.controlPanel.Location = new Point(0, 0);
			this.controlPanel.Margin = new Padding(0);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new Size(227, 1218);
			this.controlPanel.TabIndex = 25;
			this.sourceCollapsiblePanel.AutoHeight = true;
			this.sourceCollapsiblePanel.Content.Controls.Add(this.sourceTableLayoutPanel);
			this.sourceCollapsiblePanel.Content.Location = new Point(0, 24);
			this.sourceCollapsiblePanel.Content.Margin = new Padding(2);
			this.sourceCollapsiblePanel.Content.Name = "Content";
			this.sourceCollapsiblePanel.Content.Size = new Size(224, 28);
			this.sourceCollapsiblePanel.Content.TabIndex = 1;
			this.sourceCollapsiblePanel.Location = new Point(0, 0);
			this.sourceCollapsiblePanel.Margin = new Padding(0);
			this.sourceCollapsiblePanel.Name = "sourceCollapsiblePanel";
			this.sourceCollapsiblePanel.NextPanel = this.radioCollapsiblePanel;
			this.sourceCollapsiblePanel.PanelTitle = "Source";
			this.sourceCollapsiblePanel.Size = new Size(224, 52);
			this.sourceCollapsiblePanel.TabIndex = 4;
			this.sourceTableLayoutPanel.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
			this.sourceTableLayoutPanel.AutoSize = true;
			this.sourceTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.sourceTableLayoutPanel.ColumnCount = 1;
			this.sourceTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.sourceTableLayoutPanel.Controls.Add(this.iqSourceComboBox, 0, 0);
			this.sourceTableLayoutPanel.Location = new Point(0, 0);
			this.sourceTableLayoutPanel.Margin = new Padding(0);
			this.sourceTableLayoutPanel.Name = "sourceTableLayoutPanel";
			this.sourceTableLayoutPanel.RowCount = 2;
			this.sourceTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.sourceTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.sourceTableLayoutPanel.Size = new Size(224, 27);
			this.sourceTableLayoutPanel.TabIndex = 0;
			this.iqSourceComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.iqSourceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.iqSourceComboBox.DropDownWidth = 250;
			this.iqSourceComboBox.FormattingEnabled = true;
			this.iqSourceComboBox.Location = new Point(3, 3);
			this.iqSourceComboBox.Name = "iqSourceComboBox";
			this.iqSourceComboBox.Size = new Size(218, 21);
			this.iqSourceComboBox.TabIndex = 1;
			this.iqSourceComboBox.SelectedIndexChanged += this.iqSourceComboBox_SelectedIndexChanged;
			this.radioCollapsiblePanel.AutoHeight = false;
			this.radioCollapsiblePanel.Content.Controls.Add(this.radioTableLayoutPanel);
			this.radioCollapsiblePanel.Content.Location = new Point(0, 24);
			this.radioCollapsiblePanel.Content.Name = "Content";
			this.radioCollapsiblePanel.Content.Size = new Size(224, 341);
			this.radioCollapsiblePanel.Content.TabIndex = 1;
			this.radioCollapsiblePanel.Location = new Point(0, 52);
			this.radioCollapsiblePanel.Margin = new Padding(0);
			this.radioCollapsiblePanel.Name = "radioCollapsiblePanel";
			this.radioCollapsiblePanel.NextPanel = this.audioCollapsiblePanel;
			this.radioCollapsiblePanel.PanelTitle = "Radio";
			this.radioCollapsiblePanel.Size = new Size(224, 365);
			this.radioCollapsiblePanel.TabIndex = 0;
			this.radioTableLayoutPanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.ColumnCount = 4;
			this.radioTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.radioTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.radioTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.radioTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.radioTableLayoutPanel.Controls.Add(this.swapIQCheckBox, 2, 10);
			this.radioTableLayoutPanel.Controls.Add(this.frequencyShiftCheckBox, 0, 1);
			this.radioTableLayoutPanel.Controls.Add(this.tableLayoutPanel7, 0, 0);
			this.radioTableLayoutPanel.Controls.Add(this.useSquelchCheckBox, 0, 5);
			this.radioTableLayoutPanel.Controls.Add(this.stepSizeComboBox, 2, 8);
			this.radioTableLayoutPanel.Controls.Add(this.cwShiftNumericUpDown, 2, 6);
			this.radioTableLayoutPanel.Controls.Add(this.label18, 2, 7);
			this.radioTableLayoutPanel.Controls.Add(this.label15, 2, 5);
			this.radioTableLayoutPanel.Controls.Add(this.squelchNumericUpDown, 0, 6);
			this.radioTableLayoutPanel.Controls.Add(this.filterBandwidthNumericUpDown, 0, 4);
			this.radioTableLayoutPanel.Controls.Add(this.label1, 0, 3);
			this.radioTableLayoutPanel.Controls.Add(this.label5, 2, 3);
			this.radioTableLayoutPanel.Controls.Add(this.filterOrderNumericUpDown, 2, 4);
			this.radioTableLayoutPanel.Controls.Add(this.frequencyShiftNumericUpDown, 1, 1);
			this.radioTableLayoutPanel.Controls.Add(this.filterTypeComboBox, 1, 2);
			this.radioTableLayoutPanel.Controls.Add(this.label16, 0, 2);
			this.radioTableLayoutPanel.Controls.Add(this.correctIQCheckBox, 2, 9);
			this.radioTableLayoutPanel.Controls.Add(this.snapFrequencyCheckBox, 0, 8);
			this.radioTableLayoutPanel.Controls.Add(this.lockCarrierCheckBox, 0, 9);
			this.radioTableLayoutPanel.Controls.Add(this.fmStereoCheckBox, 0, 7);
			this.radioTableLayoutPanel.Controls.Add(this.useAntiFadingCheckBox, 0, 10);
			this.radioTableLayoutPanel.Location = new Point(0, 0);
			this.radioTableLayoutPanel.Name = "radioTableLayoutPanel";
			this.radioTableLayoutPanel.RowCount = 11;
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.radioTableLayoutPanel.Size = new Size(224, 341);
			this.radioTableLayoutPanel.TabIndex = 34;
			this.swapIQCheckBox.Anchor = AnchorStyles.Right;
			this.swapIQCheckBox.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.swapIQCheckBox, 2);
			this.swapIQCheckBox.Location = new Point(143, 321);
			this.swapIQCheckBox.Name = "swapIQCheckBox";
			this.swapIQCheckBox.RightToLeft = RightToLeft.Yes;
			this.swapIQCheckBox.Size = new Size(79, 17);
			this.swapIQCheckBox.TabIndex = 25;
			this.swapIQCheckBox.Text = "Swap I && Q";
			this.swapIQCheckBox.UseVisualStyleBackColor = true;
			this.swapIQCheckBox.CheckedChanged += this.swapIQCheckBox_CheckedChanged;
			this.frequencyShiftCheckBox.Anchor = AnchorStyles.Left;
			this.frequencyShiftCheckBox.AutoSize = true;
			this.frequencyShiftCheckBox.Location = new Point(3, 106);
			this.frequencyShiftCheckBox.Name = "frequencyShiftCheckBox";
			this.frequencyShiftCheckBox.Size = new Size(47, 17);
			this.frequencyShiftCheckBox.TabIndex = 7;
			this.frequencyShiftCheckBox.Text = "Shift";
			this.frequencyShiftCheckBox.UseVisualStyleBackColor = true;
			this.frequencyShiftCheckBox.CheckedChanged += this.frequencyShiftCheckBox_CheckStateChanged;
			this.tableLayoutPanel7.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel7.ColumnCount = 4;
			this.radioTableLayoutPanel.SetColumnSpan(this.tableLayoutPanel7, 4);
			this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel7.Controls.Add(this.amRadioButton, 1, 0);
			this.tableLayoutPanel7.Controls.Add(this.nfmRadioButton, 0, 0);
			this.tableLayoutPanel7.Controls.Add(this.lsbRadioButton, 2, 0);
			this.tableLayoutPanel7.Controls.Add(this.usbRadioButton, 3, 0);
			this.tableLayoutPanel7.Controls.Add(this.wfmRadioButton, 0, 1);
			this.tableLayoutPanel7.Controls.Add(this.dsbRadioButton, 1, 1);
			this.tableLayoutPanel7.Controls.Add(this.cwRadioButton, 2, 1);
			this.tableLayoutPanel7.Controls.Add(this.rawRadioButton, 3, 1);
			this.tableLayoutPanel7.Location = new Point(3, 3);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 2;
			this.tableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
			this.tableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
			this.tableLayoutPanel7.Size = new Size(219, 94);
			this.tableLayoutPanel7.TabIndex = 32;
			this.amRadioButton.Anchor = AnchorStyles.Left;
			this.amRadioButton.AutoSize = true;
			this.amRadioButton.Location = new Point(60, 15);
			this.amRadioButton.Name = "amRadioButton";
			this.amRadioButton.Size = new Size(41, 17);
			this.amRadioButton.TabIndex = 1;
			this.amRadioButton.Text = "AM";
			this.amRadioButton.UseVisualStyleBackColor = true;
			this.amRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.nfmRadioButton.Anchor = AnchorStyles.Left;
			this.nfmRadioButton.AutoSize = true;
			this.nfmRadioButton.Location = new Point(3, 15);
			this.nfmRadioButton.Name = "nfmRadioButton";
			this.nfmRadioButton.Size = new Size(48, 17);
			this.nfmRadioButton.TabIndex = 0;
			this.nfmRadioButton.Text = "NFM";
			this.nfmRadioButton.UseVisualStyleBackColor = true;
			this.nfmRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.lsbRadioButton.Anchor = AnchorStyles.Left;
			this.lsbRadioButton.AutoSize = true;
			this.lsbRadioButton.Location = new Point(113, 15);
			this.lsbRadioButton.Name = "lsbRadioButton";
			this.lsbRadioButton.Size = new Size(45, 17);
			this.lsbRadioButton.TabIndex = 2;
			this.lsbRadioButton.Text = "LSB";
			this.lsbRadioButton.UseVisualStyleBackColor = true;
			this.lsbRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.usbRadioButton.Anchor = AnchorStyles.Left;
			this.usbRadioButton.AutoSize = true;
			this.usbRadioButton.Location = new Point(164, 15);
			this.usbRadioButton.Name = "usbRadioButton";
			this.usbRadioButton.Size = new Size(47, 17);
			this.usbRadioButton.TabIndex = 3;
			this.usbRadioButton.Text = "USB";
			this.usbRadioButton.UseVisualStyleBackColor = true;
			this.usbRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.wfmRadioButton.Anchor = AnchorStyles.Left;
			this.wfmRadioButton.AutoSize = true;
			this.wfmRadioButton.Location = new Point(3, 62);
			this.wfmRadioButton.Name = "wfmRadioButton";
			this.wfmRadioButton.Size = new Size(51, 17);
			this.wfmRadioButton.TabIndex = 4;
			this.wfmRadioButton.Text = "WFM";
			this.wfmRadioButton.UseVisualStyleBackColor = true;
			this.wfmRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.dsbRadioButton.Anchor = AnchorStyles.Left;
			this.dsbRadioButton.AutoSize = true;
			this.dsbRadioButton.Location = new Point(60, 62);
			this.dsbRadioButton.Name = "dsbRadioButton";
			this.dsbRadioButton.Size = new Size(47, 17);
			this.dsbRadioButton.TabIndex = 5;
			this.dsbRadioButton.Text = "DSB";
			this.dsbRadioButton.UseVisualStyleBackColor = true;
			this.dsbRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.cwRadioButton.Anchor = AnchorStyles.Left;
			this.cwRadioButton.AutoSize = true;
			this.cwRadioButton.Location = new Point(113, 62);
			this.cwRadioButton.Name = "cwRadioButton";
			this.cwRadioButton.Size = new Size(43, 17);
			this.cwRadioButton.TabIndex = 6;
			this.cwRadioButton.Text = "CW";
			this.cwRadioButton.UseVisualStyleBackColor = true;
			this.cwRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.rawRadioButton.Anchor = AnchorStyles.Left;
			this.rawRadioButton.AutoSize = true;
			this.rawRadioButton.Location = new Point(164, 62);
			this.rawRadioButton.Name = "rawRadioButton";
			this.rawRadioButton.Size = new Size(51, 17);
			this.rawRadioButton.TabIndex = 6;
			this.rawRadioButton.Text = "RAW";
			this.rawRadioButton.UseVisualStyleBackColor = true;
			this.rawRadioButton.CheckedChanged += this.modeRadioButton_CheckStateChanged;
			this.useSquelchCheckBox.Anchor = AnchorStyles.Left;
			this.useSquelchCheckBox.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.useSquelchCheckBox, 2);
			this.useSquelchCheckBox.Location = new Point(3, 199);
			this.useSquelchCheckBox.Name = "useSquelchCheckBox";
			this.useSquelchCheckBox.Size = new Size(65, 17);
			this.useSquelchCheckBox.TabIndex = 15;
			this.useSquelchCheckBox.Text = "Squelch";
			this.useSquelchCheckBox.UseVisualStyleBackColor = true;
			this.useSquelchCheckBox.CheckedChanged += this.useSquelchCheckBox_CheckedChanged;
			this.stepSizeComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.stepSizeComboBox, 2);
			this.stepSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.stepSizeComboBox.FormattingEnabled = true;
			this.stepSizeComboBox.Location = new Point(114, 271);
			this.stepSizeComboBox.Name = "stepSizeComboBox";
			this.stepSizeComboBox.Size = new Size(108, 21);
			this.stepSizeComboBox.TabIndex = 21;
			this.stepSizeComboBox.SelectedIndexChanged += this.stepSizeComboBox_SelectedIndexChanged;
			this.cwShiftNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.cwShiftNumericUpDown, 2);
			this.cwShiftNumericUpDown.Enabled = false;
			this.cwShiftNumericUpDown.Location = new Point(114, 222);
			this.cwShiftNumericUpDown.Maximum = new decimal(new int[4]
			{
				2000,
				0,
				0,
				0
			});
			this.cwShiftNumericUpDown.Minimum = new decimal(new int[4]
			{
				2000,
				0,
				0,
				-2147483648
			});
			this.cwShiftNumericUpDown.Name = "cwShiftNumericUpDown";
			this.cwShiftNumericUpDown.Size = new Size(108, 20);
			this.cwShiftNumericUpDown.TabIndex = 18;
			this.cwShiftNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.cwShiftNumericUpDown.ThousandsSeparator = true;
			this.cwShiftNumericUpDown.Value = new decimal(new int[4]
			{
				1000,
				0,
				0,
				0
			});
			this.cwShiftNumericUpDown.ValueChanged += this.cwShiftNumericUpDown_ValueChanged;
			this.label18.Anchor = AnchorStyles.Left;
			this.label18.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.label18, 2);
			this.label18.Location = new Point(114, 250);
			this.label18.Name = "label18";
			this.label18.Size = new Size(52, 13);
			this.label18.TabIndex = 19;
			this.label18.Text = "Step Size";
			this.label18.TextAlign = ContentAlignment.MiddleLeft;
			this.label15.Anchor = AnchorStyles.Left;
			this.label15.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.label15, 2);
			this.label15.Location = new Point(114, 201);
			this.label15.Name = "label15";
			this.label15.Size = new Size(49, 13);
			this.label15.TabIndex = 16;
			this.label15.Text = "CW Shift";
			this.label15.TextAlign = ContentAlignment.MiddleLeft;
			this.squelchNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.squelchNumericUpDown, 2);
			this.squelchNumericUpDown.Enabled = false;
			this.squelchNumericUpDown.Location = new Point(3, 222);
			this.squelchNumericUpDown.Name = "squelchNumericUpDown";
			this.squelchNumericUpDown.Size = new Size(105, 20);
			this.squelchNumericUpDown.TabIndex = 17;
			this.squelchNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.squelchNumericUpDown.ValueChanged += this.squelchNumericUpDown_ValueChanged;
			this.filterBandwidthNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.filterBandwidthNumericUpDown, 2);
			this.filterBandwidthNumericUpDown.Increment = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.filterBandwidthNumericUpDown.Location = new Point(3, 173);
			this.filterBandwidthNumericUpDown.Maximum = new decimal(new int[4]
			{
				250000,
				0,
				0,
				0
			});
			this.filterBandwidthNumericUpDown.Minimum = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.filterBandwidthNumericUpDown.Name = "filterBandwidthNumericUpDown";
			this.filterBandwidthNumericUpDown.Size = new Size(105, 20);
			this.filterBandwidthNumericUpDown.TabIndex = 13;
			this.filterBandwidthNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.filterBandwidthNumericUpDown.ThousandsSeparator = true;
			this.filterBandwidthNumericUpDown.Value = new decimal(new int[4]
			{
				10000,
				0,
				0,
				0
			});
			this.filterBandwidthNumericUpDown.ValueChanged += this.filterBandwidthNumericUpDown_ValueChanged;
			this.label1.Anchor = AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.label1, 2);
			this.label1.Location = new Point(3, 157);
			this.label1.Name = "label1";
			this.label1.Size = new Size(57, 13);
			this.label1.TabIndex = 11;
			this.label1.Text = "Bandwidth";
			this.label1.TextAlign = ContentAlignment.MiddleLeft;
			this.label5.Anchor = AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.label5, 2);
			this.label5.Location = new Point(114, 157);
			this.label5.Name = "label5";
			this.label5.Size = new Size(33, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Order";
			this.label5.TextAlign = ContentAlignment.MiddleLeft;
			this.filterOrderNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.filterOrderNumericUpDown, 2);
			this.filterOrderNumericUpDown.Increment = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.filterOrderNumericUpDown.Location = new Point(114, 173);
			this.filterOrderNumericUpDown.Maximum = new decimal(new int[4]
			{
				9999,
				0,
				0,
				0
			});
			this.filterOrderNumericUpDown.Minimum = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.filterOrderNumericUpDown.Name = "filterOrderNumericUpDown";
			this.filterOrderNumericUpDown.Size = new Size(108, 20);
			this.filterOrderNumericUpDown.TabIndex = 14;
			this.filterOrderNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.filterOrderNumericUpDown.ThousandsSeparator = true;
			this.filterOrderNumericUpDown.Value = new decimal(new int[4]
			{
				400,
				0,
				0,
				0
			});
			this.filterOrderNumericUpDown.ValueChanged += this.filterOrderNumericUpDown_ValueChanged;
			this.frequencyShiftNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.frequencyShiftNumericUpDown, 3);
			this.frequencyShiftNumericUpDown.Enabled = false;
			this.frequencyShiftNumericUpDown.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.frequencyShiftNumericUpDown.Increment = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.frequencyShiftNumericUpDown.Location = new Point(56, 103);
			this.frequencyShiftNumericUpDown.Maximum = new decimal(new int[4]
			{
				276447232,
				23283,
				0,
				0
			});
			this.frequencyShiftNumericUpDown.Minimum = new decimal(new int[4]
			{
				276447232,
				23283,
				0,
				-2147483648
			});
			this.frequencyShiftNumericUpDown.Name = "frequencyShiftNumericUpDown";
			this.frequencyShiftNumericUpDown.Size = new Size(166, 24);
			this.frequencyShiftNumericUpDown.TabIndex = 8;
			this.frequencyShiftNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.frequencyShiftNumericUpDown.ThousandsSeparator = true;
			this.frequencyShiftNumericUpDown.ValueChanged += this.frequencyShiftNumericUpDown_ValueChanged;
			this.filterTypeComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.radioTableLayoutPanel.SetColumnSpan(this.filterTypeComboBox, 3);
			this.filterTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.filterTypeComboBox.FormattingEnabled = true;
			this.filterTypeComboBox.Items.AddRange(new object[6]
			{
				"Hamming",
				"Blackman",
				"Blackman-Harris 4",
				"Blackman-Harris 7",
				"Hann-Poisson",
				"Youssef"
			});
			this.filterTypeComboBox.Location = new Point(56, 133);
			this.filterTypeComboBox.Name = "filterTypeComboBox";
			this.filterTypeComboBox.Size = new Size(166, 21);
			this.filterTypeComboBox.TabIndex = 10;
			this.filterTypeComboBox.SelectedIndexChanged += this.filterTypeComboBox_SelectedIndexChanged;
			this.label16.Anchor = AnchorStyles.Left;
			this.label16.AutoSize = true;
			this.label16.Location = new Point(3, 137);
			this.label16.Name = "label16";
			this.label16.Size = new Size(29, 13);
			this.label16.TabIndex = 9;
			this.label16.Text = "Filter";
			this.label16.TextAlign = ContentAlignment.MiddleLeft;
			this.correctIQCheckBox.Anchor = AnchorStyles.Right;
			this.correctIQCheckBox.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.correctIQCheckBox, 2);
			this.correctIQCheckBox.Location = new Point(148, 298);
			this.correctIQCheckBox.Name = "correctIQCheckBox";
			this.correctIQCheckBox.RightToLeft = RightToLeft.Yes;
			this.correctIQCheckBox.Size = new Size(74, 17);
			this.correctIQCheckBox.TabIndex = 22;
			this.correctIQCheckBox.Text = "Correct IQ";
			this.correctIQCheckBox.UseVisualStyleBackColor = true;
			this.correctIQCheckBox.CheckedChanged += this.autoCorrectIQCheckBox_CheckStateChanged;
			this.snapFrequencyCheckBox.Anchor = AnchorStyles.Right;
			this.snapFrequencyCheckBox.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.snapFrequencyCheckBox, 2);
			this.snapFrequencyCheckBox.Location = new Point(23, 273);
			this.snapFrequencyCheckBox.Name = "snapFrequencyCheckBox";
			this.snapFrequencyCheckBox.RightToLeft = RightToLeft.Yes;
			this.snapFrequencyCheckBox.Size = new Size(85, 17);
			this.snapFrequencyCheckBox.TabIndex = 20;
			this.snapFrequencyCheckBox.Text = "Snap to Grid";
			this.snapFrequencyCheckBox.UseVisualStyleBackColor = true;
			this.snapFrequencyCheckBox.CheckedChanged += this.stepSizeComboBox_SelectedIndexChanged;
			this.lockCarrierCheckBox.Anchor = AnchorStyles.Right;
			this.lockCarrierCheckBox.AutoSize = true;
			this.lockCarrierCheckBox.CheckAlign = ContentAlignment.MiddleRight;
			this.radioTableLayoutPanel.SetColumnSpan(this.lockCarrierCheckBox, 2);
			this.lockCarrierCheckBox.Location = new Point(25, 298);
			this.lockCarrierCheckBox.Name = "lockCarrierCheckBox";
			this.lockCarrierCheckBox.Size = new Size(83, 17);
			this.lockCarrierCheckBox.TabIndex = 33;
			this.lockCarrierCheckBox.Text = "Lock Carrier";
			this.lockCarrierCheckBox.UseVisualStyleBackColor = true;
			this.lockCarrierCheckBox.CheckedChanged += this.lockCarrierCheckBox_CheckedChanged;
			this.fmStereoCheckBox.Anchor = AnchorStyles.Right;
			this.fmStereoCheckBox.AutoSize = true;
			this.radioTableLayoutPanel.SetColumnSpan(this.fmStereoCheckBox, 2);
			this.fmStereoCheckBox.Enabled = false;
			this.fmStereoCheckBox.Location = new Point(33, 248);
			this.fmStereoCheckBox.Name = "fmStereoCheckBox";
			this.fmStereoCheckBox.RightToLeft = RightToLeft.Yes;
			this.fmStereoCheckBox.Size = new Size(75, 17);
			this.fmStereoCheckBox.TabIndex = 24;
			this.fmStereoCheckBox.Text = "FM Stereo";
			this.fmStereoCheckBox.UseVisualStyleBackColor = true;
			this.fmStereoCheckBox.CheckedChanged += this.fmStereoCheckBox_CheckedChanged;
			this.useAntiFadingCheckBox.Anchor = AnchorStyles.Right;
			this.useAntiFadingCheckBox.AutoSize = true;
			this.useAntiFadingCheckBox.CheckAlign = ContentAlignment.MiddleRight;
			this.radioTableLayoutPanel.SetColumnSpan(this.useAntiFadingCheckBox, 2);
			this.useAntiFadingCheckBox.Location = new Point(29, 321);
			this.useAntiFadingCheckBox.Name = "useAntiFadingCheckBox";
			this.useAntiFadingCheckBox.Size = new Size(79, 17);
			this.useAntiFadingCheckBox.TabIndex = 34;
			this.useAntiFadingCheckBox.Text = "Anti-Fading";
			this.useAntiFadingCheckBox.UseVisualStyleBackColor = true;
			this.useAntiFadingCheckBox.CheckedChanged += this.useAntiFadingCheckBox_CheckedChanged;
			this.audioCollapsiblePanel.AutoHeight = false;
			this.audioCollapsiblePanel.Content.Controls.Add(this.tableLayoutPanel1);
			this.audioCollapsiblePanel.Content.Location = new Point(0, 24);
			this.audioCollapsiblePanel.Content.Name = "Content";
			this.audioCollapsiblePanel.Content.Size = new Size(224, 136);
			this.audioCollapsiblePanel.Content.TabIndex = 1;
			this.audioCollapsiblePanel.Location = new Point(0, 417);
			this.audioCollapsiblePanel.Name = "audioCollapsiblePanel";
			this.audioCollapsiblePanel.NextPanel = this.agcCollapsiblePanel;
			this.audioCollapsiblePanel.PanelTitle = "Audio";
			this.audioCollapsiblePanel.Size = new Size(224, 160);
			this.audioCollapsiblePanel.TabIndex = 1;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
			this.tableLayoutPanel1.Controls.Add(this.label13, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.sampleRateComboBox, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label11, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.inputDeviceComboBox, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label12, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.outputDeviceComboBox, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.latencyNumericUpDown, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.unityGainCheckBox, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.filterAudioCheckBox, 1, 4);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.Size = new Size(224, 136);
			this.tableLayoutPanel1.TabIndex = 31;
			this.label13.Anchor = AnchorStyles.Left;
			this.label13.AutoSize = true;
			this.label13.Location = new Point(3, 10);
			this.label13.Name = "label13";
			this.label13.Size = new Size(60, 13);
			this.label13.TabIndex = 28;
			this.label13.Text = "Samplerate";
			this.sampleRateComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.sampleRateComboBox.FormattingEnabled = true;
			this.sampleRateComboBox.Items.AddRange(new object[14]
			{
				"8000 sample/sec",
				"11025 sample/sec",
				"16000 sample/sec",
				"22050 sample/sec",
				"24000 sample/sec",
				"32000 sample/sec",
				"44100 sample/sec",
				"48000 sample/sec",
				"80000 sample/sec",
				"96000 sample/sec",
				"120000 sample/sec",
				"125000 sample/sec",
				"150000 sample/sec",
				"192000 sample/sec"
			});
			this.sampleRateComboBox.Location = new Point(92, 6);
			this.sampleRateComboBox.Name = "sampleRateComboBox";
			this.sampleRateComboBox.Size = new Size(129, 21);
			this.sampleRateComboBox.TabIndex = 1;
			this.label11.Anchor = AnchorStyles.Left;
			this.label11.AutoSize = true;
			this.label11.Location = new Point(3, 40);
			this.label11.Name = "label11";
			this.label11.Size = new Size(31, 13);
			this.label11.TabIndex = 24;
			this.label11.Text = "Input";
			this.inputDeviceComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.inputDeviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.inputDeviceComboBox.DropDownWidth = 300;
			this.inputDeviceComboBox.FormattingEnabled = true;
			this.inputDeviceComboBox.Location = new Point(92, 36);
			this.inputDeviceComboBox.Name = "inputDeviceComboBox";
			this.inputDeviceComboBox.Size = new Size(129, 21);
			this.inputDeviceComboBox.TabIndex = 2;
			this.label12.Anchor = AnchorStyles.Left;
			this.label12.AutoSize = true;
			this.label12.Location = new Point(3, 67);
			this.label12.Name = "label12";
			this.label12.Size = new Size(39, 13);
			this.label12.TabIndex = 26;
			this.label12.Text = "Output";
			this.outputDeviceComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.outputDeviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.outputDeviceComboBox.DropDownWidth = 300;
			this.outputDeviceComboBox.FormattingEnabled = true;
			this.outputDeviceComboBox.Location = new Point(92, 63);
			this.outputDeviceComboBox.Name = "outputDeviceComboBox";
			this.outputDeviceComboBox.Size = new Size(129, 21);
			this.outputDeviceComboBox.TabIndex = 3;
			this.latencyNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.latencyNumericUpDown.Location = new Point(92, 90);
			this.latencyNumericUpDown.Maximum = new decimal(new int[4]
			{
				2000,
				0,
				0,
				0
			});
			this.latencyNumericUpDown.Minimum = new decimal(new int[4]
			{
				1,
				0,
				0,
				0
			});
			this.latencyNumericUpDown.Name = "latencyNumericUpDown";
			this.latencyNumericUpDown.Size = new Size(129, 20);
			this.latencyNumericUpDown.TabIndex = 4;
			this.latencyNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.latencyNumericUpDown.Value = new decimal(new int[4]
			{
				1,
				0,
				0,
				0
			});
			this.label6.Anchor = AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new Point(3, 93);
			this.label6.Name = "label6";
			this.label6.Size = new Size(67, 13);
			this.label6.TabIndex = 30;
			this.label6.Text = "Latency (ms)";
			this.unityGainCheckBox.Anchor = AnchorStyles.Left;
			this.unityGainCheckBox.AutoSize = true;
			this.unityGainCheckBox.Location = new Point(3, 116);
			this.unityGainCheckBox.Name = "unityGainCheckBox";
			this.unityGainCheckBox.Size = new Size(75, 17);
			this.unityGainCheckBox.TabIndex = 5;
			this.unityGainCheckBox.Text = "Unity Gain";
			this.unityGainCheckBox.UseVisualStyleBackColor = true;
			this.unityGainCheckBox.CheckStateChanged += this.unityGainCheckBox_CheckStateChanged;
			this.filterAudioCheckBox.Anchor = AnchorStyles.Left;
			this.filterAudioCheckBox.AutoSize = true;
			this.filterAudioCheckBox.Location = new Point(92, 116);
			this.filterAudioCheckBox.Name = "filterAudioCheckBox";
			this.filterAudioCheckBox.Size = new Size(78, 17);
			this.filterAudioCheckBox.TabIndex = 6;
			this.filterAudioCheckBox.Text = "Filter Audio";
			this.filterAudioCheckBox.UseVisualStyleBackColor = true;
			this.filterAudioCheckBox.CheckedChanged += this.filterAudioCheckBox_CheckStateChanged;
			this.agcCollapsiblePanel.AutoHeight = false;
			this.agcCollapsiblePanel.Content.Controls.Add(this.tableLayoutPanel2);
			this.agcCollapsiblePanel.Content.Location = new Point(0, 24);
			this.agcCollapsiblePanel.Content.Name = "Content";
			this.agcCollapsiblePanel.Content.Size = new Size(224, 106);
			this.agcCollapsiblePanel.Content.TabIndex = 1;
			this.agcCollapsiblePanel.Location = new Point(0, 577);
			this.agcCollapsiblePanel.Name = "agcCollapsiblePanel";
			this.agcCollapsiblePanel.NextPanel = this.fftCollapsiblePanel;
			this.agcCollapsiblePanel.PanelTitle = "AGC";
			this.agcCollapsiblePanel.Size = new Size(224, 130);
			this.agcCollapsiblePanel.TabIndex = 2;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
			this.tableLayoutPanel2.Controls.Add(this.agcCheckBox, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.agcSlopeNumericUpDown, 1, 3);
			this.tableLayoutPanel2.Controls.Add(this.agcUseHangCheckBox, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.label22, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.label4, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.agcDecayNumericUpDown, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.label10, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.agcThresholdNumericUpDown, 1, 1);
			this.tableLayoutPanel2.Dock = DockStyle.Fill;
			this.tableLayoutPanel2.Location = new Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 4;
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
			this.tableLayoutPanel2.Size = new Size(224, 106);
			this.tableLayoutPanel2.TabIndex = 14;
			this.agcCheckBox.Anchor = AnchorStyles.Left;
			this.agcCheckBox.AutoSize = true;
			this.agcCheckBox.Location = new Point(3, 3);
			this.agcCheckBox.Name = "agcCheckBox";
			this.agcCheckBox.Size = new Size(70, 17);
			this.agcCheckBox.TabIndex = 0;
			this.agcCheckBox.Text = "Use AGC";
			this.agcCheckBox.UseVisualStyleBackColor = true;
			this.agcCheckBox.CheckedChanged += this.agcCheckBox_CheckedChanged;
			this.agcSlopeNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.agcSlopeNumericUpDown.Location = new Point(92, 83);
			this.agcSlopeNumericUpDown.Maximum = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.agcSlopeNumericUpDown.Name = "agcSlopeNumericUpDown";
			this.agcSlopeNumericUpDown.Size = new Size(129, 20);
			this.agcSlopeNumericUpDown.TabIndex = 4;
			this.agcSlopeNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.agcSlopeNumericUpDown.Value = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.agcSlopeNumericUpDown.ValueChanged += this.agcSlopeNumericUpDown_ValueChanged;
			this.agcUseHangCheckBox.Anchor = AnchorStyles.Left;
			this.agcUseHangCheckBox.AutoSize = true;
			this.agcUseHangCheckBox.Location = new Point(92, 3);
			this.agcUseHangCheckBox.Name = "agcUseHangCheckBox";
			this.agcUseHangCheckBox.Size = new Size(74, 17);
			this.agcUseHangCheckBox.TabIndex = 1;
			this.agcUseHangCheckBox.Text = "Use Hang";
			this.agcUseHangCheckBox.UseVisualStyleBackColor = true;
			this.agcUseHangCheckBox.CheckedChanged += this.agcUseHangCheckBox_CheckedChanged;
			this.label22.Anchor = AnchorStyles.Left;
			this.label22.AutoSize = true;
			this.label22.Location = new Point(3, 86);
			this.label22.Name = "label22";
			this.label22.Size = new Size(56, 13);
			this.label22.TabIndex = 13;
			this.label22.Text = "Slope (dB)";
			this.label4.Anchor = AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.Location = new Point(3, 32);
			this.label4.Name = "label4";
			this.label4.Size = new Size(76, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Threshold (dB)";
			this.agcDecayNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.agcDecayNumericUpDown.Location = new Point(92, 57);
			this.agcDecayNumericUpDown.Maximum = new decimal(new int[4]
			{
				2000,
				0,
				0,
				0
			});
			this.agcDecayNumericUpDown.Minimum = new decimal(new int[4]
			{
				10,
				0,
				0,
				0
			});
			this.agcDecayNumericUpDown.Name = "agcDecayNumericUpDown";
			this.agcDecayNumericUpDown.Size = new Size(129, 20);
			this.agcDecayNumericUpDown.TabIndex = 3;
			this.agcDecayNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.agcDecayNumericUpDown.Value = new decimal(new int[4]
			{
				2000,
				0,
				0,
				0
			});
			this.agcDecayNumericUpDown.ValueChanged += this.agcDecayNumericUpDown_ValueChanged;
			this.label10.Anchor = AnchorStyles.Left;
			this.label10.AutoSize = true;
			this.label10.Location = new Point(3, 60);
			this.label10.Name = "label10";
			this.label10.Size = new Size(60, 13);
			this.label10.TabIndex = 11;
			this.label10.Text = "Decay (ms)";
			this.agcThresholdNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.agcThresholdNumericUpDown.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.agcThresholdNumericUpDown.Location = new Point(92, 27);
			this.agcThresholdNumericUpDown.Maximum = new decimal(new int[4]);
			this.agcThresholdNumericUpDown.Minimum = new decimal(new int[4]
			{
				160,
				0,
				0,
				-2147483648
			});
			this.agcThresholdNumericUpDown.Name = "agcThresholdNumericUpDown";
			this.agcThresholdNumericUpDown.Size = new Size(129, 23);
			this.agcThresholdNumericUpDown.TabIndex = 2;
			this.agcThresholdNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.agcThresholdNumericUpDown.ValueChanged += this.agcThresholdNumericUpDown_ValueChanged;
			this.fftCollapsiblePanel.AutoHeight = false;
			this.fftCollapsiblePanel.Content.Controls.Add(this.tableLayoutPanel3);
			this.fftCollapsiblePanel.Content.Location = new Point(0, 24);
			this.fftCollapsiblePanel.Content.Name = "Content";
			this.fftCollapsiblePanel.Content.Size = new Size(224, 484);
			this.fftCollapsiblePanel.Content.TabIndex = 1;
			this.fftCollapsiblePanel.Location = new Point(0, 707);
			this.fftCollapsiblePanel.Name = "fftCollapsiblePanel";
			this.fftCollapsiblePanel.NextPanel = null;
			this.fftCollapsiblePanel.PanelTitle = "FFT Display";
			this.fftCollapsiblePanel.Size = new Size(224, 508);
			this.fftCollapsiblePanel.TabIndex = 3;
			this.tableLayoutPanel3.ColumnCount = 4;
			this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));
			this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
			this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15f));
			this.tableLayoutPanel3.Controls.Add(this.label7, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.viewComboBox, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.label8, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.fftWindowComboBox, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.label21, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.fftResolutionComboBox, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.groupBox1, 0, 7);
			this.tableLayoutPanel3.Controls.Add(this.smoothingGroupBox, 0, 6);
			this.tableLayoutPanel3.Controls.Add(this.markPeaksCheckBox, 0, 5);
			this.tableLayoutPanel3.Controls.Add(this.useTimestampsCheckBox, 0, 4);
			this.tableLayoutPanel3.Controls.Add(this.label14, 2, 4);
			this.tableLayoutPanel3.Controls.Add(this.gradientButton, 3, 4);
			this.tableLayoutPanel3.Controls.Add(this.label3, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.spectrumStyleComboBox, 1, 3);
			this.tableLayoutPanel3.Dock = DockStyle.Fill;
			this.tableLayoutPanel3.Location = new Point(0, 0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 8;
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 75f));
			this.tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.tableLayoutPanel3.Size = new Size(224, 484);
			this.tableLayoutPanel3.TabIndex = 33;
			this.label7.Anchor = AnchorStyles.Left;
			this.label7.AutoSize = true;
			this.label7.Location = new Point(3, 7);
			this.label7.Name = "label7";
			this.label7.Size = new Size(30, 13);
			this.label7.TabIndex = 12;
			this.label7.Text = "View";
			this.viewComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel3.SetColumnSpan(this.viewComboBox, 3);
			this.viewComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.viewComboBox.FormattingEnabled = true;
			this.viewComboBox.Items.AddRange(new object[4]
			{
				"Spectrum Analyzer",
				"Waterfall",
				"Both",
				"None"
			});
			this.viewComboBox.Location = new Point(92, 3);
			this.viewComboBox.Name = "viewComboBox";
			this.viewComboBox.Size = new Size(129, 21);
			this.viewComboBox.TabIndex = 0;
			this.viewComboBox.SelectedIndexChanged += this.viewComboBox_SelectedIndexChanged;
			this.label8.Anchor = AnchorStyles.Left;
			this.label8.AutoSize = true;
			this.label8.Location = new Point(3, 34);
			this.label8.Name = "label8";
			this.label8.Size = new Size(46, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "Window";
			this.fftWindowComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel3.SetColumnSpan(this.fftWindowComboBox, 3);
			this.fftWindowComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.fftWindowComboBox.FormattingEnabled = true;
			this.fftWindowComboBox.Items.AddRange(new object[7]
			{
				"None",
				"Hamming",
				"Blackman",
				"Blackman-Harris 4",
				"Blackman-Harris 7",
				"Hann-Poisson",
				"Youssef"
			});
			this.fftWindowComboBox.Location = new Point(92, 30);
			this.fftWindowComboBox.Name = "fftWindowComboBox";
			this.fftWindowComboBox.Size = new Size(129, 21);
			this.fftWindowComboBox.TabIndex = 1;
			this.fftWindowComboBox.SelectedIndexChanged += this.fftWindowComboBox_SelectedIndexChanged;
			this.label21.Anchor = AnchorStyles.Left;
			this.label21.AutoSize = true;
			this.label21.Location = new Point(3, 61);
			this.label21.Name = "label21";
			this.label21.Size = new Size(57, 13);
			this.label21.TabIndex = 18;
			this.label21.Text = "Resolution";
			this.fftResolutionComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel3.SetColumnSpan(this.fftResolutionComboBox, 3);
			this.fftResolutionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.fftResolutionComboBox.FormattingEnabled = true;
			this.fftResolutionComboBox.Items.AddRange(new object[14]
			{
				"512",
				"1024",
				"2048",
				"4096",
				"8192",
				"16384",
				"32768",
				"65536",
				"131072",
				"262144",
				"524288",
				"1048576",
				"2097152",
				"4194304"
			});
			this.fftResolutionComboBox.Location = new Point(92, 57);
			this.fftResolutionComboBox.Name = "fftResolutionComboBox";
			this.fftResolutionComboBox.Size = new Size(129, 21);
			this.fftResolutionComboBox.TabIndex = 2;
			this.fftResolutionComboBox.SelectedIndexChanged += this.fftResolutionComboBox_SelectedIndexChanged;
			this.tableLayoutPanel3.SetColumnSpan(this.groupBox1, 4);
			this.groupBox1.Controls.Add(this.tableLayoutPanel5);
			this.groupBox1.Dock = DockStyle.Fill;
			this.groupBox1.Location = new Point(3, 406);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new Size(218, 75);
			this.groupBox1.TabIndex = 32;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Spectrum";
			this.tableLayoutPanel5.ColumnCount = 2;
			this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
			this.tableLayoutPanel5.Controls.Add(this.label28, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.fftSpeedTrackBar, 1, 0);
			this.tableLayoutPanel5.Dock = DockStyle.Fill;
			this.tableLayoutPanel5.Location = new Point(3, 16);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
			this.tableLayoutPanel5.Size = new Size(212, 56);
			this.tableLayoutPanel5.TabIndex = 31;
			this.label28.Anchor = AnchorStyles.Left;
			this.label28.AutoSize = true;
			this.label28.Location = new Point(3, 21);
			this.label28.Name = "label28";
			this.label28.Size = new Size(38, 13);
			this.label28.TabIndex = 29;
			this.label28.Text = "Speed";
			this.fftSpeedTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.fftSpeedTrackBar.Location = new Point(87, 5);
			this.fftSpeedTrackBar.Maximum = 100;
			this.fftSpeedTrackBar.Minimum = 1;
			this.fftSpeedTrackBar.Name = "fftSpeedTrackBar";
			this.fftSpeedTrackBar.RightToLeftLayout = true;
			this.fftSpeedTrackBar.Size = new Size(122, 45);
			this.fftSpeedTrackBar.TabIndex = 2;
			this.fftSpeedTrackBar.TickFrequency = 10;
			this.fftSpeedTrackBar.TickStyle = TickStyle.Both;
			this.fftSpeedTrackBar.Value = 50;
			this.fftSpeedTrackBar.ValueChanged += this.fftSpeedTrackBar_ValueChanged;
			this.tableLayoutPanel3.SetColumnSpan(this.smoothingGroupBox, 4);
			this.smoothingGroupBox.Controls.Add(this.tableLayoutPanel4);
			this.smoothingGroupBox.Dock = DockStyle.Fill;
			this.smoothingGroupBox.Location = new Point(3, 163);
			this.smoothingGroupBox.Name = "smoothingGroupBox";
			this.smoothingGroupBox.Size = new Size(218, 237);
			this.smoothingGroupBox.TabIndex = 31;
			this.smoothingGroupBox.TabStop = false;
			this.smoothingGroupBox.Text = "Smoothing";
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
			this.tableLayoutPanel4.Controls.Add(this.label23, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.wDecayTrackBar, 1, 3);
			this.tableLayoutPanel4.Controls.Add(this.wAttackTrackBar, 1, 2);
			this.tableLayoutPanel4.Controls.Add(this.label25, 0, 3);
			this.tableLayoutPanel4.Controls.Add(this.sDecayTrackBar, 1, 1);
			this.tableLayoutPanel4.Controls.Add(this.sAttackTrackBar, 1, 0);
			this.tableLayoutPanel4.Controls.Add(this.label24, 0, 1);
			this.tableLayoutPanel4.Controls.Add(this.label26, 0, 2);
			this.tableLayoutPanel4.Dock = DockStyle.Fill;
			this.tableLayoutPanel4.Location = new Point(3, 16);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 4;
			this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.tableLayoutPanel4.Size = new Size(212, 218);
			this.tableLayoutPanel4.TabIndex = 27;
			this.label23.Anchor = AnchorStyles.Left;
			this.label23.AutoSize = true;
			this.label23.Location = new Point(3, 20);
			this.label23.Name = "label23";
			this.label23.Size = new Size(48, 13);
			this.label23.TabIndex = 23;
			this.label23.Text = "S-Attack";
			this.wDecayTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.wDecayTrackBar.Location = new Point(87, 167);
			this.wDecayTrackBar.Maximum = 50;
			this.wDecayTrackBar.Name = "wDecayTrackBar";
			this.wDecayTrackBar.Size = new Size(122, 45);
			this.wDecayTrackBar.TabIndex = 8;
			this.wDecayTrackBar.TickFrequency = 5;
			this.wDecayTrackBar.TickStyle = TickStyle.Both;
			this.wDecayTrackBar.ValueChanged += this.wDecayTrackBar_ValueChanged;
			this.wAttackTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.wAttackTrackBar.Location = new Point(87, 112);
			this.wAttackTrackBar.Maximum = 50;
			this.wAttackTrackBar.Name = "wAttackTrackBar";
			this.wAttackTrackBar.Size = new Size(122, 45);
			this.wAttackTrackBar.TabIndex = 7;
			this.wAttackTrackBar.TickFrequency = 5;
			this.wAttackTrackBar.TickStyle = TickStyle.Both;
			this.wAttackTrackBar.ValueChanged += this.wAttackTrackBar_ValueChanged;
			this.label25.Anchor = AnchorStyles.Left;
			this.label25.AutoSize = true;
			this.label25.Location = new Point(3, 183);
			this.label25.Name = "label25";
			this.label25.Size = new Size(52, 13);
			this.label25.TabIndex = 26;
			this.label25.Text = "W-Decay";
			this.sDecayTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.sDecayTrackBar.Location = new Point(87, 58);
			this.sDecayTrackBar.Maximum = 50;
			this.sDecayTrackBar.Name = "sDecayTrackBar";
			this.sDecayTrackBar.Size = new Size(122, 45);
			this.sDecayTrackBar.TabIndex = 6;
			this.sDecayTrackBar.TickFrequency = 5;
			this.sDecayTrackBar.TickStyle = TickStyle.Both;
			this.sDecayTrackBar.ValueChanged += this.sDecayTrackBar_ValueChanged;
			this.sAttackTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.sAttackTrackBar.Location = new Point(87, 4);
			this.sAttackTrackBar.Maximum = 50;
			this.sAttackTrackBar.Name = "sAttackTrackBar";
			this.sAttackTrackBar.Size = new Size(122, 45);
			this.sAttackTrackBar.TabIndex = 5;
			this.sAttackTrackBar.TickFrequency = 5;
			this.sAttackTrackBar.TickStyle = TickStyle.Both;
			this.sAttackTrackBar.ValueChanged += this.sAttackTrackBar_ValueChanged;
			this.label24.Anchor = AnchorStyles.Left;
			this.label24.AutoSize = true;
			this.label24.Location = new Point(3, 74);
			this.label24.Name = "label24";
			this.label24.Size = new Size(48, 13);
			this.label24.TabIndex = 24;
			this.label24.Text = "S-Decay";
			this.label26.Anchor = AnchorStyles.Left;
			this.label26.AutoSize = true;
			this.label26.Location = new Point(3, 128);
			this.label26.Name = "label26";
			this.label26.Size = new Size(52, 13);
			this.label26.TabIndex = 25;
			this.label26.Text = "W-Attack";
			this.markPeaksCheckBox.Anchor = AnchorStyles.Right;
			this.markPeaksCheckBox.AutoSize = true;
			this.markPeaksCheckBox.Location = new Point(3, 140);
			this.markPeaksCheckBox.Name = "markPeaksCheckBox";
			this.markPeaksCheckBox.Size = new Size(83, 17);
			this.markPeaksCheckBox.TabIndex = 23;
			this.markPeaksCheckBox.Text = "Mark Peaks";
			this.markPeaksCheckBox.UseVisualStyleBackColor = true;
			this.markPeaksCheckBox.CheckedChanged += this.markPeaksCheckBox_CheckedChanged;
			this.useTimestampsCheckBox.Anchor = AnchorStyles.Left;
			this.useTimestampsCheckBox.AutoSize = true;
			this.tableLayoutPanel3.SetColumnSpan(this.useTimestampsCheckBox, 2);
			this.useTimestampsCheckBox.Location = new Point(3, 114);
			this.useTimestampsCheckBox.Name = "useTimestampsCheckBox";
			this.useTimestampsCheckBox.Size = new Size(90, 17);
			this.useTimestampsCheckBox.TabIndex = 3;
			this.useTimestampsCheckBox.Text = "Time Markers";
			this.useTimestampsCheckBox.UseVisualStyleBackColor = true;
			this.useTimestampsCheckBox.CheckedChanged += this.useTimestampCheckBox_CheckedChanged;
			this.label14.Anchor = AnchorStyles.Right;
			this.label14.AutoSize = true;
			this.label14.Location = new Point(139, 116);
			this.label14.Name = "label14";
			this.label14.Size = new Size(47, 13);
			this.label14.TabIndex = 16;
			this.label14.Text = "Gradient";
			this.gradientButton.Anchor = AnchorStyles.Right;
			this.gradientButton.Location = new Point(197, 111);
			this.gradientButton.Name = "gradientButton";
			this.gradientButton.Size = new Size(24, 23);
			this.gradientButton.TabIndex = 4;
			this.gradientButton.Text = "...";
			this.gradientButton.UseVisualStyleBackColor = true;
			this.gradientButton.Click += this.gradientButton_Click;
			this.label3.Anchor = AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new Point(3, 88);
			this.label3.Name = "label3";
			this.label3.Size = new Size(78, 13);
			this.label3.TabIndex = 33;
			this.label3.Text = "Spectrum Style";
			this.spectrumStyleComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel3.SetColumnSpan(this.spectrumStyleComboBox, 3);
			this.spectrumStyleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.spectrumStyleComboBox.FormattingEnabled = true;
			this.spectrumStyleComboBox.Items.AddRange(new object[6]
			{
				"Dots",
				"Simple Curve",
				"Solid Fill",
				"Static Gradient",
				"Dynamic Gradient",
				"Min Max"
			});
			this.spectrumStyleComboBox.Location = new Point(92, 84);
			this.spectrumStyleComboBox.Name = "spectrumStyleComboBox";
			this.spectrumStyleComboBox.Size = new Size(129, 21);
			this.spectrumStyleComboBox.TabIndex = 34;
			this.spectrumStyleComboBox.SelectedIndexChanged += this.spectrumStyleComboBox_SelectedIndexChanged;
			this.fftOffsetTrackBar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
			this.fftOffsetTrackBar.Location = new Point(3, 533);
			this.fftOffsetTrackBar.Maximum = 15;
			this.fftOffsetTrackBar.Name = "fftOffsetTrackBar";
			this.fftOffsetTrackBar.Orientation = Orientation.Vertical;
			this.fftOffsetTrackBar.Size = new Size(45, 154);
			this.fftOffsetTrackBar.TabIndex = 27;
			this.fftOffsetTrackBar.TickStyle = TickStyle.Both;
			this.fftOffsetTrackBar.Scroll += this.fftOffsetTrackBar_Scroll;
			this.fftRangeTrackBar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
			this.fftRangeTrackBar.LargeChange = 10;
			this.fftRangeTrackBar.Location = new Point(3, 356);
			this.fftRangeTrackBar.Maximum = 15;
			this.fftRangeTrackBar.Minimum = 1;
			this.fftRangeTrackBar.Name = "fftRangeTrackBar";
			this.fftRangeTrackBar.Orientation = Orientation.Vertical;
			this.fftRangeTrackBar.Size = new Size(45, 151);
			this.fftRangeTrackBar.TabIndex = 28;
			this.fftRangeTrackBar.TickStyle = TickStyle.Both;
			this.fftRangeTrackBar.Value = 13;
			this.fftRangeTrackBar.Scroll += this.fftRangeTrackBar_Scroll;
			this.audioGainTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.audioGainTrackBar.Location = new Point(147, 3);
			this.audioGainTrackBar.Maximum = 60;
			this.audioGainTrackBar.Minimum = 25;
			this.audioGainTrackBar.Name = "audioGainTrackBar";
			this.audioGainTrackBar.Size = new Size(143, 45);
			this.audioGainTrackBar.TabIndex = 0;
			this.audioGainTrackBar.TickFrequency = 5;
			this.audioGainTrackBar.TickStyle = TickStyle.Both;
			this.audioGainTrackBar.Value = 40;
			this.audioGainTrackBar.ValueChanged += this.audioGainTrackBar_ValueChanged;
			this.label17.Anchor = AnchorStyles.None;
			this.label17.AutoSize = true;
			this.label17.Location = new Point(6, 340);
			this.label17.Name = "label17";
			this.label17.Size = new Size(39, 13);
			this.label17.TabIndex = 27;
			this.label17.Text = "Range";
			this.label17.TextAlign = ContentAlignment.MiddleCenter;
			this.scrollPanel.AutoScroll = true;
			this.scrollPanel.Controls.Add(this.controlPanel);
			this.scrollPanel.Dock = DockStyle.Left;
			this.scrollPanel.Location = new Point(10, 61);
			this.scrollPanel.Margin = new Padding(3, 0, 3, 0);
			this.scrollPanel.Name = "scrollPanel";
			this.scrollPanel.Size = new Size(246, 690);
			this.scrollPanel.TabIndex = 28;
			this.rightTableLayoutPanel.AutoSize = true;
			this.rightTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.rightTableLayoutPanel.ColumnCount = 1;
			this.rightTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.rightTableLayoutPanel.Controls.Add(this.label2, 0, 6);
			this.rightTableLayoutPanel.Controls.Add(this.label19, 0, 0);
			this.rightTableLayoutPanel.Controls.Add(this.label20, 0, 2);
			this.rightTableLayoutPanel.Controls.Add(this.label17, 0, 4);
			this.rightTableLayoutPanel.Controls.Add(this.fftZoomTrackBar, 0, 1);
			this.rightTableLayoutPanel.Controls.Add(this.fftContrastTrackBar, 0, 3);
			this.rightTableLayoutPanel.Controls.Add(this.fftOffsetTrackBar, 0, 7);
			this.rightTableLayoutPanel.Controls.Add(this.fftRangeTrackBar, 0, 5);
			this.rightTableLayoutPanel.Dock = DockStyle.Right;
			this.rightTableLayoutPanel.Location = new Point(922, 61);
			this.rightTableLayoutPanel.Margin = new Padding(0);
			this.rightTableLayoutPanel.Name = "rightTableLayoutPanel";
			this.rightTableLayoutPanel.RowCount = 8;
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
			this.rightTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
			this.rightTableLayoutPanel.Size = new Size(52, 690);
			this.rightTableLayoutPanel.TabIndex = 33;
			this.label2.Anchor = AnchorStyles.None;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(8, 513);
			this.label2.Name = "label2";
			this.label2.Size = new Size(35, 13);
			this.label2.TabIndex = 28;
			this.label2.Text = "Offset";
			this.label2.TextAlign = ContentAlignment.MiddleCenter;
			this.centerPanel.BackColor = Color.Black;
			this.centerPanel.Controls.Add(this.spectrumPanel);
			this.centerPanel.Controls.Add(this.bottomSplitter);
			this.centerPanel.Controls.Add(this.bottomPluginPanel);
			this.centerPanel.Controls.Add(this.topSplitter);
			this.centerPanel.Controls.Add(this.topPluginPanel);
			this.centerPanel.Controls.Add(this.rightSplitter);
			this.centerPanel.Controls.Add(this.leftSplitter);
			this.centerPanel.Controls.Add(this.leftPluginPanel);
			this.centerPanel.Controls.Add(this.rightPluginPanel);
			this.centerPanel.Dock = DockStyle.Fill;
			this.centerPanel.Location = new Point(256, 61);
			this.centerPanel.Margin = new Padding(0);
			this.centerPanel.Name = "centerPanel";
			this.centerPanel.Size = new Size(666, 690);
			this.centerPanel.TabIndex = 4;
			this.spectrumPanel.Controls.Add(this.spectrumSplitter);
			this.spectrumPanel.Controls.Add(this.waterfall);
			this.spectrumPanel.Controls.Add(this.spectrumAnalyzer);
			this.spectrumPanel.Dock = DockStyle.Fill;
			this.spectrumPanel.Location = new Point(4, 4);
			this.spectrumPanel.Name = "spectrumPanel";
			this.spectrumPanel.Size = new Size(658, 682);
			this.spectrumPanel.TabIndex = 38;
			this.spectrumSplitter.BackColor = SystemColors.Control;
			this.spectrumSplitter.Dock = DockStyle.Top;
			this.spectrumSplitter.Location = new Point(0, 324);
			this.spectrumSplitter.MinExtra = 0;
			this.spectrumSplitter.MinSize = 0;
			this.spectrumSplitter.Name = "spectrumSplitter";
			this.spectrumSplitter.Size = new Size(658, 4);
			this.spectrumSplitter.TabIndex = 1;
			this.spectrumSplitter.TabStop = false;
			this.waterfall.Attack = 0.9f;
			this.waterfall.BandType = BandType.Center;
			this.waterfall.CenterFrequency = 0L;
			this.waterfall.Contrast = 0;
			this.waterfall.Decay = 0.5f;
			this.waterfall.DisplayOffset = 0;
			this.waterfall.DisplayRange = 130;
			this.waterfall.Dock = DockStyle.Fill;
			this.waterfall.EnableFilter = true;
			this.waterfall.EnableFilterMove = true;
			this.waterfall.EnableFrequencyMarker = true;
			this.waterfall.EnableHotTracking = true;
			this.waterfall.EnableSideFilterResize = false;
			this.waterfall.FilterBandwidth = 10000;
			this.waterfall.FilterOffset = 0;
			this.waterfall.Frequency = 0L;
			this.waterfall.Location = new Point(0, 324);
			this.waterfall.Name = "waterfall";
			this.waterfall.Size = new Size(658, 358);
			this.waterfall.SpectrumWidth = 48000;
			this.waterfall.StepSize = 0;
			this.waterfall.TabIndex = 0;
			this.waterfall.TimestampInterval = 100;
			this.waterfall.UseSmoothing = true;
			this.waterfall.UseSnap = false;
			this.waterfall.UseTimestamps = false;
			this.waterfall.Zoom = 0;
			this.waterfall.FrequencyChanged += this.panview_FrequencyChanged;
			this.waterfall.CenterFrequencyChanged += this.panview_CenterFrequencyChanged;
			this.waterfall.BandwidthChanged += this.panview_BandwidthChanged;
			this.waterfall.CustomPaint += this.waterfall_CustomPaint;
			this.spectrumAnalyzer.Attack = 0.9f;
			this.spectrumAnalyzer.BandType = BandType.Center;
			this.spectrumAnalyzer.CenterFrequency = 0L;
			this.spectrumAnalyzer.Contrast = 0;
			this.spectrumAnalyzer.Decay = 0.3f;
			this.spectrumAnalyzer.DisplayOffset = 0;
			this.spectrumAnalyzer.DisplayRange = 130;
			this.spectrumAnalyzer.Dock = DockStyle.Top;
			this.spectrumAnalyzer.EnableFilter = true;
			this.spectrumAnalyzer.EnableFilterMove = true;
			this.spectrumAnalyzer.EnableFrequencyMarker = true;
			this.spectrumAnalyzer.EnableHotTracking = true;
			this.spectrumAnalyzer.EnableSideFilterResize = false;
			this.spectrumAnalyzer.EnableSNR = true;
			this.spectrumAnalyzer.FilterBandwidth = 10000;
			this.spectrumAnalyzer.FilterOffset = 100;
			this.spectrumAnalyzer.Frequency = 0L;
			this.spectrumAnalyzer.Location = new Point(0, 0);
			this.spectrumAnalyzer.MarkPeaks = false;
			this.spectrumAnalyzer.Name = "spectrumAnalyzer";
			this.spectrumAnalyzer.Size = new Size(658, 324);
			this.spectrumAnalyzer.SpectrumStyle = SpectrumStyle.StaticGradient;
			this.spectrumAnalyzer.SpectrumWidth = 48000;
			this.spectrumAnalyzer.StatusText = null;
			this.spectrumAnalyzer.StepSize = 1000;
			this.spectrumAnalyzer.TabIndex = 0;
			this.spectrumAnalyzer.UseSmoothing = true;
			this.spectrumAnalyzer.UseSnap = false;
			this.spectrumAnalyzer.UseStepSizeForDisplay = false;
			this.spectrumAnalyzer.Zoom = 0;
			this.spectrumAnalyzer.FrequencyChanged += this.panview_FrequencyChanged;
			this.spectrumAnalyzer.CenterFrequencyChanged += this.panview_CenterFrequencyChanged;
			this.spectrumAnalyzer.BandwidthChanged += this.panview_BandwidthChanged;
			this.spectrumAnalyzer.CustomPaint += this.spectrumAnalyzer_CustomPaint;
			this.spectrumAnalyzer.BackgroundCustomPaint += this.spectrumAnalyzer_BackgroundCustomPaint;
			this.bottomSplitter.BackColor = SystemColors.Control;
			this.bottomSplitter.Dock = DockStyle.Bottom;
			this.bottomSplitter.Location = new Point(4, 686);
			this.bottomSplitter.MinSize = 0;
			this.bottomSplitter.Name = "bottomSplitter";
			this.bottomSplitter.Size = new Size(658, 4);
			this.bottomSplitter.TabIndex = 42;
			this.bottomSplitter.TabStop = false;
			this.bottomSplitter.SplitterMoved += this.pluginSplitter_SplitterMoved;
			this.bottomPluginPanel.BackColor = SystemColors.Control;
			this.bottomPluginPanel.ColumnCount = 1;
			this.bottomPluginPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.bottomPluginPanel.Dock = DockStyle.Bottom;
			this.bottomPluginPanel.Location = new Point(4, 690);
			this.bottomPluginPanel.Margin = new Padding(0);
			this.bottomPluginPanel.Name = "bottomPluginPanel";
			this.bottomPluginPanel.RowCount = 1;
			this.bottomPluginPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.bottomPluginPanel.Size = new Size(658, 0);
			this.bottomPluginPanel.TabIndex = 41;
			this.topSplitter.BackColor = SystemColors.Control;
			this.topSplitter.Dock = DockStyle.Top;
			this.topSplitter.Location = new Point(4, 0);
			this.topSplitter.MinSize = 0;
			this.topSplitter.Name = "topSplitter";
			this.topSplitter.Size = new Size(658, 4);
			this.topSplitter.TabIndex = 40;
			this.topSplitter.TabStop = false;
			this.topSplitter.SplitterMoved += this.pluginSplitter_SplitterMoved;
			this.topPluginPanel.BackColor = SystemColors.Control;
			this.topPluginPanel.ColumnCount = 1;
			this.topPluginPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.topPluginPanel.Dock = DockStyle.Top;
			this.topPluginPanel.Location = new Point(4, 0);
			this.topPluginPanel.Margin = new Padding(0);
			this.topPluginPanel.Name = "topPluginPanel";
			this.topPluginPanel.RowCount = 1;
			this.topPluginPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.topPluginPanel.Size = new Size(658, 0);
			this.topPluginPanel.TabIndex = 39;
			this.rightSplitter.BackColor = SystemColors.Control;
			this.rightSplitter.Dock = DockStyle.Right;
			this.rightSplitter.Location = new Point(662, 0);
			this.rightSplitter.MinSize = 0;
			this.rightSplitter.Name = "rightSplitter";
			this.rightSplitter.Size = new Size(4, 690);
			this.rightSplitter.TabIndex = 37;
			this.rightSplitter.TabStop = false;
			this.rightSplitter.SplitterMoved += this.pluginSplitter_SplitterMoved;
			this.leftSplitter.BackColor = SystemColors.Control;
			this.leftSplitter.Location = new Point(0, 0);
			this.leftSplitter.MinSize = 0;
			this.leftSplitter.Name = "leftSplitter";
			this.leftSplitter.Size = new Size(4, 690);
			this.leftSplitter.TabIndex = 36;
			this.leftSplitter.TabStop = false;
			this.leftSplitter.SplitterMoved += this.pluginSplitter_SplitterMoved;
			this.leftPluginPanel.BackColor = SystemColors.Control;
			this.leftPluginPanel.ColumnCount = 1;
			this.leftPluginPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.leftPluginPanel.Dock = DockStyle.Left;
			this.leftPluginPanel.Location = new Point(0, 0);
			this.leftPluginPanel.Margin = new Padding(0);
			this.leftPluginPanel.Name = "leftPluginPanel";
			this.leftPluginPanel.RowCount = 1;
			this.leftPluginPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.leftPluginPanel.Size = new Size(0, 690);
			this.leftPluginPanel.TabIndex = 1;
			this.rightPluginPanel.BackColor = SystemColors.Control;
			this.rightPluginPanel.ColumnCount = 1;
			this.rightPluginPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.rightPluginPanel.Dock = DockStyle.Right;
			this.rightPluginPanel.Location = new Point(666, 0);
			this.rightPluginPanel.Margin = new Padding(0);
			this.rightPluginPanel.Name = "rightPluginPanel";
			this.rightPluginPanel.RowCount = 1;
			this.rightPluginPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.rightPluginPanel.Size = new Size(0, 690);
			this.rightPluginPanel.TabIndex = 3;
			this.settingsTableLayoutPanel.ColumnCount = 8;
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.settingsTableLayoutPanel.Controls.Add(this.playStopButton, 1, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.configureSourceButton, 2, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.toggleMenuButton, 0, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.muteButton, 3, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.audioGainTrackBar, 4, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.vfoFrequencyEdit, 5, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.tuningStyleButton, 6, 0);
			this.settingsTableLayoutPanel.Controls.Add(this.logoPictureBox, 7, 0);
			this.settingsTableLayoutPanel.Dock = DockStyle.Top;
			this.settingsTableLayoutPanel.Location = new Point(10, 10);
			this.settingsTableLayoutPanel.Margin = new Padding(0);
			this.settingsTableLayoutPanel.Name = "settingsTableLayoutPanel";
			this.settingsTableLayoutPanel.RowCount = 1;
			this.settingsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.settingsTableLayoutPanel.Size = new Size(964, 51);
			this.settingsTableLayoutPanel.TabIndex = 33;
			this.playStopButton.Anchor = AnchorStyles.None;
			this.playStopButton.FlatAppearance.BorderSize = 0;
			this.playStopButton.FlatStyle = FlatStyle.Flat;
			this.playStopButton.Image = Resources.sdr_start;
			this.playStopButton.Location = new Point(39, 10);
			this.playStopButton.Name = "playStopButton";
			this.playStopButton.Size = new Size(30, 30);
			this.playStopButton.TabIndex = 0;
			this.playStopButton.Click += this.playStopButton_Click;
			this.configureSourceButton.Anchor = AnchorStyles.None;
			this.configureSourceButton.FlatAppearance.BorderSize = 0;
			this.configureSourceButton.FlatStyle = FlatStyle.Flat;
			this.configureSourceButton.Image = Resources.config_gear;
			this.configureSourceButton.Location = new Point(75, 12);
			this.configureSourceButton.Name = "configureSourceButton";
			this.configureSourceButton.Size = new Size(30, 26);
			this.configureSourceButton.TabIndex = 0;
			this.configureSourceButton.UseVisualStyleBackColor = true;
			this.configureSourceButton.Click += this.frontendGuiButton_Click;
			this.toggleMenuButton.Anchor = AnchorStyles.None;
			this.toggleMenuButton.FlatAppearance.BorderSize = 0;
			this.toggleMenuButton.FlatStyle = FlatStyle.Flat;
			this.toggleMenuButton.Image = Resources.toggle_menu;
			this.toggleMenuButton.Location = new Point(3, 10);
			this.toggleMenuButton.Name = "toggleMenuButton";
			this.toggleMenuButton.Size = new Size(30, 30);
			this.toggleMenuButton.TabIndex = 0;
			this.toggleMenuButton.Click += this.toggleMenuButton_Click;
			this.muteButton.Anchor = AnchorStyles.None;
			this.muteButton.FlatAppearance.BorderSize = 0;
			this.muteButton.FlatStyle = FlatStyle.Flat;
			this.muteButton.Image = Resources.audio_unmuted;
			this.muteButton.Location = new Point(111, 10);
			this.muteButton.Name = "muteButton";
			this.muteButton.Size = new Size(30, 30);
			this.muteButton.TabIndex = 1;
			this.muteButton.UseVisualStyleBackColor = true;
			this.muteButton.Click += this.muteButton_Click;
			this.vfoFrequencyEdit.Anchor = AnchorStyles.Left;
			this.vfoFrequencyEdit.AutoSize = true;
			this.vfoFrequencyEdit.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.vfoFrequencyEdit.BackColor = Color.Transparent;
			this.vfoFrequencyEdit.DisableFrequencyEvents = false;
			this.vfoFrequencyEdit.Frequency = 0L;
			this.vfoFrequencyEdit.Location = new Point(296, 13);
			this.vfoFrequencyEdit.Name = "vfoFrequencyEdit";
			this.vfoFrequencyEdit.Size = new Size(274, 25);
			this.vfoFrequencyEdit.StepSize = 0;
			this.vfoFrequencyEdit.TabIndex = 1;
			this.vfoFrequencyEdit.FrequencyChanged += this.vfoFrequencyEdit_FrequencyChanged;
			this.vfoFrequencyEdit.FrequencyChanging += this.vfoFrequencyEdit_FrequencyChanging;
			this.tuningStyleButton.Anchor = AnchorStyles.Left;
			this.tuningStyleButton.FlatAppearance.BorderSize = 0;
			this.tuningStyleButton.FlatStyle = FlatStyle.Flat;
			this.tuningStyleButton.Image = Resources.free_tuning;
			this.tuningStyleButton.Location = new Point(576, 10);
			this.tuningStyleButton.Name = "tuningStyleButton";
			this.tuningStyleButton.Size = new Size(30, 30);
			this.tuningStyleButton.TabIndex = 2;
			this.tuningStyleButton.Click += this.centerButton_Click;
			this.logoPictureBox.Anchor = AnchorStyles.Right;
			this.logoPictureBox.Cursor = Cursors.Hand;
			//this.logoPictureBox.Image = (Image)componentResourceManager.GetObject("logoPictureBox.Image");
			this.logoPictureBox.Location = new Point(842, 3);
			this.logoPictureBox.Name = "logoPictureBox";
			this.logoPictureBox.Size = new Size(119, 45);
			this.logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			this.logoPictureBox.TabIndex = 3;
			this.logoPictureBox.TabStop = false;
			this.logoPictureBox.Click += this.logoPictureBox_Click;
			this.menuSpacerPanel.Dock = DockStyle.Left;
			this.menuSpacerPanel.Location = new Point(256, 61);
			this.menuSpacerPanel.Name = "menuSpacerPanel";
			this.menuSpacerPanel.Size = new Size(3, 690);
			this.menuSpacerPanel.TabIndex = 34;
			base.AutoScaleDimensions = new SizeF(96f, 96f);
			base.AutoScaleMode = AutoScaleMode.Dpi;
			base.ClientSize = new Size(984, 761);
			base.Controls.Add(this.menuSpacerPanel);
			base.Controls.Add(this.centerPanel);
			base.Controls.Add(this.rightTableLayoutPanel);
			base.Controls.Add(this.scrollPanel);
			base.Controls.Add(this.settingsTableLayoutPanel);
			base.KeyPreview = true;
			base.Name = "MainForm";
			base.Padding = new Padding(10);
			base.SizeGripStyle = SizeGripStyle.Show;
			base.StartPosition = FormStartPosition.Manual;
			this.Text = "SDR#";
			base.Closing += this.MainForm_Closing;
			base.Load += this.MainForm_Load;
			base.Move += this.MainForm_Move;
			base.Resize += this.MainForm_Resize;
			((ISupportInitialize)this.fftContrastTrackBar).EndInit();
			((ISupportInitialize)this.fftZoomTrackBar).EndInit();
			this.controlPanel.ResumeLayout(false);
			this.sourceCollapsiblePanel.Content.ResumeLayout(false);
			this.sourceCollapsiblePanel.Content.PerformLayout();
			this.sourceCollapsiblePanel.ResumeLayout(false);
			this.sourceTableLayoutPanel.ResumeLayout(false);
			this.radioCollapsiblePanel.Content.ResumeLayout(false);
			this.radioCollapsiblePanel.ResumeLayout(false);
			this.radioTableLayoutPanel.ResumeLayout(false);
			this.radioTableLayoutPanel.PerformLayout();
			this.tableLayoutPanel7.ResumeLayout(false);
			this.tableLayoutPanel7.PerformLayout();
			((ISupportInitialize)this.cwShiftNumericUpDown).EndInit();
			((ISupportInitialize)this.squelchNumericUpDown).EndInit();
			((ISupportInitialize)this.filterBandwidthNumericUpDown).EndInit();
			((ISupportInitialize)this.filterOrderNumericUpDown).EndInit();
			((ISupportInitialize)this.frequencyShiftNumericUpDown).EndInit();
			this.audioCollapsiblePanel.Content.ResumeLayout(false);
			this.audioCollapsiblePanel.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((ISupportInitialize)this.latencyNumericUpDown).EndInit();
			this.agcCollapsiblePanel.Content.ResumeLayout(false);
			this.agcCollapsiblePanel.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((ISupportInitialize)this.agcSlopeNumericUpDown).EndInit();
			((ISupportInitialize)this.agcDecayNumericUpDown).EndInit();
			((ISupportInitialize)this.agcThresholdNumericUpDown).EndInit();
			this.fftCollapsiblePanel.Content.ResumeLayout(false);
			this.fftCollapsiblePanel.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel5.PerformLayout();
			((ISupportInitialize)this.fftSpeedTrackBar).EndInit();
			this.smoothingGroupBox.ResumeLayout(false);
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel4.PerformLayout();
			((ISupportInitialize)this.wDecayTrackBar).EndInit();
			((ISupportInitialize)this.wAttackTrackBar).EndInit();
			((ISupportInitialize)this.sDecayTrackBar).EndInit();
			((ISupportInitialize)this.sAttackTrackBar).EndInit();
			((ISupportInitialize)this.fftOffsetTrackBar).EndInit();
			((ISupportInitialize)this.fftRangeTrackBar).EndInit();
			((ISupportInitialize)this.audioGainTrackBar).EndInit();
			this.scrollPanel.ResumeLayout(false);
			this.scrollPanel.PerformLayout();
			this.rightTableLayoutPanel.ResumeLayout(false);
			this.rightTableLayoutPanel.PerformLayout();
			this.centerPanel.ResumeLayout(false);
			this.spectrumPanel.ResumeLayout(false);
			this.settingsTableLayoutPanel.ResumeLayout(false);
			this.settingsTableLayoutPanel.PerformLayout();
			((ISupportInitialize)this.logoPictureBox).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		public MainForm()
		{
			this._hookManager = new HookManager();
			this._hookManager.RegisterStreamHook(this._iqBalancerProcessor, ProcessorType.RawIQ);
			this._streamControl = new StreamControl(this._hookManager);
			this._vfo = new Vfo(this._hookManager);
			this._sharpControlProxy = new SharpControlProxy(this);
			this.InitializeComponent();
			this.InitializeGUI();
		}

		private unsafe void InitializeGUI()
		{
			this._initializing = true;
			base.Icon = Resources.mainicon;
			this._sourcePanelHeight = this.sourceCollapsiblePanel.Height;
			this._modeStates[DetectorType.WFM] = Utils.GetIntArraySetting("wfmState", MainForm._defaultWFMState);
			this._modeStates[DetectorType.NFM] = Utils.GetIntArraySetting("nfmState", MainForm._defaultNFMState);
			this._modeStates[DetectorType.AM] = Utils.GetIntArraySetting("amState", MainForm._defaultAMState);
			this._modeStates[DetectorType.LSB] = Utils.GetIntArraySetting("lsbState", MainForm._defaultSSBState);
			this._modeStates[DetectorType.USB] = Utils.GetIntArraySetting("usbState", MainForm._defaultSSBState);
			this._modeStates[DetectorType.DSB] = Utils.GetIntArraySetting("dsbState", MainForm._defaultDSBState);
			this._modeStates[DetectorType.CW] = Utils.GetIntArraySetting("cwState", MainForm._defaultCWState);
			this._modeStates[DetectorType.RAW] = Utils.GetIntArraySetting("rawState", MainForm._defaultRAWState);
			ThreadPool.QueueUserWorkItem(this.TuneThreadProc);
			string stringSetting = Utils.GetStringSetting("stepSizes", "1 Hz,10 Hz,100 Hz,500 Hz,1 kHz,2.5 kHz,5 kHz,6.25 kHz,7.5 kHz,8.33 kHz,9 kHz,10 kHz,12.5 kHz,15 kHz,20 kHz,25 kHz,30 kHz,50 kHz,100 kHz,150 kHz,200 kHz,250 kHz,300 kHz,350 kHz,400 kHz,450 kHz,500 kHz");
			this.stepSizeComboBox.Items.AddRange(stringSetting.Split(','));
			this._tuningStyle = (TuningStyle)Utils.GetIntSetting("tuningStyle", 0);
			int num = 0;
			int num2 = -1;
			List<AudioDevice> devices = AudioDevice.GetDevices(DeviceDirection.Input);
			string stringSetting2 = Utils.GetStringSetting("inputDevice", string.Empty);
			for (int i = 0; i < devices.Count; i++)
			{
				this.inputDeviceComboBox.Items.Add(devices[i]);
				if (devices[i].IsDefault)
				{
					num = i;
				}
				if (devices[i].ToString() == stringSetting2)
				{
					num2 = i;
				}
			}
			if (this.inputDeviceComboBox.Items.Count > 0)
			{
				this.inputDeviceComboBox.SelectedIndex = ((num2 >= 0) ? num2 : num);
			}
			num = 0;
			devices = AudioDevice.GetDevices(DeviceDirection.Output);
			stringSetting2 = Utils.GetStringSetting("outputDevice", string.Empty);
			for (int j = 0; j < devices.Count; j++)
			{
				this.outputDeviceComboBox.Items.Add(devices[j]);
				if (devices[j].IsDefault)
				{
					num = j;
				}
				if (devices[j].ToString() == stringSetting2)
				{
					num2 = j;
				}
			}
			if (this.outputDeviceComboBox.Items.Count > 0)
			{
				this.outputDeviceComboBox.SelectedIndex = ((num2 >= 0) ? num2 : num);
			}
			this._streamControl.BufferNeeded += this.ProcessBuffer;
			this.DetectorType = (DetectorType)Utils.GetIntSetting("detectorType", 2);
			this.modeRadioButton_CheckStateChanged(null, null);
			this.filterBandwidthNumericUpDown_ValueChanged(null, null);
			this.filterOrderNumericUpDown_ValueChanged(null, null);
			this.filterTypeComboBox_SelectedIndexChanged(null, null);
			this.cwShiftNumericUpDown_ValueChanged(null, null);
			this.agcCheckBox.Checked = Utils.GetBooleanSetting("useAGC");
			this.agcCheckBox_CheckedChanged(null, null);
			this.agcThresholdNumericUpDown.Value = Utils.GetIntSetting("agcThreshold", -100);
			this.agcThresholdNumericUpDown_ValueChanged(null, null);
			this.agcDecayNumericUpDown.Value = Utils.GetIntSetting("agcDecay", 100);
			this.agcDecayNumericUpDown_ValueChanged(null, null);
			this.agcSlopeNumericUpDown.Value = Utils.GetIntSetting("agcSlope", 0);
			this.agcSlopeNumericUpDown_ValueChanged(null, null);
			this.agcUseHangCheckBox.Checked = Utils.GetBooleanSetting("agcHang");
			this.agcUseHangCheckBox_CheckedChanged(null, null);
			this.ResetFrequency(0L);
			this.frequencyShiftNumericUpDown.Value = Utils.GetLongSetting("frequencyShift", 0L);
			this.frequencyShiftNumericUpDown_ValueChanged(null, null);
			this.frequencyShiftCheckBox.Checked = Utils.GetBooleanSetting("frequencyShiftEnabled");
			this.frequencyShiftCheckBox_CheckStateChanged(null, null);
			this.swapIQCheckBox.Checked = Utils.GetBooleanSetting("swapIQ");
			this.swapIQCheckBox_CheckedChanged(null, null);
			this.correctIQCheckBox.Checked = Utils.GetBooleanSetting("correctIQ");
			this.autoCorrectIQCheckBox_CheckStateChanged(null, null);
			this.markPeaksCheckBox.Checked = Utils.GetBooleanSetting("markPeaks");
			this.markPeaksCheckBox_CheckedChanged(null, null);
			this.fmStereoCheckBox.Checked = Utils.GetBooleanSetting("fmStereo");
			this.fmStereoCheckBox_CheckedChanged(null, null);
			this.filterAudioCheckBox.Checked = Utils.GetBooleanSetting("filterAudio");
			this.filterAudioCheckBox_CheckStateChanged(null, null);
			this.unityGainCheckBox.Checked = Utils.GetBooleanSetting("unityGain");
			this.unityGainCheckBox_CheckStateChanged(null, null);
			this.audioGainTrackBar.Value = Utils.GetIntSetting("audioGain", 50);
			this.audioGainTrackBar_ValueChanged(null, null);
			this._vfo.Muted = Utils.GetBooleanSetting("AudioIsMuted");
			this.UpdateMuteButton();
			this.latencyNumericUpDown.Value = Utils.GetIntSetting("latency", 100);
			this.sampleRateComboBox.Text = Utils.GetStringSetting("sampleRate", "48000 sample/sec");
			base.WindowState = (FormWindowState)Utils.GetIntSetting("windowState", 0);
			this._usableSpectrumWidth = Utils.GetIntSetting("spectrumWidth", 48000);
			this.spectrumAnalyzer.SpectrumWidth = this._usableSpectrumWidth;
			this.waterfall.SpectrumWidth = this._usableSpectrumWidth;
			this.lockCarrierCheckBox.Checked = Utils.GetBooleanSetting("lockCarrier");
			this.lockCarrierCheckBox_CheckedChanged(null, null);
			this.useAntiFadingCheckBox.Checked = Utils.GetBooleanSetting("useAntiFading");
			this.useAntiFadingCheckBox_CheckedChanged(null, null);
			int[] intArraySetting = Utils.GetIntArraySetting("windowPosition", null);
			if (intArraySetting != null)
			{
				this._lastLocation.X = intArraySetting[0];
				this._lastLocation.Y = intArraySetting[1];
				base.Location = this._lastLocation;
			}
			else
			{
				this._lastLocation = base.Location;
			}
			int[] intArraySetting2 = Utils.GetIntArraySetting("windowSize", null);
			if (intArraySetting2 != null)
			{
				this._lastSize.Width = intArraySetting2[0];
				this._lastSize.Height = intArraySetting2[1];
				base.Size = this._lastSize;
			}
			else
			{
				this._lastSize = base.Size;
			}
			this.spectrumSplitter.SplitPosition = Utils.GetIntSetting("splitterPosition", this.spectrumSplitter.SplitPosition);
			this._waterfallTimer = new System.Windows.Forms.Timer(this.components);
			this._waterfallTimer.Tick += this.waterfallTimer_Tick;
			this._waterfallTimer.Enabled = true;
			this._spectrumAnalyzerTimer = new System.Windows.Forms.Timer(this.components);
			this._spectrumAnalyzerTimer.Tick += this.spectrumAnalyzerTimer_Tick;
			this._spectrumAnalyzerTimer.Interval = 20;
			this._spectrumAnalyzerTimer.Enabled = true;
			this.viewComboBox.SelectedIndex = Utils.GetIntSetting("fftView", 2);
			this.fftResolutionComboBox.SelectedIndex = Utils.GetIntSetting("fftResolution", 6);
			this.fftWindowComboBox.SelectedIndex = Utils.GetIntSetting("fftWindowType", 3);
			this.spectrumStyleComboBox.SelectedIndex = Utils.GetIntSetting("spectrumStyle", 3);
			this.spectrumStyleComboBox_SelectedIndexChanged(null, null);
			this.fftSpeedTrackBar.Value = Utils.GetIntSetting("fftSpeed", 40);
			this.fftSpeedTrackBar_ValueChanged(null, null);
			this.fftContrastTrackBar.Value = Utils.GetIntSetting("fftContrast", 0);
			this.fftContrastTrackBar_Changed(null, null);
			this.spectrumAnalyzer.Attack = (float)Utils.GetDoubleSetting("spectrumAnalyzer.attack", 0.5);
			this.sAttackTrackBar.Value = (int)(this.spectrumAnalyzer.Attack * (float)this.sAttackTrackBar.Maximum);
			this.spectrumAnalyzer.Decay = (float)Utils.GetDoubleSetting("spectrumAnalyzer.decay", 0.45);
			this.sDecayTrackBar.Value = (int)(this.spectrumAnalyzer.Decay * (float)this.sDecayTrackBar.Maximum);
			this.waterfall.Attack = (float)Utils.GetDoubleSetting("waterfall.attack", 0.9);
			this.wAttackTrackBar.Value = (int)(this.waterfall.Attack * (float)this.wAttackTrackBar.Maximum);
			this.waterfall.Decay = (float)Utils.GetDoubleSetting("waterfall.decay", 0.5);
			this.wDecayTrackBar.Value = (int)(this.waterfall.Decay * (float)this.wDecayTrackBar.Maximum);
			this.useTimestampsCheckBox.Checked = Utils.GetBooleanSetting("useTimeMarkers");
			this.useTimestampCheckBox_CheckedChanged(null, null);
			this.fftOffsetTrackBar.Value = Utils.GetIntSetting("fftDisplayOffset", 0);
			this.fftOffsetTrackBar_Scroll(null, null);
			this.fftRangeTrackBar.Value = Utils.GetIntSetting("fftDisplayRange", 13);
			this.fftRangeTrackBar_Scroll(null, null);
			this.LoadSource("AIRSPY", new AirspyIO(), 2147483647);
			this.LoadSource("AIRSPY HF+", new AirspyHFIO(), 2147483647);
			this.LoadSource("Spy Server", new SpyServerIO(), 2147483647);
			this.LoadSource("UHD / USRP", "SDRSharp.USRP.UsrpIO,SDRSharp.USRP", 10);
			this.LoadSource("HackRF", "SDRSharp.HackRF.HackRFIO,SDRSharp.HackRF", 10);
			this.LoadSource("RTL-SDR (R820T)", "SDRSharp.R820T.RtlSdrIO,SDRSharp.R820T", 10);
			this.LoadSource("RTL-SDR (USB)", "SDRSharp.RTLSDR.RtlSdrIO,SDRSharp.RTLSDR", 10);
			this.LoadSource("RTL-SDR (TCP)", "SDRSharp.RTLTCP.RtlTcpIO,SDRSharp.RTLTCP", 10);
			this.LoadSource("FUNcube Dongle Pro", "SDRSharp.FUNcube.FunCubeIO,SDRSharp.FUNcube", 10);
			this.LoadSource("FUNcube Dongle Pro+", "SDRSharp.FUNcubeProPlus.FunCubeProPlusIO,SDRSharp.FUNcubeProPlus", 10);
			this.LoadSource("SoftRock (Si570)", "SDRSharp.SoftRock.SoftRockIO,SDRSharp.SoftRock", 10);
			this.LoadSource("RFSPACE SDR-IQ (USB)", "SDRSharp.SDRIQ.SdrIqIO,SDRSharp.SDRIQ", 10);
			this.LoadSource("RFSPACE Networked Radios", "SDRSharp.SDRIP.SdrIpIO,SDRSharp.SDRIP", 10);
			this.LoadSource("AFEDRI Networked Radios", "SDRSharp.AfedriSDRNet.AfedriSdrNetIO,SDRSharp.AfedriSDRNet", 10);
			this.LoadSource("File Player", "SDRSharp.WAVPlayer.WAVFileIO,SDRSharp.WAVPlayer", 10);
			NameValueCollection nameValueCollection = (NameValueCollection)ConfigurationManager.GetSection("frontendPlugins");
			foreach (string key in nameValueCollection.Keys)
			{
				string text = nameValueCollection[key];
				if (!this.IsFrontEndTypeLoaded(text))
				{
					this.LoadSource(key, text, 0);
				}
			}
			this.iqSourceComboBox.Items.Add("IQ File (*.wav)");
			this.iqSourceComboBox.Items.Add("IQ from Sound Card");
			this._waveFile = Utils.GetStringSetting("waveFile", string.Empty);
			int intSetting = Utils.GetIntSetting("iqSource", this.iqSourceComboBox.Items.Count - 1);
			this.iqSourceComboBox.SelectedIndex = ((intSetting < this.iqSourceComboBox.Items.Count) ? intSetting : (this.iqSourceComboBox.Items.Count - 1));
			this.ResetFrequency(Utils.GetLongSetting("centerFrequency", this._centerFrequency));
			long longSetting = Utils.GetLongSetting("vfo", this._centerFrequency);
			this.vfoFrequencyEdit.Frequency = longSetting;
			this._tooltip.SetToolTip(this.playStopButton, "Start");
			this._tooltip.SetToolTip(this.logoPictureBox, "Visit our website and check our high performance radios!");
			this.UpdateTuningStyle();
			bool visible = !Utils.GetBooleanSetting("menuIsHidden");
			this.scrollPanel.Visible = visible;
			this.menuSpacerPanel.Visible = visible;
			this.rightTableLayoutPanel.Visible = visible;
			this._tooltip.SetToolTip(this.toggleMenuButton, "Menu");
			this._initializing = false;
		}

		private void LoadSource(string name, string fqdn, int access)
		{
			try
			{
				IFrontendController controller = (IFrontendController)this.LoadExtension(fqdn);
				this.LoadSource(name, controller, access);
			}
			catch (Exception)
			{
			}
		}

		private void LoadSource(string name, IFrontendController controller, int access)
		{
			if (access > 0)
			{
				ISampleRateChangeSource sampleRateChangeSource = controller as ISampleRateChangeSource;
				if (sampleRateChangeSource != null)
				{
					sampleRateChangeSource.SampleRateChanged += this.frontendController_SampleRateChanged;
				}
				IFFTSource iFFTSource = controller as IFFTSource;
				if (iFFTSource != null)
				{
					iFFTSource.FFTAvailable += this.frontendController_FFTAvailable;
				}
				IControlAwareObject controlAwareObject = controller as IControlAwareObject;
				if (controlAwareObject != null)
				{
					controlAwareObject.SetControl(this._sharpControlProxy);
				}
				this._builtinControllers.Add(controller);
			}
			this._frontendControllers.Add(name, controller);
			this.iqSourceComboBox.Items.Add(name);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.InitialiseSharpPlugins();
			this.scrollPanel.VerticalScroll.Value = Utils.GetIntSetting("menuPosition", 0);
		}

		private bool IsFrontEndTypeLoaded(string fqtn)
		{
			fqtn = fqtn.Replace(" ", string.Empty);
			foreach (IFrontendController value in this._frontendControllers.Values)
			{
				if (value.GetType().AssemblyQualifiedName.Replace(" ", string.Empty).StartsWith(fqtn))
				{
					return true;
				}
			}
			return false;
		}

		private object LoadExtension(string fqtn)
		{
			string[] array = fqtn.Split(',');
			string typeName = array[0].Trim();
			string assemblyString = array[1].Trim();
			Assembly assembly = Assembly.Load(assemblyString);
			return assembly.CreateInstance(typeName);
		}

		private void frontendController_SampleRateChanged(object sender, EventArgs e)
		{
			if (base.InvokeRequired)
			{
				base.BeginInvoke((Action)delegate
				{
					this.frontendController_SampleRateChanged(sender, e);
				});
			}
			else
			{
				if (this._streamControl.IsPlaying)
				{
					this._changingSampleRate = true;
					try
					{
						this.StopRadio();
						this.StartRadio();
					}
					catch (ApplicationException ex)
					{
						this.StopRadio();
						MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					finally
					{
						this._changingSampleRate = false;
					}
				}
				this.ResetFrequency(this._centerFrequency);
				this.fftZoomTrackBar.Value = 0;
			}
		}

		private void MainForm_Closing(object sender, CancelEventArgs e)
		{
			this._terminated = true;
			this._streamControl.Stop();
			this._fftEvent.Set();
			if (this._frontendController != null)
			{
				this._frontendController.Close();
			}
			foreach (ISharpPlugin value in this._sharpPlugins.Values)
			{
				value.Close();
			}
			this._modeStates[this._vfo.DetectorType] = this.GetModeState();
			Utils.SaveSetting("spectrumAnalyzer.attack", (float)this.sAttackTrackBar.Value / (float)this.sAttackTrackBar.Maximum);
			Utils.SaveSetting("spectrumAnalyzer.decay", (float)this.sDecayTrackBar.Value / (float)this.sDecayTrackBar.Maximum);
			Utils.SaveSetting("waterfall.Attack", (float)this.wAttackTrackBar.Value / (float)this.wAttackTrackBar.Maximum);
			Utils.SaveSetting("waterfall.decay", (float)this.wDecayTrackBar.Value / (float)this.wDecayTrackBar.Maximum);
			Utils.SaveSetting("useTimeMarkers", this.useTimestampsCheckBox.Checked);
			Utils.SaveSetting("fftSpeed", this.fftSpeedTrackBar.Value);
			Utils.SaveSetting("fftContrast", this.fftContrastTrackBar.Value);
			Utils.SaveSetting("fftWindowType", this.fftWindowComboBox.SelectedIndex);
			Utils.SaveSetting("spectrumStyle", this.spectrumStyleComboBox.SelectedIndex);
			Utils.SaveSetting("fftView", this.viewComboBox.SelectedIndex);
			Utils.SaveSetting("fftResolution", this.fftResolutionComboBox.SelectedIndex);
			Utils.SaveSetting("detectorType", (int)this.DetectorType);
			Utils.SaveSetting("useAGC", this.agcCheckBox.Checked);
			Utils.SaveSetting("agcThreshold", (int)this.agcThresholdNumericUpDown.Value);
			Utils.SaveSetting("agcDecay", (int)this.agcDecayNumericUpDown.Value);
			Utils.SaveSetting("agcSlope", (int)this.agcSlopeNumericUpDown.Value);
			Utils.SaveSetting("agcHang", this.agcUseHangCheckBox.Checked);
			Utils.SaveSetting("frequencyShift", (long)this.frequencyShiftNumericUpDown.Value);
			Utils.SaveSetting("frequencyShiftEnabled", this.frequencyShiftCheckBox.Checked);
			Utils.SaveSetting("swapIQ", this.swapIQCheckBox.Checked);
			Utils.SaveSetting("correctIQ", this.correctIQCheckBox.Checked);
			Utils.SaveSetting("markPeaks", this.markPeaksCheckBox.Checked);
			Utils.SaveSetting("fmStereo", this.fmStereoCheckBox.Checked);
			Utils.SaveSetting("filterAudio", this.filterAudioCheckBox.Checked);
			Utils.SaveSetting("unityGain", this.unityGainCheckBox.Checked);
			Utils.SaveSetting("latency", (int)this.latencyNumericUpDown.Value);
			Utils.SaveSetting("sampleRate", this.sampleRateComboBox.Text);
			Utils.SaveSetting("audioGain", this.audioGainTrackBar.Value);
			Utils.SaveSetting("AudioIsMuted", this._vfo.Muted);
			Utils.SaveSetting("windowState", (int)base.WindowState);
			Utils.SaveSetting("windowPosition", Utils.IntArrayToString(this._lastLocation.X, this._lastLocation.Y));
			Utils.SaveSetting("windowSize", Utils.IntArrayToString(this._lastSize.Width, this._lastSize.Height));
			Utils.SaveSetting("collapsiblePanelStates", Utils.IntArrayToString(this.GetCollapsiblePanelStates()));
			Utils.SaveSetting("splitterPosition", this.spectrumSplitter.SplitPosition);
			Utils.SaveSetting("iqSource", this.iqSourceComboBox.SelectedIndex);
			Utils.SaveSetting("waveFile", this._waveFile ?? "");
			Utils.SaveSetting("centerFrequency", this._centerFrequency);
			Utils.SaveSetting("vfo", this.vfoFrequencyEdit.Frequency);
			Utils.SaveSetting("tuningStyle", (int)this._tuningStyle);
			Utils.SaveSetting("fftDisplayOffset", this.fftOffsetTrackBar.Value);
			Utils.SaveSetting("fftDisplayRange", this.fftRangeTrackBar.Value);
			Utils.SaveSetting("inputDevice", this.inputDeviceComboBox.SelectedItem);
			Utils.SaveSetting("outputDevice", this.outputDeviceComboBox.SelectedItem);
			Utils.SaveSetting("spectrumWidth", this.spectrumAnalyzer.SpectrumWidth);
			Utils.SaveSetting("lockCarrier", this.lockCarrierCheckBox.Checked);
			Utils.SaveSetting("useAntiFading", this.useAntiFadingCheckBox.Checked);
			Utils.SaveSetting("menuPosition", this.scrollPanel.VerticalScroll.Value);
			Utils.SaveSetting("wfmState", Utils.IntArrayToString(this._modeStates[DetectorType.WFM]));
			Utils.SaveSetting("nfmState", Utils.IntArrayToString(this._modeStates[DetectorType.NFM]));
			Utils.SaveSetting("amState", Utils.IntArrayToString(this._modeStates[DetectorType.AM]));
			Utils.SaveSetting("lsbState", Utils.IntArrayToString(this._modeStates[DetectorType.LSB]));
			Utils.SaveSetting("usbState", Utils.IntArrayToString(this._modeStates[DetectorType.USB]));
			Utils.SaveSetting("dsbState", Utils.IntArrayToString(this._modeStates[DetectorType.DSB]));
			Utils.SaveSetting("cwState", Utils.IntArrayToString(this._modeStates[DetectorType.CW]));
			Utils.SaveSetting("rawState", Utils.IntArrayToString(this._modeStates[DetectorType.RAW]));
			if (this._oldTopSplitterPosition > 0)
			{
				Utils.SaveSetting("topSplitter", this._oldTopSplitterPosition);
			}
			if (this._oldBottomSplitterPosition > 0)
			{
				Utils.SaveSetting("bottomSplitter", this._oldBottomSplitterPosition);
			}
			if (this._oldLeftSplitterPosition > 0)
			{
				Utils.SaveSetting("leftSplitter", this._oldLeftSplitterPosition);
			}
			if (this._oldRightSplitterPosition > 0)
			{
				Utils.SaveSetting("rightSplitter", this._oldRightSplitterPosition);
			}
			Utils.SaveSetting("menuIsHidden", !this.scrollPanel.Visible);
		}

		private void toggleMenuButton_Click(object sender, EventArgs e)
		{
			this.scrollPanel.Visible = !this.scrollPanel.Visible;
			this.menuSpacerPanel.Visible = this.scrollPanel.Visible;
			this.rightTableLayoutPanel.Visible = this.scrollPanel.Visible;
		}

		private unsafe void ProcessBuffer(Complex* iqBuffer, float* audioBuffer, int length)
		{
			if (!this.UseFFTSource && this.spectrumPanel.Visible)
			{
				this._inputBufferLength = length;
				this._fftStream.Write(iqBuffer, length);
			}
			this._vfo.ProcessBuffer(iqBuffer, audioBuffer, length);
		}

		private unsafe void ProcessFFT(object parameter)
		{
			this._fftIsRunning = true;
			while (this._streamControl.IsPlaying && this.spectrumPanel.Visible)
			{
				this._fftResolutionLock.AcquireReaderLock(300000);
				double num = (double)this._fftBins / ((double)this._waterfallTimer.Interval * 0.001);
				double num2 = this._streamControl.SampleRate / num;
				int num3 = (int)((double)this._fftBins * num2);
				int num4 = Math.Min(num3, this._fftBins);
				int count = num3 - num4;
				if (num4 < this._fftBins)
				{
					Utils.Memcpy(this._iqPtr, this._iqPtr + num4, (this._fftBins - num4) * sizeof(Complex));
				}
				int num5 = num4;
				int num6 = 0;
				while (this._streamControl.IsPlaying && !this._terminated && num6 < num5)
				{
					int count2 = num5 - num6;
					num6 += this._fftStream.Read(this._iqPtr, this._fftBins - num5 + num6, count2);
				}
				if (this._streamControl.IsPlaying && !this._terminated)
				{
					this._fftStream.Advance(count);
					float num7 = (float)(10.0 * Math.Log10((double)this._fftBins / 2.0));
					float offset = 24f - num7 + this._fftOffset;
					Utils.Memcpy(this._fftPtr, this._iqPtr, this._fftBins * sizeof(Complex));
					Fourier.ApplyFFTWindow(this._fftPtr, this._fftWindowPtr, this._fftBins);
					Fourier.ForwardTransform(this._fftPtr, this._fftBins, true);
					Fourier.SpectrumPower(this._fftPtr, this._fftSpectrumPtr, this._fftBins, offset);
					if (num4 < this._fftBins)
					{
						int num8 = this._fftBins - num4;
						while (this._streamControl.IsPlaying && !this._terminated && this._fftStream.Length > this._inputBufferLength * 2 && this._fftStream.Length >= num8)
						{
							this._fftStream.Read(this._iqPtr + num4, num8);
						}
					}
					else
					{
						while (this._streamControl.IsPlaying && !this._terminated && this._fftStream.Length > this._inputBufferLength * 2 && this._fftStream.Length >= this._fftBins)
						{
							this._fftStream.Advance(this._fftBins);
						}
					}
					this._fftResolutionLock.ReleaseReaderLock();
					if (this._streamControl.IsPlaying && !this._terminated)
					{
						this._fftEvent.WaitOne();
					}
					continue;
				}
				this._fftResolutionLock.ReleaseReaderLock();
				break;
			}
			this._fftStream.Flush();
			this._fftIsRunning = false;
		}

		private unsafe void frontendController_FFTAvailable(object sender, ByteSamplesEventArgs e)
		{
			IFFTSource iFFTSource = this._frontendController as IFFTSource;
			if (this._fftFrames == null || this._fftFrames.BufferSize != e.Length)
			{
				this._fftFrames = new FloatCircularBuffer(e.Length, 3);
			}
			float* ptr = stackalloc float[e.Length];
			for (int i = 0; i < e.Length; i++)
			{
				ptr[i] = (float)(e.Buffer[i] * iFFTSource.FFTRange) / 255f - (float)iFFTSource.FFTRange + (float)iFFTSource.FFTOffset;
			}
			if (!this._fftFrames.Write(ptr, e.Length, false) && this._fftcorrectionFPS < 10)
			{
				this._fftcorrectionFPS++;
			}
			Interlocked.Increment(ref this._fftFramesCount);
		}

		private unsafe void RenderFFT()
		{
			if (this.spectrumPanel.Visible)
			{
				if (this.UseFFTSource)
				{
					FloatCircularBuffer fftFrames = this._fftFrames;
					if (fftFrames != null)
					{
						float* ptr = fftFrames.Acquire(false);
						if (ptr != null)
						{
							Utils.Memcpy(this._fftDisplayPtr, ptr, fftFrames.BufferSize * 4);
							fftFrames.Release();
						}
					}
				}
				if (this.waterfall.Visible)
				{
					this._fftResolutionLock.AcquireReaderLock(300000);
					this.waterfall.Render(this._fftDisplayPtr, this._fftDisplaySize);
					this._fftResolutionLock.ReleaseReaderLock();
				}
			}
		}

		private void waterfallTimer_Tick(object sender, EventArgs e)
		{
			if (this._streamControl.IsPlaying)
			{
				this.RenderFFT();
				if (this.UseFFTSource)
				{
					DateTime now = DateTime.Now;
					float num = (float)(now - this._lastFFTTick).TotalMilliseconds;
					this._lastFFTTick = now;
					int num2 = Interlocked.Exchange(ref this._fftFramesCount, 0);
					if (num2 == 0 && this._fftcorrectionFPS > -10)
					{
						this._fftcorrectionFPS--;
					}
					float num3 = (float)num2 / (num * 0.001f);
					this._fftAverageFPS += 0.1f * (num3 - this._fftAverageFPS);
					int num4 = (int)this._fftAverageFPS + this._fftcorrectionFPS;
					if (num4 > 0 && num4 <= 1000)
					{
						this._waterfallTimer.Interval = 1000 / num4;
					}
				}
				else
				{
					this._fftEvent.Set();
				}
			}
		}

		private unsafe void spectrumAnalyzerTimer_Tick(object sender, EventArgs e)
		{
			if (this._streamControl.IsPlaying && this.spectrumAnalyzer.Visible && this.spectrumPanel.Visible)
			{
				this._fftResolutionLock.AcquireReaderLock(300000);
				this.spectrumAnalyzer.Render(this._fftDisplayPtr, this._fftDisplaySize);
				this._fftResolutionLock.ReleaseReaderLock();
			}
		}

		private void iqTimer_Tick(object sender, EventArgs e)
		{
			if (this._vfo.DetectorType == DetectorType.WFM)
			{
				if (this._vfo.SignalIsStereo)
				{
					this.spectrumAnalyzer.StatusText = "((( " + this._vfo.RdsStationName + " )))";
				}
				else
				{
					this.spectrumAnalyzer.StatusText = this._vfo.RdsStationName;
				}
				if (this._vfo.RdsPICode != 0)
				{
					SpectrumAnalyzer obj = this.spectrumAnalyzer;
					obj.StatusText = obj.StatusText + " - " + string.Format("{0:X4}", this._vfo.RdsPICode);
				}
				if (!string.IsNullOrEmpty(this._vfo.RdsStationText))
				{
					SpectrumAnalyzer obj2 = this.spectrumAnalyzer;
					obj2.StatusText = obj2.StatusText + " - " + this._vfo.RdsStationText;
				}
			}
		}

		private unsafe void BuildFFTWindow()
		{
			if (!this.UseFFTSource)
			{
				float[] array = FilterBuilder.MakeWindow(this._fftWindowType, this._fftBins);
				float[] array2 = array;
				fixed (float* src = array2)
				{
					Utils.Memcpy(this._fftWindow, src, this._fftBins * 4);
				}
			}
		}

		private unsafe void InitFFTBuffers()
		{
			int length = this.UseFFTSource ? (this._frontendController as IFFTSource).DisplayPixels : this._fftBins;
			this._iqBuffer = null;
			this._fftBuffer = null;
			this._fftWindow = null;
			this._fftSpectrum = null;
			this._scaledFFTSpectrum = null;
			GC.Collect();
			this._iqBuffer = UnsafeBuffer.Create(length, sizeof(Complex));
			this._fftBuffer = UnsafeBuffer.Create(length, sizeof(Complex));
			this._fftWindow = UnsafeBuffer.Create(length, 4);
			this._fftSpectrum = UnsafeBuffer.Create(length, 4);
			this._scaledFFTSpectrum = UnsafeBuffer.Create(length, 1);
			this._iqPtr = (Complex*)(void*)this._iqBuffer;
			this._fftPtr = (Complex*)(void*)this._fftBuffer;
			this._fftWindowPtr = (float*)(void*)this._fftWindow;
			this._fftSpectrumPtr = (float*)(void*)this._fftSpectrum;
			this._scaledFFTSpectrumPtr = (byte*)(void*)this._scaledFFTSpectrum;
			this.InitDisplayFFT();
		}

		private unsafe void InitDisplayFFT()
		{
			if (this.UseFFTSource)
			{
				this._fftDisplaySize = (this._frontendController as IFFTSource).DisplayPixels;
				this._fftDisplayPtr = this._fftSpectrumPtr;
			}
			else
			{
				double num = 0.5 * (double)this._fftBins * (1.0 - (double)this._usableSpectrumWidth / this._vfo.SampleRate);
				double num2 = (double)this._fftBins / this._vfo.SampleRate * (double)this._ifOffset;
				double a = num - num2;
				this._fftDisplaySize = (int)((double)this._fftBins - 2.0 * num);
				this._fftDisplayPtr = this._fftSpectrumPtr + (int)Math.Ceiling(a);
			}
		}

		private void iqSourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.sourceCollapsiblePanel.PanelTitle = "Source: " + this.iqSourceComboBox.SelectedItem;
			this.Text = MainForm._baseTitle + " - " + this.iqSourceComboBox.SelectedItem;
			if (this._streamControl.IsPlaying)
			{
				this.StopRadio();
			}
			try
			{
				this.Open(true);
			}
			catch (Exception ex)
			{
				if (this.SourceIsWaveFile && !this._initializing)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			this.NotifyPropertyChanged("SourceName");
		}

		private void SelectWaveFile()
		{
			if (this.openDlg.ShowDialog() == DialogResult.OK)
			{
				this.StopRadio();
				this._waveFile = this.openDlg.FileName;
			}
		}

		private void OpenWaveSource(bool refreshSource)
		{
			if (!this._initializing & refreshSource)
			{
				this.SelectWaveFile();
			}
			this._tooltip.SetToolTip(this.configureSourceButton, "Select File");
			this.configureSourceButton.Enabled = true;
			this.sampleRateComboBox.Enabled = false;
			this.inputDeviceComboBox.Enabled = false;
			this.outputDeviceComboBox.Enabled = true;
			this.latencyNumericUpDown.Enabled = true;
			this.frequencyShiftCheckBox.Enabled = false;
			this.frequencyShiftCheckBox.Checked = false;
			this.frequencyShiftNumericUpDown.Enabled = false;
			this._tuningStyle = TuningStyle.Free;
			this.UpdateTuningStyle();
			StreamControl.ReducedBandwidth = false;
			AudioDevice audioDevice = (AudioDevice)this.outputDeviceComboBox.SelectedItem;
			if (audioDevice == null)
			{
				throw new ApplicationException("No audio playback device found.");
			}
			this._streamControl.OpenFile(this._waveFile, audioDevice.Index, (int)this.latencyNumericUpDown.Value);
			string input = Path.GetFileName(this._waveFile) ?? "";
			Match match = Regex.Match(input, "([0-9]+)kHz", RegexOptions.IgnoreCase);
			long num;
			if (match.Success)
			{
				num = int.Parse(match.Groups[1].Value) * 1000;
			}
			else
			{
				match = Regex.Match(input, "([\\-0-9]+)Hz", RegexOptions.IgnoreCase);
				num = ((!match.Success) ? 0 : long.Parse(match.Groups[1].Value));
			}
			this._ifOffset = 0;
			if (num > 0)
			{
				match = Regex.Match(input, "([\\-0-9]+)o", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					this._ifOffset = int.Parse(match.Groups[1].Value);
					num -= this._ifOffset;
				}
			}
			if (refreshSource)
			{
				this.ResetFrequency(num);
			}
			this._usableSpectrumRatio = 0.9f;
			this._usableSpectrumWidth = (int)Math.Ceiling(this._streamControl.SampleRate * (double)this._usableSpectrumRatio);
			this.NotifyPropertyChanged("TunableBandwidth");
			this.InitDisplayFFT();
			this.NotifyPropertyChanged("IFOffset");
		}

		private void OpenSoundCardSource()
		{
			this._tooltip.SetToolTip(this.configureSourceButton, string.Empty);
			this.configureSourceButton.Enabled = false;
			this.inputDeviceComboBox.Enabled = true;
			this.outputDeviceComboBox.Enabled = true;
			this.sampleRateComboBox.Enabled = true;
			this.frequencyShiftCheckBox.Checked = false;
			this.frequencyShiftCheckBox.Enabled = false;
			this.frequencyShiftNumericUpDown.Enabled = false;
			this.tuningStyleButton.Enabled = false;
			this._tuningStyle = TuningStyle.Free;
			this.UpdateTuningStyle();
			StreamControl.ReducedBandwidth = false;
			AudioDevice audioDevice = (AudioDevice)this.outputDeviceComboBox.SelectedItem;
			if (audioDevice == null)
			{
				throw new ApplicationException("No audio playback device found.");
			}
			AudioDevice audioDevice2 = (AudioDevice)this.inputDeviceComboBox.SelectedItem;
			if (audioDevice == null)
			{
				throw new ApplicationException("No audio recording device found.");
			}
			double audioInputSampleRate = this.GetAudioInputSampleRate();
			this._streamControl.OpenSoundDevice(audioDevice2.Index, audioDevice.Index, audioInputSampleRate, (int)this.latencyNumericUpDown.Value);
			this._usableSpectrumRatio = 0.9f;
			this._usableSpectrumWidth = (int)Math.Ceiling(this._streamControl.SampleRate * (double)this._usableSpectrumRatio);
			this.NotifyPropertyChanged("TunableBandwidth");
			this._ifOffset = 0;
			this.NotifyPropertyChanged("IFOffset");
			this.ResetFrequency(0L);
		}

		private void OpenFrontEndSource(bool refreshSource, FrequencyInitType init)
		{
			AudioDevice audioDevice = (AudioDevice)this.outputDeviceComboBox.SelectedItem;
			if (audioDevice == null)
			{
				throw new ApplicationException("No audio playback device found.");
			}
			string key = (string)this.iqSourceComboBox.SelectedItem;
			this._frontendController = this._frontendControllers[key];
			this.UpdateVFOSource();
			if (refreshSource)
			{
				try
				{
					this._frontendController.Open();
				}
				catch (Exception)
				{
				}
				if (this._builtinControllers.Contains(this._frontendController))
				{
					IConfigurationPanelProvider configurationPanelProvider = this._frontendController as IConfigurationPanelProvider;
					if (configurationPanelProvider != null && configurationPanelProvider.Gui != null)
					{
						this.ShowControllerPanel(configurationPanelProvider.Gui);
					}
				}
			}
			bool flag = this._frontendController is ITunableSource && ((ITunableSource)this._frontendController).CanTune;
			if (flag)
			{
				this.tuningStyleButton.Enabled = true;
			}
			else
			{
				this.frequencyShiftCheckBox.Checked = false;
				this.tuningStyleButton.Enabled = false;
				this._tuningStyle = TuningStyle.Free;
				this.UpdateTuningStyle();
			}
			this._tooltip.SetToolTip(this.configureSourceButton, "Configure Source");
			this.configureSourceButton.Enabled = true;
			this.frequencyShiftCheckBox.Enabled = flag;
			this.frequencyShiftNumericUpDown.Enabled = this.frequencyShiftCheckBox.Checked;
			this.sampleRateComboBox.Enabled = (this._frontendController is ISoundcardController);
			this.inputDeviceComboBox.Enabled = (this._frontendController is ISoundcardController);
			this.outputDeviceComboBox.Enabled = true;
			if (this._frontendController is ISpectrumProvider)
			{
				this._usableSpectrumRatio = ((ISpectrumProvider)this._frontendController).UsableSpectrumRatio;
			}
			else
			{
				this._usableSpectrumRatio = 0.9f;
			}
			if (this._frontendController is ISoundcardController)
			{
				string soundCardHint = ((ISoundcardController)this._frontendController).SoundCardHint;
				if (!string.IsNullOrEmpty(soundCardHint))
				{
					Regex regex = new Regex(soundCardHint, RegexOptions.IgnoreCase);
					for (int i = 0; i < this.inputDeviceComboBox.Items.Count; i++)
					{
						string input = this.inputDeviceComboBox.Items[i].ToString();
						if (regex.IsMatch(input))
						{
							this.inputDeviceComboBox.SelectedIndex = i;
							break;
						}
					}
				}
				AudioDevice audioDevice2 = (AudioDevice)this.inputDeviceComboBox.SelectedItem;
				if (audioDevice == null)
				{
					throw new ApplicationException("No audio recording device found.");
				}
				double num;
				if (refreshSource && !this._initializing)
				{
					num = ((ISoundcardController)this._frontendController).SampleRateHint;
					this.sampleRateComboBox.Text = num + " sample/sec";
				}
				else
				{
					Match match = Regex.Match(this.sampleRateComboBox.Text, "([0-9\\.]+)", RegexOptions.IgnoreCase);
					num = ((!match.Success) ? 48000.0 : double.Parse(match.Groups[1].Value));
				}
				this._streamControl.OpenSoundDevice(audioDevice2.Index, audioDevice.Index, num, (int)this.latencyNumericUpDown.Value);
			}
			else
			{
				this._streamControl.OpenPlugin(this._frontendController, audioDevice.Index, (int)this.latencyNumericUpDown.Value);
			}
			if (this.UseFFTSource)
			{
				this._usableSpectrumWidth = (this._frontendController as IFFTSource).DisplayBandwidth;
			}
			else
			{
				this._usableSpectrumWidth = (int)Math.Ceiling((double)this._usableSpectrumRatio * this._streamControl.SampleRate);
			}
			if (this._frontendController is IIQStreamController)
			{
				int num2 = (this._frontendController is IFrontendOffset) ? ((IFrontendOffset)this._frontendController).Offset : 0;
				this._ifOffset = (((((IIQStreamController)this._frontendController).Samplerate - (double)this._usableSpectrumWidth) * 0.5 >= (double)Math.Abs(num2)) ? num2 : 0);
			}
			else
			{
				this._ifOffset = 0;
			}
			switch (init)
			{
			case FrequencyInitType.Vfo:
				this.ResetFrequency(this.vfoFrequencyEdit.Frequency);
				break;
			case FrequencyInitType.Device:
				if (this._frontendController is ITunableSource)
				{
					this.ResetFrequency(((ITunableSource)this._frontendController).Frequency - this._ifOffset);
				}
				break;
			}
			this.NotifyPropertyChanged("IFOffset");
			this.NotifyPropertyChanged("TunableBandwidth");
		}

		public void RefreshSource(bool reload)
		{
			this.Open(reload);
		}

		private void Open(bool refreshSource)
		{
			bool flag = true;
			if (refreshSource)
			{
				string text = (string)this.iqSourceComboBox.SelectedItem;
				if (text != this._lastSourceName)
				{
					if (this._frontendController != null)
					{
						if (this._frontendController is IFloatingConfigDialogProvider)
						{
							((IFloatingConfigDialogProvider)this._frontendController).HideSettingGUI();
						}
						this._frontendController.Close();
						if (this._builtinControllers.Contains(this._frontendController) && this._frontendController is IConfigurationPanelProvider)
						{
							IConfigurationPanelProvider configurationPanelProvider = (IConfigurationPanelProvider)this._frontendController;
							if (configurationPanelProvider.Gui != null)
							{
								this.HideControllerPanel(configurationPanelProvider.Gui);
							}
						}
						this._frontendController = null;
					}
					this._lastSourceName = text;
					flag = false;
				}
			}
			if (this.SourceIsWaveFile)
			{
				this.OpenWaveSource(refreshSource);
			}
			else if (this.SourceIsSoundCard)
			{
				this.OpenSoundCardSource();
			}
			else
			{
				FrequencyInitType init = flag ? ((!(this._frontendController is IFFTSource) || (this._frontendController as IFFTSource).FFTEnabled) ? (this._changingSampleRate ? FrequencyInitType.Vfo : FrequencyInitType.None) : FrequencyInitType.None) : FrequencyInitType.Device;
				this.OpenFrontEndSource(refreshSource, init);
			}
			if (this.UseFFTSource)
			{
				this.fftResolutionComboBox.Enabled = false;
				this.fftWindowComboBox.Enabled = false;
				this.fftSpeedTrackBar.Enabled = false;
				this.sAttackTrackBar.Enabled = false;
				this.sDecayTrackBar.Enabled = false;
				this.wAttackTrackBar.Enabled = false;
				this.wDecayTrackBar.Enabled = false;
				this.spectrumAnalyzer.Attack = 0.25f;
				this.spectrumAnalyzer.Decay = 0.15f;
				this.waterfall.Attack = 0.95f;
				this.waterfall.Decay = 0.95f;
				this._fftAverageFPS = 50f;
				this._fftcorrectionFPS = 0;
				this._lastFFTTick = DateTime.Now;
			}
			else
			{
				this.fftResolutionComboBox.Enabled = true;
				this.fftWindowComboBox.Enabled = true;
				this.fftSpeedTrackBar.Enabled = true;
				this.sAttackTrackBar.Enabled = true;
				this.sDecayTrackBar.Enabled = true;
				this.wAttackTrackBar.Enabled = true;
				this.fftSpeedTrackBar_ValueChanged(null, null);
				this.sAttackTrackBar_ValueChanged(null, null);
				this.sDecayTrackBar_ValueChanged(null, null);
				this.wAttackTrackBar_ValueChanged(null, null);
				this.wDecayTrackBar.Enabled = true;
				this.wDecayTrackBar_ValueChanged(null, null);
			}
			if (this._streamControl.SampleRate > 0.0)
			{
				this._vfo.SampleRate = this._streamControl.SampleRate;
				this._vfo.DecimationStageCount = this._streamControl.DecimationStageCount;
				this.spectrumAnalyzer.SpectrumWidth = this._usableSpectrumWidth;
				this.waterfall.SpectrumWidth = this._usableSpectrumWidth;
				this.UpdateFilterBandwidth();
			}
			if (refreshSource)
			{
				this.fftZoomTrackBar.Value = 0;
				this.fftZoomTrackBar_ValueChanged(null, null);
			}
			this._frequencySet = 0L;
			this.InitFFTBuffers();
			this.BuildFFTWindow();
			this.UpdateVfoFrequency();
			this._sharpControlProxy.Enabled = (this.SourceIsSoundCard || this.SourceIsWaveFile || this._builtinControllers.Contains(this._frontendController));
			this._iqBalancerProcessor.Engine.Enabled = (this.correctIQCheckBox.Checked && this._sharpControlProxy.Enabled);
			this.spectrumAnalyzer.EnableSNR = this._sharpControlProxy.Enabled;
			this._vfo.HookdEnabled = this._sharpControlProxy.Enabled;
			if (this._vfo.SampleRate > 0.0)
			{
				this._vfo.Init();
			}
			this._vfo.RdsReset();
		}

		private double GetAudioInputSampleRate()
		{
			double result = 0.0;
			Match match = Regex.Match(this.sampleRateComboBox.Text, "([0-9\\.]+)", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				result = double.Parse(match.Groups[1].Value);
			}
			return result;
		}

		private void audioGainTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this._streamControl.AudioGain = (float)this.audioGainTrackBar.Value;
			if (this.audioGainTrackBar.Value == this.audioGainTrackBar.Minimum)
			{
				this._vfo.Muted = true;
				this._tooltip.SetToolTip(this.audioGainTrackBar, "Muted");
			}
			else
			{
				this._vfo.Muted = false;
				this._tooltip.SetToolTip(this.audioGainTrackBar, this.audioGainTrackBar.Value + " dB");
			}
			this.UpdateMuteButton();
			this.NotifyPropertyChanged("AudioGain");
		}

		private void filterAudioCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			this._vfo.FilterAudio = this.filterAudioCheckBox.Checked;
			this.NotifyPropertyChanged("FilterAudio");
		}

		private void unityGainCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			this._streamControl.ScaleOutput = !this.unityGainCheckBox.Checked;
			this.audioGainTrackBar.Enabled = !this.unityGainCheckBox.Checked;
			this.NotifyPropertyChanged("UnityGain");
		}

		private void muteButton_Click(object sender, EventArgs e)
		{
			if (this._vfo.Muted && this.audioGainTrackBar.Value == this.audioGainTrackBar.Minimum)
			{
				this.audioGainTrackBar.Value = 30;
				this._vfo.Muted = false;
			}
			else
			{
				this._vfo.Muted = !this._vfo.Muted;
			}
			this.UpdateMuteButton();
		}

		private void UpdateMuteButton()
		{
			if (this._vfo.Muted)
			{
				this.muteButton.Image = Resources.audio_muted;
				this._tooltip.SetToolTip(this.muteButton, "Unmute");
			}
			else
			{
				this.muteButton.Image = Resources.audio_unmuted;
				this._tooltip.SetToolTip(this.muteButton, "Mute");
			}
		}

		private void playStopButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (this._streamControl.IsPlaying)
				{
					this.StopRadio();
				}
				else
				{
					this.StartRadio();
				}
			}
			catch (Exception ex)
			{
				this.StopRadio();
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void centerButton_Click(object sender, EventArgs e)
		{
			int num = (int)(this._tuningStyle = (TuningStyle)((int)(this._tuningStyle + 1) % 3));
			this.UpdateTuningStyle();
		}

		private void UpdateTuningStyle()
		{
			this.tuningStyleButton.Enabled = !this._tuningStyleFreezed;
			switch (this._tuningStyle)
			{
			case TuningStyle.Free:
				this.tuningStyleButton.Image = Resources.free_tuning;
				this._tooltip.SetToolTip(this.tuningStyleButton, "Free tuning");
				break;
			case TuningStyle.Sticky:
				this.tuningStyleButton.Image = Resources.sticky;
				this._tooltip.SetToolTip(this.tuningStyleButton, "Sticky tuning");
				break;
			case TuningStyle.Center:
				this.tuningStyleButton.Image = Resources.center_24;
				this._tooltip.SetToolTip(this.tuningStyleButton, "Center tuning");
				if (this.SourceIsTunable)
				{
					this.ResetFrequency(this.Frequency);
				}
				this.waterfall.CenterZoom();
				this.spectrumAnalyzer.CenterZoom();
				break;
			}
			this.NotifyPropertyChanged("TuningStyle");
		}

		private void UpdateVfoFrequency()
		{
			if (this.UseFFTSource && this._frontendController is IVFOSource)
			{
				IVFOSource iVFOSource = this._frontendController as IVFOSource;
				iVFOSource.VFOFrequency = this.vfoFrequencyEdit.Frequency - this._frequencyShift;
				this._vfo.Frequency = 0;
			}
			else
			{
				this._vfo.Frequency = (int)(this.vfoFrequencyEdit.Frequency - this._centerFrequency);
			}
		}

		private void vfoFrequencyEdit_FrequencyChanged(object sender, EventArgs e)
		{
			this.waterfall.Frequency = this.vfoFrequencyEdit.Frequency;
			this.spectrumAnalyzer.Frequency = this.vfoFrequencyEdit.Frequency;
			if (this._tuningStyle == TuningStyle.Center)
			{
				this.waterfall.CenterZoom();
				this.spectrumAnalyzer.CenterZoom();
			}
			this.UpdateVfoFrequency();
			this._vfo.IFOffset = -this._ifOffset;
			if (this._vfo.DetectorType == DetectorType.WFM)
			{
				this._vfo.RdsReset();
			}
			else
			{
				this._vfo.CarrierLockerReset();
			}
			this.NotifyPropertyChanged("Frequency");
		}

		private void vfoFrequencyEdit_FrequencyChanging(object sender, FrequencyChangingEventArgs e)
		{
			if (!this._initializing)
			{
				if (!this.SourceIsTunable)
				{
					float num = (float)this.spectrumAnalyzer.DisplayCenterFrequency - 0.5f * (float)this.spectrumAnalyzer.DisplayedBandwidth;
					float num2 = (float)this.spectrumAnalyzer.DisplayCenterFrequency + 0.5f * (float)this.spectrumAnalyzer.DisplayedBandwidth;
					if ((float)e.Frequency > num2)
					{
						e.Frequency = (long)num2;
					}
					else if ((float)e.Frequency < num)
					{
						e.Frequency = (long)num;
					}
				}
				else
				{
					e.Frequency = this.ApplyFrequencyBoundaries(e.Frequency, this.spectrumAnalyzer.DisplayedBandwidth / 2);
					long num3 = this._centerFrequency;
					switch (this._tuningStyle)
					{
					case TuningStyle.Center:
						if (!this._changingCenterSpot)
						{
							e.Accept = this.UpdateCenterFrequency(e.Frequency, false, true);
							num3 = e.Frequency;
						}
						break;
					case TuningStyle.Sticky:
						if (!this._changingStickySpot)
						{
							long num4 = e.Frequency - this.vfoFrequencyEdit.Frequency;
							num3 = this._centerFrequency + num4;
							e.Accept = this.UpdateCenterFrequency(num3, false, false);
						}
						break;
					case TuningStyle.Free:
					{
						float val = (float)this.spectrumAnalyzer.DisplayCenterFrequency - this._tuningLimit * (float)this.spectrumAnalyzer.DisplayedBandwidth;
						val = Math.Max(val, (float)this._centerFrequency - this._tuningLimit * (float)this._usableSpectrumWidth);
						float val2 = (float)this.spectrumAnalyzer.DisplayCenterFrequency + this._tuningLimit * (float)this.spectrumAnalyzer.DisplayedBandwidth;
						val2 = Math.Min(val2, (float)this._centerFrequency + this._tuningLimit * (float)this._usableSpectrumWidth);
						if (!((float)e.Frequency < val) && !((float)e.Frequency > val2))
						{
							break;
						}
						long num4 = e.Frequency - this.vfoFrequencyEdit.Frequency;
						num3 = this._centerFrequency + num4;
						e.Accept = this.UpdateCenterFrequency(num3, false, false);
						break;
					}
					}
					if (e.Accept && this._centerFrequency != num3)
					{
						e.Frequency = Math.Max(e.Frequency, 0L);
						this.fftZoomTrackBar.Value = 0;
						if (this._tuningStyle == TuningStyle.Center)
						{
							this._tuningStyle = TuningStyle.Free;
							this.UpdateTuningStyle();
						}
					}
				}
			}
		}

		public void SetFrequency(long frequency, bool onlyMoveCenterFrequency)
		{
			if (onlyMoveCenterFrequency && this._tuningStyle == TuningStyle.Free)
			{
				long num = frequency - this.vfoFrequencyEdit.Frequency;
				this.UpdateCenterFrequency(this._centerFrequency + num, false, false);
			}
			this.vfoFrequencyEdit.Frequency = frequency;
		}

		private void SetCenterFrequency(long newCenterFreq)
		{
			this.UpdateCenterFrequency(newCenterFreq, true, true);
		}

		private void panview_FrequencyChanged(object sender, FrequencyEventArgs e)
		{
			this._changingStickySpot = (e.Source != FrequencyChangeSource.Scroll);
			this.vfoFrequencyEdit.Frequency = e.Frequency;
			this._changingStickySpot = false;
			if (this.vfoFrequencyEdit.Frequency != e.Frequency)
			{
				e.Cancel = true;
			}
		}

		private void panview_CenterFrequencyChanged(object sender, FrequencyEventArgs e)
		{
			if (!this.SourceIsTunable)
			{
				e.Cancel = true;
			}
			else
			{
				e.Cancel = !this.UpdateCenterFrequency(e.Frequency, true, false);
				e.Cancel |= (this._centerFrequency != e.Frequency);
			}
		}

		public void ResetFrequency(long frequency, long centerFrequency)
		{
			if (this._tuningStyle == TuningStyle.Center && this.SourceIsTunable)
			{
				frequency = Math.Max(frequency, (long)((float)this._usableSpectrumWidth * this._tuningLimit));
			}
			if (this.SourceIsTunable)
			{
				this._centerFrequency = centerFrequency;
				this.waterfall.CenterFrequency = centerFrequency;
				this.spectrumAnalyzer.CenterFrequency = centerFrequency;
				this.vfoFrequencyEdit.Frequency = frequency;
			}
			else
			{
				this._centerFrequency = centerFrequency;
				this.waterfall.Frequency = frequency;
				this.waterfall.CenterFrequency = centerFrequency;
				this.spectrumAnalyzer.Frequency = frequency;
				this.spectrumAnalyzer.CenterFrequency = centerFrequency;
				this.vfoFrequencyEdit.DisableFrequencyEvents = true;
				this.vfoFrequencyEdit.Frequency = frequency;
				this.vfoFrequencyEdit.DisableFrequencyEvents = false;
				this._vfo.Frequency = 0;
				this._vfo.IFOffset = -this._ifOffset;
			}
		}

		public void ResetFrequency(long frequency)
		{
			this.ResetFrequency(frequency, frequency);
		}

		private long ApplyFrequencyBoundaries(long frequency, long delta = 0L)
		{
			if (this.SourceIsTunable)
			{
				ITunableSource tunableSource = this._frontendController as ITunableSource;
				long num = tunableSource.MinimumTunableFrequency - delta + this._frequencyShift;
				long num2 = tunableSource.MaximumTunableFrequency + delta + this._frequencyShift;
				if (num < 0)
				{
					num = 0L;
				}
				if (num2 < 0)
				{
					num2 = 0L;
				}
				if (frequency < num)
				{
					frequency = num;
				}
				else if (frequency > num2)
				{
					frequency = num2;
				}
			}
			return frequency;
		}

		private bool UpdateCenterFrequency(long frequency, bool setVFO, bool centerZoom = true)
		{
			if (!this.SourceIsTunable)
			{
				return false;
			}
			frequency = this.ApplyFrequencyBoundaries(frequency, 0L);
			frequency = Math.Max(frequency, (long)((float)this._usableSpectrumWidth * this._tuningLimit));
			this.waterfall.CenterFrequency = frequency;
			this.spectrumAnalyzer.CenterFrequency = frequency;
			long num = frequency - this._centerFrequency;
			Interlocked.Exchange(ref this._centerFrequency, frequency);
			if (setVFO)
			{
				this._changingStickySpot = (this._tuningStyle == TuningStyle.Sticky);
				this._changingCenterSpot = (this._tuningStyle == TuningStyle.Center);
				this.vfoFrequencyEdit.Frequency += num;
				this._changingStickySpot = false;
				this._changingCenterSpot = false;
			}
			if (this._vfo.DetectorType == DetectorType.WFM)
			{
				this._vfo.RdsReset();
			}
			else
			{
				this._vfo.CarrierLockerReset();
			}
			if (centerZoom)
			{
				this.waterfall.CenterZoom();
				this.spectrumAnalyzer.CenterZoom();
			}
			this.NotifyPropertyChanged("CenterFrequency");
			return true;
		}

		private void UpdateTunableBandwidth()
		{
			this.ResetFrequency(this.vfoFrequencyEdit.Frequency);
			this.NotifyPropertyChanged("TuningLimit");
			this.NotifyPropertyChanged("TunableBandwidth");
		}

		private void TuneThreadProc(object state)
		{
			while (!this._terminated)
			{
				long num = Interlocked.Read(ref this._centerFrequency) + this._ifOffset;
				long num2 = Interlocked.Read(ref this._frequencyShift);
				long num3 = num - num2;
				IFrontendController frontendController = this._frontendController;
				ITunableSource tunableSource = frontendController as ITunableSource;
				if (tunableSource != null && this._frequencySet != num3)
				{
					try
					{
						tunableSource.Frequency = num3;
						this._frequencySet = num3;
					}
					catch
					{
					}
				}
				Thread.Sleep(1);
			}
		}

		private void filterBandwidthNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this._vfo.Bandwidth = (int)this.filterBandwidthNumericUpDown.Value;
			this.waterfall.FilterBandwidth = this._vfo.Bandwidth;
			this.spectrumAnalyzer.FilterBandwidth = this._vfo.Bandwidth;
			this.NotifyPropertyChanged("FilterBandwidth");
		}

		private void filterOrderNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this._vfo.FilterOrder = (int)this.filterOrderNumericUpDown.Value;
			this.NotifyPropertyChanged("FilterOrder");
		}

		private void filterTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._vfo.WindowType = (WindowType)(this.filterTypeComboBox.SelectedIndex + 1);
			this.NotifyPropertyChanged("FilterType");
		}

		private void autoCorrectIQCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			this._iqBalancerProcessor.Engine.Enabled = (this.correctIQCheckBox.Checked && this._sharpControlProxy.Enabled);
			this.NotifyPropertyChanged("CorrectIQ");
		}

		private void UpdateFrequencyShift()
		{
			long num = 0L;
			if (this.frequencyShiftCheckBox.Checked)
			{
				num = (long)this.frequencyShiftNumericUpDown.Value;
			}
			long num2 = num - this._frequencyShift;
			this._frequencyShift = num;
			long frequency = Math.Max(this._usableSpectrumWidth / 2, this._centerFrequency + num2);
			this.UpdateCenterFrequency(frequency, false, false);
			if (Math.Abs(num2) > 10000)
			{
				long frequency2 = Math.Max(0L, this.vfoFrequencyEdit.Frequency + num2);
				this.SetFrequency(frequency2, false);
			}
			this.UpdateVfoFrequency();
		}

		private void frequencyShiftCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			this.frequencyShiftNumericUpDown.Enabled = this.frequencyShiftCheckBox.Checked;
			this.UpdateFrequencyShift();
			this.NotifyPropertyChanged("FrequencyShiftEnabled");
		}

		private void frequencyShiftNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this.UpdateFrequencyShift();
			this.NotifyPropertyChanged("FrequencyShift");
		}

		private void modeRadioButton_CheckStateChanged(object sender, EventArgs e)
		{
			this.agcCheckBox.Enabled = (!this.wfmRadioButton.Checked && !this.rawRadioButton.Checked);
			this.agcDecayNumericUpDown.Enabled = this.agcCheckBox.Enabled;
			this.agcSlopeNumericUpDown.Enabled = this.agcCheckBox.Enabled;
			this.agcThresholdNumericUpDown.Enabled = this.agcCheckBox.Enabled;
			this.agcUseHangCheckBox.Enabled = this.agcCheckBox.Enabled;
			this.fmStereoCheckBox.Enabled = this.wfmRadioButton.Checked;
			this.useSquelchCheckBox.Enabled = (this.nfmRadioButton.Checked || this.amRadioButton.Checked);
			this.squelchNumericUpDown.Enabled = (this.useSquelchCheckBox.Enabled && this.useSquelchCheckBox.Checked);
			this.cwShiftNumericUpDown.Enabled = this.cwRadioButton.Checked;
			this._streamControl.ScaleOutput = !this.unityGainCheckBox.Checked;
			this.audioGainTrackBar.Enabled = !this.unityGainCheckBox.Checked;
			this.filterAudioCheckBox.Enabled = !this.rawRadioButton.Checked;
			this.lockCarrierCheckBox.Enabled = (this.dsbRadioButton.Checked || this.amRadioButton.Checked || this.usbRadioButton.Checked || this.lsbRadioButton.Checked);
			this.useAntiFadingCheckBox.Enabled = (this.lockCarrierCheckBox.Checked && this.lockCarrierCheckBox.Enabled && (this.amRadioButton.Checked || this.dsbRadioButton.Checked));
			this.spectrumAnalyzer.StatusText = string.Empty;
			if (!this._initializing)
			{
				this._modeStates[this._vfo.DetectorType] = this.GetModeState();
			}
			if (this.wfmRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.WFM;
				this.waterfall.BandType = BandType.Center;
				this.spectrumAnalyzer.BandType = BandType.Center;
				this.waterfall.FilterOffset = 0;
				this.spectrumAnalyzer.FilterOffset = 0;
			}
			else if (this.nfmRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.NFM;
				this.waterfall.BandType = BandType.Center;
				this.spectrumAnalyzer.BandType = BandType.Center;
				this.waterfall.FilterOffset = 0;
				this.spectrumAnalyzer.FilterOffset = 0;
			}
			else if (this.amRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.AM;
				this.waterfall.BandType = BandType.Center;
				this.spectrumAnalyzer.BandType = BandType.Center;
				this.waterfall.FilterOffset = 0;
				this.spectrumAnalyzer.FilterOffset = 0;
			}
			else if (this.lsbRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.LSB;
				this.waterfall.BandType = BandType.Lower;
				this.spectrumAnalyzer.BandType = BandType.Lower;
				this.waterfall.FilterOffset = -100;
				this.spectrumAnalyzer.FilterOffset = -100;
			}
			else if (this.usbRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.USB;
				this.waterfall.BandType = BandType.Upper;
				this.spectrumAnalyzer.BandType = BandType.Upper;
				this.waterfall.FilterOffset = 100;
				this.spectrumAnalyzer.FilterOffset = 100;
			}
			else if (this.dsbRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.DSB;
				this.waterfall.BandType = BandType.Center;
				this.spectrumAnalyzer.BandType = BandType.Center;
				this.waterfall.FilterOffset = 0;
				this.spectrumAnalyzer.FilterOffset = 0;
			}
			else if (this.cwRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.CW;
				this.waterfall.BandType = BandType.Center;
				this.spectrumAnalyzer.BandType = BandType.Center;
				this.waterfall.FilterOffset = 0;
				this.spectrumAnalyzer.FilterOffset = 0;
			}
			else if (this.rawRadioButton.Checked)
			{
				this._vfo.DetectorType = DetectorType.RAW;
				this.waterfall.BandType = BandType.Center;
				this.spectrumAnalyzer.BandType = BandType.Center;
				this.waterfall.FilterOffset = 0;
				this.spectrumAnalyzer.FilterOffset = 0;
			}
			this._vfo.RdsReset();
			this.UpdateVFOSource();
			this.UpdateFilterBandwidth();
			this.SetModeState(this._modeStates[this._vfo.DetectorType]);
			this.NotifyPropertyChanged("DetectorType");
		}

		private void UpdateVFOSource()
		{
			if (this.UseFFTSource && this._frontendController is IVFOSource)
			{
				StreamControl.ReducedBandwidth = true;
				IVFOSource iVFOSource = this._frontendController as IVFOSource;
				int num = iVFOSource.VFOMinIQDecimation + StreamControl.GetDecimationStageCount(iVFOSource.VFOMaxSampleRate / (double)(1 << iVFOSource.VFOMinIQDecimation), this._vfo.DetectorType);
				StreamControl.ReducedBandwidth = (this._vfo.DetectorType != DetectorType.WFM);
				if (iVFOSource.VFODecimation != num)
				{
					bool isPlaying = this.IsPlaying;
					if (isPlaying)
					{
						this.StopRadio();
					}
					iVFOSource.VFODecimation = num;
					this._vfo.DecimationStageCount = iVFOSource.VFOMinIQDecimation + StreamControl.GetDecimationStageCount(iVFOSource.VFOMaxSampleRate / (double)(1 << iVFOSource.VFOMinIQDecimation), DetectorType.AM) - iVFOSource.VFODecimation;
					this._vfo.SampleRate = iVFOSource.VFOMaxSampleRate / (double)(1 << iVFOSource.VFODecimation);
					if (isPlaying)
					{
						this.StartRadio();
					}
				}
			}
			else
			{
				StreamControl.ReducedBandwidth = false;
			}
		}

		private void UpdateFilterBandwidth()
		{
			switch (this._vfo.DetectorType)
			{
			case DetectorType.WFM:
				this.filterBandwidthNumericUpDown.Maximum = ((this._streamControl.SampleRate == 0.0) ? 250000 : ((int)Math.Min(this._streamControl.SampleRate, 250000.0)));
				break;
			case DetectorType.NFM:
			case DetectorType.AM:
			case DetectorType.DSB:
			case DetectorType.CW:
			case DetectorType.RAW:
				this.filterBandwidthNumericUpDown.Maximum = ((this._streamControl.AudioSampleRate == 0.0) ? this._minOutputSampleRate : ((int)Math.Min(this._streamControl.AudioSampleRate, (double)this._minOutputSampleRate)));
				break;
			case DetectorType.LSB:
			case DetectorType.USB:
				this.filterBandwidthNumericUpDown.Maximum = ((this._streamControl.AudioSampleRate == 0.0) ? (this._minOutputSampleRate / 2) : ((int)Math.Min(this._streamControl.AudioSampleRate, (double)this._minOutputSampleRate) / 2));
				break;
			}
		}

		private void fmStereoCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this._vfo.FmStereo = this.fmStereoCheckBox.Checked;
			this.NotifyPropertyChanged("FmStereo");
		}

		private void cwShiftNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this._vfo.CWToneShift = (int)this.cwShiftNumericUpDown.Value;
			this.NotifyPropertyChanged("CWShift");
		}

		private void squelchNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (!this._configuringSquelch)
			{
				this._vfo.SquelchThreshold = (int)this.squelchNumericUpDown.Value;
				this.NotifyPropertyChanged("SquelchThreshold");
			}
		}

		private void useSquelchCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!this._configuringSquelch)
			{
				this.squelchNumericUpDown.Enabled = this.useSquelchCheckBox.Checked;
				if (this.useSquelchCheckBox.Checked)
				{
					this._vfo.SquelchThreshold = (int)this.squelchNumericUpDown.Value;
				}
				else
				{
					this._vfo.SquelchThreshold = 0;
				}
				this.NotifyPropertyChanged("SquelchEnabled");
			}
		}

		private static int ParseStepSize(string s)
		{
			int result = 0;
			Match match = Regex.Match(s, "([0-9\\.]+) ([kMG]?)Hz", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				int num;
				switch (match.Groups[2].Value.ToLower())
				{
				default:
					num = 1;
					break;
				case "k":
					num = 1000;
					break;
				case "m":
					num = 1000000;
					break;
				case "g":
					num = 1000000000;
					break;
				}
				result = (int)(double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * (double)num);
			}
			return result;
		}

		private bool SetStepSize(int stepSize)
		{
			for (int i = 0; i < this.stepSizeComboBox.Items.Count; i++)
			{
				int num = MainForm.ParseStepSize(this.stepSizeComboBox.Items[i].ToString());
				if (stepSize == num)
				{
					this.stepSizeComboBox.SelectedIndex = i;
					this.stepSizeComboBox_SelectedIndexChanged(null, null);
					return true;
				}
			}
			return false;
		}

		private void stepSizeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this._configuringSnap)
			{
				this.waterfall.UseSnap = this.snapFrequencyCheckBox.Checked;
				this.spectrumAnalyzer.UseSnap = this.snapFrequencyCheckBox.Checked;
				int num = MainForm.ParseStepSize(this.stepSizeComboBox.Text);
				if (num > 0 && num != this._stepSize)
				{
					this.waterfall.StepSize = num;
					this.spectrumAnalyzer.StepSize = num;
					if (this.snapFrequencyCheckBox.Checked && this.SourceIsTunable)
					{
						long frequency = (this._centerFrequency + num / 2) / num * num;
						this.UpdateCenterFrequency(frequency, false, false);
						long num2 = (this.vfoFrequencyEdit.Frequency + num / 2) / num * num;
						if (this.vfoFrequencyEdit.Frequency != num2)
						{
							this.vfoFrequencyEdit.Frequency = num2;
						}
					}
					this._stepSize = num;
					if (sender == this.snapFrequencyCheckBox)
					{
						this.NotifyPropertyChanged("SnapToGrid");
					}
					this.NotifyPropertyChanged("StepSize");
				}
			}
		}

		private void panview_BandwidthChanged(object sender, BandwidthEventArgs e)
		{
			if ((decimal)e.Bandwidth < this.filterBandwidthNumericUpDown.Minimum)
			{
				e.Bandwidth = (int)this.filterBandwidthNumericUpDown.Minimum;
			}
			else if ((decimal)e.Bandwidth > this.filterBandwidthNumericUpDown.Maximum)
			{
				e.Bandwidth = (int)this.filterBandwidthNumericUpDown.Maximum;
			}
			this.filterBandwidthNumericUpDown.Value = e.Bandwidth;
		}

		private void frontendGuiButton_Click(object sender, EventArgs e)
		{
			if (this.SourceIsWaveFile)
			{
				if (this._streamControl.IsPlaying)
				{
					this.StopRadio();
				}
				try
				{
					this.Open(true);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else if (this.SourceIsFrontEnd)
			{
				if (this._frontendController is IConfigurationPanelProvider)
				{
					if (this.scrollPanel.Visible && this.sourceCollapsiblePanel.PanelState == PanelStateOptions.Expanded)
					{
						this.sourceCollapsiblePanel.PanelState = PanelStateOptions.Collapsed;
					}
					else
					{
						this.scrollPanel.Visible = true;
						this.menuSpacerPanel.Visible = true;
						this.rightTableLayoutPanel.Visible = true;
						this.sourceCollapsiblePanel.PanelState = PanelStateOptions.Expanded;
					}
					this.scrollPanel.ScrollControlIntoView(this.sourceCollapsiblePanel);
				}
				else if (this._frontendController is IFloatingConfigDialogProvider)
				{
					((IFloatingConfigDialogProvider)this._frontendController).ShowSettingGUI(this);
				}
			}
		}

		private int[] GetModeState()
		{
			return new int[12]
			{
				this.FilterBandwidth,
				this.FilterOrder,
				(int)this.FilterType,
				this.SquelchThreshold,
				this.SquelchEnabled ? 1 : 0,
				this.CWShift,
				this.SnapToGrid ? 1 : 0,
				this.stepSizeComboBox.SelectedIndex,
				this.unityGainCheckBox.Checked ? 1 : 0,
				this.agcCheckBox.Checked ? 1 : 0,
				this.lockCarrierCheckBox.Checked ? 1 : 0,
				this.useAntiFadingCheckBox.Checked ? 1 : 0
			};
		}

		private void SetModeState(int[] state)
		{
			this.FilterBandwidth = Math.Min(state[0], (int)this.filterBandwidthNumericUpDown.Maximum);
			this.FilterOrder = state[1];
			this.FilterType = (WindowType)state[2];
			this._configuringSquelch = true;
			this.SquelchThreshold = state[3];
			this.SquelchEnabled = (state[4] == 1);
			this._configuringSquelch = false;
			this.useSquelchCheckBox_CheckedChanged(null, null);
			this.CWShift = state[5];
			this._configuringSnap = true;
			this.SnapToGrid = (state[6] == 1);
			this.stepSizeComboBox.SelectedIndex = Math.Min(this.stepSizeComboBox.Items.Count - 1, state[7]);
			this._configuringSnap = false;
			this.stepSizeComboBox_SelectedIndexChanged(null, null);
			this.unityGainCheckBox.Checked = (state[8] == 1);
			this.unityGainCheckBox_CheckStateChanged(null, null);
			this.agcCheckBox.Checked = (state[9] == 1);
			this.agcCheckBox_CheckedChanged(null, null);
			this.lockCarrierCheckBox.Checked = (state[10] == 1);
			this.lockCarrierCheckBox_CheckedChanged(null, null);
			this.useAntiFadingCheckBox.Checked = (state[11] == 1);
			this.useAntiFadingCheckBox_CheckedChanged(null, null);
		}

		private void lockCarrierCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this._vfo.LockCarrier = this.lockCarrierCheckBox.Checked;
			this.useAntiFadingCheckBox.Enabled = (this.lockCarrierCheckBox.Checked && this.lockCarrierCheckBox.Enabled && (this.amRadioButton.Checked || this.dsbRadioButton.Checked));
		}

		private void useAntiFadingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this._vfo.UseAntiFading = this.useAntiFadingCheckBox.Checked;
		}

		private void agcCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this._vfo.UseAGC = this.agcCheckBox.Checked;
			this.agcThresholdNumericUpDown.Enabled = (this.agcCheckBox.Checked && this.agcCheckBox.Enabled);
			this.agcDecayNumericUpDown.Enabled = (this.agcCheckBox.Checked && this.agcCheckBox.Enabled);
			this.agcSlopeNumericUpDown.Enabled = (this.agcCheckBox.Checked && this.agcCheckBox.Enabled);
			this.agcUseHangCheckBox.Enabled = (this.agcCheckBox.Checked && this.agcCheckBox.Enabled);
			this.NotifyPropertyChanged("UseAgc");
		}

		private void agcUseHangCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this._vfo.AgcHang = this.agcUseHangCheckBox.Checked;
			this.NotifyPropertyChanged("UseHang");
		}

		private void agcDecayNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this._vfo.AgcDecay = (float)(int)this.agcDecayNumericUpDown.Value;
			this.NotifyPropertyChanged("AgcDecay");
		}

		private void agcThresholdNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this._vfo.AgcThreshold = (float)(int)this.agcThresholdNumericUpDown.Value;
			this.NotifyPropertyChanged("AgcThreshold");
		}

		private void agcSlopeNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			this._vfo.AgcSlope = (float)(int)this.agcSlopeNumericUpDown.Value;
			this.NotifyPropertyChanged("AgcSlope");
		}

		private void swapIQCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this._streamControl.SwapIQ = this.swapIQCheckBox.Checked;
			this.NotifyPropertyChanged("SwapIq");
		}

		private void viewComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool flag = false;
			bool flag2 = false;
			if (this._streamControl.IsPlaying)
			{
				if (this.viewComboBox.SelectedIndex < 3 && !this.spectrumPanel.Visible)
				{
					flag = true;
				}
				else if (this.viewComboBox.SelectedIndex == 3 && this.spectrumPanel.Visible)
				{
					flag2 = true;
				}
			}
			switch (this.viewComboBox.SelectedIndex)
			{
			case 0:
				this.spectrumPanel.Visible = true;
				this.spectrumAnalyzer.Visible = true;
				this.waterfall.Visible = false;
				this.spectrumAnalyzer.Dock = DockStyle.Fill;
				this.spectrumSplitter.Visible = false;
				break;
			case 1:
				this.spectrumPanel.Visible = true;
				this.spectrumAnalyzer.Visible = false;
				this.waterfall.Visible = true;
				this.spectrumAnalyzer.Dock = DockStyle.Top;
				this.spectrumSplitter.Visible = false;
				break;
			case 2:
				this.spectrumPanel.Visible = true;
				this.spectrumAnalyzer.Visible = true;
				this.waterfall.Visible = true;
				this.spectrumAnalyzer.Dock = DockStyle.Top;
				this.spectrumSplitter.Visible = true;
				break;
			case 3:
				this.spectrumPanel.Visible = false;
				break;
			}
			if (!this.UseFFTSource)
			{
				if (flag)
				{
					this._fftStream.Open();
					ThreadPool.QueueUserWorkItem(this.ProcessFFT);
				}
				else if (flag2)
				{
					this._fftStream.Close();
					this._fftEvent.Set();
				}
			}
		}

		private void fftResolutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._fftResolutionLock.AcquireWriterLock(300000);
			this._fftBins = int.Parse(this.fftResolutionComboBox.SelectedItem.ToString());
			this.InitFFTBuffers();
			this.BuildFFTWindow();
			this._fftResolutionLock.ReleaseWriterLock();
			this.NotifyPropertyChanged("FFTResolution");
		}

		private void spectrumStyleComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.SpectrumStyle = (SpectrumStyle)this.spectrumStyleComboBox.SelectedIndex;
			this.NotifyPropertyChanged("FFTSpectrumStyle");
		}

		private void fftWindowComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._fftWindowType = (WindowType)this.fftWindowComboBox.SelectedIndex;
			this.BuildFFTWindow();
		}

		private void gradientButton_Click(object sender, EventArgs e)
		{
			ColorBlend gradient = GradientDialog.GetGradient(this.waterfall.GradientColorBlend);
			if (gradient != null && gradient.Positions.Length != 0)
			{
				this.waterfall.GradientColorBlend = gradient;
				this.spectrumAnalyzer.VerticalLinesGradient = gradient;
				Utils.SaveSetting("waterfall.gradient", MainForm.GradientToString(gradient.Colors));
				this.NotifyPropertyChanged("Gradient");
			}
		}

		private static string GradientToString(Color[] colors)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < colors.Length; i++)
			{
				stringBuilder.AppendFormat(",{0:X2}{1:X2}{2:X2}", colors[i].R, colors[i].G, colors[i].B);
			}
			return stringBuilder.ToString().Substring(1);
		}

		private void fftContrastTrackBar_Changed(object sender, EventArgs e)
		{
			this.waterfall.Contrast = this.fftContrastTrackBar.Value * 100 / (this.fftContrastTrackBar.Maximum - this.fftContrastTrackBar.Minimum);
			this.spectrumAnalyzer.Contrast = this.waterfall.Contrast;
			this.NotifyPropertyChanged("FFTContrast");
		}

		private void sAttackTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.Attack = (float)this.sAttackTrackBar.Value / (float)this.sAttackTrackBar.Maximum;
			this.NotifyPropertyChanged("SAttack");
		}

		private void sDecayTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.Decay = (float)this.sDecayTrackBar.Value / (float)this.sDecayTrackBar.Maximum;
			this.NotifyPropertyChanged("SDecay");
		}

		private void wAttackTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this.waterfall.Attack = (float)this.wAttackTrackBar.Value / (float)this.wAttackTrackBar.Maximum;
			this.NotifyPropertyChanged("WAttack");
		}

		private void wDecayTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this.waterfall.Decay = (float)this.wDecayTrackBar.Value / (float)this.wDecayTrackBar.Maximum;
			this.NotifyPropertyChanged("WDecay");
		}

		private void markPeaksCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.MarkPeaks = this.markPeaksCheckBox.Checked;
			this.NotifyPropertyChanged("MarkPeaks");
		}

		private void useTimestampCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.waterfall.UseTimestamps = this.useTimestampsCheckBox.Checked;
			this.NotifyPropertyChanged("UseTimeMarkers");
		}

		private void fftSpeedTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this._waterfallTimer.Interval = (int)(1.0 / (double)this.fftSpeedTrackBar.Value * 1000.0);
		}

		private void fftZoomTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.Zoom = this.fftZoomTrackBar.Value * 100 / this.fftZoomTrackBar.Maximum;
			this.waterfall.Zoom = this.spectrumAnalyzer.Zoom;
			this.NotifyPropertyChanged("Zoom");
		}

		private void MainForm_Move(object sender, EventArgs e)
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				this._lastLocation = base.Location;
			}
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				this._lastSize = base.Size;
			}
		}

		private int[] GetCollapsiblePanelStates()
		{
			List<int> list = new List<int>();
			for (SDRSharp.CollapsiblePanel.CollapsiblePanel nextPanel = this.sourceCollapsiblePanel; nextPanel != null; nextPanel = nextPanel.NextPanel)
			{
				list.Add((int)nextPanel.PanelState);
			}
			return list.ToArray();
		}

		private void fftOffsetTrackBar_Scroll(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.DisplayOffset = -this.fftOffsetTrackBar.Value * 10;
			this.waterfall.DisplayOffset = this.spectrumAnalyzer.DisplayOffset;
			this.NotifyPropertyChanged("FFTOffset");
		}

		private void fftRangeTrackBar_Scroll(object sender, EventArgs e)
		{
			this.spectrumAnalyzer.DisplayRange = this.fftRangeTrackBar.Value * 10;
			this.waterfall.DisplayRange = this.spectrumAnalyzer.DisplayRange;
			this.NotifyPropertyChanged("FFTRange");
		}

		private void InitialiseSharpPlugins()
		{
			NameValueCollection nameValueCollection = (NameValueCollection)ConfigurationManager.GetSection("sharpPlugins");
			if (nameValueCollection == null)
			{
				MessageBox.Show("Configuration section 'sharpPlugins' was not found. Please check 'SDRSharp.exe.config'.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				this._oldTopSplitterPosition = Utils.GetIntSetting("topSplitter", this._oldTopSplitterPosition);
				this._oldBottomSplitterPosition = Utils.GetIntSetting("bottomSplitter", this._oldBottomSplitterPosition);
				this._oldLeftSplitterPosition = Utils.GetIntSetting("leftSplitter", this._oldLeftSplitterPosition);
				this._oldRightSplitterPosition = Utils.GetIntSetting("rightSplitter", this._oldRightSplitterPosition);
				this.topSplitter.Visible = false;
				this.bottomSplitter.Visible = false;
				this.leftSplitter.Visible = false;
				this.rightSplitter.Visible = false;
				foreach (string key in nameValueCollection.Keys)
				{
					try
					{
						string fqtn = nameValueCollection[key];
						ISharpPlugin sharpPlugin = (ISharpPlugin)this.LoadExtension(fqtn);
						this._sharpPlugins.Add(key, sharpPlugin);
						sharpPlugin.Initialize(this._sharpControlProxy);
						if (sharpPlugin.Gui != null)
						{
							this.CreatePluginCollapsiblePanel(sharpPlugin);
						}
					}
					catch (Exception)
					{
					}
				}
				this.sourceCollapsiblePanel.PanelState = PanelStateOptions.Collapsed;
				int[] intArraySetting = Utils.GetIntArraySetting("collapsiblePanelStates", null);
				if (intArraySetting != null)
				{
					SDRSharp.CollapsiblePanel.CollapsiblePanel nextPanel = this.sourceCollapsiblePanel;
					for (int i = 0; i < intArraySetting.Length; i++)
					{
						if (nextPanel == null)
						{
							break;
						}
						nextPanel.PanelState = (PanelStateOptions)intArraySetting[i];
						nextPanel = nextPanel.NextPanel;
					}
				}
				else
				{
					this.sourceCollapsiblePanel.PanelState = PanelStateOptions.Expanded;
				}
			}
		}

		private void ShowControllerPanel(UserControl gui)
		{
			if (this.sourceTableLayoutPanel.Controls.Count == 1)
			{
				this.sourceCollapsiblePanel.Height = this._sourcePanelHeight + gui.Height;
				this.sourceTableLayoutPanel.Controls.Add(gui, 0, 1);
				gui.Dock = DockStyle.Fill;
			}
		}

		private void HideControllerPanel(UserControl gui)
		{
			if (this.sourceTableLayoutPanel.Controls.Count > 1)
			{
				this.sourceTableLayoutPanel.Controls.Remove(gui);
				this.sourceCollapsiblePanel.Height = this._sourcePanelHeight;
			}
		}

		private void CreatePluginCollapsiblePanel(ISharpPlugin plugin)
		{
			UserControl gui = plugin.Gui;
			if (gui != null)
			{
				SDRSharp.CollapsiblePanel.CollapsiblePanel collapsiblePanel = new SDRSharp.CollapsiblePanel.CollapsiblePanel();
				collapsiblePanel.PanelTitle = plugin.DisplayName + " *";
				collapsiblePanel.AutoHeight = true;
				collapsiblePanel.Content.Controls.Add(gui);
				collapsiblePanel.Height = gui.Height;
				collapsiblePanel.Width = this.fftCollapsiblePanel.Width;
				collapsiblePanel.PanelState = PanelStateOptions.Collapsed;
				gui.Dock = DockStyle.Fill;
				SDRSharp.CollapsiblePanel.CollapsiblePanel nextPanel = this.fftCollapsiblePanel;
				while (nextPanel.NextPanel != null)
				{
					nextPanel = nextPanel.NextPanel;
				}
				nextPanel.NextPanel = collapsiblePanel;
				this.controlPanel.Controls.Add(collapsiblePanel);
			}
		}

		public void RegisterFrontControl(UserControl c, PluginPosition position)
		{
			SizeType sizeType = c.Visible ? SizeType.Absolute : SizeType.AutoSize;
			SizeType sizeType2 = c.Visible ? SizeType.Percent : SizeType.AutoSize;
			switch (position)
			{
			case PluginPosition.Top:
				if (this.topPluginPanel.Controls.Count > 0)
				{
					this.topPluginPanel.ColumnCount++;
					this.topPluginPanel.ColumnStyles.Add(new ColumnStyle(sizeType, 4f));
					this.topPluginPanel.ColumnCount++;
					this.topPluginPanel.ColumnStyles.Add(new ColumnStyle(sizeType2, 100f));
				}
				else
				{
					this.topPluginPanel.ColumnStyles[0].SizeType = sizeType2;
				}
				this._oldTopSplitterPosition = Math.Max(this._oldTopSplitterPosition, c.Height);
				this.topPluginPanel.Controls.Add(c, this.topPluginPanel.ColumnCount - 1, 0);
				break;
			case PluginPosition.Bottom:
				if (this.bottomPluginPanel.Controls.Count > 0)
				{
					this.bottomPluginPanel.ColumnCount++;
					this.bottomPluginPanel.ColumnStyles.Add(new ColumnStyle(sizeType, 4f));
					this.bottomPluginPanel.ColumnCount++;
					this.bottomPluginPanel.ColumnStyles.Add(new ColumnStyle(sizeType2, 100f));
				}
				else
				{
					this.bottomPluginPanel.ColumnStyles[0].SizeType = sizeType2;
				}
				this._oldBottomSplitterPosition = Math.Max(this._oldBottomSplitterPosition, c.Height);
				this.bottomPluginPanel.Controls.Add(c, this.bottomPluginPanel.ColumnCount - 1, 0);
				break;
			case PluginPosition.Left:
				if (this.leftPluginPanel.Controls.Count > 0)
				{
					this.leftPluginPanel.RowCount++;
					this.leftPluginPanel.RowStyles.Add(new RowStyle(sizeType, 4f));
					this.leftPluginPanel.RowCount++;
					this.leftPluginPanel.RowStyles.Add(new RowStyle(sizeType2, 100f));
				}
				else
				{
					this.leftPluginPanel.RowStyles[0].SizeType = sizeType2;
				}
				this._oldLeftSplitterPosition = Math.Max(this._oldLeftSplitterPosition, c.Width);
				this.leftPluginPanel.Controls.Add(c, 0, this.leftPluginPanel.RowCount - 1);
				break;
			case PluginPosition.Right:
				if (this.rightPluginPanel.Controls.Count > 0)
				{
					this.rightPluginPanel.RowCount++;
					this.rightPluginPanel.RowStyles.Add(new RowStyle(sizeType, 4f));
					this.rightPluginPanel.RowCount++;
					this.rightPluginPanel.RowStyles.Add(new RowStyle(sizeType2, 100f));
				}
				else
				{
					this.rightPluginPanel.RowStyles[0].SizeType = sizeType2;
				}
				this._oldRightSplitterPosition = Math.Max(this._oldRightSplitterPosition, c.Width);
				this.rightPluginPanel.Controls.Add(c, 0, this.rightPluginPanel.RowCount - 1);
				break;
			}
			c.Margin = new Padding(0);
			c.Dock = DockStyle.Fill;
			c.VisibleChanged += this.plugin_VisibleChanged;
			this.plugin_VisibleChanged(c, null);
		}

		private void plugin_VisibleChanged(object sender, EventArgs e)
		{
			UserControl userControl = (UserControl)sender;
			TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)userControl.Parent;
			int index = tableLayoutPanel.Controls.IndexOf(userControl) * 2;
			bool flag = tableLayoutPanel == this.leftPluginPanel || tableLayoutPanel == this.rightPluginPanel;
			if (userControl.Visible)
			{
				if (flag)
				{
					tableLayoutPanel.RowStyles[index].SizeType = SizeType.Percent;
				}
				else
				{
					tableLayoutPanel.ColumnStyles[index].SizeType = SizeType.Percent;
				}
			}
			else if (flag)
			{
				tableLayoutPanel.RowStyles[index].SizeType = SizeType.AutoSize;
			}
			else
			{
				tableLayoutPanel.ColumnStyles[index].SizeType = SizeType.AutoSize;
			}
			for (int i = 0; i < tableLayoutPanel.ColumnStyles.Count - 1; i += 2)
			{
				if (tableLayoutPanel.ColumnStyles[i].SizeType == SizeType.Percent)
				{
					SizeType sizeType = SizeType.AutoSize;
					int num = i + 2;
					while (num < tableLayoutPanel.ColumnStyles.Count)
					{
						if (tableLayoutPanel.ColumnStyles[num].SizeType != SizeType.Percent)
						{
							num += 2;
							continue;
						}
						sizeType = SizeType.Absolute;
						break;
					}
					tableLayoutPanel.ColumnStyles[i + 1].SizeType = sizeType;
				}
				else
				{
					tableLayoutPanel.ColumnStyles[i + 1].SizeType = SizeType.AutoSize;
				}
			}
			for (int j = 0; j < tableLayoutPanel.RowStyles.Count - 1; j += 2)
			{
				if (tableLayoutPanel.RowStyles[j].SizeType == SizeType.Percent)
				{
					SizeType sizeType2 = SizeType.AutoSize;
					int num2 = j + 2;
					while (num2 < tableLayoutPanel.RowStyles.Count)
					{
						if (tableLayoutPanel.RowStyles[num2].SizeType != SizeType.Percent)
						{
							num2 += 2;
							continue;
						}
						sizeType2 = SizeType.Absolute;
						break;
					}
					tableLayoutPanel.RowStyles[j + 1].SizeType = sizeType2;
				}
				else
				{
					tableLayoutPanel.RowStyles[j + 1].SizeType = SizeType.AutoSize;
				}
			}
			this.UpdatePluginPanel(tableLayoutPanel);
		}

		private void UpdatePluginPanel(TableLayoutPanel panel)
		{
			bool flag = true;
			int num = 0;
			while (num < panel.Controls.Count)
			{
				if (!((Control.ControlCollection)panel.Controls)[num].Visible)
				{
					num++;
					continue;
				}
				flag = false;
				break;
			}
			if (panel == this.topPluginPanel)
			{
				if (flag)
				{
					this.topSplitter.Visible = false;
					this.topSplitter.SplitPosition = 0;
				}
				else
				{
					this.topSplitter.Visible = true;
					this.topSplitter.SplitPosition = this._oldTopSplitterPosition;
				}
			}
			else if (panel == this.bottomPluginPanel)
			{
				if (flag)
				{
					this.bottomSplitter.Visible = false;
					this.bottomSplitter.SplitPosition = 0;
				}
				else
				{
					this.bottomSplitter.Visible = true;
					this.bottomSplitter.SplitPosition = this._oldBottomSplitterPosition;
				}
			}
			else if (panel == this.leftPluginPanel)
			{
				if (flag)
				{
					this.leftSplitter.Visible = false;
					this.leftSplitter.SplitPosition = 0;
				}
				else
				{
					this.leftSplitter.Visible = true;
					this.leftSplitter.SplitPosition = this._oldLeftSplitterPosition;
				}
			}
			else if (panel == this.rightPluginPanel)
			{
				if (flag)
				{
					this.rightSplitter.Visible = false;
					this.rightSplitter.SplitPosition = 0;
				}
				else
				{
					this.rightSplitter.Visible = true;
					this.rightSplitter.SplitPosition = this._oldRightSplitterPosition;
				}
			}
		}

		private void pluginSplitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (this.topSplitter.Visible)
			{
				this._oldTopSplitterPosition = this.topSplitter.SplitPosition;
			}
			if (this.bottomSplitter.Visible)
			{
				this._oldBottomSplitterPosition = this.bottomSplitter.SplitPosition;
			}
			if (this.leftSplitter.Visible)
			{
				this._oldLeftSplitterPosition = this.leftSplitter.SplitPosition;
			}
			if (this.rightSplitter.Visible)
			{
				this._oldRightSplitterPosition = this.rightSplitter.SplitPosition;
			}
		}

		private void ConnectSource()
		{
			if (this.SourceIsFrontEnd && this._frontendController is IConnectableSource)
			{
				IConnectableSource connectableSource = this._frontendController as IConnectableSource;
				if (!connectableSource.Connected)
				{
					connectableSource.Connect();
				}
			}
		}

		public void StartRadio()
		{
			this.playStopButton.Image = Resources.sdr_stop;
			this._tooltip.SetToolTip(this.playStopButton, "Stop");
			try
			{
				this.ConnectSource();
				this.Open(false);
				this._streamControl.Play();
			}
			catch
			{
				this.ConnectSource();
				this.Open(true);
				this._streamControl.Play();
			}
			this._fftStream.Open();
			if (!this.UseFFTSource)
			{
				ThreadPool.QueueUserWorkItem(this.ProcessFFT);
			}
			this.sampleRateComboBox.Enabled = false;
			this.inputDeviceComboBox.Enabled = false;
			this.outputDeviceComboBox.Enabled = false;
			this.latencyNumericUpDown.Enabled = false;
			this.NotifyPropertyChanged("StartRadio");
		}

		public void StopRadio()
		{
			this.playStopButton.Image = Resources.sdr_start;
			this._tooltip.SetToolTip(this.playStopButton, "Start");
			this._streamControl.Stop();
			this._iqBalancerProcessor.Engine.Reset();
			this._fftStream.Close();
			if (!this.SourceIsWaveFile)
			{
				this.inputDeviceComboBox.Enabled = (this._frontendController == null || this._frontendController is ISoundcardController);
				this.sampleRateComboBox.Enabled = (this._frontendController == null || this._frontendController is ISoundcardController);
			}
			this.outputDeviceComboBox.Enabled = true;
			this.latencyNumericUpDown.Enabled = true;
			this._fftEvent.Set();
			while (this._fftIsRunning)
			{
			}
			this.NotifyPropertyChanged("StopRadio");
			GC.Collect();
		}

		public unsafe void GetSpectrumSnapshot(byte[] destArray)
		{
			this._fftResolutionLock.AcquireReaderLock(300000);
			float[] array = new float[destArray.Length];
			fixed (byte* dest = destArray)
			{
				float[] array2 = array;
				fixed (float* ptr = array2)
				{
					Fourier.SmoothMaxCopy(this._fftDisplayPtr, ptr, this._fftDisplaySize, array.Length, 1f, 0f);
					Fourier.ScaleFFT(ptr, dest, array.Length, -130f, 0f);
				}
			}
			this._fftResolutionLock.ReleaseReaderLock();
		}

		public unsafe void GetSpectrumSnapshot(float[] destArray, float scale = 1f, float offset = 0f)
		{
			this._fftResolutionLock.AcquireReaderLock(300000);
			fixed (float* dstPtr = destArray)
			{
				Fourier.SmoothMaxCopy(this._fftDisplayPtr, dstPtr, this._fftDisplaySize, destArray.Length, scale, offset);
			}
			this._fftResolutionLock.ReleaseReaderLock();
		}

		public void RegisterStreamHook(object streamHook, ProcessorType processorType)
		{
			this._hookManager.RegisterStreamHook(streamHook, processorType);
		}

		public void UnregisterStreamHook(object streamHook)
		{
			this._hookManager.UnregisterStreamHook(streamHook);
		}

		private void NotifyPropertyChanged(string property)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if (propertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(property));
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

		public void Perform()
		{
			this.spectrumAnalyzer.Perform();
			this.waterfall.Perform();
		}

		private void logoPictureBox_Click(object sender, EventArgs e)
		{
			Process.Start("http://airspy.com");
		}
	}
}
