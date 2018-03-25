using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SDRSharp.PanView
{
	public class GradientDialog : Form
	{
		private IContainer components;

		private ListBox colorListBox;

		private Button upButton;

		private Button downButton;

		private PictureBox gradientPictureBox;

		private Button addButton;

		private Button deleteButton;

		private Button cancelButton;

		private Button okButton;

		private ColorDialog colorDialog;

		private GradientDialog()
		{
			this.InitializeComponent();
		}

		public static ColorBlend GetGradient(ColorBlend originalBlend)
		{
			using (GradientDialog gradientDialog = new GradientDialog())
			{
				gradientDialog.SetColorBlend(originalBlend);
				if (gradientDialog.ShowDialog() == DialogResult.OK)
				{
					return gradientDialog.GetColorBlend();
				}
			}
			return null;
		}

		private ColorBlend GetColorBlend()
		{
			ColorBlend colorBlend = new ColorBlend(this.colorListBox.Items.Count);
			float num = 1f / (float)(colorBlend.Positions.Length - 1);
			for (int i = 0; i < colorBlend.Positions.Length; i++)
			{
				colorBlend.Positions[i] = (float)i * num;
				colorBlend.Colors[i] = (Color)this.colorListBox.Items[i];
			}
			return colorBlend;
		}

		private void SetColorBlend(ColorBlend colorBlend)
		{
			for (int i = 0; i < colorBlend.Positions.Length; i++)
			{
				this.colorListBox.Items.Add(colorBlend.Colors[i]);
			}
		}

		private void colorListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index >= 0)
			{
				Color color = (Color)this.colorListBox.Items[e.Index];
				Rectangle bounds;
				if ((e.State & DrawItemState.Selected) == DrawItemState.None)
				{
					using (SolidBrush solidBrush = new SolidBrush(color))
					{
						Graphics graphics = e.Graphics;
						SolidBrush brush = solidBrush;
						bounds = e.Bounds;
						int x = bounds.Left + 1;
						bounds = e.Bounds;
						int y = bounds.Top + 1;
						bounds = e.Bounds;
						int width = bounds.Width - 2;
						bounds = e.Bounds;
						graphics.FillRectangle(brush, x, y, width, bounds.Height - 1);
					}
				}
				else
				{
					using (HatchBrush hatchBrush = new HatchBrush(HatchStyle.Percent70, color, Color.Gray))
					{
						Graphics graphics2 = e.Graphics;
						HatchBrush brush2 = hatchBrush;
						bounds = e.Bounds;
						int x2 = bounds.Left + 1;
						bounds = e.Bounds;
						int y2 = bounds.Top + 1;
						bounds = e.Bounds;
						int width2 = bounds.Width - 2;
						bounds = e.Bounds;
						graphics2.FillRectangle(brush2, x2, y2, width2, bounds.Height - 1);
					}
				}
			}
		}

		private void upButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.colorListBox.SelectedIndex;
			if (selectedIndex > 0)
			{
				object item = this.colorListBox.Items[selectedIndex];
				this.colorListBox.Items.RemoveAt(selectedIndex);
				this.colorListBox.Items.Insert(selectedIndex - 1, item);
				this.colorListBox.SelectedIndex = selectedIndex - 1;
				this.gradientPictureBox.Invalidate();
			}
		}

		private void downButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.colorListBox.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < this.colorListBox.Items.Count - 1)
			{
				object item = this.colorListBox.Items[selectedIndex];
				this.colorListBox.Items.RemoveAt(selectedIndex);
				this.colorListBox.Items.Insert(selectedIndex + 1, item);
				this.colorListBox.SelectedIndex = selectedIndex + 1;
				this.gradientPictureBox.Invalidate();
			}
		}

		private void addButton_Click(object sender, EventArgs e)
		{
			if (this.colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				this.colorListBox.Items.Add(this.colorDialog.Color);
				this.gradientPictureBox.Invalidate();
			}
		}

		private void deleteButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.colorListBox.SelectedIndex;
			if (selectedIndex >= 0 && this.colorListBox.Items.Count > 2)
			{
				this.colorListBox.Items.RemoveAt(selectedIndex);
				this.gradientPictureBox.Invalidate();
			}
		}

		private void gradientPictureBox_Paint(object sender, PaintEventArgs e)
		{
			ColorBlend colorBlend = this.GetColorBlend();
			using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(this.gradientPictureBox.ClientRectangle, Color.White, Color.Black, LinearGradientMode.Vertical))
			{
				linearGradientBrush.InterpolationColors = colorBlend;
				e.Graphics.FillRectangle(linearGradientBrush, e.ClipRectangle);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.colorListBox = new ListBox();
			this.upButton = new Button();
			this.downButton = new Button();
			this.gradientPictureBox = new PictureBox();
			this.addButton = new Button();
			this.deleteButton = new Button();
			this.cancelButton = new Button();
			this.okButton = new Button();
			this.colorDialog = new ColorDialog();
			((ISupportInitialize)this.gradientPictureBox).BeginInit();
			base.SuspendLayout();
			this.colorListBox.DrawMode = DrawMode.OwnerDrawVariable;
			this.colorListBox.FormattingEnabled = true;
			this.colorListBox.Location = new Point(12, 12);
			this.colorListBox.Name = "colorListBox";
			this.colorListBox.Size = new Size(107, 238);
			this.colorListBox.TabIndex = 0;
			this.colorListBox.DrawItem += this.colorListBox_DrawItem;
			this.upButton.Location = new Point(164, 12);
			this.upButton.Name = "upButton";
			this.upButton.Size = new Size(75, 23);
			this.upButton.TabIndex = 1;
			this.upButton.Text = "Up";
			this.upButton.UseVisualStyleBackColor = true;
			this.upButton.Click += this.upButton_Click;
			this.downButton.Location = new Point(164, 41);
			this.downButton.Name = "downButton";
			this.downButton.Size = new Size(75, 23);
			this.downButton.TabIndex = 2;
			this.downButton.Text = "Down";
			this.downButton.UseVisualStyleBackColor = true;
			this.downButton.Click += this.downButton_Click;
			this.gradientPictureBox.BorderStyle = BorderStyle.FixedSingle;
			this.gradientPictureBox.Location = new Point(125, 12);
			this.gradientPictureBox.Name = "gradientPictureBox";
			this.gradientPictureBox.Size = new Size(33, 238);
			this.gradientPictureBox.TabIndex = 3;
			this.gradientPictureBox.TabStop = false;
			this.gradientPictureBox.Paint += this.gradientPictureBox_Paint;
			this.addButton.Location = new Point(164, 70);
			this.addButton.Name = "addButton";
			this.addButton.Size = new Size(75, 23);
			this.addButton.TabIndex = 3;
			this.addButton.Text = "Add";
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += this.addButton_Click;
			this.deleteButton.Location = new Point(164, 99);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new Size(75, 23);
			this.deleteButton.TabIndex = 4;
			this.deleteButton.Text = "Delete";
			this.deleteButton.UseVisualStyleBackColor = true;
			this.deleteButton.Click += this.deleteButton_Click;
			this.cancelButton.DialogResult = DialogResult.Cancel;
			this.cancelButton.Location = new Point(164, 227);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new Size(75, 23);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.okButton.DialogResult = DialogResult.OK;
			this.okButton.Location = new Point(164, 198);
			this.okButton.Name = "okButton";
			this.okButton.Size = new Size(75, 23);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.colorDialog.AnyColor = true;
			this.colorDialog.FullOpen = true;
			base.AcceptButton = this.okButton;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.CancelButton = this.cancelButton;
			base.ClientSize = new Size(251, 262);
			base.Controls.Add(this.okButton);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.deleteButton);
			base.Controls.Add(this.addButton);
			base.Controls.Add(this.gradientPictureBox);
			base.Controls.Add(this.downButton);
			base.Controls.Add(this.upButton);
			base.Controls.Add(this.colorListBox);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "GradientDialog";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Gradient Editor";
			((ISupportInitialize)this.gradientPictureBox).EndInit();
			base.ResumeLayout(false);
		}
	}
}
