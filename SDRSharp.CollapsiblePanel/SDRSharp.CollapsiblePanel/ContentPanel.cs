using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SDRSharp.CollapsiblePanel
{
	[Designer(typeof(ContentPanelDesigner))]
	public class ContentPanel : Panel
	{
		public ContentPanel()
		{
			base.Dock = DockStyle.Fill;
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			e.Control.Resize += this.sourceControlPanel_Resize;
			base.OnControlAdded(e);
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			e.Control.Resize -= this.sourceControlPanel_Resize;
			base.OnControlRemoved(e);
		}

		private void sourceControlPanel_Resize(object sender, EventArgs e)
		{
			((CollapsiblePanel)base.Parent).UpdateState();
		}
	}
}
