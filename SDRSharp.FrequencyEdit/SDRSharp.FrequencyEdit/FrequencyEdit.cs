using System;
using System.Drawing;
using System.Windows.Forms;
using SDRSharp.FrequencyEdit.Properties;

namespace SDRSharp.FrequencyEdit
{
	public sealed class FrequencyEdit : UserControl
	{
		public event EventHandler FrequencyChanged;

		public event EventHandler<FrequencyChangingEventArgs> FrequencyChanging;

		public int StepSize
		{
			get
			{
				return this._stepSize;
			}
			set
			{
				this._stepSize = value;
			}
		}

		public bool DisableFrequencyEvents
		{
			get
			{
				return this._disableFrequencyEvents;
			}
			set
			{
				this._disableFrequencyEvents = value;
			}
		}

		public bool EntryModeActive
		{
			get
			{
				return this._currentEntryMode > EntryMode.None;
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
				this._frequencyChangingEventArgs.Accept = true;
				this._frequencyChangingEventArgs.Frequency = value;
				if (!this._disableFrequencyEvents && this.FrequencyChanging != null)
				{
					this.FrequencyChanging(this, this._frequencyChangingEventArgs);
				}
				if (this._frequencyChangingEventArgs.Accept)
				{
					this._frequency = this._frequencyChangingEventArgs.Frequency;
					this.UpdateDigitsValues();
					if (!this._disableFrequencyEvents && this.FrequencyChanged != null)
					{
						this.FrequencyChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public FrequencyEdit()
		{
			this.DoubleBuffered = true;
			this.AutoSize = true;
			base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this._digitImages = Resources.Numbers;
			this._renderTimer.Interval = 30;
			this._renderTimer.Tick += this.renderTimer_Tick;
			this._renderTimer.Enabled = true;
			this.ConfigureComponent();
		}

		private void renderTimer_Tick(object sender, EventArgs e)
		{
			for (int i = 0; i < base.Controls.Count; i++)
			{
				if (base.Controls[i] is IRenderable)
				{
					((IRenderable)base.Controls[i]).Render();
				}
			}
		}

		private void ConfigureComponent()
		{
			this.BackColor = Color.Transparent;
			if (this._digitImages != null)
			{
				for (int i = 0; i < 12; i++)
				{
					if (this._digitControls[i] != null && base.Controls.Contains(this._digitControls[i]))
					{
						base.Controls.Remove(this._digitControls[i]);
						this._digitControls[i] = null;
					}
				}
				for (int j = 0; j < 12; j++)
				{
					if (this._separatorControls[j] != null && base.Controls.Contains(this._separatorControls[j]))
					{
						base.Controls.Remove(this._separatorControls[j]);
						this._separatorControls[j] = null;
					}
				}
				this.SplitDigitImages();
			}
			if (this._imageList.Images.Count == 0)
			{
				return;
			}
			int num = 0;
			int y = 0;
			int width = this._imageList.ImageSize.Width;
			int height = this._imageList.ImageSize.Height;
			for (int k = 11; k >= 0; k--)
			{
				if ((k + 1) % 3 == 0 && k != 11)
				{
					FrequencyEditSeparator frequencyEditSeparator = new FrequencyEditSeparator();
					int num2 = width / 2;
					int num3 = k / 3;
					frequencyEditSeparator.Image = this._imageList.Images[11];
					frequencyEditSeparator.Width = num2;
					frequencyEditSeparator.Height = height;
					frequencyEditSeparator.Location = new Point(num, y);
					base.Controls.Add(frequencyEditSeparator);
					this._separatorControls[num3] = frequencyEditSeparator;
					num += num2 + 2;
				}
				FrequencyEditDigit frequencyEditDigit = new FrequencyEditDigit(k);
				frequencyEditDigit.Location = new Point(num, y);
				frequencyEditDigit.OnDigitClick += this.DigitClickHandler;
				frequencyEditDigit.MouseLeave += this.DigitMouseLeave;
				frequencyEditDigit.Width = width;
				frequencyEditDigit.Height = height;
				frequencyEditDigit.ImageList = this._imageList;
				base.Controls.Add(frequencyEditDigit);
				this._digitControls[k] = frequencyEditDigit;
				num += width + 2;
			}
			long num4 = 1L;
			for (int l = 0; l < 12; l++)
			{
				this._digitControls[l].Weight = num4;
				num4 *= 10L;
			}
			base.Height = height;
			this.UpdateDigitMask();
		}

		private void SplitDigitImages()
		{
			int height = this._digitImages.Height;
			int num = (int)Math.Round((double)((float)this._digitImages.Width / 11.5f));
			this._imageList.Images.Clear();
			this._imageList.ImageSize = new Size(num, height);
			int num2 = 0;
			Bitmap bitmap;
			for (int i = 0; i < 11; i++)
			{
				bitmap = new Bitmap(num, height);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.DrawImage(this._digitImages, new Rectangle(0, 0, num, height), new Rectangle(num2, 0, num, height), GraphicsUnit.Pixel);
				}
				num2 += num;
				this._imageList.Images.Add(bitmap);
			}
			bitmap = new Bitmap(num, height);
			using (Graphics graphics2 = Graphics.FromImage(bitmap))
			{
				graphics2.DrawImage(this._digitImages, new Rectangle(0, 0, num, height), new Rectangle(num2, 0, num / 2, height), GraphicsUnit.Pixel);
			}
			this._imageList.Images.Add(bitmap);
		}

		private void DigitClickHandler(object sender, FrequencyEditDigitClickEventArgs args)
		{
			if (this._currentEntryMode != EntryMode.None)
			{
				this.LeaveEntryMode();
				return;
			}
			FrequencyEditDigit frequencyEditDigit = (FrequencyEditDigit)sender;
			if (frequencyEditDigit != null)
			{
				this._newFrequency = this._frequency;
				if (args.Button == MouseButtons.Right)
				{
					this.ZeroDigits(frequencyEditDigit.DigitIndex);
				}
				else if (args.IsUpperHalf && this._frequency >= 0L)
				{
					this.IncrementDigit(frequencyEditDigit.DigitIndex, true);
				}
				else
				{
					this.DecrementDigit(frequencyEditDigit.DigitIndex, true);
				}
				if (this._newFrequency != this._frequency)
				{
					this._frequencyChangingEventArgs.Accept = true;
					this._frequencyChangingEventArgs.Frequency = this._newFrequency;
					if (!this._disableFrequencyEvents && this.FrequencyChanging != null)
					{
						this.FrequencyChanging(this, this._frequencyChangingEventArgs);
					}
					if (this._frequencyChangingEventArgs.Accept)
					{
						this._frequency = this._frequencyChangingEventArgs.Frequency;
						this.UpdateDigitsValues();
						if (!this._disableFrequencyEvents && this.FrequencyChanged != null)
						{
							this.FrequencyChanged(this, EventArgs.Empty);
							return;
						}
					}
					else
					{
						this.UpdateDigitsValues();
					}
				}
			}
		}

		private void IncrementDigit(int index, bool updateDigit)
		{
			FrequencyEditDigit frequencyEditDigit = this._digitControls[index];
			if (frequencyEditDigit != null)
			{
				int displayedDigit = frequencyEditDigit.DisplayedDigit;
				int num = (frequencyEditDigit.DisplayedDigit == 9) ? 0 : (frequencyEditDigit.DisplayedDigit + 1);
				long newFrequency = this._newFrequency - (long)displayedDigit * frequencyEditDigit.Weight + (long)num * frequencyEditDigit.Weight;
				if (updateDigit)
				{
					frequencyEditDigit.DisplayedDigit = num;
				}
				this._newFrequency = newFrequency;
				if (displayedDigit == 9 && index < 11)
				{
					this.IncrementDigit(index + 1, updateDigit);
				}
			}
		}

		private void DecrementDigit(int index, bool updateDigit)
		{
			FrequencyEditDigit frequencyEditDigit = this._digitControls[index];
			if (frequencyEditDigit != null)
			{
				int displayedDigit = frequencyEditDigit.DisplayedDigit;
				int num = (frequencyEditDigit.DisplayedDigit == 0) ? 9 : (frequencyEditDigit.DisplayedDigit - 1);
				long newFrequency = this._newFrequency - (long)displayedDigit * frequencyEditDigit.Weight + (long)num * frequencyEditDigit.Weight;
				if (updateDigit)
				{
					frequencyEditDigit.DisplayedDigit = num;
				}
				this._newFrequency = newFrequency;
				if (displayedDigit == 0 && index < 11 && (double)this._newFrequency > Math.Pow(10.0, (double)(index + 1)))
				{
					this.DecrementDigit(index + 1, updateDigit);
				}
			}
		}

		private void ZeroDigits(int index)
		{
			for (int i = 0; i <= index; i++)
			{
				this._digitControls[i].DisplayedDigit = 0;
			}
			long num = (long)Math.Pow(10.0, (double)(index + 1));
			this._newFrequency = this._newFrequency / num * num;
		}

		private void UpdateDigitsValues()
		{
			if (this._digitControls[0] == null)
			{
				return;
			}
			long num = this._frequency;
			for (int i = 11; i >= 0; i--)
			{
				long num2 = num / this._digitControls[i].Weight;
				this._digitControls[i].DisplayedDigit = (int)num2;
				num -= (long)this._digitControls[i].DisplayedDigit * this._digitControls[i].Weight;
			}
			this.UpdateDigitMask();
		}

		private void UpdateDigitMask()
		{
			long frequency = this._frequency;
			if (frequency >= 0L)
			{
				for (int i = 1; i < 12; i++)
				{
					if ((i + 1) % 3 == 0 && i != 11)
					{
						int num = i / 3;
						if (this._separatorControls[num] != null)
						{
							this._separatorControls[num].Masked = (this._digitControls[i + 1].Weight > frequency);
						}
					}
					if (this._digitControls[i] != null)
					{
						this._digitControls[i].Masked = (this._digitControls[i].Weight > frequency);
					}
				}
			}
		}

		private void DigitMouseLeave(object sender, EventArgs e)
		{
			if (!base.ClientRectangle.Contains(base.PointToClient(Control.MousePosition)) && this._currentEntryMode != EntryMode.None)
			{
				this.AbortEntryMode();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			if (!base.ClientRectangle.Contains(base.PointToClient(Control.MousePosition)) && this._currentEntryMode != EntryMode.None)
			{
				this.AbortEntryMode();
			}
		}

		private long GetFrequencyValue()
		{
			long num = 0L;
			for (int i = 0; i < this._digitControls.Length; i++)
			{
				num += this._digitControls[i].Weight * (long)this._digitControls[i].DisplayedDigit;
			}
			return num;
		}

		private void SetFrequencyValue(long newFrequency)
		{
			if (newFrequency != this._frequency)
			{
				this._frequencyChangingEventArgs.Accept = true;
				this._frequencyChangingEventArgs.Frequency = newFrequency;
				if (!this._disableFrequencyEvents && this.FrequencyChanging != null)
				{
					this.FrequencyChanging(this, this._frequencyChangingEventArgs);
				}
				if (this._frequencyChangingEventArgs.Accept)
				{
					this._frequency = this._frequencyChangingEventArgs.Frequency;
					this.UpdateDigitsValues();
					if (!this._disableFrequencyEvents && this.FrequencyChanged != null)
					{
						this.FrequencyChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		private void EnterDirectMode()
		{
			if (this._changingEntryMode)
			{
				return;
			}
			this._changingEntryMode = true;
			for (int i = 0; i < this._digitControls.Length; i++)
			{
				if (this._digitControls[i] != null)
				{
					this._digitControls[i].Masked = false;
					if (this._digitControls[i].CursorInside)
					{
						this._editModePosition = i;
						this._digitControls[i].Highlight = true;
					}
				}
			}
			this.ZeroDigits(this._digitControls.Length - 1);
			this._currentEntryMode = EntryMode.Direct;
			this._changingEntryMode = false;
		}

		private void DirectModeHandler(KeyEventArgs args)
		{
			Keys keyCode = args.KeyCode;
			if (keyCode <= Keys.Return)
			{
				if (keyCode != Keys.Back)
				{
					if (keyCode != Keys.Tab)
					{
						if (keyCode != Keys.Return)
						{
							return;
						}
						this.LeaveEntryMode();
						return;
					}
				}
				else
				{
					this._digitControls[this._editModePosition].DisplayedDigit = 0;
					if (this._editModePosition < this._digitControls.Length - 1)
					{
						this._digitControls[this._editModePosition].Highlight = false;
						this._editModePosition++;
						this._digitControls[this._editModePosition].Highlight = true;
						return;
					}
					return;
				}
			}
			else
			{
				if (keyCode <= Keys.D9)
				{
					if (keyCode == Keys.Escape)
					{
						this.AbortEntryMode();
						return;
					}
					switch (keyCode)
					{
					case Keys.Left:
						if (this._editModePosition < this._digitControls.Length - 1)
						{
							this._digitControls[this._editModePosition].Highlight = false;
							this._editModePosition++;
							this._digitControls[this._editModePosition].Highlight = true;
							return;
						}
						return;
					case Keys.Up:
					case Keys.Down:
					case Keys.Select:
					case Keys.Print:
					case Keys.Execute:
					case Keys.Snapshot:
					case Keys.Insert:
					case Keys.Delete:
					case Keys.Help:
						return;
					case Keys.Right:
						if (this._editModePosition > 0)
						{
							this._digitControls[this._editModePosition].Highlight = false;
							this._editModePosition--;
							this._digitControls[this._editModePosition].Highlight = true;
							return;
						}
						return;
					case Keys.D0:
					case Keys.D1:
					case Keys.D2:
					case Keys.D3:
					case Keys.D4:
					case Keys.D5:
					case Keys.D6:
					case Keys.D7:
					case Keys.D8:
					case Keys.D9:
						break;
					default:
						return;
					}
				}
				else
				{
					switch (keyCode)
					{
					case Keys.NumPad0:
					case Keys.NumPad1:
					case Keys.NumPad2:
					case Keys.NumPad3:
					case Keys.NumPad4:
					case Keys.NumPad5:
					case Keys.NumPad6:
					case Keys.NumPad7:
					case Keys.NumPad8:
					case Keys.NumPad9:
						break;
					case Keys.Multiply:
					case Keys.Add:
					case Keys.Separator:
					case Keys.Subtract:
						return;
					case Keys.Decimal:
						goto IL_249;
					default:
						if (keyCode != Keys.OemPeriod)
						{
							return;
						}
						goto IL_249;
					}
				}
				int displayedDigit = (args.KeyCode >= Keys.D0 && args.KeyCode <= Keys.D9) ? (args.KeyCode - Keys.D0) : (args.KeyCode - Keys.NumPad0);
				this._digitControls[this._editModePosition].DisplayedDigit = displayedDigit;
				if (this._editModePosition > 0)
				{
					this._digitControls[this._editModePosition].Highlight = false;
					this._editModePosition--;
					this._digitControls[this._editModePosition].Highlight = true;
					return;
				}
				this.LeaveEntryMode();
				return;
			}
			IL_249:
			this._digitControls[this._editModePosition].Highlight = false;
			this._editModePosition -= this._editModePosition % 3 + 1;
			if (this._editModePosition < 2)
			{
				if (args.KeyCode != Keys.Tab)
				{
					this._editModePosition = 0;
					this.LeaveEntryMode();
					return;
				}
				this._editModePosition = this._digitControls.Length - 1;
			}
			this._digitControls[this._editModePosition].Highlight = true;
		}

		private void EnterArrowMode()
		{
			if (this._changingEntryMode)
			{
				return;
			}
			this._changingEntryMode = true;
			for (int i = 0; i < this._digitControls.Length; i++)
			{
				if (this._digitControls[i] != null)
				{
					this._digitControls[i].Masked = false;
					if (this._digitControls[i].CursorInside)
					{
						this._editModePosition = i;
						this._digitControls[i].Highlight = true;
					}
				}
			}
			this._currentEntryMode = EntryMode.Arrow;
			this._changingEntryMode = false;
		}

		private void ArrowModeHandler(KeyEventArgs args)
		{
			Keys keyCode = args.KeyCode;
			if (keyCode <= Keys.Return)
			{
				if (keyCode == Keys.Tab)
				{
					this._digitControls[this._editModePosition].Highlight = false;
					this._editModePosition -= this._editModePosition % 3 + 1;
					if (this._editModePosition < 2)
					{
						this._editModePosition = this._digitControls.Length - 1;
					}
					this._digitControls[this._editModePosition].Highlight = true;
					return;
				}
				if (keyCode != Keys.Return)
				{
					return;
				}
			}
			else if (keyCode != Keys.Escape)
			{
				switch (keyCode)
				{
				case Keys.Left:
					if (this._editModePosition < this._digitControls.Length - 1)
					{
						this._digitControls[this._editModePosition].Highlight = false;
						this._editModePosition++;
						this._digitControls[this._editModePosition].Highlight = true;
						return;
					}
					return;
				case Keys.Up:
					this._newFrequency = this._frequency;
					this.IncrementDigit(this._editModePosition, false);
					this.SetFrequencyValue(this._newFrequency);
					return;
				case Keys.Right:
					if (this._editModePosition > 0)
					{
						this._digitControls[this._editModePosition].Highlight = false;
						this._editModePosition--;
						this._digitControls[this._editModePosition].Highlight = true;
						return;
					}
					return;
				case Keys.Down:
					this._newFrequency = this._frequency;
					this.DecrementDigit(this._editModePosition, false);
					this.SetFrequencyValue(this._newFrequency);
					return;
				default:
					return;
				}
			}
			this.AbortEntryMode();
		}

		private void AbortEntryMode()
		{
			if (this._changingEntryMode)
			{
				return;
			}
			this._changingEntryMode = true;
			this._digitControls[this._editModePosition].Highlight = false;
			this.UpdateDigitsValues();
			this._currentEntryMode = EntryMode.None;
			this._changingEntryMode = false;
		}

		private void LeaveEntryMode()
		{
			if (this._changingEntryMode)
			{
				return;
			}
			this._changingEntryMode = true;
			this._digitControls[this._editModePosition].Highlight = false;
			if (this._currentEntryMode == EntryMode.Direct)
			{
				long frequencyValue = this.GetFrequencyValue();
				this.SetFrequencyValue(frequencyValue);
			}
			this._currentEntryMode = EntryMode.None;
			this._changingEntryMode = false;
		}

		private bool DigitKeyHandler(KeyEventArgs args)
		{
			if (!base.ClientRectangle.Contains(base.PointToClient(Control.MousePosition)) || this._changingEntryMode)
			{
				return false;
			}
			if (this._currentEntryMode != EntryMode.None)
			{
				EntryMode currentEntryMode = this._currentEntryMode;
				if (currentEntryMode != EntryMode.Direct)
				{
					if (currentEntryMode == EntryMode.Arrow)
					{
						this.ArrowModeHandler(args);
					}
				}
				else
				{
					this.DirectModeHandler(args);
				}
				return true;
			}
			if ((args.KeyCode >= Keys.D0 && args.KeyCode <= Keys.D9) || (args.KeyCode >= Keys.NumPad0 && args.KeyCode <= Keys.NumPad9))
			{
				this.EnterDirectMode();
				this.DirectModeHandler(args);
				return true;
			}
			if (args.KeyCode == Keys.Up || args.KeyCode == Keys.Down || args.KeyCode == Keys.Left || args.KeyCode == Keys.Right)
			{
				this.EnterArrowMode();
				this.ArrowModeHandler(args);
				return true;
			}
			if (args.Modifiers == Keys.Control)
			{
				if (args.KeyCode == Keys.C)
				{
					Clipboard.SetText(string.Format("{0}", this.GetFrequencyValue()), TextDataFormat.Text);
					return true;
				}
				if (args.KeyCode == Keys.V)
				{
					long frequencyValue = 0L;
					if (long.TryParse(Clipboard.GetText(), out frequencyValue))
					{
						this.SetFrequencyValue(frequencyValue);
					}
					return true;
				}
			}
			return false;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (msg.Msg == 256 || msg.Msg == 260)
			{
				return this.DigitKeyHandler(new KeyEventArgs(keyData));
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private const int DigitCount = 12;

		private const int DigitImageSplitCount = 12;

		private const int DigitSeperatorCount = 12;

		private readonly FrequencyEditDigit[] _digitControls = new FrequencyEditDigit[12];

		private readonly FrequencyEditSeparator[] _separatorControls = new FrequencyEditSeparator[12];

		private readonly ImageList _imageList = new ImageList();

		private readonly Image _digitImages;

		private readonly Timer _renderTimer = new Timer();

		private readonly FrequencyChangingEventArgs _frequencyChangingEventArgs = new FrequencyChangingEventArgs();

		private long _frequency;

		private long _newFrequency;

		private int _stepSize;

		private int _editModePosition;

		private bool _changingEntryMode;

		private bool _disableFrequencyEvents;

		private EntryMode _currentEntryMode;
	}
}
