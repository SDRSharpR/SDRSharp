using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDRSharp.FrequencyEdit
{
	internal sealed class FrequencyEditDigit : UserControl, IRenderable
	{
		public event OnDigitClickDelegate OnDigitClick;

		public ImageList ImageList
		{
			get
			{
				return this._imageList;
			}
			set
			{
				this._imageList = value;
			}
		}

		public bool Highlight
		{
			get
			{
				return this._highlight;
			}
			set
			{
				this._highlight = value;
				this._renderNeeded = true;
			}
		}

		public bool CursorInside
		{
			get
			{
				return this._cursorInside;
			}
		}

		public int DisplayedDigit
		{
			get
			{
				return this._displayedDigit;
			}
			set
			{
				if (value >= 0 && value <= 9 && this._displayedDigit != value)
				{
					this._displayedDigit = value;
					this._renderNeeded = true;
				}
			}
		}

		public int DigitIndex
		{
			get
			{
				return this._digitIndex;
			}
		}

		public bool Masked
		{
			get
			{
				return this._masked;
			}
			set
			{
				if (this._masked != value)
				{
					this._masked = value;
					this._renderNeeded = true;
				}
			}
		}

		public long Weight
		{
			get
			{
				return this._weight;
			}
			set
			{
				this._weight = value;
			}
		}

		public FrequencyEditDigit(int digitIndex)
		{
			this.DoubleBuffered = true;
			this.BackColor = Color.Transparent;
			this._tickTimer.Tick += this.timer_Tick;
			base.UpdateStyles();
			this._digitIndex = digitIndex;
			ColorMatrix colorMatrix = new ColorMatrix();
			colorMatrix.Matrix33 = 0.3f;
			this._maskedAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (this._imageList != null && this._displayedDigit < this._imageList.Images.Count)
			{
				Image image = this._imageList.Images[this._displayedDigit];
				ImageAttributes imageAttrs = ((this._masked && !this._cursorInside) || !base.Parent.Enabled) ? this._maskedAttributes : null;
				e.Graphics.DrawImage(image, new Rectangle(0, 0, base.Width, base.Height), 0f, 0f, (float)image.Width, (float)image.Height, GraphicsUnit.Pixel, imageAttrs);
			}
			if (this._cursorInside && !((FrequencyEdit)base.Parent).EntryModeActive)
			{
				bool flag = this._lastMouseY <= base.ClientRectangle.Height / 2;
				using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(100, flag ? Color.Red : Color.Blue)))
				{
					if (flag)
					{
						e.Graphics.FillRectangle(solidBrush, new Rectangle(0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height / 2));
					}
					else
					{
						e.Graphics.FillRectangle(solidBrush, new Rectangle(0, base.ClientRectangle.Height / 2, base.ClientRectangle.Width, base.ClientRectangle.Height));
					}
				}
			}
			if (this._highlight)
			{
				SolidBrush brush = new SolidBrush(Color.FromArgb(25, Color.Red));
				e.Graphics.FillRectangle(brush, new Rectangle(0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height));
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			this._isUpperHalf = (e.Y <= base.ClientRectangle.Height / 2);
			this._lastMouseY = e.Y;
			if (this._isUpperHalf != this._lastIsUpperHalf)
			{
				this._renderNeeded = true;
				this._tickCount = 0;
			}
			this._lastIsUpperHalf = this._isUpperHalf;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			this._cursorInside = true;
			this._renderNeeded = true;
			base.Focus();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			this._cursorInside = false;
			this._renderNeeded = true;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			this._isUpperHalf = (e.Y <= base.ClientRectangle.Height / 2);
			if (this.OnDigitClick != null)
			{
				this.OnDigitClick(this, new FrequencyEditDigitClickEventArgs(this._isUpperHalf, e.Button));
			}
			this._tickCount = 1;
			this._tickTimer.Interval = 300;
			this._tickTimer.Enabled = true;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			this._tickTimer.Enabled = false;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (this.OnDigitClick != null)
			{
				this.OnDigitClick(this, new FrequencyEditDigitClickEventArgs(e.Delta > 0, e.Button));
			}
		}

		public void Render()
		{
			if (this._renderNeeded)
			{
				base.Invalidate();
				this._renderNeeded = false;
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (this.OnDigitClick != null)
			{
				this.OnDigitClick(this, new FrequencyEditDigitClickEventArgs(this._isUpperHalf, MouseButtons.Left));
			}
			this._tickCount++;
			int tickCount = this._tickCount;
			if (tickCount <= 20)
			{
				if (tickCount == 10)
				{
					this._tickTimer.Interval = 200;
					return;
				}
				if (tickCount != 20)
				{
					return;
				}
				this._tickTimer.Interval = 100;
				return;
			}
			else
			{
				if (tickCount == 50)
				{
					this._tickTimer.Interval = 50;
					return;
				}
				if (tickCount != 100)
				{
					return;
				}
				this._tickTimer.Interval = 20;
				return;
			}
		}

		private const float MaskedDigitTransparency = 0.3f;

		private bool _masked;

		private int _displayedDigit;

		private long _weight;

		private bool _renderNeeded;

		private bool _cursorInside;

		private bool _highlight;

		private int _lastMouseY;

		private bool _lastIsUpperHalf;

		private bool _isUpperHalf;

		private int _tickCount;

		private ImageList _imageList;

		private readonly int _digitIndex;

		private readonly Timer _tickTimer = new Timer();

		private readonly ImageAttributes _maskedAttributes = new ImageAttributes();
	}
}
