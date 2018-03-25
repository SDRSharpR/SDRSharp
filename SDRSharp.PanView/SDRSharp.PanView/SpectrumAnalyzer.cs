using SDRSharp.Radio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDRSharp.PanView
{
	public class SpectrumAnalyzer : UserControl
	{
		private const int RefreshInterval = 20;

		private const int GradientAlpha = 180;

		private const int SnrMeterWidth = 10;

		private static readonly Color _spectrumEnvelopeColor = Utils.GetColorSetting("spectrumAnalyzer.envelopeColor", Color.DarkGray);

		private static readonly Color _spectrumFillColor = Utils.GetColorSetting("spectrumAnalyzer.fillColor", Color.DodgerBlue, 100);

		private static readonly bool _useAntiAliasedDisplay = Utils.GetBooleanSetting("useAntiAliasedDisplay", false);

		private float _attack;

		private float _decay;

		private bool _performNeeded;

		private byte[] _scaledSpectrumEnvelope;

		private byte[] _scaledSpectrumMinimum;

		private float[] _smoothedSpectrumEnvelope;

		private float[] _smoothedSpectrumMinimum;

		private float[] _temp;

		private float _peak;

		private float _floor;

		private float _snr;

		private List<int> _peaks = new List<int>();

		private Bitmap _buffer;

		private Graphics _graphics;

		private long _spectrumWidth;

		private long _centerFrequency;

		private long _displayCenterFrequency;

		private Point[] _envelopePoints;

		private Point[] _minMaxPoints;

		private BandType _bandType;

		private BandType _side;

		private bool _useStepSizeForDisplay;

		private int _filterBandwidth;

		private int _filterOffset;

		private int _stepSize = 1000;

		private float _xIncrement;

		private long _frequency;

		private float _lower;

		private float _upper;

		private int _zoom;

		private float _scale = 1f;

		private int _oldX;

		private float _snappedX;

		private long _trackingFrequency;

		private int _oldFilterBandwidth;

		private int _displayedBandwidth;

		private long _oldFrequency;

		private long _oldCenterFrequency;

		private bool _changingBandwidth;

		private bool _changingFrequency;

		private bool _changingCenterFrequency;

		private bool _useSmoothing;

		private bool _enableFilter = true;

		private bool _enableHotTracking = true;

		private bool _enableFrequencyMarker = true;

		private bool _enableSideFilterResize;

		private bool _enableFilterMove = true;

		private bool _enableSnrBar;

		private bool _hotTrackNeeded;

		private bool _useSnap;

		private bool _markPeaks;

		private float _trackingPower;

		private string _statusText;

		private int _displayRange = 130;

		private int _displayOffset;

		private int _contrast;

		private Point _cursorPosition;

		private string _customTitle;

		private SpectrumStyle _spectrumStyle = SpectrumStyle.StaticGradient;

		private Pen[] _gradientPens = new Pen[256];

		private Pen[] _snrGradientPens = new Pen[100];

		private Timer _performTimer;

		private LinearGradientBrush _gradientBrush;

		private ColorBlend _staticGradient = Utils.GetGradientBlend(180, "spectrumAnalyzer.gradient");

		private ColorBlend _verticalLinesGradient = Utils.GetGradientBlend(255, "waterfall.gradient");

		private ColorBlend _snrGradient = Utils.GetGradientBlend(255, "spectrumAnalyzer.snrGradient");

		public int SpectrumWidth
		{
			get
			{
				return (int)this._spectrumWidth;
			}
			set
			{
				if (this._spectrumWidth != value)
				{
					this._spectrumWidth = value;
					this.ApplyZoom();
				}
			}
		}

		public int FilterBandwidth
		{
			get
			{
				return this._filterBandwidth;
			}
			set
			{
				if (this._filterBandwidth != value)
				{
					this._filterBandwidth = value;
					this._performNeeded = true;
				}
			}
		}

		public int FilterOffset
		{
			get
			{
				return this._filterOffset;
			}
			set
			{
				if (this._filterOffset != value)
				{
					this._filterOffset = value;
					this._performNeeded = true;
				}
			}
		}

		public BandType BandType
		{
			get
			{
				return this._bandType;
			}
			set
			{
				if (this._bandType != value)
				{
					this._bandType = value;
					this._performNeeded = true;
				}
			}
		}

		public bool UseStepSizeForDisplay
		{
			get
			{
				return this._useStepSizeForDisplay;
			}
			set
			{
				if (this._useStepSizeForDisplay != value)
				{
					this._useStepSizeForDisplay = value;
					this._performNeeded = true;
				}
			}
		}

		public long Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				if (this._frequency != value)
				{
					this._frequency = value;
					this._performNeeded = true;
				}
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
				if (this._centerFrequency != value)
				{
					this._displayCenterFrequency += value - this._centerFrequency;
					this._centerFrequency = value;
					this._performNeeded = true;
				}
			}
		}

		public int DisplayedBandwidth
		{
			get
			{
				return this._displayedBandwidth;
			}
		}

		public long DisplayCenterFrequency
		{
			get
			{
				return this._displayCenterFrequency;
			}
		}

		public int DisplayRange
		{
			get
			{
				return this._displayRange;
			}
			set
			{
				if (this._displayRange != value)
				{
					this._displayRange = value;
					this._performNeeded = true;
				}
			}
		}

		public int DisplayOffset
		{
			get
			{
				return this._displayOffset;
			}
			set
			{
				if (this._displayOffset != value)
				{
					this._displayOffset = value;
					this._performNeeded = true;
				}
			}
		}

		public int Zoom
		{
			get
			{
				return this._zoom;
			}
			set
			{
				if (this._zoom != value)
				{
					this._zoom = value;
					this.ApplyZoom();
				}
			}
		}

		public bool UseSmoothing
		{
			get
			{
				return this._useSmoothing;
			}
			set
			{
				this._useSmoothing = value;
			}
		}

		public bool EnableFilter
		{
			get
			{
				return this._enableFilter;
			}
			set
			{
				this._enableFilter = value;
				this._performNeeded = true;
			}
		}

		public bool EnableSNR
		{
			get
			{
				return this._enableSnrBar;
			}
			set
			{
				this._enableSnrBar = value;
				this._performNeeded = true;
			}
		}

		public float VisualSNR
		{
			get
			{
				return this._snr;
			}
		}

		public bool EnableHotTracking
		{
			get
			{
				return this._enableHotTracking;
			}
			set
			{
				this._enableHotTracking = value;
				this._performNeeded = true;
			}
		}

		public bool EnableFrequencyMarker
		{
			get
			{
				return this._enableFrequencyMarker;
			}
			set
			{
				this._enableFrequencyMarker = value;
				this._performNeeded = true;
			}
		}

		public bool EnableSideFilterResize
		{
			get
			{
				return this._enableSideFilterResize;
			}
			set
			{
				this._enableSideFilterResize = value;
				this._performNeeded = true;
			}
		}

		public bool EnableFilterMove
		{
			get
			{
				return this._enableFilterMove;
			}
			set
			{
				this._enableFilterMove = value;
				this._performNeeded = true;
			}
		}

		public string StatusText
		{
			get
			{
				return this._statusText;
			}
			set
			{
				if (this._statusText != value)
				{
					this._statusText = value;
					this._performNeeded = true;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ColorBlend GradientColorBlend
		{
			get
			{
				return this._staticGradient;
			}
			set
			{
				this._staticGradient = new ColorBlend(value.Colors.Length);
				for (int i = 0; i < value.Colors.Length; i++)
				{
					this._staticGradient.Colors[i] = Color.FromArgb(180, value.Colors[i]);
					this._staticGradient.Positions[i] = value.Positions[i];
				}
				this._gradientBrush.Dispose();
				this._gradientBrush = new LinearGradientBrush(new Rectangle(30, 30, this._buffer.Width - 30, this._buffer.Height - 30), Color.White, Color.Black, LinearGradientMode.Vertical);
				this._gradientBrush.InterpolationColors = this._staticGradient;
				this._performNeeded = true;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ColorBlend SNRGradient
		{
			get
			{
				return this._snrGradient;
			}
			set
			{
				if (this._snrGradient != value)
				{
					this._snrGradient = value;
					this.BuildSNRGradientVector();
					this._performNeeded = true;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ColorBlend VerticalLinesGradient
		{
			get
			{
				return this._verticalLinesGradient;
			}
			set
			{
				if (this._verticalLinesGradient != value)
				{
					this._verticalLinesGradient = value;
					this.BuildDynamicGradientVector();
					this._performNeeded = true;
				}
			}
		}

		public SpectrumStyle SpectrumStyle
		{
			get
			{
				return this._spectrumStyle;
			}
			set
			{
				if (this._spectrumStyle != value)
				{
					this._spectrumStyle = value;
					this._performNeeded = true;
				}
			}
		}

		public int Contrast
		{
			get
			{
				return this._contrast;
			}
			set
			{
				this._contrast = value;
			}
		}

		public float Attack
		{
			get
			{
				return this._attack;
			}
			set
			{
				this._attack = value;
			}
		}

		public float Decay
		{
			get
			{
				return this._decay;
			}
			set
			{
				this._decay = value;
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
				if (this._stepSize != value)
				{
					this._stepSize = value;
					this._performNeeded = true;
				}
			}
		}

		public bool UseSnap
		{
			get
			{
				return this._useSnap;
			}
			set
			{
				this._useSnap = value;
			}
		}

		public bool MarkPeaks
		{
			get
			{
				return this._markPeaks;
			}
			set
			{
				this._markPeaks = value;
			}
		}

		public event ManualFrequencyChangeEventHandler FrequencyChanged;

		public event ManualFrequencyChangeEventHandler CenterFrequencyChanged;

		public event ManualBandwidthChangeEventHandler BandwidthChanged;

		public event CustomPaintEventHandler CustomPaint;

		public event CustomPaintEventHandler BackgroundCustomPaint;

		public SpectrumAnalyzer()
		{
			this._performTimer = new Timer();
			this._performTimer.Enabled = true;
			this._performTimer.Interval = 20;
			this._performTimer.Tick += this.performTimer_Tick;
			Rectangle clientRectangle = base.ClientRectangle;
			int width = clientRectangle.Width;
			clientRectangle = base.ClientRectangle;
			this._buffer = new Bitmap(width, clientRectangle.Height, PixelFormat.Format32bppPArgb);
			this._graphics = Graphics.FromImage(this._buffer);
			this._gradientBrush = new LinearGradientBrush(new Rectangle(30, 30, this._buffer.Width - 30, this._buffer.Height - 30), Color.White, Color.Black, LinearGradientMode.Vertical);
			this._gradientBrush.InterpolationColors = this._staticGradient;
			int num = this._buffer.Width - 60;
			this._smoothedSpectrumEnvelope = new float[num];
			this._smoothedSpectrumMinimum = new float[num];
			this._scaledSpectrumEnvelope = new byte[num];
			this._scaledSpectrumMinimum = new byte[num];
			this._envelopePoints = new Point[num + 2];
			this._minMaxPoints = new Point[num * 2 + 2];
			this._temp = new float[num];
			for (int i = 0; i < this._smoothedSpectrumEnvelope.Length; i++)
			{
				this._smoothedSpectrumEnvelope[i] = -250f;
				this._smoothedSpectrumMinimum[i] = -250f;
			}
			base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			base.SetStyle(ControlStyles.DoubleBuffer, true);
			base.SetStyle(ControlStyles.UserPaint, true);
			base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			base.UpdateStyles();
			this.BuildDynamicGradientVector();
			this.BuildSNRGradientVector();
		}

		~SpectrumAnalyzer()
		{
			this._buffer.Dispose();
			this._graphics.Dispose();
			this._gradientBrush.Dispose();
		}

		private void BuildDynamicGradientVector()
		{
			using (Bitmap bitmap = new Bitmap(1, this._gradientPens.Length))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, 1, this._gradientPens.Length), Color.White, Color.Black, LinearGradientMode.Vertical))
					{
						linearGradientBrush.InterpolationColors = this._verticalLinesGradient;
						Pen pen = new Pen(linearGradientBrush);
						graphics.DrawLine(pen, 0, 0, 0, this._gradientPens.Length - 1);
						for (int i = 0; i < this._gradientPens.Length; i++)
						{
							this._gradientPens[this._gradientPens.Length - 1 - i] = new Pen(Color.FromArgb(bitmap.GetPixel(0, i).ToArgb()));
						}
					}
				}
			}
		}

		private void BuildSNRGradientVector()
		{
			using (Bitmap bitmap = new Bitmap(1, this._snrGradientPens.Length))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, 1, this._snrGradientPens.Length), Color.Black, Color.White, LinearGradientMode.Vertical))
					{
						linearGradientBrush.InterpolationColors = this._snrGradient;
						Pen pen = new Pen(linearGradientBrush);
						graphics.DrawLine(pen, 0, 0, 0, this._snrGradientPens.Length - 1);
						for (int i = 0; i < this._snrGradientPens.Length; i++)
						{
							this._snrGradientPens[this._snrGradientPens.Length - 1 - i] = new Pen(Color.FromArgb(bitmap.GetPixel(0, i).ToArgb()), 10f);
						}
					}
				}
			}
		}

		private void ApplyZoom()
		{
			this._scale = (float)Math.Pow(10.0, (double)((float)this._zoom * 4f / 100f));
			if (this._spectrumWidth > 0)
			{
				this._displayCenterFrequency = this.GetDisplayCenterFrequency();
				this._xIncrement = this._scale * (float)(this._buffer.Width - 60) / (float)this._spectrumWidth;
				this._displayedBandwidth = (int)((float)this._spectrumWidth / this._scale);
				this._performNeeded = true;
			}
		}

		public void CenterZoom()
		{
			this._displayCenterFrequency = this.GetDisplayCenterFrequency();
			this._performNeeded = true;
		}

		private long GetDisplayCenterFrequency()
		{
			long num = this._frequency + this._filterOffset;
			switch (this._bandType)
			{
			case BandType.Lower:
				num = (long)((float)num - (float)this._filterBandwidth * 0.5f);
				break;
			case BandType.Upper:
				num = (long)((float)num + (float)this._filterBandwidth * 0.5f);
				break;
			}
			long num2 = (long)((double)((float)this._centerFrequency - (float)this._spectrumWidth * 0.5f) - ((double)num - (double)this._spectrumWidth * 0.5 / (double)this._scale));
			if (num2 > 0)
			{
				num += num2;
			}
			long num3 = (long)((double)((float)num + (float)this._spectrumWidth * 0.5f / this._scale) - ((double)this._centerFrequency + (double)this._spectrumWidth * 0.5));
			if (num3 > 0)
			{
				num -= num3;
			}
			return num;
		}

		private void performTimer_Tick(object sender, EventArgs e)
		{
			this.Perform(false);
		}

		private void Perform(bool force)
		{
			if (this._performNeeded | force)
			{
				this.DrawLayers();
				base.Invalidate();
				this._performNeeded = false;
			}
		}

		public void Perform()
		{
			this._performNeeded = true;
		}

		private void DrawCursor(bool drawFilter = true, bool drawFrequencyMarker = true, bool drawPeaks = true, bool drawHotTrackingLine = true, bool drawHotTrackingText = true)
		{
			this._lower = 0f;
			int num = (int)Math.Max((float)this._filterBandwidth * this._xIncrement, 2f) | 1;
			float num2 = (float)this._buffer.Width * 0.5f + (float)(this._frequency - this._displayCenterFrequency) * this._xIncrement;
			switch (this._bandType)
			{
			case BandType.Upper:
				this._lower = num2;
				break;
			case BandType.Lower:
				this._lower = num2 - (float)num;
				break;
			case BandType.Center:
				this._lower = num2 - (float)num * 0.5f;
				break;
			}
			this._lower += (float)this._filterOffset * this._xIncrement;
			this._upper = this._lower + (float)num;
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, Color.DarkGray)))
			{
				using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(200, 50, 50, 50)))
				{
					using (Pen pen5 = new Pen(Color.FromArgb(200, Color.Gray)))
					{
						using (Pen pen3 = new Pen(Color.DodgerBlue))
						{
							using (Pen pen2 = new Pen(Color.LimeGreen))
							{
								using (Pen pen = new Pen(Color.Red))
								{
									using (FontFamily family = new FontFamily("Verdana"))
									{
										using (GraphicsPath graphicsPath = new GraphicsPath())
										{
											using (Pen pen4 = new Pen(Color.Black))
											{
												if (drawFilter && this._enableFilter && num < this._buffer.Width - 60)
												{
													float num3 = this._lower;
													float num4 = (float)num;
													if (this._lower < 30f)
													{
														num3 = 31f;
														num4 -= num3 - this._lower;
													}
													if (this._upper > (float)(this._buffer.Width - 30))
													{
														num4 -= this._upper - (float)(this._buffer.Width - 30);
													}
													this._graphics.FillRectangle(brush, num3, 30f, num4, (float)(this._buffer.Height - 60));
												}
												if (drawFrequencyMarker && this._enableFrequencyMarker && num2 > 30f && num2 < (float)(this._buffer.Width - 30))
												{
													pen.Width = 1f;
													this._graphics.DrawLine(pen, num2, 30f, num2, (float)(this._buffer.Height - 30));
												}
												if (drawPeaks && this._markPeaks && this._spectrumWidth > 0)
												{
													int val = num;
													val = Math.Max(val, 10);
													val = Math.Min(val, this._scaledSpectrumEnvelope.Length);
													PeakDetector.GetPeaks(this._scaledSpectrumEnvelope, this._peaks, val);
													float num5 = (float)(this._buffer.Height - 60) / 255f;
													foreach (int peak in this._peaks)
													{
														int num6 = (int)((float)(this._buffer.Height - 30) - (float)(int)this._scaledSpectrumEnvelope[peak] * num5);
														int num7 = peak + 30;
														this._graphics.DrawEllipse(Pens.Yellow, num7 - 5, num6 - 5, 10, 10);
													}
												}
												if (this._enableHotTracking && this._hotTrackNeeded && this._cursorPosition.X >= 30 && this._cursorPosition.X <= this._buffer.Width - 30 && this._cursorPosition.Y >= 30 && this._cursorPosition.Y <= this._buffer.Height - 30)
												{
													if (drawHotTrackingLine && this.Cursor != Cursors.SizeWE && (this._snappedX < this._lower || this._snappedX > this._upper) && this._scaledSpectrumEnvelope != null && !this._changingFrequency && !this._changingCenterFrequency && !this._changingBandwidth)
													{
														pen2.DashStyle = DashStyle.Dash;
														this._graphics.DrawLine(pen3, this._snappedX, 30f, this._snappedX, (float)(this._buffer.Height - 30));
														int num8 = num / 2;
														switch (this._bandType)
														{
														case BandType.Center:
															this._graphics.DrawLine(pen2, this._snappedX - (float)num8, 30f, this._snappedX - (float)num8, (float)(this._buffer.Height - 30));
															this._graphics.DrawLine(pen2, this._snappedX + (float)num8, 30f, this._snappedX + (float)num8, (float)(this._buffer.Height - 30));
															break;
														case BandType.Lower:
															this._graphics.DrawLine(pen2, this._snappedX - (float)num, 30f, this._snappedX - (float)num, (float)(this._buffer.Height - 30));
															break;
														case BandType.Upper:
															this._graphics.DrawLine(pen2, this._snappedX + (float)num, 30f, this._snappedX + (float)num, (float)(this._buffer.Height - 30));
															break;
														}
													}
													if (drawHotTrackingText)
													{
														string text = string.Empty;
														if (this._changingBandwidth || this.Cursor == Cursors.SizeWE)
														{
															text = "Bandwidth: " + SpectrumAnalyzer.GetFrequencyDisplay(this._filterBandwidth);
														}
														else if (!this._changingCenterFrequency)
														{
															if (!string.IsNullOrEmpty(this._customTitle))
															{
																text = this._customTitle;
															}
															if (this._changingFrequency || ((float)this._cursorPosition.X >= this._lower && (float)this._cursorPosition.X <= this._upper))
															{
																if (!string.IsNullOrEmpty(text))
																{
																	text = text + Environment.NewLine + Environment.NewLine;
																}
																text += string.Format("VFO:\t{0}{1}Peak:\t{2:0.0}dBFS{3}Floor:\t{4:0.0}dBFS{5}SNR:\t{6:0.0}dB", SpectrumAnalyzer.GetFrequencyDisplay(this._frequency), Environment.NewLine, this._peak, Environment.NewLine, this._floor, Environment.NewLine, this._snr);
															}
															if (string.IsNullOrEmpty(text))
															{
																text = string.Format("{0}\r\n{1:0.##}dBFS", SpectrumAnalyzer.GetFrequencyDisplay(this._trackingFrequency), this._trackingPower);
															}
														}
														graphicsPath.AddString(text, family, 0, 16f, Point.Empty, StringFormat.GenericTypographic);
														RectangleF bounds = graphicsPath.GetBounds();
														Cursor current2 = Cursor.Current;
														float val2 = (float)this._cursorPosition.X + 30f;
														float val3 = (float)this._cursorPosition.Y + ((current2 == (Cursor)null) ? 32f : ((float)current2.Size.Height)) - 8f;
														val2 = Math.Min(val2, (float)this._buffer.Width - bounds.Width - 30f - 20f);
														val3 = Math.Min(val3, (float)this._buffer.Height - bounds.Height - 30f - 20f);
														graphicsPath.Reset();
														graphicsPath.AddString(text, family, 0, 16f, new Point((int)val2, (int)val3), StringFormat.GenericTypographic);
														SmoothingMode smoothingMode = this._graphics.SmoothingMode;
														InterpolationMode interpolationMode = this._graphics.InterpolationMode;
														this._graphics.SmoothingMode = SmoothingMode.AntiAlias;
														this._graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
														pen4.Width = 2f;
														RectangleF bounds2 = graphicsPath.GetBounds();
														bounds2.X -= 10f;
														bounds2.Y -= 10f;
														bounds2.Width += 20f;
														bounds2.Height += 20f;
														this._graphics.FillRoundedRectangle(brush2, bounds2, 6);
														this._graphics.DrawRoundedRectangle(pen5, bounds2, 6);
														this._graphics.DrawPath(pen4, graphicsPath);
														this._graphics.FillPath(Brushes.White, graphicsPath);
														this._graphics.SmoothingMode = smoothingMode;
														this._graphics.InterpolationMode = interpolationMode;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private void ProcessVfo()
		{
			int num = Math.Max((int)this._lower - 30, 0);
			int num2 = Math.Min((int)this._upper - 30, this._smoothedSpectrumEnvelope.Length - 1);
			float num3 = -600f;
			for (int i = num; i <= num2; i++)
			{
				if (this._smoothedSpectrumEnvelope[i] > num3)
				{
					num3 = this._smoothedSpectrumEnvelope[i];
				}
			}
			float num4 = (num3 > this._peak) ? 0.3f : 0.01f;
			this._peak += num4 * (num3 - this._peak);
			float val = (this._upper - this._lower) * 0.25f;
			val = Math.Min(5f, val);
			float num5 = 0f;
			float val2 = 0f;
			float val3 = 0f;
			num = Math.Max((int)(this._lower - val) - 30, 0);
			num2 = Math.Min((int)this._lower - 30, this._smoothedSpectrumEnvelope.Length - 1);
			if (num2 > num)
			{
				for (int j = num; j <= num2; j++)
				{
					num5 += this._smoothedSpectrumEnvelope[j];
				}
				val2 = num5 / (float)(num2 - num + 1);
			}
			num = Math.Max((int)this._upper - 30, 0);
			num2 = Math.Min((int)(this._upper + val) - 30, this._smoothedSpectrumEnvelope.Length - 1);
			if (num2 > num)
			{
				num5 = 0f;
				for (int k = num; k <= num2; k++)
				{
					num5 += this._smoothedSpectrumEnvelope[k];
				}
				val3 = num5 / (float)(num2 - num + 1);
			}
			float num6 = Math.Min(val2, val3);
			if (num6 == 0f)
			{
				num = Math.Max((int)this._lower - 30, 0);
				num2 = Math.Min((int)this._upper - 30, this._smoothedSpectrumEnvelope.Length - 1);
				float num7 = 600f;
				for (int l = num; l <= num2; l++)
				{
					if (this._smoothedSpectrumEnvelope[l] < num7)
					{
						num7 = this._smoothedSpectrumEnvelope[l];
					}
				}
				num6 = num7;
			}
			this._floor += 0.03f * (num6 - this._floor);
			this._snr = this._peak - this._floor;
		}

		public unsafe void Render(float* powerSpectrum, int length)
		{
			float offset = (float)(this._displayCenterFrequency - this._centerFrequency) / (float)this._spectrumWidth;
			this.ExtractSpectrum(powerSpectrum, length, offset, this._scale, this._useSmoothing);
			this._performNeeded = true;
		}

		private unsafe void ExtractSpectrum(float* powerSpectrum, int length, float offset, float scale, bool useSmoothing)
		{
			int num = this._displayOffset / 10 * 10;
			int num2 = this._displayRange / 10 * 10;
			float[] temp = this._temp;
			fixed (float* ptr = temp)
			{
				float[] smoothedSpectrumEnvelope = this._smoothedSpectrumEnvelope;
				fixed (float* ptr2 = smoothedSpectrumEnvelope)
				{
					byte[] scaledSpectrumEnvelope = this._scaledSpectrumEnvelope;
					fixed (byte* dest = scaledSpectrumEnvelope)
					{
						byte[] scaledSpectrumMinimum = this._scaledSpectrumMinimum;
						fixed (byte* ptr3 = scaledSpectrumMinimum)
						{
							Fourier.SmoothMaxCopy(powerSpectrum, ptr, length, this._smoothedSpectrumEnvelope.Length, scale, offset);
							if (useSmoothing)
							{
								for (int i = 0; i < this._temp.Length; i++)
								{
									float num3 = (ptr2[i] < ptr[i]) ? this.Attack : this.Decay;
									ptr2[i] = ptr2[i] * (1f - num3) + ptr[i] * num3;
								}
							}
							else
							{
								Utils.Memcpy(ptr2, ptr, this._smoothedSpectrumEnvelope.Length * 4);
							}
							Fourier.ScaleFFT(ptr2, dest, this._smoothedSpectrumEnvelope.Length, (float)(num - num2), (float)num);
						}
					}
				}
			}
			this.ProcessVfo();
			if (this._spectrumStyle == SpectrumStyle.MinMax)
			{
				temp = this._temp;
				fixed (float* ptr4 = temp)
				{
					float[] smoothedSpectrumEnvelope = this._smoothedSpectrumMinimum;
					fixed (float* ptr5 = smoothedSpectrumEnvelope)
					{
						byte[] scaledSpectrumEnvelope = this._scaledSpectrumMinimum;
						fixed (byte* dest2 = scaledSpectrumEnvelope)
						{
							Fourier.SmoothMinCopy(powerSpectrum, ptr4, length, this._smoothedSpectrumMinimum.Length, scale, offset);
							if (useSmoothing)
							{
								for (int j = 0; j < this._temp.Length; j++)
								{
									float num4 = (ptr5[j] < ptr4[j]) ? this.Attack : this.Decay;
									ptr5[j] = ptr5[j] * (1f - num4) + ptr4[j] * num4;
								}
							}
							else
							{
								Utils.Memcpy(ptr5, ptr4, this._smoothedSpectrumMinimum.Length * 4);
							}
							Fourier.ScaleFFT(ptr5, dest2, this._smoothedSpectrumMinimum.Length, (float)(num - num2), (float)num);
						}
					}
				}
			}
		}

		private void DrawStatusText()
		{
			using (Font font = new Font("Lucida Console", 9f))
			{
				if (!string.IsNullOrEmpty(this._statusText))
				{
					this._graphics.DrawString(this._statusText, font, Brushes.White, 30f, 10f);
				}
			}
		}

		private void DrawGrid()
		{
			if (this._displayRange > 0)
			{
				using (SolidBrush brush = new SolidBrush(Color.Silver))
				{
					using (Pen pen = new Pen(Color.FromArgb(80, 80, 80)))
					{
						using (Font font = new Font("Arial", 8f))
						{
							using (new Pen(Color.DarkGray))
							{
								int num = (int)this._graphics.MeasureString("100", font).Height;
								int num2 = this._buffer.Height - 60;
								int num3 = 1;
								int num4 = this._displayRange / num3;
								if (num2 < num * num4)
								{
									num3 = 5;
									num4 = this._displayRange / num3;
								}
								if (num2 < num * num4)
								{
									num3 = 10;
									num4 = this._displayRange / num3;
								}
								float num5 = (float)(this._buffer.Height - 60) / (float)num4;
								for (int i = 1; i <= num4; i++)
								{
									this._graphics.DrawLine(pen, 30, (int)((float)(this._buffer.Height - 30) - (float)i * num5), this._buffer.Width - 30, (int)((float)(this._buffer.Height - 30) - (float)i * num5));
								}
								int num6 = this._displayOffset / 10 * 10;
								for (int j = 0; j <= num4; j++)
								{
									string text = (num6 - (num4 - j) * num3).ToString();
									SizeF sizeF = this._graphics.MeasureString(text, font);
									float width = sizeF.Width;
									float height = sizeF.Height;
									this._graphics.DrawString(text, font, brush, 30f - width, (float)(this._buffer.Height - 30) - (float)j * num5 - height * 0.5f);
								}
							}
						}
					}
				}
			}
		}

		private void DrawFrequencyMarkers()
		{
			if (this._spectrumWidth > 0)
			{
				using (SolidBrush brush = new SolidBrush(Color.Silver))
				{
					using (Pen pen = new Pen(Color.FromArgb(80, 80, 80)))
					{
						using (Font font = new Font("Arial", 8f))
						{
							using (Pen pen2 = new Pen(Color.DarkGray))
							{
								string frequencyDisplay = Utils.GetFrequencyDisplay((long)((float)this._centerFrequency + (float)this._spectrumWidth * 0.5f), false);
								float num = this._graphics.MeasureString(frequencyDisplay, font).Width + 30f;
								long num2 = (long)((float)(this._buffer.Width - 60) / num);
								long num3;
								long num4;
								if (this._useStepSizeForDisplay)
								{
									num3 = 0L;
									do
									{
										num3 += this._stepSize;
										num4 = (int)((float)this._spectrumWidth / this._scale) / num3;
									}
									while (num4 > num2);
								}
								else
								{
									int num5 = 2;
									num3 = 10L;
									do
									{
										num5 = ((num5 == 2) ? 5 : 2);
										num3 *= num5;
										num4 = (int)((float)this._spectrumWidth / this._scale) / num3;
									}
									while (num4 > num2);
									if (num4 > 0)
									{
										if (num4 * 5 < num2)
										{
											num4 *= 5;
											num3 /= 5;
										}
										if (num4 * 2 < num2)
										{
											num3 /= 2;
										}
									}
								}
								num4 = num2 * 2;
								long num6 = this._displayCenterFrequency / num3 * num3;
								for (long num7 = -num4 / 2; num7 < num4 / 2; num7++)
								{
									long frequency = num6 + num3 * num7;
									float num8 = this.FrequencyToPoint(frequency);
									if (num8 >= 29f && num8 <= (float)(this._buffer.Width - 30 + 1))
									{
										this._graphics.DrawLine(pen, num8, 30f, num8, (float)(this._buffer.Height - 30));
										this._graphics.DrawLine(pen2, num8, (float)(this._buffer.Height - 30), num8, (float)(this._buffer.Height - 30 + 5));
										string frequencyDisplay2 = Utils.GetFrequencyDisplay(frequency, false);
										float width = this._graphics.MeasureString(frequencyDisplay2, font).Width;
										num8 -= width * 0.5f;
										this._graphics.DrawString(frequencyDisplay2, font, brush, num8, (float)(this._buffer.Height - 30) + 8f);
									}
								}
							}
						}
					}
				}
			}
		}

		private void DrawAxis()
		{
			using (Pen pen = new Pen(Color.DarkGray))
			{
				this._graphics.DrawLine(pen, 30, 30, 30, this._buffer.Height - 30);
				this._graphics.DrawLine(pen, 30, this._buffer.Height - 30, this._buffer.Width - 30, this._buffer.Height - 30);
			}
		}

		public static string GetFrequencyDisplay(long frequency)
		{
			return Utils.GetFrequencyDisplay(frequency, true);
		}

		public static void ConfigureGraphics(Graphics graphics, bool useAntiAliasedDisplay)
		{
			if (useAntiAliasedDisplay)
			{
				graphics.CompositingMode = CompositingMode.SourceOver;
				graphics.CompositingQuality = CompositingQuality.AssumeLinear;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.PixelOffsetMode = PixelOffsetMode.Default;
				graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
			}
			else
			{
				graphics.CompositingMode = CompositingMode.SourceOver;
				graphics.CompositingQuality = CompositingQuality.HighSpeed;
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				graphics.InterpolationMode = InterpolationMode.Low;
			}
		}

		public void ResetSpectrum()
		{
			for (int i = 0; i < this._scaledSpectrumEnvelope.Length; i++)
			{
				this._scaledSpectrumEnvelope[i] = 0;
				this._scaledSpectrumMinimum[i] = 0;
			}
			this._peak = 0f;
			this._floor = 0f;
			this._snr = 0f;
			this._performNeeded = true;
		}

		private void DrawLayers()
		{
			if (this._buffer.Width > 30 && this._buffer.Height > 30)
			{
				this._graphics.Clear(Color.Black);
				this.DrawStatusText();
				this.DrawSNR();
				this.OnBackgroundCustomPaint(new CustomPaintEventArgs(this._graphics, this._cursorPosition));
				this.DrawGrid();
				this.DrawFrequencyMarkers();
				this.DrawSpectrum();
				this.DrawAxis();
			}
		}

		private void DrawSpectrum()
		{
			if (this._scaledSpectrumEnvelope != null && this._scaledSpectrumEnvelope.Length != 0)
			{
				float num = (float)(this._buffer.Width - 60) / (float)this._scaledSpectrumEnvelope.Length;
				float num2 = (float)(this._buffer.Height - 60) / 255f;
				if (this._spectrumStyle == SpectrumStyle.Dots)
				{
					this.DrawCursor(true, true, true, false, false);
					for (int i = 0; i < this._scaledSpectrumEnvelope.Length; i++)
					{
						Math.Min(Math.Max(0, this._scaledSpectrumEnvelope[i] + this._contrast * 2), 255);
						int x = (int)(30f + (float)i * num);
						int y = (int)((float)(this._buffer.Height - 30) - (float)(int)this._scaledSpectrumEnvelope[i] * num2);
						this._buffer.SetPixel(x, y, SpectrumAnalyzer._spectrumEnvelopeColor);
					}
					this.OnCustomPaint(new CustomPaintEventArgs(this._graphics, this._cursorPosition));
					this.DrawCursor(false, false, false, true, true);
				}
				else if (this._spectrumStyle == SpectrumStyle.DynamicGradient)
				{
					for (int j = 0; j < this._scaledSpectrumEnvelope.Length; j++)
					{
						int num3 = Math.Min(Math.Max(0, this._scaledSpectrumEnvelope[j] + this._contrast * 2), 255);
						int num4 = (int)(30f + (float)j * num);
						int num5 = (int)((float)(this._buffer.Height - 30) - (float)(int)this._scaledSpectrumEnvelope[j] * num2);
						this._envelopePoints[j + 1].X = num4;
						this._envelopePoints[j + 1].Y = num5;
						this._graphics.DrawLine(this._gradientPens[num3], num4, num5, num4, this._buffer.Height - 30);
					}
					this.DrawCursor(true, true, true, false, false);
					this._envelopePoints[0] = this._envelopePoints[1];
					this._envelopePoints[this._envelopePoints.Length - 1] = this._envelopePoints[this._envelopePoints.Length - 2];
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, SpectrumAnalyzer._useAntiAliasedDisplay);
					this._graphics.DrawLines(Pens.Gray, this._envelopePoints);
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, false);
					this.OnCustomPaint(new CustomPaintEventArgs(this._graphics, this._cursorPosition));
					this.DrawCursor(false, false, false, true, true);
				}
				else if (this._spectrumStyle == SpectrumStyle.MinMax)
				{
					for (int k = 0; k < this._scaledSpectrumEnvelope.Length; k++)
					{
						byte b = this._scaledSpectrumMinimum[k];
						byte b2 = this._scaledSpectrumEnvelope[k];
						int x2 = (int)(30f + (float)k * num);
						int y2 = (int)((float)(this._buffer.Height - 30) - (float)(int)b * num2);
						int y3 = (int)((float)(this._buffer.Height - 30) - (float)(int)b2 * num2);
						this._minMaxPoints[k * 2 + 1].X = x2;
						this._minMaxPoints[k * 2 + 1].Y = y2;
						this._minMaxPoints[k * 2 + 2].X = x2;
						this._minMaxPoints[k * 2 + 2].Y = y3;
					}
					this.DrawCursor(true, true, true, false, false);
					this._minMaxPoints[0] = this._minMaxPoints[1];
					this._minMaxPoints[this._minMaxPoints.Length - 1] = this._minMaxPoints[this._minMaxPoints.Length - 2];
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, SpectrumAnalyzer._useAntiAliasedDisplay);
					this._graphics.DrawLines(new Pen(SpectrumAnalyzer._spectrumEnvelopeColor), this._minMaxPoints);
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, false);
					this.OnCustomPaint(new CustomPaintEventArgs(this._graphics, this._cursorPosition));
					this.DrawCursor(false, false, false, true, true);
				}
				else
				{
					for (int l = 0; l < this._scaledSpectrumEnvelope.Length; l++)
					{
						byte b3 = this._scaledSpectrumEnvelope[l];
						int x3 = (int)(30f + (float)l * num);
						int y4 = (int)((float)(this._buffer.Height - 30) - (float)(int)b3 * num2);
						this._envelopePoints[l + 1].X = x3;
						this._envelopePoints[l + 1].Y = y4;
					}
					if (this._spectrumStyle == SpectrumStyle.StaticGradient)
					{
						this._envelopePoints[0].X = 30;
						this._envelopePoints[0].Y = this._buffer.Height - 30 + 1;
						this._envelopePoints[this._envelopePoints.Length - 1].X = this._buffer.Width - 30;
						this._envelopePoints[this._envelopePoints.Length - 1].Y = this._buffer.Height - 30 + 1;
						this._graphics.FillPolygon(this._gradientBrush, this._envelopePoints);
					}
					else if (this._spectrumStyle == SpectrumStyle.SolidFill)
					{
						this._envelopePoints[0].X = 30;
						this._envelopePoints[0].Y = this._buffer.Height - 30 + 1;
						this._envelopePoints[this._envelopePoints.Length - 1].X = this._buffer.Width - 30;
						this._envelopePoints[this._envelopePoints.Length - 1].Y = this._buffer.Height - 30 + 1;
						this._graphics.FillPolygon(new SolidBrush(SpectrumAnalyzer._spectrumFillColor), this._envelopePoints);
					}
					this.DrawCursor(true, true, true, false, false);
					this._envelopePoints[0] = this._envelopePoints[1];
					this._envelopePoints[this._envelopePoints.Length - 1] = this._envelopePoints[this._envelopePoints.Length - 2];
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, SpectrumAnalyzer._useAntiAliasedDisplay);
					this._graphics.DrawLines(new Pen(SpectrumAnalyzer._spectrumEnvelopeColor), this._envelopePoints);
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, false);
					this.OnCustomPaint(new CustomPaintEventArgs(this._graphics, this._cursorPosition));
					this.DrawCursor(false, false, false, true, true);
				}
			}
		}

		private void DrawSNR()
		{
			if (this._enableSnrBar)
			{
				int val = (int)(this._snr / 100f * (float)this._snrGradientPens.Length);
				val = Math.Max(0, val);
				val = Math.Min(this._snrGradientPens.Length - 1, val);
				float val2 = (float)(this._buffer.Height - 60) * this._snr / 100f;
				val2 = Math.Max(0f, val2);
				val2 = Math.Min((float)(this._buffer.Height - 30), val2);
				this._graphics.DrawLine(new Pen(Color.FromArgb(50, 50, 50), 14f), (float)this._buffer.Width - 15f, 30f, (float)this._buffer.Width - 15f, (float)(this._buffer.Height - 30 + 1));
				this._graphics.DrawLine(this._snrGradientPens[val], (float)this._buffer.Width - 15f, (float)(this._buffer.Height - 30) - val2, (float)this._buffer.Width - 15f, (float)(this._buffer.Height - 30 + 1));
				string text = this._snr.ToString("##");
				SizeF sizeF = this._graphics.MeasureString(text, this.Font);
				this._graphics.DrawString(text, this.Font, new SolidBrush(Color.White), (float)this._buffer.Width - (30f + sizeF.Width) * 0.5f, (float)(this._buffer.Height - 30) - val2 - sizeF.Height);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 60)
			{
				clientRectangle = base.ClientRectangle;
				if (clientRectangle.Height <= 60)
				{
					goto IL_0024;
				}
				SpectrumAnalyzer.ConfigureGraphics(e.Graphics, false);
				e.Graphics.DrawImageUnscaled(this._buffer, 0, 0);
				return;
			}
			goto IL_0024;
			IL_0024:
			e.Graphics.Clear(Color.Black);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 60)
			{
				clientRectangle = base.ClientRectangle;
				if (clientRectangle.Height > 60)
				{
					this._buffer.Dispose();
					this._graphics.Dispose();
					clientRectangle = base.ClientRectangle;
					int width = clientRectangle.Width;
					clientRectangle = base.ClientRectangle;
					this._buffer = new Bitmap(width, clientRectangle.Height, PixelFormat.Format32bppPArgb);
					this._graphics = Graphics.FromImage(this._buffer);
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, false);
					int num = this._buffer.Width - 60;
					this._scaledSpectrumEnvelope = new byte[num];
					this._scaledSpectrumMinimum = new byte[num];
					this._smoothedSpectrumEnvelope = new float[num];
					this._smoothedSpectrumMinimum = new float[num];
					this._temp = new float[num];
					this._envelopePoints = new Point[num + 2];
					this._minMaxPoints = new Point[num * 2 + 2];
					if (this._spectrumWidth > 0)
					{
						this._xIncrement = this._scale * (float)num / (float)this._spectrumWidth;
					}
					this._gradientBrush.Dispose();
					this._gradientBrush = new LinearGradientBrush(new Rectangle(30, 30, this._buffer.Width - 30, this._buffer.Height - 30), Color.White, Color.Black, LinearGradientMode.Vertical);
					this._gradientBrush.InterpolationColors = this._staticGradient;
					this.Perform(true);
				}
			}
		}

		public float FrequencyToPoint(long frequency)
		{
			return (float)this._buffer.Width * 0.5f + (float)(frequency - this._displayCenterFrequency) * this._xIncrement;
		}

		public long PointToFrequency(float point)
		{
			return (long)((point - (float)this._buffer.Width * 0.5f) / this._xIncrement) + this._displayCenterFrequency;
		}

		protected virtual void OnFrequencyChanged(FrequencyEventArgs e)
		{
			ManualFrequencyChangeEventHandler frequencyChanged = this.FrequencyChanged;
			if (frequencyChanged != null)
			{
				frequencyChanged(this, e);
			}
		}

		protected virtual void OnCenterFrequencyChanged(FrequencyEventArgs e)
		{
			ManualFrequencyChangeEventHandler centerFrequencyChanged = this.CenterFrequencyChanged;
			if (centerFrequencyChanged != null)
			{
				centerFrequencyChanged(this, e);
			}
		}

		protected virtual void OnBandwidthChanged(BandwidthEventArgs e)
		{
			ManualBandwidthChangeEventHandler bandwidthChanged = this.BandwidthChanged;
			if (bandwidthChanged != null)
			{
				bandwidthChanged(this, e);
			}
		}

		private bool UpdateFrequency(long f, FrequencyChangeSource source)
		{
			if (this._useSnap)
			{
				f = (long)((float)f + (float)(Math.Sign(f) * this._stepSize) * 0.5f) / this._stepSize * this._stepSize;
			}
			long num = (long)((float)this._displayCenterFrequency - (float)this._spectrumWidth * 0.5f / this._scale);
			long num2 = (long)((float)this._displayCenterFrequency + (float)this._spectrumWidth * 0.5f / this._scale);
			if (source == FrequencyChangeSource.Scroll)
			{
				if (f < num || f > num2)
				{
					long num3 = f - this._frequency;
					if (num3 != 0L && !this.UpdateCenterFrequency(this._centerFrequency + num3, source))
					{
						return false;
					}
				}
			}
			else if (f < num)
			{
				f = num;
			}
			else if (f > num2)
			{
				f = num2;
			}
			if (f != this._frequency)
			{
				FrequencyEventArgs frequencyEventArgs = new FrequencyEventArgs(f, source);
				this.OnFrequencyChanged(frequencyEventArgs);
				if (!frequencyEventArgs.Cancel)
				{
					this._frequency = frequencyEventArgs.Frequency;
					this._performNeeded = true;
				}
				return true;
			}
			return false;
		}

		private bool UpdateCenterFrequency(long f, FrequencyChangeSource source)
		{
			if (this._useSnap)
			{
				f = (long)((float)f + (float)(Math.Sign(f) * this._stepSize) * 0.5f) / this._stepSize * this._stepSize;
			}
			if (f < 0)
			{
				f = 0L;
			}
			if (f != this._centerFrequency)
			{
				FrequencyEventArgs frequencyEventArgs = new FrequencyEventArgs(f, source);
				this.OnCenterFrequencyChanged(frequencyEventArgs);
				if (!frequencyEventArgs.Cancel)
				{
					long num = frequencyEventArgs.Frequency - this._centerFrequency;
					this._displayCenterFrequency += num;
					this._centerFrequency = frequencyEventArgs.Frequency;
					this._performNeeded = true;
				}
				return true;
			}
			return false;
		}

		private void UpdateBandwidth(int bw)
		{
			bw = 10 * (bw / 10);
			if (bw < 10)
			{
				bw = 10;
			}
			int num = (int)((float)(18 * this._spectrumWidth) / this._scale / (float)(this._buffer.Width - 60));
			if (bw < num)
			{
				bw = num;
			}
			if (bw != this._filterBandwidth)
			{
				int num2 = this._enableSideFilterResize ? ((int)((float)(bw - this._filterBandwidth) * 0.5f)) : 0;
				int offset = this._filterOffset + ((this._side == BandType.Upper) ? num2 : (-num2));
				BandwidthEventArgs bandwidthEventArgs = new BandwidthEventArgs(bw, offset, this._side);
				this.OnBandwidthChanged(bandwidthEventArgs);
				if (!bandwidthEventArgs.Cancel)
				{
					this._filterOffset = bandwidthEventArgs.Offset;
					this._filterBandwidth = bandwidthEventArgs.Bandwidth;
					this._performNeeded = true;
				}
			}
		}

		protected virtual void OnCustomPaint(CustomPaintEventArgs e)
		{
			CustomPaintEventHandler customPaint = this.CustomPaint;
			if (customPaint != null)
			{
				customPaint(this, e);
				this._customTitle = e.CustomTitle;
			}
		}

		protected virtual void OnBackgroundCustomPaint(CustomPaintEventArgs e)
		{
			CustomPaintEventHandler backgroundCustomPaint = this.BackgroundCustomPaint;
			if (backgroundCustomPaint != null)
			{
				backgroundCustomPaint(this, e);
				this._customTitle = e.CustomTitle;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.X >= 30 && e.X <= this._buffer.Width - 30 && e.Y >= 30)
			{
				if (e.Button == MouseButtons.Left)
				{
					float num = Math.Max((float)this._filterBandwidth * this._xIncrement, 2f);
					if (this._enableFilter && e.Y <= this._buffer.Height - 30)
					{
						if (this._enableFilterMove && (float)e.X > this._lower && (float)e.X < this._upper && num < (float)this._buffer.Width)
						{
							this._oldX = e.X;
							this._oldFrequency = this._frequency;
							this._changingFrequency = true;
						}
						else if (this._upper - this._lower > 12f)
						{
							if (Math.Abs((float)e.X - this._upper - 6f) <= 6f && (this._bandType == BandType.Center || this._bandType == BandType.Upper))
							{
								this._side = BandType.Upper;
								this._oldX = e.X;
								this._oldFilterBandwidth = this._filterBandwidth;
								this._changingBandwidth = true;
							}
							else if (Math.Abs((float)e.X - this._lower + 6f) <= 6f && (this._bandType == BandType.Center || this._bandType == BandType.Lower))
							{
								this._side = BandType.Lower;
								this._oldX = e.X;
								this._oldFilterBandwidth = this._filterBandwidth;
								this._changingBandwidth = true;
							}
						}
					}
					if (!this._changingBandwidth && !this._changingFrequency)
					{
						this._oldX = e.X;
						this._oldCenterFrequency = this._centerFrequency;
						this._changingCenterFrequency = true;
					}
				}
				else if (e.Button == MouseButtons.Right)
				{
					this.UpdateFrequency(this._frequency / 500 * 500, FrequencyChangeSource.Click);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (this._enableFilterMove && this._changingCenterFrequency && e.X == this._oldX)
			{
				long f = (long)(((float)this._oldX - (float)this._buffer.Width * 0.5f) * (float)this._spectrumWidth / this._scale / (float)(this._buffer.Width - 60) + (float)this._displayCenterFrequency);
				this.UpdateFrequency(f, FrequencyChangeSource.Click);
			}
			this._changingCenterFrequency = false;
			this._changingBandwidth = false;
			this._changingFrequency = false;
			this._performNeeded = true;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			this._cursorPosition.X = e.X;
			this._cursorPosition.Y = e.Y;
			if (this._enableHotTracking)
			{
				this._snappedX = (float)e.X;
				this._trackingFrequency = (long)(((float)e.X - (float)this._buffer.Width * 0.5f) * (float)this._spectrumWidth / this._scale / (float)(this._buffer.Width - 60) + (float)this._displayCenterFrequency);
				if (this._useSnap)
				{
					this._trackingFrequency = (long)((float)this._trackingFrequency + (float)(Math.Sign(this._trackingFrequency) * this._stepSize) * 0.5f) / this._stepSize * this._stepSize;
					this._snappedX = this.FrequencyToPoint(this._trackingFrequency);
				}
				int num = this._displayRange / 10 * 10;
				int num2 = this._displayOffset / 10 * 10;
				float num3 = (float)(this._buffer.Height - 60) / (float)num;
				this._trackingPower = (float)(num2 - num) - (float)(e.Y + 30 - this._buffer.Height) / num3;
			}
			if (this._changingFrequency)
			{
				long f = (long)((float)((e.X - this._oldX) * this._spectrumWidth) / this._scale / (float)(this._buffer.Width - 60) + (float)this._oldFrequency);
				this.UpdateFrequency(f, FrequencyChangeSource.Drag);
			}
			else if (this._changingCenterFrequency)
			{
				long f2 = (long)((float)((this._oldX - e.X) * this._spectrumWidth) / this._scale / (float)(this._buffer.Width - 60) + (float)this._oldCenterFrequency);
				this.UpdateCenterFrequency(f2, FrequencyChangeSource.Drag);
			}
			else if (this._changingBandwidth)
			{
				int num4 = 0;
				switch (this._side)
				{
				case BandType.Upper:
					num4 = e.X - this._oldX;
					break;
				case BandType.Lower:
					num4 = this._oldX - e.X;
					break;
				}
				if (this._bandType == BandType.Center && !this._enableSideFilterResize)
				{
					num4 *= 2;
				}
				num4 = (int)((float)(num4 * this._spectrumWidth) / this._scale / (float)(this._buffer.Width - 60) + (float)this._oldFilterBandwidth);
				this.UpdateBandwidth(num4);
			}
			else if (this._enableFilter)
			{
				if (e.Y >= 30 && e.Y <= this._buffer.Height - 30 && e.X >= 30 && e.X <= this._buffer.Width - 30 && this._upper - this._lower > 12f)
				{
					if (Math.Abs((float)e.X - this._lower + 6f) <= 6f && (this._bandType == BandType.Center || this._bandType == BandType.Lower))
					{
						goto IL_0325;
					}
					if (Math.Abs((float)e.X - this._upper - 6f) <= 6f && (this._bandType == BandType.Center || this._bandType == BandType.Upper))
					{
						goto IL_0325;
					}
					this.Cursor = Cursors.Default;
				}
				else
				{
					this.Cursor = Cursors.Default;
				}
			}
			goto IL_034a;
			IL_034a:
			this._performNeeded = true;
			return;
			IL_0325:
			this.Cursor = Cursors.SizeWE;
			goto IL_034a;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (this._enableFilter)
			{
				this.UpdateFrequency(this._frequency + this._stepSize * Math.Sign(e.Delta), FrequencyChangeSource.Scroll);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this._hotTrackNeeded = false;
			this._performNeeded = true;
			this._cursorPosition = Point.Empty;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.Focus();
			base.OnMouseEnter(e);
			this._hotTrackNeeded = true;
		}
	}
}
