using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDRSharp.FrequencyEdit
{
	internal sealed class FrequencyEditSeparator : UserControl, IRenderable
	{
		public Image Image
		{
			get
			{
				return this._image;
			}
			set
			{
				this._image = value;
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

		public FrequencyEditSeparator()
		{
			this.DoubleBuffered = true;
			base.UpdateStyles();
			ColorMatrix colorMatrix = new ColorMatrix();
			colorMatrix.Matrix33 = 0.3f;
			this._maskedAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		}

		public void Render()
		{
			if (this._renderNeeded)
			{
				base.Invalidate();
				this._renderNeeded = false;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (this._image != null)
			{
				ImageAttributes imageAttrs = (this._masked || !base.Parent.Enabled) ? this._maskedAttributes : null;
				e.Graphics.DrawImage(this._image, new Rectangle(0, 0, base.Width, base.Height), 0f, 0f, (float)this._image.Width, (float)this._image.Height, GraphicsUnit.Pixel, imageAttrs);
			}
		}

		private const float MaskedDigitTransparency = 0.3f;

		private Image _image;

		private bool _masked;

		private bool _renderNeeded;

		private readonly ImageAttributes _maskedAttributes = new ImageAttributes();
	}
}
