using SDRSharp.Common;
using SDRSharp.Radio;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.Airspy
{
	public class ControllerPanel : UserControl
	{
		private AirspyDevice _device;

		private AirspyGainMode _gainMode;

		private AirspyIO _owner;

		private uint _sampleRate;

		private int _baseHeight;

		private bool _debug;

		private IContainer components;

		private TrackBar lnaTrackBar;

		private Label label2;

		private CheckBox lnaAgcCheckBox;

		private Label lnaGainLabel;

		private Label vgaGainLabel;

		private Label label3;

		private TrackBar vgaTrackBar;

		private Label mixerGainLabel;

		private CheckBox mixerAgcCheckBox;

		private Label label6;

		private TrackBar mixerTrackBar;

		private Label label8;

		private ComboBox decimationComboBox;

		private ComboBox sampleRateComboBox;

		private Label label9;

		private Label label10;

		private Label displayBandwidthRateLabel;

		private TableLayoutPanel mainTableLayoutPanel;

		private Panel debugPanel;

		private Label label4;

		private Button writeClockButton;

		private Label label1;

		private Button readClockButton;

		private TextBox tunerRegTextBox;

		private TextBox clockValTextBox;

		private Label label5;

		private TextBox clockRegTextBox;

		private TextBox tunerValTextBox;

		private Label label7;

		private Button readTunerButton;

		private Button writeTunerButton;

		private Label label11;

		private NumericUpDown lpfNumericUpDown;

		private Button writeGPIOButton;

		private Button readGPIOButton;

		private TextBox gpioValueTextBox;

		private TextBox gpioAddressTextBox;

		private Label label12;

		private Button dumpButton;

		private Button readMemoryButton;

		private TextBox memoryValueTextBox;

		private TextBox memoryAddressTextBox;

		private Label label13;

		private Label label14;

		private CheckBox biasTeeCheckBox;

		private CheckBox usePackingCheckBox;

		private CheckBox spyVerterCheckBox;

		private Label label15;

		private Label label16;

		private NumericUpDown spyverterPPMnumericUpDown;

		private System.Windows.Forms.Timer refreshTimer;

		private TableLayoutPanel advancedTableLayoutPanel;

		private RadioButton freeRadioButton;

		private RadioButton linearityRadioButton;

		private RadioButton sensitivityRadioButton;

		private TableLayoutPanel simplifiedTableLayoutPanel;

		private TrackBar simplifiedTrackBar;

		private Label simplifiedGainLabel;

		private Label label18;

		private TableLayoutPanel tableLayoutPanel2;

		private CheckBox trackingFilterCheckBox;

		private Button calibrateButton;

		private NumericUpDown hpfNumericUpDown;

		private CheckBox dynamicRangeCheckBox;

		public AirspyDevice Device
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
					this.InitDevice();
				}
				else
				{
					this.sampleRateComboBox.Items.Clear();
					this.UpdateActualSampleRate();
				}
			}
		}

		public bool RefreshTimerEnabled
		{
			get
			{
				return this.refreshTimer.Enabled;
			}
			set
			{
				this.refreshTimer.Enabled = value;
			}
		}

		public ControllerPanel(AirspyIO owner)
		{
			this._owner = owner;
			this.InitializeComponent();
			this.vgaTrackBar.Value = Utils.GetIntSetting("airspy.vga", 2);
			this.mixerAgcCheckBox.Checked = Utils.GetBooleanSetting("airspy.mixerAgc");
			this.mixerTrackBar.Value = Utils.GetIntSetting("airspy.mixer", 0);
			this.lnaAgcCheckBox.Checked = Utils.GetBooleanSetting("airspy.lnaAgc");
			this.lnaTrackBar.Value = Utils.GetIntSetting("airspy.lna", 0);
			this.simplifiedTrackBar.Value = Utils.GetIntSetting("airspy.simplifiedGain", 0);
			this.decimationComboBox.SelectedIndex = Utils.GetIntSetting("airspy.decimation", 0);
			this.trackingFilterCheckBox.Checked = Utils.GetBooleanSetting("airspy.trackingFilterEnabled");
			this.biasTeeCheckBox.Checked = Utils.GetBooleanSetting("airspy.biasTeeEnabled");
			this.spyVerterCheckBox.Checked = Utils.GetBooleanSetting("airspy.spyVerterEnabled");
			this.spyverterPPMnumericUpDown.Value = (decimal)Utils.GetDoubleSetting("airspy.spyVerterPPM", 0.0);
			this._sampleRate = (uint)Utils.GetIntSetting("airspy.sampleRate", 0);
			this.usePackingCheckBox.Checked = Utils.GetBooleanSetting("airspy.usePacking");
			this.dynamicRangeCheckBox.Checked = Utils.GetBooleanSetting("airspy.useDynamicRangeEnhancements", true);
			this._debug = (Utils.GetIntSetting("airspy.debug", 0) != 0);
			base.Height -= this.simplifiedTableLayoutPanel.Height;
			base.Height -= this.advancedTableLayoutPanel.Height;
			if (this._debug)
			{
				this.sampleRateComboBox.DropDownStyle = ComboBoxStyle.DropDown;
				this.sampleRateComboBox.SelectedValueChanged += this.SampleRate_Changed;
				this.sampleRateComboBox.KeyDown += this.SampleRateComboBox_KeyDown;
			}
			else
			{
				this.debugPanel.Visible = false;
				base.Height -= this.debugPanel.Height;
				this.sampleRateComboBox.SelectedIndexChanged += this.SampleRate_Changed;
			}
			this._baseHeight = base.Height;
			this._gainMode = (AirspyGainMode)Utils.GetIntSetting("airspy.gainMode", 0);
			switch (this._gainMode)
			{
			case AirspyGainMode.Custom:
				this.freeRadioButton.Checked = true;
				break;
			case AirspyGainMode.Linearity:
				this.linearityRadioButton.Checked = true;
				break;
			case AirspyGainMode.Sensitivity:
				this.sensitivityRadioButton.Checked = true;
				break;
			}
			this.gainModeRadioButton_CheckedChanged(null, null);
		}

		private void InitDevice()
		{
			this.sampleRateComboBox.Items.Clear();
			uint[] supportedSampleRates = this._device.SupportedSampleRates;
			foreach (uint sampleRate in supportedSampleRates)
			{
				this.sampleRateComboBox.Items.Add(ControllerPanel.SampleRateToString(sampleRate) + "SPS");
			}
			if (this._sampleRate == 0)
			{
				this._sampleRate = this._device.SupportedSampleRates[0];
			}
			string text = ControllerPanel.SampleRateToString(this._sampleRate) + "SPS";
			if (this._debug)
			{
				this.sampleRateComboBox.Text = text;
			}
			else
			{
				int num = this.sampleRateComboBox.Items.IndexOf(text);
				if (num < 0)
				{
					num = 0;
				}
				this.sampleRateComboBox.SelectedIndex = num;
			}
			this.SampleRate_Changed(null, null);
			this.trackingFilterCheckBox_CheckedChanged(null, null);
			this.biasTeeCheckBox_CheckedChanged(null, null);
			this.spyVerterCheckBox_CheckedChanged(null, null);
			this.spyverterPPMnumericUpDown_ValueChanged(null, null);
			this.usePackingCheckBox_CheckedChanged(null, null);
			this.dynamicRangeCheckBox_CheckedChanged(null, null);
			this.InitGains();
		}

		private void InitGains()
		{
			if (this._device != null)
			{
				this._device.GainMode = this._gainMode;
			}
			switch (this._gainMode)
			{
			case AirspyGainMode.Custom:
				this.vgaTrackBar_Scroll(null, null);
				this.mixerAgcCheckBox_CheckedChanged(null, null);
				this.mixerTrackBar_Scroll(null, null);
				this.lnaAgcCheckBox_CheckedChanged(null, null);
				this.lnaTrackBar_Scroll(null, null);
				break;
			case AirspyGainMode.Linearity:
			case AirspyGainMode.Sensitivity:
				this.simplifiedTrackBar_Scroll(null, null);
				break;
			}
		}

		public void SaveSettings()
		{
			Utils.SaveSetting("airspy.vga", this.vgaTrackBar.Value);
			Utils.SaveSetting("airspy.mixerAgc", this.mixerAgcCheckBox.Checked);
			Utils.SaveSetting("airspy.mixer", this.mixerTrackBar.Value);
			Utils.SaveSetting("airspy.lnaAgc", this.lnaAgcCheckBox.Checked);
			Utils.SaveSetting("airspy.lna", this.lnaTrackBar.Value);
			Utils.SaveSetting("airspy.simplifiedGain", this.simplifiedTrackBar.Value);
			Utils.SaveSetting("airspy.sampleRate", this._sampleRate);
			Utils.SaveSetting("airspy.decimation", this.decimationComboBox.SelectedIndex);
			Utils.SaveSetting("airspy.trackingFilterEnabled", this.trackingFilterCheckBox.Checked);
			Utils.SaveSetting("airspy.biasTeeEnabled", this.biasTeeCheckBox.Checked);
			Utils.SaveSetting("airspy.spyVerterEnabled", this.spyVerterCheckBox.Checked);
			Utils.SaveSetting("airspy.spyVerterPPM", this.spyverterPPMnumericUpDown.Value);
			Utils.SaveSetting("airspy.usePacking", this.usePackingCheckBox.Checked);
			Utils.SaveSetting("airspy.gainMode", (int)this._gainMode);
			Utils.SaveSetting("airspy.useDynamicRangeEnhancements", this.dynamicRangeCheckBox.Checked);
		}

		private void vgaTrackBar_Scroll(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.VgaGain = (byte)this.vgaTrackBar.Value;
				this.vgaGainLabel.Text = this.vgaTrackBar.Value.ToString();
			}
		}

		private void mixerAgcCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.MixerGainAuto = this.mixerAgcCheckBox.Checked;
				this.mixerTrackBar.Enabled = !this.mixerAgcCheckBox.Checked;
				this.mixerGainLabel.Visible = !this.mixerAgcCheckBox.Checked;
			}
		}

		private void mixerTrackBar_Scroll(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.MixerGain = (byte)this.mixerTrackBar.Value;
				this.mixerGainLabel.Text = this.mixerTrackBar.Value.ToString();
			}
		}

		private void lnaAgcCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.LnaGainAuto = this.lnaAgcCheckBox.Checked;
				this.lnaTrackBar.Enabled = !this.lnaAgcCheckBox.Checked;
				this.lnaGainLabel.Visible = !this.lnaAgcCheckBox.Checked;
			}
		}

		private void lnaTrackBar_Scroll(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.LnaGain = (byte)this.lnaTrackBar.Value;
				this.lnaGainLabel.Text = this.lnaTrackBar.Value.ToString();
			}
		}

		private void SampleRate_Changed(object sender, EventArgs e)
		{
			try
			{
				this.UpdateActualSampleRate();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void SampleRateComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.SampleRate_Changed(sender, e);
			}
		}

		private void UpdateActualSampleRate()
		{
			if (this._device != null)
			{
				string[] array = this.sampleRateComboBox.Text.Split(' ');
				this._sampleRate = (uint)(float.Parse((array == null || array.Length == 0) ? this.sampleRateComboBox.Text : array[0], CultureInfo.InvariantCulture) * 1000000f);
				this._device.SampleRate = this._sampleRate;
				this._device.DecimationStages = this.decimationComboBox.SelectedIndex;
				this.displayBandwidthRateLabel.Text = ControllerPanel.SampleRateToString((uint)((float)(double)this._device.DecimatedSampleRate * this._owner.UsableSpectrumRatio)) + "Hz";
			}
			else
			{
				this.displayBandwidthRateLabel.Text = "Unknown";
			}
		}

		private static string SampleRateToString(uint sampleRate)
		{
			double num;
			if ((double)sampleRate >= 1000000.0)
			{
				num = (double)sampleRate * 1E-06;
				return num.ToString(CultureInfo.InvariantCulture) + " M";
			}
			if ((double)sampleRate >= 1000.0)
			{
				num = (double)sampleRate * 0.001;
				return num.ToString(CultureInfo.InvariantCulture) + " k";
			}
			return sampleRate.ToString(CultureInfo.InvariantCulture) + " ";
		}

		private void biasTeeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.BiasTeeEnabled = this.biasTeeCheckBox.Checked;
			}
		}

		private void spyVerterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.SpyVerterEnabled = this.spyVerterCheckBox.Checked;
			}
			this.spyverterPPMnumericUpDown.Enabled = this.spyVerterCheckBox.Checked;
		}

		private void spyverterPPMnumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.SpyVerterPPM = (float)this.spyverterPPMnumericUpDown.Value;
			}
		}

		private void usePackingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				ISharpControl sharpControl = this._owner.SharpControl;
				if (sharpControl != null && sharpControl.IsPlaying && e != null)
				{
					sharpControl.StopRadio();
					this._device.UsePacking = this.usePackingCheckBox.Checked;
					sharpControl.StartRadio();
				}
				else
				{
					this._device.UsePacking = this.usePackingCheckBox.Checked;
				}
			}
		}

		private void refreshTimer_Tick(object sender, EventArgs e)
		{
			if (this._owner.IsDeviceHung)
			{
				this._owner.SharpControl.StopRadio();
			}
			AirspyDevice device = this._device;
			if (device != null)
			{
				this.dynamicRangeCheckBox.Enabled = !device.IsStreaming;
			}
			else
			{
				this.dynamicRangeCheckBox.Enabled = true;
			}
		}

		private void gainModeRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.sensitivityRadioButton.Checked)
			{
				this.advancedTableLayoutPanel.Visible = false;
				this.simplifiedTableLayoutPanel.Visible = true;
				base.Height = this._baseHeight + this.simplifiedTableLayoutPanel.Height;
				this._gainMode = AirspyGainMode.Sensitivity;
				this.InitGains();
			}
			else if (this.linearityRadioButton.Checked)
			{
				this.advancedTableLayoutPanel.Visible = false;
				this.simplifiedTableLayoutPanel.Visible = true;
				base.Height = this._baseHeight + this.simplifiedTableLayoutPanel.Height;
				this._gainMode = AirspyGainMode.Linearity;
				this.InitGains();
			}
			else if (this.freeRadioButton.Checked)
			{
				this.simplifiedTableLayoutPanel.Visible = false;
				this.advancedTableLayoutPanel.Visible = true;
				base.Height = this._baseHeight + this.advancedTableLayoutPanel.Height;
				this._gainMode = AirspyGainMode.Custom;
				this.InitGains();
			}
		}

		private void simplifiedTrackBar_Scroll(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				switch (this._gainMode)
				{
				case AirspyGainMode.Sensitivity:
					this._device.SensitivityGain = (byte)this.simplifiedTrackBar.Value;
					break;
				case AirspyGainMode.Linearity:
					this._device.LinearityGain = (byte)this.simplifiedTrackBar.Value;
					break;
				}
			}
			this.simplifiedGainLabel.Text = this.simplifiedTrackBar.Value.ToString();
		}

		private void trackingFilterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.BypassTrackingFilter = !this.trackingFilterCheckBox.Checked;
			}
		}

		private void dynamicRangeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.UseDynamicRangeEnhancements = this.dynamicRangeCheckBox.Checked;
			}
		}

		private void readTunerButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					byte reg = this.ParseHex(this.tunerRegTextBox.Text);
					byte r820TRegister = this._device.GetR820TRegister(reg);
					string text = Convert.ToString(r820TRegister, 2);
					text = text.PadLeft(8, '0');
					this.tunerValTextBox.Text = text.Insert(4, " ");
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The register address is not a valid hex number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void readClockButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					byte reg = this.ParseHex(this.clockRegTextBox.Text);
					byte si5351CRegister = this._device.GetSi5351CRegister(reg);
					string text = Convert.ToString(si5351CRegister, 2);
					text = text.PadLeft(8, '0');
					this.clockValTextBox.Text = text.Insert(4, " ");
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The register address is not a valid hex number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void writeTunerButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					byte reg = this.ParseHex(this.tunerRegTextBox.Text);
					byte value = this.ParseBin(this.tunerValTextBox.Text);
					this._device.SetR820TRegister(reg, value);
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The register address is not a valid hex number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void writeClockButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					byte reg = this.ParseHex(this.clockRegTextBox.Text);
					byte value = this.ParseBin(this.clockValTextBox.Text);
					this._device.SetSi5351CRegister(reg, value);
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The register address is not a valid hex number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private byte ParseHex(string s)
		{
			s = s.Replace("0X", string.Empty).Replace("0x", string.Empty);
			return byte.Parse(s, NumberStyles.HexNumber);
		}

		private byte ParseBin(string s)
		{
			s = s.Replace(" ", string.Empty);
			return Convert.ToByte(s, 2);
		}

		private void tunerRegTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.readTunerButton_Click(sender, e);
			}
		}

		private void clockRegTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.readClockButton_Click(sender, e);
			}
		}

		private void tunerValTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.writeTunerButton_Click(sender, e);
			}
		}

		private void clockValTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.writeClockButton_Click(sender, e);
			}
		}

		private void gpioAddressTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.readGPIOButton_Click(sender, e);
			}
		}

		private void gpioValueTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.writeGPIOButton_Click(sender, e);
			}
		}

		private void readGPIOButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					string[] array = this.gpioAddressTextBox.Text.Split(':');
					int port = int.Parse(array[0]);
					int pin = int.Parse(array[1]);
					bool gPIO = this._device.GetGPIO((airspy_gpio_port_t)port, (airspy_gpio_pin_t)pin);
					this.gpioValueTextBox.Text = (gPIO ? "1" : "0");
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The register address is not in the \"port:pin\" format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void writeGPIOButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					string[] array = this.gpioAddressTextBox.Text.Split(':');
					int port = int.Parse(array[0]);
					int pin = int.Parse(array[1]);
					int num = int.Parse(this.gpioValueTextBox.Text);
					this._device.SetGPIO((airspy_gpio_port_t)port, (airspy_gpio_pin_t)pin, num != 0);
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The register address is not in the \"port:pin\" format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void dumpButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this.dumpButton.Enabled = false;
				WaitCallback callBack = delegate
				{
					this._device.Dump("");
					base.BeginInvoke((Action)delegate
					{
						this.dumpButton.Enabled = true;
					});
				};
				ThreadPool.QueueUserWorkItem(callBack);
			}
		}

		private void readMemoryButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				try
				{
					uint address = uint.Parse(this.memoryAddressTextBox.Text.Remove(0, 2), NumberStyles.HexNumber);
					uint memory = this._device.GetMemory(address);
					this.memoryValueTextBox.Text = "0x" + memory.ToString("X").PadLeft(8, '0');
				}
				catch (FormatException)
				{
					MessageBox.Show(this, "The memory address is not in the hex format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}

		private void memoryAddressTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.readMemoryButton_Click(sender, e);
			}
		}

		private void calibrateButton_Click(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.CalibrateIF();
			}
		}

		private void hpfNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.SetAnalogIFFilters((byte)this.lpfNumericUpDown.Value, (byte)this.hpfNumericUpDown.Value);
			}
		}

		private void lpfNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (this._device != null)
			{
				this._device.SetAnalogIFFilters((byte)this.lpfNumericUpDown.Value, (byte)this.hpfNumericUpDown.Value);
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
			this.components = new Container();
			this.lnaTrackBar = new TrackBar();
			this.label2 = new Label();
			this.lnaAgcCheckBox = new CheckBox();
			this.lnaGainLabel = new Label();
			this.vgaGainLabel = new Label();
			this.label3 = new Label();
			this.vgaTrackBar = new TrackBar();
			this.mixerGainLabel = new Label();
			this.mixerAgcCheckBox = new CheckBox();
			this.label6 = new Label();
			this.mixerTrackBar = new TrackBar();
			this.label8 = new Label();
			this.decimationComboBox = new ComboBox();
			this.sampleRateComboBox = new ComboBox();
			this.label9 = new Label();
			this.label10 = new Label();
			this.displayBandwidthRateLabel = new Label();
			this.mainTableLayoutPanel = new TableLayoutPanel();
			this.debugPanel = new Panel();
			this.hpfNumericUpDown = new NumericUpDown();
			this.calibrateButton = new Button();
			this.usePackingCheckBox = new CheckBox();
			this.readMemoryButton = new Button();
			this.memoryValueTextBox = new TextBox();
			this.memoryAddressTextBox = new TextBox();
			this.label13 = new Label();
			this.dumpButton = new Button();
			this.writeGPIOButton = new Button();
			this.readGPIOButton = new Button();
			this.gpioValueTextBox = new TextBox();
			this.gpioAddressTextBox = new TextBox();
			this.label12 = new Label();
			this.label11 = new Label();
			this.lpfNumericUpDown = new NumericUpDown();
			this.label4 = new Label();
			this.writeClockButton = new Button();
			this.label1 = new Label();
			this.readClockButton = new Button();
			this.tunerRegTextBox = new TextBox();
			this.clockValTextBox = new TextBox();
			this.label5 = new Label();
			this.clockRegTextBox = new TextBox();
			this.tunerValTextBox = new TextBox();
			this.label7 = new Label();
			this.readTunerButton = new Button();
			this.writeTunerButton = new Button();
			this.biasTeeCheckBox = new CheckBox();
			this.label14 = new Label();
			this.advancedTableLayoutPanel = new TableLayoutPanel();
			this.simplifiedTableLayoutPanel = new TableLayoutPanel();
			this.simplifiedTrackBar = new TrackBar();
			this.simplifiedGainLabel = new Label();
			this.label18 = new Label();
			this.spyverterPPMnumericUpDown = new NumericUpDown();
			this.label16 = new Label();
			this.tableLayoutPanel2 = new TableLayoutPanel();
			this.freeRadioButton = new RadioButton();
			this.sensitivityRadioButton = new RadioButton();
			this.linearityRadioButton = new RadioButton();
			this.trackingFilterCheckBox = new CheckBox();
			this.dynamicRangeCheckBox = new CheckBox();
			this.label15 = new Label();
			this.spyVerterCheckBox = new CheckBox();
			this.refreshTimer = new System.Windows.Forms.Timer(this.components);
			((ISupportInitialize)this.lnaTrackBar).BeginInit();
			((ISupportInitialize)this.vgaTrackBar).BeginInit();
			((ISupportInitialize)this.mixerTrackBar).BeginInit();
			this.mainTableLayoutPanel.SuspendLayout();
			this.debugPanel.SuspendLayout();
			((ISupportInitialize)this.hpfNumericUpDown).BeginInit();
			((ISupportInitialize)this.lpfNumericUpDown).BeginInit();
			this.advancedTableLayoutPanel.SuspendLayout();
			this.simplifiedTableLayoutPanel.SuspendLayout();
			((ISupportInitialize)this.simplifiedTrackBar).BeginInit();
			((ISupportInitialize)this.spyverterPPMnumericUpDown).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			base.SuspendLayout();
			this.lnaTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.advancedTableLayoutPanel.SetColumnSpan(this.lnaTrackBar, 3);
			this.lnaTrackBar.Location = new Point(0, 170);
			this.lnaTrackBar.Margin = new Padding(0);
			this.lnaTrackBar.Maximum = 15;
			this.lnaTrackBar.Name = "lnaTrackBar";
			this.lnaTrackBar.Size = new Size(202, 45);
			this.lnaTrackBar.TabIndex = 4;
			this.lnaTrackBar.Scroll += this.lnaTrackBar_Scroll;
			this.label2.Anchor = AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(0, 147);
			this.label2.Margin = new Padding(0);
			this.label2.Name = "label2";
			this.label2.Size = new Size(53, 13);
			this.label2.TabIndex = 22;
			this.label2.Text = "LNA Gain";
			this.lnaAgcCheckBox.Anchor = AnchorStyles.Left;
			this.lnaAgcCheckBox.AutoSize = true;
			this.lnaAgcCheckBox.Location = new Point(60, 145);
			this.lnaAgcCheckBox.Name = "lnaAgcCheckBox";
			this.lnaAgcCheckBox.Size = new Size(48, 17);
			this.lnaAgcCheckBox.TabIndex = 3;
			this.lnaAgcCheckBox.Text = "Auto";
			this.lnaAgcCheckBox.UseVisualStyleBackColor = true;
			this.lnaAgcCheckBox.CheckedChanged += this.lnaAgcCheckBox_CheckedChanged;
			this.lnaGainLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.lnaGainLabel.Location = new Point(111, 147);
			this.lnaGainLabel.Margin = new Padding(0);
			this.lnaGainLabel.Name = "lnaGainLabel";
			this.lnaGainLabel.Size = new Size(91, 13);
			this.lnaGainLabel.TabIndex = 26;
			this.lnaGainLabel.Text = "0";
			this.lnaGainLabel.TextAlign = ContentAlignment.MiddleRight;
			this.vgaGainLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.vgaGainLabel.Location = new Point(111, 0);
			this.vgaGainLabel.Margin = new Padding(0);
			this.vgaGainLabel.Name = "vgaGainLabel";
			this.vgaGainLabel.Size = new Size(91, 13);
			this.vgaGainLabel.TabIndex = 32;
			this.vgaGainLabel.Text = "0";
			this.vgaGainLabel.TextAlign = ContentAlignment.MiddleRight;
			this.label3.Anchor = AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new Point(0, 0);
			this.label3.Margin = new Padding(0);
			this.label3.Name = "label3";
			this.label3.Size = new Size(41, 13);
			this.label3.TabIndex = 31;
			this.label3.Text = "IF Gain";
			this.vgaTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.advancedTableLayoutPanel.SetColumnSpan(this.vgaTrackBar, 3);
			this.vgaTrackBar.Location = new Point(0, 17);
			this.vgaTrackBar.Margin = new Padding(0);
			this.vgaTrackBar.Maximum = 15;
			this.vgaTrackBar.Name = "vgaTrackBar";
			this.vgaTrackBar.Size = new Size(202, 45);
			this.vgaTrackBar.TabIndex = 0;
			this.vgaTrackBar.Scroll += this.vgaTrackBar_Scroll;
			this.mixerGainLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.mixerGainLabel.Location = new Point(111, 71);
			this.mixerGainLabel.Margin = new Padding(0);
			this.mixerGainLabel.Name = "mixerGainLabel";
			this.mixerGainLabel.Size = new Size(91, 13);
			this.mixerGainLabel.TabIndex = 36;
			this.mixerGainLabel.Text = "0";
			this.mixerGainLabel.TextAlign = ContentAlignment.MiddleRight;
			this.mixerAgcCheckBox.Anchor = AnchorStyles.Left;
			this.mixerAgcCheckBox.AutoSize = true;
			this.mixerAgcCheckBox.Location = new Point(60, 69);
			this.mixerAgcCheckBox.Name = "mixerAgcCheckBox";
			this.mixerAgcCheckBox.Size = new Size(48, 17);
			this.mixerAgcCheckBox.TabIndex = 1;
			this.mixerAgcCheckBox.Text = "Auto";
			this.mixerAgcCheckBox.UseVisualStyleBackColor = true;
			this.mixerAgcCheckBox.CheckedChanged += this.mixerAgcCheckBox_CheckedChanged;
			this.label6.Anchor = AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new Point(0, 71);
			this.label6.Margin = new Padding(0);
			this.label6.Name = "label6";
			this.label6.Size = new Size(57, 13);
			this.label6.TabIndex = 35;
			this.label6.Text = "Mixer Gain";
			this.mixerTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.advancedTableLayoutPanel.SetColumnSpan(this.mixerTrackBar, 3);
			this.mixerTrackBar.Location = new Point(0, 93);
			this.mixerTrackBar.Margin = new Padding(0);
			this.mixerTrackBar.Maximum = 15;
			this.mixerTrackBar.Name = "mixerTrackBar";
			this.mixerTrackBar.Size = new Size(202, 45);
			this.mixerTrackBar.TabIndex = 2;
			this.mixerTrackBar.Scroll += this.mixerTrackBar_Scroll;
			this.label8.Anchor = AnchorStyles.Left;
			this.label8.AutoSize = true;
			this.label8.Location = new Point(0, 352);
			this.label8.Margin = new Padding(0);
			this.label8.Name = "label8";
			this.label8.Size = new Size(60, 13);
			this.label8.TabIndex = 46;
			this.label8.Text = "Decimation";
			this.decimationComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.mainTableLayoutPanel.SetColumnSpan(this.decimationComboBox, 3);
			this.decimationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.decimationComboBox.FormattingEnabled = true;
			this.decimationComboBox.Items.AddRange(new object[7]
			{
				"None",
				"2",
				"4",
				"8",
				"16",
				"32",
				"64"
			});
			this.decimationComboBox.Location = new Point(63, 348);
			this.decimationComboBox.Margin = new Padding(0, 3, 0, 3);
			this.decimationComboBox.Name = "decimationComboBox";
			this.decimationComboBox.Size = new Size(139, 21);
			this.decimationComboBox.TabIndex = 6;
			this.decimationComboBox.SelectedIndexChanged += this.SampleRate_Changed;
			this.sampleRateComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.mainTableLayoutPanel.SetColumnSpan(this.sampleRateComboBox, 3);
			this.sampleRateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.sampleRateComboBox.FormattingEnabled = true;
			this.sampleRateComboBox.Location = new Point(63, 321);
			this.sampleRateComboBox.Margin = new Padding(0, 3, 0, 3);
			this.sampleRateComboBox.Name = "sampleRateComboBox";
			this.sampleRateComboBox.Size = new Size(139, 21);
			this.sampleRateComboBox.TabIndex = 5;
			this.label9.Anchor = AnchorStyles.Left;
			this.label9.AutoSize = true;
			this.label9.Location = new Point(0, 325);
			this.label9.Margin = new Padding(0);
			this.label9.Name = "label9";
			this.label9.Size = new Size(63, 13);
			this.label9.TabIndex = 48;
			this.label9.Text = "Sample rate";
			this.label10.Anchor = AnchorStyles.Left;
			this.label10.AutoSize = true;
			this.label10.Location = new Point(0, 377);
			this.label10.Margin = new Padding(0);
			this.label10.Name = "label10";
			this.label10.Size = new Size(41, 13);
			this.label10.TabIndex = 49;
			this.label10.Text = "Display";
			this.displayBandwidthRateLabel.Anchor = AnchorStyles.Left;
			this.displayBandwidthRateLabel.AutoSize = true;
			this.mainTableLayoutPanel.SetColumnSpan(this.displayBandwidthRateLabel, 3);
			this.displayBandwidthRateLabel.Font = new Font("Microsoft Sans Serif", 14f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.displayBandwidthRateLabel.Location = new Point(66, 372);
			this.displayBandwidthRateLabel.Name = "displayBandwidthRateLabel";
			this.displayBandwidthRateLabel.Size = new Size(80, 24);
			this.displayBandwidthRateLabel.TabIndex = 50;
			this.displayBandwidthRateLabel.Text = "10 MHz";
			this.mainTableLayoutPanel.ColumnCount = 4;
			this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.mainTableLayoutPanel.Controls.Add(this.label10, 0, 5);
			this.mainTableLayoutPanel.Controls.Add(this.displayBandwidthRateLabel, 1, 5);
			this.mainTableLayoutPanel.Controls.Add(this.label8, 0, 4);
			this.mainTableLayoutPanel.Controls.Add(this.label9, 0, 3);
			this.mainTableLayoutPanel.Controls.Add(this.sampleRateComboBox, 1, 3);
			this.mainTableLayoutPanel.Controls.Add(this.decimationComboBox, 1, 4);
			this.mainTableLayoutPanel.Controls.Add(this.debugPanel, 0, 10);
			this.mainTableLayoutPanel.Controls.Add(this.biasTeeCheckBox, 1, 6);
			this.mainTableLayoutPanel.Controls.Add(this.label14, 0, 6);
			this.mainTableLayoutPanel.Controls.Add(this.advancedTableLayoutPanel, 0, 2);
			this.mainTableLayoutPanel.Controls.Add(this.simplifiedTableLayoutPanel, 0, 1);
			this.mainTableLayoutPanel.Controls.Add(this.spyverterPPMnumericUpDown, 3, 9);
			this.mainTableLayoutPanel.Controls.Add(this.label16, 2, 9);
			this.mainTableLayoutPanel.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.trackingFilterCheckBox, 2, 6);
			this.mainTableLayoutPanel.Controls.Add(this.dynamicRangeCheckBox, 2, 7);
			this.mainTableLayoutPanel.Controls.Add(this.label15, 0, 7);
			this.mainTableLayoutPanel.Controls.Add(this.spyVerterCheckBox, 1, 7);
			this.mainTableLayoutPanel.Dock = DockStyle.Fill;
			this.mainTableLayoutPanel.Location = new Point(0, 0);
			this.mainTableLayoutPanel.Margin = new Padding(0);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			this.mainTableLayoutPanel.RowCount = 11;
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.mainTableLayoutPanel.Size = new Size(202, 648);
			this.mainTableLayoutPanel.TabIndex = 51;
			this.debugPanel.Anchor = AnchorStyles.None;
			this.mainTableLayoutPanel.SetColumnSpan(this.debugPanel, 4);
			this.debugPanel.Controls.Add(this.hpfNumericUpDown);
			this.debugPanel.Controls.Add(this.calibrateButton);
			this.debugPanel.Controls.Add(this.usePackingCheckBox);
			this.debugPanel.Controls.Add(this.readMemoryButton);
			this.debugPanel.Controls.Add(this.memoryValueTextBox);
			this.debugPanel.Controls.Add(this.memoryAddressTextBox);
			this.debugPanel.Controls.Add(this.label13);
			this.debugPanel.Controls.Add(this.dumpButton);
			this.debugPanel.Controls.Add(this.writeGPIOButton);
			this.debugPanel.Controls.Add(this.readGPIOButton);
			this.debugPanel.Controls.Add(this.gpioValueTextBox);
			this.debugPanel.Controls.Add(this.gpioAddressTextBox);
			this.debugPanel.Controls.Add(this.label12);
			this.debugPanel.Controls.Add(this.label11);
			this.debugPanel.Controls.Add(this.lpfNumericUpDown);
			this.debugPanel.Controls.Add(this.label4);
			this.debugPanel.Controls.Add(this.writeClockButton);
			this.debugPanel.Controls.Add(this.label1);
			this.debugPanel.Controls.Add(this.readClockButton);
			this.debugPanel.Controls.Add(this.tunerRegTextBox);
			this.debugPanel.Controls.Add(this.clockValTextBox);
			this.debugPanel.Controls.Add(this.label5);
			this.debugPanel.Controls.Add(this.clockRegTextBox);
			this.debugPanel.Controls.Add(this.tunerValTextBox);
			this.debugPanel.Controls.Add(this.label7);
			this.debugPanel.Controls.Add(this.readTunerButton);
			this.debugPanel.Controls.Add(this.writeTunerButton);
			this.debugPanel.Location = new Point(0, 474);
			this.debugPanel.Margin = new Padding(0);
			this.debugPanel.Name = "debugPanel";
			this.debugPanel.Size = new Size(202, 167);
			this.debugPanel.TabIndex = 7;
			this.hpfNumericUpDown.Location = new Point(105, 119);
			this.hpfNumericUpDown.Maximum = new decimal(new int[4]
			{
				15,
				0,
				0,
				0
			});
			this.hpfNumericUpDown.Name = "hpfNumericUpDown";
			this.hpfNumericUpDown.Size = new Size(38, 20);
			this.hpfNumericUpDown.TabIndex = 60;
			this.hpfNumericUpDown.ValueChanged += this.hpfNumericUpDown_ValueChanged;
			this.calibrateButton.Location = new Point(147, 142);
			this.calibrateButton.Name = "calibrateButton";
			this.calibrateButton.Size = new Size(47, 22);
			this.calibrateButton.TabIndex = 59;
			this.calibrateButton.Text = "Cal";
			this.calibrateButton.UseVisualStyleBackColor = true;
			this.calibrateButton.Click += this.calibrateButton_Click;
			this.usePackingCheckBox.AutoSize = true;
			this.usePackingCheckBox.Location = new Point(58, 146);
			this.usePackingCheckBox.Name = "usePackingCheckBox";
			this.usePackingCheckBox.Size = new Size(87, 17);
			this.usePackingCheckBox.TabIndex = 58;
			this.usePackingCheckBox.Text = "Use Packing";
			this.usePackingCheckBox.TextAlign = ContentAlignment.MiddleRight;
			this.usePackingCheckBox.UseVisualStyleBackColor = true;
			this.usePackingCheckBox.CheckedChanged += this.usePackingCheckBox_CheckedChanged;
			this.readMemoryButton.Location = new Point(147, 94);
			this.readMemoryButton.Margin = new Padding(2);
			this.readMemoryButton.Name = "readMemoryButton";
			this.readMemoryButton.Size = new Size(21, 21);
			this.readMemoryButton.TabIndex = 56;
			this.readMemoryButton.Text = "R";
			this.readMemoryButton.UseVisualStyleBackColor = true;
			this.readMemoryButton.Click += this.readMemoryButton_Click;
			this.memoryValueTextBox.Location = new Point(105, 95);
			this.memoryValueTextBox.Margin = new Padding(2);
			this.memoryValueTextBox.Name = "memoryValueTextBox";
			this.memoryValueTextBox.Size = new Size(38, 20);
			this.memoryValueTextBox.TabIndex = 55;
			this.memoryAddressTextBox.Location = new Point(58, 95);
			this.memoryAddressTextBox.Margin = new Padding(2);
			this.memoryAddressTextBox.Name = "memoryAddressTextBox";
			this.memoryAddressTextBox.Size = new Size(43, 20);
			this.memoryAddressTextBox.TabIndex = 54;
			this.memoryAddressTextBox.Text = "0x100800c0";
			this.memoryAddressTextBox.KeyDown += this.memoryAddressTextBox_KeyDown;
			this.label13.AutoSize = true;
			this.label13.Location = new Point(9, 98);
			this.label13.Name = "label13";
			this.label13.Size = new Size(44, 13);
			this.label13.TabIndex = 57;
			this.label13.Text = "Memory";
			this.dumpButton.Location = new Point(147, 119);
			this.dumpButton.Name = "dumpButton";
			this.dumpButton.Size = new Size(47, 22);
			this.dumpButton.TabIndex = 53;
			this.dumpButton.Text = "Dump";
			this.dumpButton.UseVisualStyleBackColor = true;
			this.dumpButton.Click += this.dumpButton_Click;
			this.writeGPIOButton.Location = new Point(173, 70);
			this.writeGPIOButton.Margin = new Padding(2);
			this.writeGPIOButton.Name = "writeGPIOButton";
			this.writeGPIOButton.Size = new Size(21, 21);
			this.writeGPIOButton.TabIndex = 51;
			this.writeGPIOButton.Text = "W";
			this.writeGPIOButton.UseVisualStyleBackColor = true;
			this.writeGPIOButton.Click += this.writeGPIOButton_Click;
			this.readGPIOButton.Location = new Point(147, 70);
			this.readGPIOButton.Margin = new Padding(2);
			this.readGPIOButton.Name = "readGPIOButton";
			this.readGPIOButton.Size = new Size(21, 21);
			this.readGPIOButton.TabIndex = 50;
			this.readGPIOButton.Text = "R";
			this.readGPIOButton.UseVisualStyleBackColor = true;
			this.readGPIOButton.Click += this.readGPIOButton_Click;
			this.gpioValueTextBox.Location = new Point(105, 71);
			this.gpioValueTextBox.Margin = new Padding(2);
			this.gpioValueTextBox.Name = "gpioValueTextBox";
			this.gpioValueTextBox.Size = new Size(38, 20);
			this.gpioValueTextBox.TabIndex = 49;
			this.gpioValueTextBox.KeyDown += this.gpioValueTextBox_KeyDown;
			this.gpioAddressTextBox.Location = new Point(58, 71);
			this.gpioAddressTextBox.Margin = new Padding(2);
			this.gpioAddressTextBox.Name = "gpioAddressTextBox";
			this.gpioAddressTextBox.Size = new Size(43, 20);
			this.gpioAddressTextBox.TabIndex = 48;
			this.gpioAddressTextBox.Text = "0:12";
			this.gpioAddressTextBox.KeyDown += this.gpioAddressTextBox_KeyDown;
			this.label12.AutoSize = true;
			this.label12.Location = new Point(20, 74);
			this.label12.Name = "label12";
			this.label12.Size = new Size(33, 13);
			this.label12.TabIndex = 52;
			this.label12.Text = "GPIO";
			this.label11.AutoSize = true;
			this.label11.Location = new Point(11, 121);
			this.label11.Name = "label11";
			this.label11.Size = new Size(41, 13);
			this.label11.TabIndex = 47;
			this.label11.Text = "IF Filter";
			this.lpfNumericUpDown.Location = new Point(58, 119);
			this.lpfNumericUpDown.Maximum = new decimal(new int[4]
			{
				63,
				0,
				0,
				0
			});
			this.lpfNumericUpDown.Name = "lpfNumericUpDown";
			this.lpfNumericUpDown.Size = new Size(43, 20);
			this.lpfNumericUpDown.TabIndex = 46;
			this.lpfNumericUpDown.Value = new decimal(new int[4]
			{
				55,
				0,
				0,
				0
			});
			this.lpfNumericUpDown.ValueChanged += this.lpfNumericUpDown_ValueChanged;
			this.label4.AutoSize = true;
			this.label4.Location = new Point(56, 6);
			this.label4.Name = "label4";
			this.label4.Size = new Size(45, 13);
			this.label4.TabIndex = 40;
			this.label4.Text = "Address";
			this.writeClockButton.Location = new Point(173, 46);
			this.writeClockButton.Margin = new Padding(2);
			this.writeClockButton.Name = "writeClockButton";
			this.writeClockButton.Size = new Size(21, 21);
			this.writeClockButton.TabIndex = 14;
			this.writeClockButton.Text = "W";
			this.writeClockButton.UseVisualStyleBackColor = true;
			this.writeClockButton.Click += this.writeClockButton_Click;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(7, 26);
			this.label1.Name = "label1";
			this.label1.Size = new Size(46, 13);
			this.label1.TabIndex = 38;
			this.label1.Text = "R820T2";
			this.readClockButton.Location = new Point(147, 46);
			this.readClockButton.Margin = new Padding(2);
			this.readClockButton.Name = "readClockButton";
			this.readClockButton.Size = new Size(21, 21);
			this.readClockButton.TabIndex = 13;
			this.readClockButton.Text = "R";
			this.readClockButton.UseVisualStyleBackColor = true;
			this.readClockButton.Click += this.readClockButton_Click;
			this.tunerRegTextBox.Location = new Point(58, 23);
			this.tunerRegTextBox.Margin = new Padding(2);
			this.tunerRegTextBox.Name = "tunerRegTextBox";
			this.tunerRegTextBox.Size = new Size(43, 20);
			this.tunerRegTextBox.TabIndex = 7;
			this.tunerRegTextBox.Text = "0x0A";
			this.tunerRegTextBox.KeyDown += this.tunerRegTextBox_KeyDown;
			this.clockValTextBox.Location = new Point(105, 47);
			this.clockValTextBox.Margin = new Padding(2);
			this.clockValTextBox.Name = "clockValTextBox";
			this.clockValTextBox.Size = new Size(38, 20);
			this.clockValTextBox.TabIndex = 12;
			this.clockValTextBox.KeyDown += this.clockValTextBox_KeyDown;
			this.label5.AutoSize = true;
			this.label5.Location = new Point(102, 6);
			this.label5.Name = "label5";
			this.label5.Size = new Size(34, 13);
			this.label5.TabIndex = 41;
			this.label5.Text = "Value";
			this.clockRegTextBox.Location = new Point(58, 47);
			this.clockRegTextBox.Margin = new Padding(2);
			this.clockRegTextBox.Name = "clockRegTextBox";
			this.clockRegTextBox.Size = new Size(43, 20);
			this.clockRegTextBox.TabIndex = 11;
			this.clockRegTextBox.Text = "0x00";
			this.clockRegTextBox.KeyDown += this.clockRegTextBox_KeyDown;
			this.tunerValTextBox.Location = new Point(105, 23);
			this.tunerValTextBox.Margin = new Padding(2);
			this.tunerValTextBox.Name = "tunerValTextBox";
			this.tunerValTextBox.Size = new Size(38, 20);
			this.tunerValTextBox.TabIndex = 8;
			this.tunerValTextBox.KeyDown += this.tunerValTextBox_KeyDown;
			this.label7.AutoSize = true;
			this.label7.Location = new Point(6, 49);
			this.label7.Name = "label7";
			this.label7.Size = new Size(47, 13);
			this.label7.TabIndex = 45;
			this.label7.Text = "Si5351C";
			this.readTunerButton.Location = new Point(147, 22);
			this.readTunerButton.Margin = new Padding(2);
			this.readTunerButton.Name = "readTunerButton";
			this.readTunerButton.Size = new Size(21, 21);
			this.readTunerButton.TabIndex = 9;
			this.readTunerButton.Text = "R";
			this.readTunerButton.UseVisualStyleBackColor = true;
			this.readTunerButton.Click += this.readTunerButton_Click;
			this.writeTunerButton.Location = new Point(173, 22);
			this.writeTunerButton.Margin = new Padding(2);
			this.writeTunerButton.Name = "writeTunerButton";
			this.writeTunerButton.Size = new Size(21, 21);
			this.writeTunerButton.TabIndex = 10;
			this.writeTunerButton.Text = "W";
			this.writeTunerButton.UseVisualStyleBackColor = true;
			this.writeTunerButton.Click += this.writeTunerButton_Click;
			this.biasTeeCheckBox.Anchor = AnchorStyles.Left;
			this.biasTeeCheckBox.AutoSize = true;
			this.biasTeeCheckBox.Location = new Point(66, 400);
			this.biasTeeCheckBox.Name = "biasTeeCheckBox";
			this.biasTeeCheckBox.Size = new Size(15, 14);
			this.biasTeeCheckBox.TabIndex = 52;
			this.biasTeeCheckBox.UseVisualStyleBackColor = true;
			this.biasTeeCheckBox.CheckedChanged += this.biasTeeCheckBox_CheckedChanged;
			this.label14.Anchor = AnchorStyles.Left;
			this.label14.AutoSize = true;
			this.label14.Location = new Point(0, 401);
			this.label14.Margin = new Padding(0);
			this.label14.Name = "label14";
			this.label14.Size = new Size(49, 13);
			this.label14.TabIndex = 51;
			this.label14.Text = "Bias-Tee";
			this.advancedTableLayoutPanel.ColumnCount = 3;
			this.mainTableLayoutPanel.SetColumnSpan(this.advancedTableLayoutPanel, 4);
			this.advancedTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.advancedTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.advancedTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.advancedTableLayoutPanel.Controls.Add(this.vgaTrackBar, 0, 1);
			this.advancedTableLayoutPanel.Controls.Add(this.label3, 0, 0);
			this.advancedTableLayoutPanel.Controls.Add(this.vgaGainLabel, 2, 0);
			this.advancedTableLayoutPanel.Controls.Add(this.label6, 0, 2);
			this.advancedTableLayoutPanel.Controls.Add(this.mixerAgcCheckBox, 1, 2);
			this.advancedTableLayoutPanel.Controls.Add(this.mixerGainLabel, 2, 2);
			this.advancedTableLayoutPanel.Controls.Add(this.lnaTrackBar, 0, 5);
			this.advancedTableLayoutPanel.Controls.Add(this.lnaAgcCheckBox, 1, 4);
			this.advancedTableLayoutPanel.Controls.Add(this.lnaGainLabel, 2, 4);
			this.advancedTableLayoutPanel.Controls.Add(this.label2, 0, 4);
			this.advancedTableLayoutPanel.Controls.Add(this.mixerTrackBar, 0, 3);
			this.advancedTableLayoutPanel.Dock = DockStyle.Fill;
			this.advancedTableLayoutPanel.Location = new Point(0, 98);
			this.advancedTableLayoutPanel.Margin = new Padding(0);
			this.advancedTableLayoutPanel.Name = "advancedTableLayoutPanel";
			this.advancedTableLayoutPanel.RowCount = 6;
			this.advancedTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.advancedTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
			this.advancedTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.advancedTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
			this.advancedTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.advancedTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
			this.advancedTableLayoutPanel.Size = new Size(202, 220);
			this.advancedTableLayoutPanel.TabIndex = 58;
			this.advancedTableLayoutPanel.Visible = false;
			this.simplifiedTableLayoutPanel.ColumnCount = 2;
			this.mainTableLayoutPanel.SetColumnSpan(this.simplifiedTableLayoutPanel, 4);
			this.simplifiedTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			this.simplifiedTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.simplifiedTableLayoutPanel.Controls.Add(this.simplifiedTrackBar, 0, 1);
			this.simplifiedTableLayoutPanel.Controls.Add(this.simplifiedGainLabel, 1, 0);
			this.simplifiedTableLayoutPanel.Controls.Add(this.label18, 0, 0);
			this.simplifiedTableLayoutPanel.Dock = DockStyle.Fill;
			this.simplifiedTableLayoutPanel.Location = new Point(0, 30);
			this.simplifiedTableLayoutPanel.Margin = new Padding(0);
			this.simplifiedTableLayoutPanel.Name = "simplifiedTableLayoutPanel";
			this.simplifiedTableLayoutPanel.RowCount = 2;
			this.simplifiedTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.simplifiedTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.simplifiedTableLayoutPanel.Size = new Size(202, 68);
			this.simplifiedTableLayoutPanel.TabIndex = 62;
			this.simplifiedTableLayoutPanel.Visible = false;
			this.simplifiedTrackBar.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.simplifiedTableLayoutPanel.SetColumnSpan(this.simplifiedTrackBar, 3);
			this.simplifiedTrackBar.Location = new Point(0, 18);
			this.simplifiedTrackBar.Margin = new Padding(0);
			this.simplifiedTrackBar.Maximum = 21;
			this.simplifiedTrackBar.Name = "simplifiedTrackBar";
			this.simplifiedTrackBar.Size = new Size(202, 45);
			this.simplifiedTrackBar.TabIndex = 35;
			this.simplifiedTrackBar.Scroll += this.simplifiedTrackBar_Scroll;
			this.simplifiedGainLabel.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.simplifiedGainLabel.Location = new Point(29, 1);
			this.simplifiedGainLabel.Margin = new Padding(0);
			this.simplifiedGainLabel.Name = "simplifiedGainLabel";
			this.simplifiedGainLabel.Size = new Size(173, 10);
			this.simplifiedGainLabel.TabIndex = 33;
			this.simplifiedGainLabel.Text = "0";
			this.simplifiedGainLabel.TextAlign = ContentAlignment.MiddleRight;
			this.label18.Anchor = AnchorStyles.Left;
			this.label18.AutoSize = true;
			this.label18.Location = new Point(0, 0);
			this.label18.Margin = new Padding(0);
			this.label18.Name = "label18";
			this.label18.Size = new Size(29, 13);
			this.label18.TabIndex = 34;
			this.label18.Text = "Gain";
			this.spyverterPPMnumericUpDown.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.spyverterPPMnumericUpDown.DecimalPlaces = 2;
			this.spyverterPPMnumericUpDown.Increment = new decimal(new int[4]
			{
				5,
				0,
				0,
				131072
			});
			this.spyverterPPMnumericUpDown.Location = new Point(117, 445);
			this.spyverterPPMnumericUpDown.Minimum = new decimal(new int[4]
			{
				100,
				0,
				0,
				-2147483648
			});
			this.spyverterPPMnumericUpDown.Name = "spyverterPPMnumericUpDown";
			this.spyverterPPMnumericUpDown.Size = new Size(82, 20);
			this.spyverterPPMnumericUpDown.TabIndex = 57;
			this.spyverterPPMnumericUpDown.TextAlign = HorizontalAlignment.Right;
			this.spyverterPPMnumericUpDown.ValueChanged += this.spyverterPPMnumericUpDown_ValueChanged;
			this.label16.Anchor = AnchorStyles.Right;
			this.label16.AutoSize = true;
			this.label16.Location = new Point(84, 448);
			this.label16.Margin = new Padding(0);
			this.label16.Name = "label16";
			this.label16.Size = new Size(30, 13);
			this.label16.TabIndex = 56;
			this.label16.Text = "PPM";
			this.tableLayoutPanel2.ColumnCount = 3;
			this.mainTableLayoutPanel.SetColumnSpan(this.tableLayoutPanel2, 4);
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.freeRadioButton, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.sensitivityRadioButton, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.linearityRadioButton, 1, 0);
			this.tableLayoutPanel2.Dock = DockStyle.Fill;
			this.tableLayoutPanel2.Location = new Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel2.Size = new Size(196, 24);
			this.tableLayoutPanel2.TabIndex = 63;
			this.freeRadioButton.AutoSize = true;
			this.freeRadioButton.Location = new Point(151, 3);
			this.freeRadioButton.Name = "freeRadioButton";
			this.freeRadioButton.Size = new Size(46, 17);
			this.freeRadioButton.TabIndex = 61;
			this.freeRadioButton.TabStop = true;
			this.freeRadioButton.Text = "Free";
			this.freeRadioButton.UseVisualStyleBackColor = true;
			this.freeRadioButton.CheckedChanged += this.gainModeRadioButton_CheckedChanged;
			this.sensitivityRadioButton.AutoSize = true;
			this.sensitivityRadioButton.Location = new Point(3, 3);
			this.sensitivityRadioButton.Name = "sensitivityRadioButton";
			this.sensitivityRadioButton.Size = new Size(72, 17);
			this.sensitivityRadioButton.TabIndex = 59;
			this.sensitivityRadioButton.TabStop = true;
			this.sensitivityRadioButton.Text = "Sensitivity";
			this.sensitivityRadioButton.UseVisualStyleBackColor = true;
			this.sensitivityRadioButton.CheckedChanged += this.gainModeRadioButton_CheckedChanged;
			this.linearityRadioButton.AutoSize = true;
			this.linearityRadioButton.Location = new Point(81, 3);
			this.linearityRadioButton.Name = "linearityRadioButton";
			this.linearityRadioButton.Size = new Size(64, 17);
			this.linearityRadioButton.TabIndex = 60;
			this.linearityRadioButton.TabStop = true;
			this.linearityRadioButton.Text = "Linearity";
			this.linearityRadioButton.UseVisualStyleBackColor = true;
			this.linearityRadioButton.CheckedChanged += this.gainModeRadioButton_CheckedChanged;
			this.trackingFilterCheckBox.Anchor = AnchorStyles.Right;
			this.trackingFilterCheckBox.AutoSize = true;
			this.trackingFilterCheckBox.CheckAlign = ContentAlignment.MiddleRight;
			this.trackingFilterCheckBox.Checked = true;
			this.trackingFilterCheckBox.CheckState = CheckState.Checked;
			this.mainTableLayoutPanel.SetColumnSpan(this.trackingFilterCheckBox, 2);
			this.trackingFilterCheckBox.Location = new Point(106, 399);
			this.trackingFilterCheckBox.Name = "trackingFilterCheckBox";
			this.trackingFilterCheckBox.Size = new Size(93, 17);
			this.trackingFilterCheckBox.TabIndex = 65;
			this.trackingFilterCheckBox.Text = "Tracking Filter";
			this.trackingFilterCheckBox.UseVisualStyleBackColor = true;
			this.trackingFilterCheckBox.CheckedChanged += this.trackingFilterCheckBox_CheckedChanged;
			this.dynamicRangeCheckBox.Anchor = AnchorStyles.Right;
			this.dynamicRangeCheckBox.AutoSize = true;
			this.dynamicRangeCheckBox.CheckAlign = ContentAlignment.MiddleRight;
			this.mainTableLayoutPanel.SetColumnSpan(this.dynamicRangeCheckBox, 2);
			this.dynamicRangeCheckBox.Location = new Point(113, 422);
			this.dynamicRangeCheckBox.Name = "dynamicRangeCheckBox";
			this.dynamicRangeCheckBox.Size = new Size(86, 17);
			this.dynamicRangeCheckBox.TabIndex = 66;
			this.dynamicRangeCheckBox.Text = "Enable HDR";
			this.dynamicRangeCheckBox.UseVisualStyleBackColor = true;
			this.dynamicRangeCheckBox.CheckedChanged += this.dynamicRangeCheckBox_CheckedChanged;
			this.label15.Anchor = AnchorStyles.Left;
			this.label15.AutoSize = true;
			this.label15.Location = new Point(0, 424);
			this.label15.Margin = new Padding(0);
			this.label15.Name = "label15";
			this.label15.Size = new Size(53, 13);
			this.label15.TabIndex = 54;
			this.label15.Text = "SpyVerter";
			this.spyVerterCheckBox.Anchor = AnchorStyles.Left;
			this.spyVerterCheckBox.AutoSize = true;
			this.spyVerterCheckBox.Location = new Point(66, 423);
			this.spyVerterCheckBox.Name = "spyVerterCheckBox";
			this.spyVerterCheckBox.Size = new Size(15, 14);
			this.spyVerterCheckBox.TabIndex = 53;
			this.spyVerterCheckBox.UseVisualStyleBackColor = true;
			this.spyVerterCheckBox.CheckedChanged += this.spyVerterCheckBox_CheckedChanged;
			this.refreshTimer.Tick += this.refreshTimer_Tick;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.mainTableLayoutPanel);
			base.Name = "ControllerPanel";
			base.Size = new Size(202, 648);
			((ISupportInitialize)this.lnaTrackBar).EndInit();
			((ISupportInitialize)this.vgaTrackBar).EndInit();
			((ISupportInitialize)this.mixerTrackBar).EndInit();
			this.mainTableLayoutPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.PerformLayout();
			this.debugPanel.ResumeLayout(false);
			this.debugPanel.PerformLayout();
			((ISupportInitialize)this.hpfNumericUpDown).EndInit();
			((ISupportInitialize)this.lpfNumericUpDown).EndInit();
			this.advancedTableLayoutPanel.ResumeLayout(false);
			this.advancedTableLayoutPanel.PerformLayout();
			this.simplifiedTableLayoutPanel.ResumeLayout(false);
			this.simplifiedTableLayoutPanel.PerformLayout();
			((ISupportInitialize)this.simplifiedTrackBar).EndInit();
			((ISupportInitialize)this.spyverterPPMnumericUpDown).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
