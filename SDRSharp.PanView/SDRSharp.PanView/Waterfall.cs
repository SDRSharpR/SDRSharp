using SDRSharp.Radio;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDRSharp.PanView
{
	public class Waterfall : UserControl
	{
		private const int RefreshInterval = 20;

		public const float TrackingFontSize = 16f;

		public const float TimestampFontSize = 14f;

		public const int CarrierPenWidth = 1;

		public const int AxisMargin = 30;

		public const int CursorSnapDistance = 6;

		public const float MaxZoom = 4f;

		public const int RightClickSnapDistance = 500;

		public const float DefaultCursorHeight = 32f;

		private static readonly bool _useUtcTimeStamp = Utils.GetBooleanSetting("waterfall.useUtcTimeStamp");

		private float _attack;

		private float _decay;

		private bool _performNeeded;

		private Bitmap _buffer;

		private Bitmap _buffer2;

		private Graphics _graphics;

		private Graphics _graphics2;

		private BandType _bandType;

		private BandType _side;

		private int _filterBandwidth;

		private int _filterOffset;

		private float _xIncrement;

		private float[] _temp;

		private float[] _smoothedSpectrum;

		private byte[] _scaledSpectrum;

		private long _centerFrequency;

		private long _spectrumWidth;

		private int _stepSize;

		private long _frequency;

		private float _lower;

		private float _upper;

		private float _scale = 1f;

		private long _displayCenterFrequency;

		private bool _changingBandwidth;

		private bool _changingFrequency;

		private bool _changingCenterFrequency;

		private bool _mouseIn;

		private int _oldX;

		private int _displayedBandwidth;

		private long _oldFrequency;

		private long _oldCenterFrequency;

		private int _oldFilterBandwidth;

		private int[] _gradientPixels;

		private int _contrast;

		private int _zoom;

		private bool _useSmoothing;

		private bool _enableFilter = true;

		private bool _enableHotTracking = true;

		private bool _enableFrequencyMarker = true;

		private bool _enableSideFilterResize;

		private bool _enableFilterMove = true;

		private bool _useSnap;

		private float _snappedX;

		private long _trackingFrequency;

		private bool _useTimestamps;

		private int _scanlines;

		private int _timestampInterval;

		private int _displayRange = 130;

		private int _displayOffset;

		private Point _cursorPosition;

		private string _customTitle;

		private Timer _performTimer;

		private LinearGradientBrush _gradientBrush;

		private ColorBlend _gradientColorBlend = Utils.GetGradientBlend(255, "waterfall.gradient");

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ColorBlend GradientColorBlend
		{
			get
			{
				return this._gradientColorBlend;
			}
			set
			{
				if (this._gradientColorBlend != value)
				{
					this._gradientColorBlend = value;
					this._gradientBrush.Dispose();
					this._gradientBrush = new LinearGradientBrush(new RectangleF(15f, 15f, (float)this._buffer.Width - 15f, (float)this._buffer.Height - 15f), Color.White, Color.Black, LinearGradientMode.Vertical);
					this._gradientBrush.InterpolationColors = this._gradientColorBlend;
					this.DrawGradient();
					this.BuildGradientVector();
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

		public int DisplayRange
		{
			get
			{
				return this._displayRange;
			}
			set
			{
				this._displayRange = value;
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
				this._displayOffset = value;
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

		public int StepSize
		{
			get
			{
				return this._stepSize;
			}
			set
			{
				this._performNeeded = true;
				this._stepSize = value;
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

		public bool UseTimestamps
		{
			get
			{
				return this._useTimestamps;
			}
			set
			{
				this._useTimestamps = value;
				this._scanlines = 0;
			}
		}

		public int TimestampInterval
		{
			get
			{
				return this._timestampInterval;
			}
			set
			{
				this._timestampInterval = value;
			}
		}

		public event ManualFrequencyChangeEventHandler FrequencyChanged;

		public event ManualFrequencyChangeEventHandler CenterFrequencyChanged;

		public event ManualBandwidthChangeEventHandler BandwidthChanged;

		public event CustomPaintEventHandler CustomPaint;

		public event LineInsertEventHandler LineInserted;

		public Waterfall()
		{
			this._performTimer = new Timer();
			this._performTimer.Enabled = true;
			this._performTimer.Interval = 20;
			this._performTimer.Tick += this.performTimer_Tick;
			Rectangle clientRectangle = base.ClientRectangle;
			int width = clientRectangle.Width;
			clientRectangle = base.ClientRectangle;
			this._buffer = new Bitmap(width, clientRectangle.Height, PixelFormat.Format32bppPArgb);
			clientRectangle = base.ClientRectangle;
			int width2 = clientRectangle.Width;
			clientRectangle = base.ClientRectangle;
			this._buffer2 = new Bitmap(width2, clientRectangle.Height, PixelFormat.Format32bppPArgb);
			this._graphics = Graphics.FromImage(this._buffer);
			this._graphics2 = Graphics.FromImage(this._buffer2);
			this._gradientBrush = new LinearGradientBrush(new RectangleF(15f, 15f, (float)this._buffer.Width - 15f, (float)this._buffer.Height - 15f), Color.White, Color.Black, LinearGradientMode.Vertical);
			this._gradientBrush.InterpolationColors = this._gradientColorBlend;
			this._smoothedSpectrum = new float[this._buffer.Width - 60];
			this._scaledSpectrum = new byte[this._smoothedSpectrum.Length];
			this._temp = new float[this._smoothedSpectrum.Length];
			this._gradientPixels = new int[256];
			for (int i = 0; i < this._smoothedSpectrum.Length; i++)
			{
				this._smoothedSpectrum[i] = -250f;
			}
			base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			base.SetStyle(ControlStyles.DoubleBuffer, true);
			base.SetStyle(ControlStyles.UserPaint, true);
			base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			base.UpdateStyles();
			this.BuildGradientVector();
		}

		~Waterfall()
		{
			this._buffer.Dispose();
			this._buffer2.Dispose();
			this._graphics.Dispose();
			this._graphics2.Dispose();
			this._gradientBrush.Dispose();
		}

		private void performTimer_Tick(object sender, EventArgs e)
		{
			this.Perform(false);
		}

		private void Perform(bool force)
		{
			if (this._performNeeded | force)
			{
				this.DrawGradient();
				this.CopyMainBuffer();
				this.OnCustomPaint(new CustomPaintEventArgs(this._graphics2, this._cursorPosition));
				if (this._mouseIn)
				{
					this.DrawCursor();
				}
				base.Invalidate();
				this._performNeeded = false;
			}
		}

		public void Perform()
		{
			this._performNeeded = true;
		}

		private void ApplyZoom()
		{
			this._scale = (float)Math.Pow(10.0, (double)((float)this._zoom * 4f / 100f));
			this._displayCenterFrequency = this.GetDisplayCenterFrequency();
			if (this._spectrumWidth > 0)
			{
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
			long num2 = (long)((double)this._centerFrequency - (double)this._spectrumWidth * 0.5 - ((double)num - (double)this._spectrumWidth * 0.5 / (double)this._scale));
			if (num2 > 0)
			{
				num += num2;
			}
			long num3 = (long)((double)num + (double)this._spectrumWidth * 0.5 / (double)this._scale - ((double)this._centerFrequency + (double)this._spectrumWidth * 0.5));
			if (num3 > 0)
			{
				num -= num3;
			}
			return num;
		}

		public unsafe void Render(float* powerSpectrum, int length)
		{
			float offset = (float)(this._displayCenterFrequency - this._centerFrequency) / (float)this._spectrumWidth;
			this.ExtractSpectrum(powerSpectrum, length, offset, this._scale, this._useSmoothing);
			this.Draw();
		}

		private unsafe void ExtractSpectrum(float* powerSpectrum, int length, float offset, float scale, bool useSmoothing)
		{
			int num = this._displayOffset / 10 * 10;
			int num2 = this._displayRange / 10 * 10;
			float[] temp = this._temp;
			fixed (float* ptr = temp)
			{
				float[] smoothedSpectrum = this._smoothedSpectrum;
				fixed (float* ptr2 = smoothedSpectrum)
				{
					byte[] scaledSpectrum = this._scaledSpectrum;
					fixed (byte* dest = scaledSpectrum)
					{
						Fourier.SmoothMaxCopy(powerSpectrum, ptr, length, this._smoothedSpectrum.Length, scale, offset);
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
							Utils.Memcpy(ptr2, ptr, this._smoothedSpectrum.Length * 4);
						}
						Fourier.ScaleFFT(ptr2, dest, this._smoothedSpectrum.Length, (float)(num - num2), (float)num);
					}
				}
			}
		}

		private void Draw()
		{
			if (this._buffer.Width > 30 && this._buffer.Height > 30)
			{
				this.InsertNewLine();
				if (this._useTimestamps && this._scanlines == 0)
				{
					this.DrawTimestamp();
				}
				this._performNeeded = true;
			}
		}

		private unsafe void InsertNewLine()
		{
			if (this._buffer.Width > 0 && this._buffer.Height > 0)
			{
				Rectangle rect = new Rectangle(0, 0, this._buffer.Width, this._buffer.Height);
				BitmapData bitmapData = this._buffer.LockBits(rect, ImageLockMode.ReadWrite, this._buffer.PixelFormat);
				void* ptr;
				void* dest;
				if (bitmapData.Stride > 0)
				{
					ptr = (void*)bitmapData.Scan0;
					dest = (void*)((long)bitmapData.Scan0 + bitmapData.Stride);
				}
				else
				{
					dest = (void*)bitmapData.Scan0;
					ptr = (void*)((long)bitmapData.Scan0 - bitmapData.Stride);
				}
				Utils.Memmove(dest, ptr, (bitmapData.Height - 1) * Math.Abs(bitmapData.Stride));
				if (this._scaledSpectrum != null && this._scaledSpectrum.Length != 0)
				{
					int* ptr2 = (int*)((byte*)ptr + 30L * 4L);
					int* ptr3 = ptr2;
					for (int i = 0; i < this._scaledSpectrum.Length; i++)
					{
						int val = (this._scaledSpectrum[i] + this._contrast * 2) * this._gradientPixels.Length / 255;
						val = Math.Max(val, 0);
						val = Math.Min(val, this._gradientPixels.Length - 1);
						int* intPtr = ptr3;
						ptr3 = intPtr + 1;
						*intPtr = this._gradientPixels[val];
					}
					this.OnLineInserted(new LineInsertEventArgs(ptr2, this._scaledSpectrum.Length));
				}
				this._buffer.UnlockBits(bitmapData);
				this._scanlines++;
				if (this._scanlines >= this.TimestampInterval)
				{
					this._scanlines = 0;
				}
			}
		}

		private void DrawTimestamp()
		{
			using (FontFamily family = new FontFamily("Verdana"))
			{
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					using (Pen pen = new Pen(Color.Black))
					{
						DateTime dateTime;
						string s;
						if (Waterfall._useUtcTimeStamp)
						{
							dateTime = DateTime.UtcNow;
							s = dateTime.ToString("u");
						}
						else
						{
							dateTime = DateTime.Now;
							s = dateTime.ToString();
						}
						graphicsPath.AddString(s, family, 0, 14f, new Point(30, 0), StringFormat.GenericTypographic);
						SmoothingMode smoothingMode = this._graphics.SmoothingMode;
						InterpolationMode interpolationMode = this._graphics.InterpolationMode;
						this._graphics.SmoothingMode = SmoothingMode.AntiAlias;
						this._graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
						pen.Width = 2f;
						this._graphics.DrawPath(pen, graphicsPath);
						this._graphics.FillPath(Brushes.White, graphicsPath);
						this._graphics.SmoothingMode = smoothingMode;
						this._graphics.InterpolationMode = interpolationMode;
					}
				}
			}
		}

		private void DrawCursor()
		{
			this._lower = 0f;
			float num = (float)((int)Math.Max((float)this._filterBandwidth * this._xIncrement, 2f) | 1);
			float num2 = (float)this._buffer.Width * 0.5f + (float)(this._frequency - this._displayCenterFrequency) * this._xIncrement;
			switch (this._bandType)
			{
			case BandType.Upper:
				this._lower = num2;
				break;
			case BandType.Lower:
				this._lower = num2 - num;
				break;
			case BandType.Center:
				this._lower = num2 - num * 0.5f;
				break;
			}
			this._lower += (float)this._filterOffset * this._xIncrement;
			this._upper = this._lower + num;
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
												if (this._enableFilter && num < (float)this._buffer.Width)
												{
													float num3 = this._lower;
													float num4 = num;
													if (this._lower < 30f)
													{
														num3 = 31f;
														num4 -= num3 - this._lower;
													}
													if (this._upper > (float)(this._buffer.Width - 30))
													{
														num4 -= this._upper - (float)(this._buffer.Width - 30);
													}
													this._graphics2.FillRectangle(brush, num3, 0f, num4, (float)this._buffer.Height);
												}
												if (this._enableFrequencyMarker && num2 > 30f && num2 < (float)(this._buffer.Width - 30))
												{
													pen.Width = 1f;
													this._graphics2.DrawLine(pen, num2, 0f, num2, (float)this._buffer.Height);
												}
												if (this._enableHotTracking && this._cursorPosition.X >= 30 && this._cursorPosition.X <= this._buffer.Width - 30)
												{
													if (this.Cursor != Cursors.SizeWE && ((float)this._cursorPosition.X < this._lower || (float)this._cursorPosition.X > this._upper) && !this._changingFrequency && !this._changingCenterFrequency && !this._changingBandwidth)
													{
														pen2.DashStyle = DashStyle.Dash;
														this._graphics2.DrawLine(pen3, this._snappedX, 0f, this._snappedX, (float)this._buffer.Height);
														float num5 = num / 2f;
														switch (this._bandType)
														{
														case BandType.Center:
															this._graphics2.DrawLine(pen2, this._snappedX - num5, 0f, this._snappedX - num5, (float)this._buffer.Height);
															this._graphics2.DrawLine(pen2, this._snappedX + num5, 0f, this._snappedX + num5, (float)this._buffer.Height);
															break;
														case BandType.Lower:
															this._graphics2.DrawLine(pen2, this._snappedX - num, 0f, this._snappedX - num, (float)this._buffer.Height);
															break;
														case BandType.Upper:
															this._graphics2.DrawLine(pen2, this._snappedX + num, 0f, this._snappedX + num, (float)this._buffer.Height);
															break;
														}
													}
													string s = (this._changingBandwidth || this.Cursor == Cursors.SizeWE) ? ("Bandwidth: " + SpectrumAnalyzer.GetFrequencyDisplay(this._filterBandwidth)) : (string.IsNullOrEmpty(this._customTitle) ? ((this._changingFrequency || ((float)this._cursorPosition.X >= this._lower && (float)this._cursorPosition.X <= this._upper)) ? ("VFO: " + SpectrumAnalyzer.GetFrequencyDisplay(this._frequency)) : ((!this._changingCenterFrequency) ? SpectrumAnalyzer.GetFrequencyDisplay(this._trackingFrequency) : ("Center Frequency: " + SpectrumAnalyzer.GetFrequencyDisplay(this._centerFrequency)))) : this._customTitle);
													graphicsPath.AddString(s, family, 0, 16f, Point.Empty, StringFormat.GenericTypographic);
													RectangleF bounds = graphicsPath.GetBounds();
													Cursor current = Cursor.Current;
													float val = (float)this._cursorPosition.X + 30f;
													float val2 = (float)this._cursorPosition.Y + ((current == (Cursor)null) ? 32f : ((float)current.Size.Height)) - 8f;
													val = Math.Min(val, (float)this._buffer.Width - bounds.Width - 30f - 20f);
													val2 = Math.Min(val2, (float)this._buffer.Height - bounds.Height - 20f);
													graphicsPath.Reset();
													graphicsPath.AddString(s, family, 0, 16f, new Point((int)val, (int)val2), StringFormat.GenericTypographic);
													SmoothingMode smoothingMode = this._graphics2.SmoothingMode;
													InterpolationMode interpolationMode = this._graphics2.InterpolationMode;
													this._graphics2.SmoothingMode = SmoothingMode.AntiAlias;
													this._graphics2.InterpolationMode = InterpolationMode.HighQualityBilinear;
													pen4.Width = 2f;
													RectangleF bounds2 = graphicsPath.GetBounds();
													bounds2.X -= 10f;
													bounds2.Y -= 10f;
													bounds2.Width += 20f;
													bounds2.Height += 20f;
													this._graphics2.FillRoundedRectangle(brush2, bounds2, 6);
													this._graphics2.DrawRoundedRectangle(pen5, bounds2, 6);
													this._graphics2.DrawPath(pen4, graphicsPath);
													this._graphics2.FillPath(Brushes.White, graphicsPath);
													this._graphics2.SmoothingMode = smoothingMode;
													this._graphics2.InterpolationMode = interpolationMode;
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

		private unsafe void CopyMainBuffer()
		{
			if (this._buffer.Width > 0 && this._buffer.Height > 0)
			{
				Rectangle rect = new Rectangle(0, 0, this._buffer.Width, this._buffer.Height);
				BitmapData bitmapData = this._buffer.LockBits(rect, ImageLockMode.ReadOnly, this._buffer.PixelFormat);
				BitmapData bitmapData2 = this._buffer2.LockBits(rect, ImageLockMode.WriteOnly, this._buffer2.PixelFormat);
				Utils.Memcpy((void*)bitmapData2.Scan0, (void*)bitmapData.Scan0, Math.Abs(bitmapData.Stride) * bitmapData.Height);
				this._buffer.UnlockBits(bitmapData);
				this._buffer2.UnlockBits(bitmapData2);
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
				e.Graphics.DrawImageUnscaled(this._buffer2, 0, 0);
				return;
			}
			goto IL_0024;
			IL_0024:
			e.Graphics.Clear(Color.Black);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected unsafe override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this._performNeeded = true;
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 60)
			{
				clientRectangle = base.ClientRectangle;
				if (clientRectangle.Height > 60)
				{
					Bitmap buffer = this._buffer;
					clientRectangle = base.ClientRectangle;
					int width = clientRectangle.Width;
					clientRectangle = base.ClientRectangle;
					this._buffer = new Bitmap(width, clientRectangle.Height, PixelFormat.Format32bppPArgb);
					Bitmap buffer2 = this._buffer2;
					clientRectangle = base.ClientRectangle;
					int width2 = clientRectangle.Width;
					clientRectangle = base.ClientRectangle;
					this._buffer2 = new Bitmap(width2, clientRectangle.Height, PixelFormat.Format32bppPArgb);
					int num = this._buffer.Width - 60;
					float[] smoothedSpectrum = this._smoothedSpectrum;
					this._scaledSpectrum = new byte[num];
					this._smoothedSpectrum = new float[num];
					this._temp = new float[num];
					float[] array = smoothedSpectrum;
					fixed (float* powerSpectrum = array)
					{
						this.ExtractSpectrum(powerSpectrum, smoothedSpectrum.Length, 0f, 1f, false);
					}
					this._graphics.Dispose();
					this._graphics = Graphics.FromImage(this._buffer);
					SpectrumAnalyzer.ConfigureGraphics(this._graphics, false);
					this._graphics2.Dispose();
					this._graphics2 = Graphics.FromImage(this._buffer2);
					SpectrumAnalyzer.ConfigureGraphics(this._graphics2, false);
					this._graphics.Clear(Color.Black);
					Rectangle destRect = new Rectangle(30, 0, this._buffer.Width - 60, buffer.Height);
					this._graphics.DrawImage(buffer, destRect, 30, 0, buffer.Width - 60, buffer.Height, GraphicsUnit.Pixel);
					buffer.Dispose();
					buffer2.Dispose();
					if (this._spectrumWidth > 0)
					{
						this._xIncrement = this._scale * (float)(this._buffer.Width - 60) / (float)this._spectrumWidth;
					}
					this._gradientBrush.Dispose();
					this._gradientBrush = new LinearGradientBrush(new RectangleF(15f, 15f, (float)base.Width - 15f, (float)this._buffer.Height - 15f), Color.White, Color.Black, LinearGradientMode.Vertical);
					this._gradientBrush.InterpolationColors = this._gradientColorBlend;
					this.Perform(true);
				}
			}
		}

		private void DrawGradient()
		{
			using (Pen pen = new Pen(this._gradientBrush, 10f))
			{
				this._graphics.FillRectangle(Brushes.Black, this._buffer.Width - 30, 0, 30, this._buffer.Height);
				this._graphics.DrawLine(pen, (float)this._buffer.Width - 15f, (float)this._buffer.Height - 15f, (float)this._buffer.Width - 15f, 15f);
			}
		}

		private void BuildGradientVector()
		{
			using (Bitmap bitmap = new Bitmap(1, this._gradientPixels.Length))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, 1, this._gradientPixels.Length - 1), Color.White, Color.Black, LinearGradientMode.Vertical))
					{
						linearGradientBrush.InterpolationColors = this._gradientColorBlend;
						Pen pen = new Pen(linearGradientBrush);
						graphics.DrawLine(pen, 0, 0, 0, this._gradientPixels.Length - 1);
						for (int i = 0; i < this._gradientPixels.Length; i++)
						{
							this._gradientPixels[this._gradientPixels.Length - 1 - i] = bitmap.GetPixel(0, i).ToArgb();
						}
					}
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
			long num = (long)((float)this._displayCenterFrequency - (float)this._spectrumWidth / this._scale * 0.5f);
			long num2 = (long)((float)this._displayCenterFrequency + (float)this._spectrumWidth / this._scale * 0.5f);
			if (source == FrequencyChangeSource.Scroll)
			{
				long num3 = 0L;
				if (f < num)
				{
					num3 = f - num;
				}
				else if (f > num2)
				{
					num3 = f - num2;
				}
				if (num3 != 0L && !this.UpdateCenterFrequency(this._centerFrequency + num3, source))
				{
					return false;
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
			if (f < 0)
			{
				f = 0L;
			}
			if (this._useSnap)
			{
				f = (long)((float)f + (float)(Math.Sign(f) * this._stepSize) * 0.5f) / this._stepSize * this._stepSize;
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

		protected virtual void OnLineInserted(LineInsertEventArgs e)
		{
			LineInsertEventHandler lineInserted = this.LineInserted;
			if (lineInserted != null)
			{
				lineInserted(this, e);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.X >= 30 && e.X <= this._buffer.Width - 30)
			{
				if (e.Button == MouseButtons.Left)
				{
					float num = Math.Max((float)this._filterBandwidth * this._xIncrement, 2f);
					if (this._enableFilter)
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
				this._trackingFrequency = this.PointToFrequency((float)e.X);
				if (this._useSnap)
				{
					this._trackingFrequency = (long)((float)this._trackingFrequency + (float)(Math.Sign(this._trackingFrequency) * this._stepSize) * 0.5f) / this._stepSize * this._stepSize;
					this._snappedX = this.FrequencyToPoint(this._trackingFrequency);
				}
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
				int num = 0;
				switch (this._side)
				{
				case BandType.Upper:
					num = e.X - this._oldX;
					break;
				case BandType.Lower:
					num = this._oldX - e.X;
					break;
				}
				if (this._bandType == BandType.Center && !this._enableSideFilterResize)
				{
					num *= 2;
				}
				num = (int)((float)(num * this._spectrumWidth) / this._scale / (float)(this._buffer.Width - 60) + (float)this._oldFilterBandwidth);
				this.UpdateBandwidth(num);
			}
			else if (this._enableFilter)
			{
				if (e.X >= 30 && e.X <= this._buffer.Width - 30 && this._upper - this._lower > 12f)
				{
					if (Math.Abs((float)e.X - this._lower + 6f) <= 6f && (this._bandType == BandType.Center || this._bandType == BandType.Lower))
					{
						goto IL_0267;
					}
					if (Math.Abs((float)e.X - this._upper - 6f) <= 6f && (this._bandType == BandType.Center || this._bandType == BandType.Upper))
					{
						goto IL_0267;
					}
					this.Cursor = Cursors.Default;
				}
				else
				{
					this.Cursor = Cursors.Default;
				}
			}
			goto IL_028c;
			IL_028c:
			this._performNeeded = this._enableHotTracking;
			return;
			IL_0267:
			this.Cursor = Cursors.SizeWE;
			goto IL_028c;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.Focus();
			base.OnMouseEnter(e);
			this._mouseIn = true;
			this._performNeeded = true;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this._mouseIn = false;
			this._performNeeded = true;
			this._cursorPosition = Point.Empty;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (this._enableFilter)
			{
				this.UpdateFrequency(this._frequency + this._stepSize * Math.Sign(e.Delta), FrequencyChangeSource.Scroll);
			}
		}
	}
}
