using SDRSharp.CollapsiblePanel.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SDRSharp.CollapsiblePanel
{
	[DesignTimeVisible(true)]
	[Category("Containers")]
	[Description("Visual Studio like Collapsible Panel")]
	[Designer(typeof(CollapsiblePanelDesigner))]
	public class CollapsiblePanel : UserControl
	{
		private bool _autoHeight;

		private PanelStateOptions _panelState = PanelStateOptions.Expanded;

		private CollapsiblePanel _nextPanel;

		private IContainer components;

		private Panel titlePanel;

		private PictureBox togglingImage;

		private ImageList collapsiblePanelImageList;

		private Label lblPanelTitle;

		private ContentPanel contentPanel;

		private TableLayoutPanel titleTableLayoutPanel;

		[Description("Gets or sets panel title")]
		[DisplayName("Panel Title")]
		[Category("Collapsible Panel")]
		public string PanelTitle
		{
			get
			{
				return this.lblPanelTitle.Text;
			}
			set
			{
				this.lblPanelTitle.Text = value;
			}
		}

		[DefaultValue(typeof(PanelStateOptions), "Expanded")]
		[Description("Gets or sets current panel state")]
		[DisplayName("Panel State")]
		[Category("Collapsible Panel")]
		public PanelStateOptions PanelState
		{
			get
			{
				return this._panelState;
			}
			set
			{
				this._panelState = value;
				this.UpdateState();
			}
		}

		[Category("Collapsible Panel")]
		[Description("Gets or sets the panel to be located beneath this panel")]
		public CollapsiblePanel NextPanel
		{
			get
			{
				return this._nextPanel;
			}
			set
			{
				this._nextPanel = value;
				this.MoveNextPanel();
			}
		}

		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ContentPanel Content
		{
			get
			{
				return this.contentPanel;
			}
		}

		public bool AutoHeight
		{
			get
			{
				return this._autoHeight;
			}
			set
			{
				if (this._autoHeight != value)
				{
					this._autoHeight = value;
					this.UpdateState();
				}
			}
		}

		public CollapsiblePanel()
		{
			this.InitializeComponent();
			base.Load += this.CollapsiblePanel_Load;
			base.SizeChanged += this.CollapsiblePanel_SizeChanged;
			base.LocationChanged += this.CollapsiblePanel_LocationChanged;
		}

		private void CollapsiblePanel_Load(object sender, EventArgs e)
		{
			if (this._panelState == PanelStateOptions.Collapsed)
			{
				this.togglingImage.Image = Resources.CollapsedIcon;
			}
			else
			{
				this.togglingImage.Image = Resources.ExpandedIcon;
			}
		}

		private void CollapsiblePanel_SizeChanged(object sender, EventArgs e)
		{
			this.MoveNextPanel();
		}

		private void CollapsiblePanel_LocationChanged(object sender, EventArgs e)
		{
			this.MoveNextPanel();
		}

		private void ToggleState(object sender, EventArgs e)
		{
			this._panelState = ((this._panelState == PanelStateOptions.Collapsed) ? PanelStateOptions.Expanded : PanelStateOptions.Collapsed);
			this.UpdateState();
		}

		internal void UpdateState()
		{
			if (this._panelState == PanelStateOptions.Collapsed)
			{
				this.contentPanel.Visible = false;
				base.Height = this.titlePanel.Height;
				this.togglingImage.Image = Resources.CollapsedIcon;
			}
			else
			{
				int num = (this.contentPanel.Controls.Count != 1 || !this._autoHeight) ? this.contentPanel.Height : this.contentPanel.Controls[0].Height;
				base.Height = this.titlePanel.Height + num;
				this.contentPanel.Visible = true;
				this.togglingImage.Image = Resources.ExpandedIcon;
				Panel panel = base.Parent as Panel;
				while (panel != null && panel.AutoSize)
				{
					panel = (panel.Parent as Panel);
				}
				if (panel != null)
				{
					panel.ScrollControlIntoView(this);
				}
			}
		}

		private void MoveNextPanel()
		{
			if (this._nextPanel != null)
			{
				CollapsiblePanel nextPanel = this._nextPanel;
				Point location = base.Location;
				int x = location.X;
				location = base.Location;
				nextPanel.Location = new Point(x, location.Y + base.Size.Height);
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(CollapsiblePanel));
			this.collapsiblePanelImageList = new ImageList(this.components);
			this.titlePanel = new Panel();
			this.titleTableLayoutPanel = new TableLayoutPanel();
			this.togglingImage = new PictureBox();
			this.lblPanelTitle = new Label();
			this.contentPanel = new ContentPanel();
			this.titlePanel.SuspendLayout();
			this.titleTableLayoutPanel.SuspendLayout();
			((ISupportInitialize)this.togglingImage).BeginInit();
			base.SuspendLayout();
			this.collapsiblePanelImageList.ImageStream = (ImageListStreamer)componentResourceManager.GetObject("collapsiblePanelImageList.ImageStream");
			this.collapsiblePanelImageList.TransparentColor = Color.Transparent;
			this.collapsiblePanelImageList.Images.SetKeyName(0, "ExpandIcon.jpg");
			this.titlePanel.BackColor = Color.DarkGray;
			this.titlePanel.BackgroundImage = Resources.titleBackground;
			this.titlePanel.BackgroundImageLayout = ImageLayout.Stretch;
			this.titlePanel.Controls.Add(this.titleTableLayoutPanel);
			this.titlePanel.Dock = DockStyle.Top;
			this.titlePanel.Location = new Point(0, 0);
			this.titlePanel.Name = "titlePanel";
			this.titlePanel.Size = new Size(150, 24);
			this.titlePanel.TabIndex = 0;
			this.titleTableLayoutPanel.BackColor = Color.Transparent;
			this.titleTableLayoutPanel.ColumnCount = 2;
			this.titleTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24f));
			this.titleTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.titleTableLayoutPanel.Controls.Add(this.togglingImage, 0, 0);
			this.titleTableLayoutPanel.Controls.Add(this.lblPanelTitle, 1, 0);
			this.titleTableLayoutPanel.Dock = DockStyle.Fill;
			this.titleTableLayoutPanel.Location = new Point(0, 0);
			this.titleTableLayoutPanel.Name = "titleTableLayoutPanel";
			this.titleTableLayoutPanel.RowCount = 1;
			this.titleTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.titleTableLayoutPanel.Size = new Size(150, 24);
			this.titleTableLayoutPanel.TabIndex = 2;
			this.titleTableLayoutPanel.Click += this.ToggleState;
			this.togglingImage.Anchor = AnchorStyles.None;
			this.togglingImage.BackColor = Color.Transparent;
			this.togglingImage.Image = Resources.ExpandedIcon;
			this.togglingImage.Location = new Point(7, 7);
			this.togglingImage.Name = "togglingImage";
			this.togglingImage.Size = new Size(10, 10);
			this.togglingImage.SizeMode = PictureBoxSizeMode.StretchImage;
			this.togglingImage.TabIndex = 0;
			this.togglingImage.TabStop = false;
			this.togglingImage.Click += this.ToggleState;
			this.lblPanelTitle.Anchor = AnchorStyles.Left;
			this.lblPanelTitle.AutoEllipsis = true;
			this.lblPanelTitle.AutoSize = true;
			this.lblPanelTitle.BackColor = Color.Transparent;
			this.lblPanelTitle.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
			this.lblPanelTitle.ForeColor = Color.WhiteSmoke;
			this.lblPanelTitle.Location = new Point(27, 4);
			this.lblPanelTitle.Name = "lblPanelTitle";
			this.lblPanelTitle.Size = new Size(59, 15);
			this.lblPanelTitle.TabIndex = 1;
			this.lblPanelTitle.Text = "Panel title";
			this.lblPanelTitle.Click += this.ToggleState;
			this.contentPanel.Location = new Point(0, 24);
			this.contentPanel.Margin = new Padding(2);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = new Size(150, 126);
			this.contentPanel.TabIndex = 1;
			base.AutoScaleDimensions = new SizeF(96f, 96f);
			base.AutoScaleMode = AutoScaleMode.Dpi;
			base.Controls.Add(this.contentPanel);
			base.Controls.Add(this.titlePanel);
			base.Name = "CollapsiblePanel";
			this.titlePanel.ResumeLayout(false);
			this.titleTableLayoutPanel.ResumeLayout(false);
			this.titleTableLayoutPanel.PerformLayout();
			((ISupportInitialize)this.togglingImage).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
