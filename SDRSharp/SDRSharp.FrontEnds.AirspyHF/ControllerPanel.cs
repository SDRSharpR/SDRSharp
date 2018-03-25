using SDRSharp.Radio;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.AirspyHF
{
	public class ControllerPanel : UserControl
	{
		private AirspyHFIO _owner;

		private AirspyHFDevice _device;

		private IContainer components;

		private Label label18;

		private TableLayoutPanel tableLayoutPanel;

		private Label label1;

		private TextBox valueTextBox;

		private TextBox addressTextBox;

		private Button readButton;

		private Button writeButton;

		private Label label2;

		private ComboBox spanComboBox;

		private TableLayoutPanel tableLayoutPanel1;

		private Label label3;

		private NumericUpDown calibrationNumericUpDown;

		private Button flashButton;

		public AirspyHFDevice Device
		{
			get
			{
				return this._device;
			}
			set
			{
				this._device = value;
				if (this._device != null)
				{
					this.calibrationNumericUpDown.Value = this._device.CalibrationPPB;
				}
				else
				{
					this.calibrationNumericUpDown.Value = decimal.Zero;
				}
			}
		}

		public ControllerPanel(AirspyHFIO owner)
		{
			this._owner = owner;
			this.InitializeComponent();
			for (int i = 0; i < 5; i++)
			{
				double num = (double)((float)(double)(768000u >> i) * this._owner.UsableSpectrumRatio) * 0.001;
				this.spanComboBox.Items.Add(num + " kHz");
			}
			this.spanComboBox.SelectedIndex = Utils.GetIntSetting("airspyhf.decimation", 0);
			if (Utils.GetIntSetting("airspyhf.debug", 0) == 0)
			{
				this.tableLayoutPanel.Visible = false;
				base.Height -= this.tableLayoutPanel.Height;
			}
		}

		private void spanComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this._owner.DecimationStages = this.spanComboBox.SelectedIndex;
			Utils.SaveSetting("airspyhf.decimation", this.spanComboBox.SelectedIndex);
		}

		private void calibrationNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.CalibrationPPB = (int)this.calibrationNumericUpDown.Value;
			}
		}

		private void flashButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.FlashCalibration();
			}
		}

		private unsafe void readButton_Click(object sender, EventArgs e)
		{
			if (this._device != null && this.addressTextBox.Text.Length > 0)
			{
				uint address = this.ParseHex(this.addressTextBox.Text);
				byte[] array = new byte[6];
				byte[] array2 = array;
				fixed (byte* data = array2)
				{
					this._device.TunerRead(address, data);
				}
				uint num = (uint)(array[0] << 16 | array[1] << 8 | array[2]);
				this.valueTextBox.Text = "0x" + string.Format("{0:x6}", num);
			}
		}

		private void writeButton_Click(object sender, EventArgs e)
		{
			if (this._device != null && this.addressTextBox.Text.Length > 0 && this.valueTextBox.Text.Length > 0)
			{
				uint address = this.ParseHex(this.addressTextBox.Text);
				uint value = this.ParseHex(this.valueTextBox.Text);
				this._device.TunerWrite(address, value);
			}
		}

		private uint ParseHex(string s)
		{
			s = s.Replace("0X", string.Empty).Replace("0x", string.Empty);
			return uint.Parse(s, NumberStyles.HexNumber);
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
			this.tableLayoutPanel = new TableLayoutPanel();
			this.valueTextBox = new TextBox();
			this.label18 = new Label();
			this.label1 = new Label();
			this.addressTextBox = new TextBox();
			this.readButton = new Button();
			this.writeButton = new Button();
			this.label2 = new Label();
			this.spanComboBox = new ComboBox();
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.label3 = new Label();
			this.calibrationNumericUpDown = new NumericUpDown();
			this.flashButton = new Button();
			this.tableLayoutPanel.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((ISupportInitialize)this.calibrationNumericUpDown).BeginInit();
			base.SuspendLayout();
			this.tableLayoutPanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel.ColumnCount = 4;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel, 2);
			this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel.Controls.Add(this.valueTextBox, 1, 2);
			this.tableLayoutPanel.Controls.Add(this.label18, 0, 1);
			this.tableLayoutPanel.Controls.Add(this.label1, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.addressTextBox, 0, 2);
			this.tableLayoutPanel.Controls.Add(this.readButton, 2, 2);
			this.tableLayoutPanel.Controls.Add(this.writeButton, 3, 2);
			this.tableLayoutPanel.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.calibrationNumericUpDown, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.flashButton, 2, 0);
			this.tableLayoutPanel.Location = new Point(0, 27);
			this.tableLayoutPanel.Margin = new Padding(0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 3;
			this.tableLayoutPanel.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel.Size = new Size(202, 73);
			this.tableLayoutPanel.TabIndex = 63;
			this.valueTextBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.valueTextBox.Location = new Point(78, 47);
			this.valueTextBox.Name = "valueTextBox";
			this.valueTextBox.Size = new Size(69, 20);
			this.valueTextBox.TabIndex = 37;
			this.label18.Anchor = AnchorStyles.Left;
			this.label18.AutoSize = true;
			this.label18.Location = new Point(0, 29);
			this.label18.Margin = new Padding(0);
			this.label18.Name = "label18";
			this.label18.Size = new Size(45, 13);
			this.label18.TabIndex = 34;
			this.label18.Text = "Address";
			this.label1.Anchor = AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(75, 29);
			this.label1.Margin = new Padding(0);
			this.label1.Name = "label1";
			this.label1.Size = new Size(34, 13);
			this.label1.TabIndex = 35;
			this.label1.Text = "Value";
			this.addressTextBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.addressTextBox.Location = new Point(3, 47);
			this.addressTextBox.Name = "addressTextBox";
			this.addressTextBox.Size = new Size(69, 20);
			this.addressTextBox.TabIndex = 36;
			this.readButton.Anchor = AnchorStyles.None;
			this.readButton.Location = new Point(153, 47);
			this.readButton.Name = "readButton";
			this.readButton.Size = new Size(20, 20);
			this.readButton.TabIndex = 38;
			this.readButton.Text = "R";
			this.readButton.UseVisualStyleBackColor = true;
			this.readButton.Click += this.readButton_Click;
			this.writeButton.Anchor = AnchorStyles.None;
			this.writeButton.Location = new Point(179, 47);
			this.writeButton.Name = "writeButton";
			this.writeButton.Size = new Size(20, 20);
			this.writeButton.TabIndex = 39;
			this.writeButton.Text = "W";
			this.writeButton.UseVisualStyleBackColor = true;
			this.writeButton.Click += this.writeButton_Click;
			this.label2.Anchor = AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(3, 7);
			this.label2.Name = "label2";
			this.label2.Size = new Size(57, 13);
			this.label2.TabIndex = 40;
			this.label2.Text = "Bandwidth";
			this.spanComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.spanComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.spanComboBox.FormattingEnabled = true;
			this.spanComboBox.Location = new Point(79, 3);
			this.spanComboBox.Name = "spanComboBox";
			this.spanComboBox.Size = new Size(120, 21);
			this.spanComboBox.TabIndex = 41;
			this.spanComboBox.SelectedIndexChanged += this.spanComboBox_SelectedIndexChanged;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.62376f));
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62.37624f));
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.spanComboBox, 1, 0);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Size = new Size(202, 100);
			this.tableLayoutPanel1.TabIndex = 64;
			this.label3.Anchor = AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new Point(3, 8);
			this.label3.Name = "label3";
			this.label3.Size = new Size(57, 13);
			this.label3.TabIndex = 40;
			this.label3.Text = "CLK (PPB)";
			this.calibrationNumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.calibrationNumericUpDown.Location = new Point(78, 4);
			this.calibrationNumericUpDown.Maximum = new decimal(new int[4]
			{
				10000,
				0,
				0,
				0
			});
			this.calibrationNumericUpDown.Minimum = new decimal(new int[4]
			{
				10000,
				0,
				0,
				-2147483648
			});
			this.calibrationNumericUpDown.Name = "calibrationNumericUpDown";
			this.calibrationNumericUpDown.Size = new Size(69, 20);
			this.calibrationNumericUpDown.TabIndex = 41;
			this.calibrationNumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.calibrationNumericUpDown.ValueChanged += this.calibrationNumericUpDown_ValueChanged;
			this.flashButton.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.tableLayoutPanel.SetColumnSpan(this.flashButton, 2);
			this.flashButton.Location = new Point(153, 3);
			this.flashButton.Name = "flashButton";
			this.flashButton.Size = new Size(46, 23);
			this.flashButton.TabIndex = 42;
			this.flashButton.Text = "Flash";
			this.flashButton.UseVisualStyleBackColor = true;
			this.flashButton.Click += this.flashButton_Click;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.tableLayoutPanel1);
			base.Name = "ControllerPanel";
			base.Size = new Size(202, 100);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((ISupportInitialize)this.calibrationNumericUpDown).EndInit();
			base.ResumeLayout(false);
		}
	}
}
