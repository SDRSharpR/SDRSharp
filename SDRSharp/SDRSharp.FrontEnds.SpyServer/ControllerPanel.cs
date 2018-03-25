using SDRSharp.Radio;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SDRSharp.FrontEnds.SpyServer
{
	public class ControllerPanel : UserControl
	{
		private const int DefaultPort = 5555;

		private readonly string _historyFileName = ".\\spybrowser.history";

		private SpyServerIO _owner;

		private float _rate;

		private int _minDecimation;

		private IContainer components;

		private TableLayoutPanel hostTableLayoutPanel;

		private Button connectButton;

		private TableLayoutPanel deviceInfoTableLayoutPanel;

		private TableLayoutPanel bandwidthTableLayoutPanel;

		private TableLayoutPanel iqFormatTableLayoutPanel;

		private TableLayoutPanel gainTableLayoutPanel;

		private Label label1;

		private ComboBox bandwidthComboBox;

		private Label gainLabel;

		private Label label2;

		private TrackBar gainTrackBar;

		private Label deviceSerialLabel;

		private Label deviceNameLabel;

		private CheckBox useFullIQCheckBox;

		private Label bitrateLabel;

		private Timer downStreamTimer;

		private Label label3;

		private ComboBox streamFormatComboBox;

		private ComboBox uriComboBox;

		private Label serverVersionLabel;

		public string Host
		{
			get
			{
				Uri uri = new Uri(this.uriComboBox.Text);
				return uri.Host;
			}
		}

		public int Port
		{
			get
			{
				Uri uri = new Uri(this.uriComboBox.Text);
				if (uri.Port > 0)
				{
					return uri.Port;
				}
				return 5555;
			}
		}

		public bool UseFullIQ
		{
			get
			{
				return this.useFullIQCheckBox.Checked;
			}
			set
			{
				this.useFullIQCheckBox.Checked = value;
			}
		}

		public int Decimation
		{
			get
			{
				return Math.Max(0, this.bandwidthComboBox.SelectedIndex);
			}
		}

		public ControllerPanel(SpyServerIO owner)
		{
			this._owner = owner;
			this.InitializeComponent();
			try
			{
				this.uriComboBox.Items.AddRange(File.ReadAllLines(this._historyFileName));
			}
			catch
			{
			}
			this.uriComboBox.Text = Utils.GetStringSetting("spyserver.uri", "sdr://127.0.0.1:5555");
			this.streamFormatComboBox.SelectedIndex = Utils.GetIntSetting("spyserver.streamFormat", 3);
			this.useFullIQCheckBox.Checked = Utils.GetBooleanSetting("spyserver.fullIQ", false);
			this.useFullIQCheckBox_CheckStateChanged(null, null);
			this.streamFormatComboBox_SelectedIndexChanged(null, null);
			this.useFullIQCheckBox_CheckStateChanged(null, null);
			this.uriComboBox.SelectedValueChanged += this.connectButton_Click;
		}

		public void SaveSettings()
		{
			Utils.SaveSetting("spyserver.uri", this.uriComboBox.Text);
			Utils.SaveSetting("spyserver.streamFormat", this.streamFormatComboBox.SelectedIndex);
			Utils.SaveSetting("spyserver.fullIQ", this.useFullIQCheckBox.Checked);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.uriComboBox.Items.Count; i++)
			{
				stringBuilder.AppendLine(this.uriComboBox.Items[i].ToString());
			}
			File.WriteAllText(this._historyFileName, stringBuilder.ToString());
		}

		public void Force8bit()
		{
			this.streamFormatComboBox.SelectedIndex = 3;
		}

		public void EnableURI(bool enable)
		{
			this.uriComboBox.Enabled = enable;
		}

		public void EnableFullIQ(bool enable)
		{
			if (base.InvokeRequired)
			{
				this.useFullIQCheckBox.Enabled = enable;
			}
			else if (base.IsHandleCreated)
			{
				base.BeginInvoke((Action)delegate
				{
					this.useFullIQCheckBox.Enabled = enable;
				});
			}
		}

		public void UpdateDisplaySections(bool connected, bool bandwidth, bool format, bool gain)
		{
			int num = this.hostTableLayoutPanel.Height;
			this.deviceInfoTableLayoutPanel.Visible = connected;
			this.bandwidthTableLayoutPanel.Visible = bandwidth;
			this.iqFormatTableLayoutPanel.Visible = format;
			this.gainTableLayoutPanel.Visible = gain;
			if (connected)
			{
				num += this.deviceInfoTableLayoutPanel.Height;
			}
			if (bandwidth)
			{
				num += this.bandwidthTableLayoutPanel.Height;
			}
			if (format)
			{
				num += this.iqFormatTableLayoutPanel.Height;
			}
			if (gain)
			{
				num += this.gainTableLayoutPanel.Height;
			}
			base.Height = num;
		}

		public void UpdateGain(int value, bool canControl)
		{
			if (value >= this.gainTrackBar.Minimum && value <= this.gainTrackBar.Maximum)
			{
				this.gainTrackBar.Value = value;
			}
			this.gainTrackBar.Enabled = canControl;
		}

		internal void UpdateGain(object gain, bool canControl)
		{
			throw new NotImplementedException();
		}

		private void connectButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (this._owner.Connected)
				{
					this._owner.Disconnect();
				}
				else
				{
					this._owner.Connect();
					this.UpdateHistory();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void UpdateBandwidthOptions(int[] bandwidthOptions)
		{
			this.bandwidthComboBox.Items.Clear();
			for (int i = 0; i < bandwidthOptions.Length; i++)
			{
				string frequencyDisplay = Utils.GetFrequencyDisplay(bandwidthOptions[i], true);
				this.bandwidthComboBox.Items.Add(frequencyDisplay);
			}
			if (bandwidthOptions.Length != 0)
			{
				this.bandwidthComboBox.SelectedIndex = 0;
			}
		}

		public void UpdateControlOptions(string deviceName, string deviceSerial, string serverVersion, int[] bandwidthOptions, int minDecimation, int maxGain)
		{
			this.deviceNameLabel.Text = "Device: " + deviceName;
			this.deviceSerialLabel.Text = "SN: " + deviceSerial;
			this.serverVersionLabel.Text = "Server: " + serverVersion;
			this._minDecimation = minDecimation;
			this.UpdateBandwidthOptions(bandwidthOptions);
			this._owner.SetDecimation(this.bandwidthComboBox.SelectedIndex);
			this._owner.SetFormat((StreamFormat)(4 - this.streamFormatComboBox.SelectedIndex));
			this.gainTrackBar.ValueChanged -= this.gainTrackBar_ValueChanged;
			this.gainTrackBar.Maximum = maxGain;
			this.gainTrackBar.Value = 0;
			this.gainTrackBar.ValueChanged += this.gainTrackBar_ValueChanged;
			this.gainLabel.Text = "0";
		}

		private void bandwidthComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.bandwidthComboBox.SelectedIndex < this._minDecimation)
			{
				this.UseFullIQ = false;
				this.EnableFullIQ(false);
			}
			else
			{
				this.EnableFullIQ(!this._owner.Control.IsPlaying);
			}
			this._owner.SetDecimation(this.bandwidthComboBox.SelectedIndex);
		}

		private void streamFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this._owner != null)
			{
				this._owner.SetFormat((StreamFormat)(4 - this.streamFormatComboBox.SelectedIndex));
			}
		}

		private void gainTrackBar_ValueChanged(object sender, EventArgs e)
		{
			this.gainLabel.Text = this.gainTrackBar.Value.ToString();
			this._owner.SetGain(this.gainTrackBar.Value);
		}

		private void useFullIQCheckBox_CheckStateChanged(object sender, EventArgs e)
		{
			this._owner.FFTEnabled = !this.useFullIQCheckBox.Checked;
		}

		private void downStreamTimer_Tick(object sender, EventArgs e)
		{
			long downstreamBytes = this._owner.GetDownstreamBytes();
			if (downstreamBytes == 0L)
			{
				this.bitrateLabel.Text = "0 kB/s";
				this._rate = 0f;
			}
			else
			{
				float num = (float)downstreamBytes / (0.001f * (float)this.downStreamTimer.Interval);
				float num2 = num - this._rate;
				float num3 = (num2 > 0f) ? 0.8f : 0.2f;
				this._rate += num3 * num2;
				if (this._rate < 1000000f)
				{
					this.bitrateLabel.Text = Math.Round((double)(this._rate * 0.001f)) + " kB/s";
				}
				else
				{
					this.bitrateLabel.Text = Math.Round((double)(this._rate * 1E-06f), 1) + " MB/s";
				}
			}
		}

		private void uriComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.connectButton_Click(sender, e);
			}
		}

		private void UpdateHistory()
		{
			string text = this.uriComboBox.Text;
			Uri uri = new Uri(text);
			if (uri.IsWellFormedOriginalString())
			{
				for (int i = 0; i < this.uriComboBox.Items.Count; i++)
				{
					Uri uri2 = new Uri(this.uriComboBox.Items[i].ToString());
					if (string.Compare(uri2.AbsoluteUri, uri.AbsoluteUri) == 0)
					{
						return;
					}
				}
				this.uriComboBox.Items.Add(uri.AbsoluteUri);
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
			this.hostTableLayoutPanel = new TableLayoutPanel();
			this.connectButton = new Button();
			this.uriComboBox = new ComboBox();
			this.bandwidthTableLayoutPanel = new TableLayoutPanel();
			this.label1 = new Label();
			this.bandwidthComboBox = new ComboBox();
			this.label3 = new Label();
			this.streamFormatComboBox = new ComboBox();
			this.gainTableLayoutPanel = new TableLayoutPanel();
			this.gainLabel = new Label();
			this.label2 = new Label();
			this.gainTrackBar = new TrackBar();
			this.deviceInfoTableLayoutPanel = new TableLayoutPanel();
			this.bitrateLabel = new Label();
			this.deviceSerialLabel = new Label();
			this.deviceNameLabel = new Label();
			this.useFullIQCheckBox = new CheckBox();
			this.downStreamTimer = new Timer(this.components);
			this.iqFormatTableLayoutPanel = new TableLayoutPanel();
			this.serverVersionLabel = new Label();
			this.hostTableLayoutPanel.SuspendLayout();
			this.bandwidthTableLayoutPanel.SuspendLayout();
			this.gainTableLayoutPanel.SuspendLayout();
			((ISupportInitialize)this.gainTrackBar).BeginInit();
			this.deviceInfoTableLayoutPanel.SuspendLayout();
			this.iqFormatTableLayoutPanel.SuspendLayout();
			base.SuspendLayout();
			this.hostTableLayoutPanel.ColumnCount = 2;
			this.hostTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.hostTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32f));
			this.hostTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20f));
			this.hostTableLayoutPanel.Controls.Add(this.connectButton, 1, 0);
			this.hostTableLayoutPanel.Controls.Add(this.uriComboBox, 0, 0);
			this.hostTableLayoutPanel.Dock = DockStyle.Top;
			this.hostTableLayoutPanel.Location = new Point(0, 0);
			this.hostTableLayoutPanel.Name = "hostTableLayoutPanel";
			this.hostTableLayoutPanel.RowCount = 1;
			this.hostTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.hostTableLayoutPanel.Size = new Size(202, 30);
			this.hostTableLayoutPanel.TabIndex = 64;
			this.connectButton.Location = new Point(173, 3);
			this.connectButton.Name = "connectButton";
			this.connectButton.Size = new Size(25, 23);
			this.connectButton.TabIndex = 2;
			this.connectButton.Text = "C";
			this.connectButton.UseVisualStyleBackColor = true;
			this.connectButton.Click += this.connectButton_Click;
			this.uriComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.uriComboBox.FormattingEnabled = true;
			this.uriComboBox.Location = new Point(3, 4);
			this.uriComboBox.Name = "uriComboBox";
			this.uriComboBox.Size = new Size(164, 21);
			this.uriComboBox.TabIndex = 3;
			this.uriComboBox.KeyDown += this.uriComboBox_KeyDown;
			this.bandwidthTableLayoutPanel.ColumnCount = 2;
			this.bandwidthTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.bandwidthTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
			this.bandwidthTableLayoutPanel.Controls.Add(this.label1, 0, 0);
			this.bandwidthTableLayoutPanel.Controls.Add(this.bandwidthComboBox, 1, 0);
			this.bandwidthTableLayoutPanel.Dock = DockStyle.Top;
			this.bandwidthTableLayoutPanel.Location = new Point(0, 106);
			this.bandwidthTableLayoutPanel.Name = "bandwidthTableLayoutPanel";
			this.bandwidthTableLayoutPanel.RowCount = 1;
			this.bandwidthTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.bandwidthTableLayoutPanel.Size = new Size(202, 31);
			this.bandwidthTableLayoutPanel.TabIndex = 65;
			this.label1.Anchor = AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(3, 9);
			this.label1.Name = "label1";
			this.label1.Size = new Size(57, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Bandwidth";
			this.bandwidthComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.bandwidthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.bandwidthComboBox.FormattingEnabled = true;
			this.bandwidthComboBox.Location = new Point(83, 5);
			this.bandwidthComboBox.Name = "bandwidthComboBox";
			this.bandwidthComboBox.Size = new Size(116, 21);
			this.bandwidthComboBox.TabIndex = 1;
			this.bandwidthComboBox.SelectedIndexChanged += this.bandwidthComboBox_SelectedIndexChanged;
			this.label3.Anchor = AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new Point(3, 9);
			this.label3.Name = "label3";
			this.label3.Size = new Size(53, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "IQ Format";
			this.streamFormatComboBox.Anchor = (AnchorStyles.Left | AnchorStyles.Right);
			this.streamFormatComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			this.streamFormatComboBox.FormattingEnabled = true;
			this.streamFormatComboBox.Items.AddRange(new object[4]
			{
				"Float 32bit",
				"PCM 24bit",
				"PCM 16bit",
				"PCM 8bit"
			});
			this.streamFormatComboBox.Location = new Point(83, 5);
			this.streamFormatComboBox.Name = "streamFormatComboBox";
			this.streamFormatComboBox.Size = new Size(116, 21);
			this.streamFormatComboBox.TabIndex = 1;
			this.streamFormatComboBox.SelectedIndexChanged += this.streamFormatComboBox_SelectedIndexChanged;
			this.gainTableLayoutPanel.ColumnCount = 2;
			this.gainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.gainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.gainTableLayoutPanel.Controls.Add(this.gainLabel, 1, 0);
			this.gainTableLayoutPanel.Controls.Add(this.label2, 0, 0);
			this.gainTableLayoutPanel.Controls.Add(this.gainTrackBar, 0, 1);
			this.gainTableLayoutPanel.Dock = DockStyle.Top;
			this.gainTableLayoutPanel.Location = new Point(0, 169);
			this.gainTableLayoutPanel.Name = "gainTableLayoutPanel";
			this.gainTableLayoutPanel.RowCount = 2;
			this.gainTableLayoutPanel.RowStyles.Add(new RowStyle());
			this.gainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.gainTableLayoutPanel.Size = new Size(202, 57);
			this.gainTableLayoutPanel.TabIndex = 66;
			this.gainLabel.Anchor = AnchorStyles.Right;
			this.gainLabel.AutoSize = true;
			this.gainLabel.Location = new Point(180, 0);
			this.gainLabel.Name = "gainLabel";
			this.gainLabel.Size = new Size(19, 13);
			this.gainLabel.TabIndex = 1;
			this.gainLabel.Text = "10";
			this.label2.Anchor = AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new Size(29, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Gain";
			this.gainTrackBar.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.gainTableLayoutPanel.SetColumnSpan(this.gainTrackBar, 2);
			this.gainTrackBar.Location = new Point(3, 16);
			this.gainTrackBar.Name = "gainTrackBar";
			this.gainTrackBar.Size = new Size(196, 38);
			this.gainTrackBar.TabIndex = 2;
			this.gainTrackBar.ValueChanged += this.gainTrackBar_ValueChanged;
			this.deviceInfoTableLayoutPanel.ColumnCount = 2;
			this.deviceInfoTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.deviceInfoTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.deviceInfoTableLayoutPanel.Controls.Add(this.deviceSerialLabel, 0, 0);
			this.deviceInfoTableLayoutPanel.Controls.Add(this.deviceNameLabel, 0, 0);
			this.deviceInfoTableLayoutPanel.Controls.Add(this.useFullIQCheckBox, 0, 2);
			this.deviceInfoTableLayoutPanel.Controls.Add(this.bitrateLabel, 1, 1);
			this.deviceInfoTableLayoutPanel.Controls.Add(this.serverVersionLabel, 0, 1);
			this.deviceInfoTableLayoutPanel.Dock = DockStyle.Top;
			this.deviceInfoTableLayoutPanel.Location = new Point(0, 30);
			this.deviceInfoTableLayoutPanel.Name = "deviceInfoTableLayoutPanel";
			this.deviceInfoTableLayoutPanel.RowCount = 3;
			this.deviceInfoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
			this.deviceInfoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
			this.deviceInfoTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
			this.deviceInfoTableLayoutPanel.Size = new Size(202, 76);
			this.deviceInfoTableLayoutPanel.TabIndex = 67;
			this.bitrateLabel.Anchor = AnchorStyles.Right;
			this.bitrateLabel.AutoSize = true;
			this.bitrateLabel.Location = new Point(160, 31);
			this.bitrateLabel.Name = "bitrateLabel";
			this.bitrateLabel.Size = new Size(39, 13);
			this.bitrateLabel.TabIndex = 4;
			this.bitrateLabel.Text = "0 kbps";
			this.deviceSerialLabel.Anchor = AnchorStyles.Right;
			this.deviceSerialLabel.AutoSize = true;
			this.deviceSerialLabel.Location = new Point(165, 6);
			this.deviceSerialLabel.Name = "deviceSerialLabel";
			this.deviceSerialLabel.Size = new Size(34, 13);
			this.deviceSerialLabel.TabIndex = 2;
			this.deviceSerialLabel.Text = "SN: 0";
			this.deviceNameLabel.Anchor = AnchorStyles.Left;
			this.deviceNameLabel.AutoSize = true;
			this.deviceNameLabel.Location = new Point(3, 6);
			this.deviceNameLabel.Name = "deviceNameLabel";
			this.deviceNameLabel.Size = new Size(58, 13);
			this.deviceNameLabel.TabIndex = 1;
			this.deviceNameLabel.Text = "Airspy One";
			this.useFullIQCheckBox.AutoSize = true;
			this.useFullIQCheckBox.Location = new Point(3, 53);
			this.useFullIQCheckBox.Name = "useFullIQCheckBox";
			this.useFullIQCheckBox.Size = new Size(75, 17);
			this.useFullIQCheckBox.TabIndex = 3;
			this.useFullIQCheckBox.Text = "Use full IQ";
			this.useFullIQCheckBox.UseVisualStyleBackColor = true;
			this.useFullIQCheckBox.CheckStateChanged += this.useFullIQCheckBox_CheckStateChanged;
			this.downStreamTimer.Enabled = true;
			this.downStreamTimer.Interval = 500;
			this.downStreamTimer.Tick += this.downStreamTimer_Tick;
			this.iqFormatTableLayoutPanel.ColumnCount = 2;
			this.iqFormatTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
			this.iqFormatTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
			this.iqFormatTableLayoutPanel.Controls.Add(this.label3, 0, 0);
			this.iqFormatTableLayoutPanel.Controls.Add(this.streamFormatComboBox, 1, 0);
			this.iqFormatTableLayoutPanel.Dock = DockStyle.Top;
			this.iqFormatTableLayoutPanel.Location = new Point(0, 137);
			this.iqFormatTableLayoutPanel.Name = "iqFormatTableLayoutPanel";
			this.iqFormatTableLayoutPanel.RowCount = 1;
			this.iqFormatTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.iqFormatTableLayoutPanel.Size = new Size(202, 32);
			this.iqFormatTableLayoutPanel.TabIndex = 68;
			this.serverVersionLabel.Anchor = AnchorStyles.Left;
			this.serverVersionLabel.AutoSize = true;
			this.serverVersionLabel.Location = new Point(3, 31);
			this.serverVersionLabel.Name = "serverVersionLabel";
			this.serverVersionLabel.Size = new Size(86, 13);
			this.serverVersionLabel.TabIndex = 5;
			this.serverVersionLabel.Text = "Server: 2.0.1606";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.gainTableLayoutPanel);
			base.Controls.Add(this.iqFormatTableLayoutPanel);
			base.Controls.Add(this.bandwidthTableLayoutPanel);
			base.Controls.Add(this.deviceInfoTableLayoutPanel);
			base.Controls.Add(this.hostTableLayoutPanel);
			base.Name = "ControllerPanel";
			base.Size = new Size(202, 30);
			this.hostTableLayoutPanel.ResumeLayout(false);
			this.bandwidthTableLayoutPanel.ResumeLayout(false);
			this.bandwidthTableLayoutPanel.PerformLayout();
			this.gainTableLayoutPanel.ResumeLayout(false);
			this.gainTableLayoutPanel.PerformLayout();
			((ISupportInitialize)this.gainTrackBar).EndInit();
			this.deviceInfoTableLayoutPanel.ResumeLayout(false);
			this.deviceInfoTableLayoutPanel.PerformLayout();
			this.iqFormatTableLayoutPanel.ResumeLayout(false);
			this.iqFormatTableLayoutPanel.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
