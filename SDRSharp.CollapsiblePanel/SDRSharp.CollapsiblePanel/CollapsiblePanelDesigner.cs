using System.ComponentModel;
using System.Windows.Forms.Design;

namespace SDRSharp.CollapsiblePanel
{
	public class CollapsiblePanelDesigner : ParentControlDesigner
	{
		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			if (this.Control is CollapsiblePanel)
			{
				base.EnableDesignMode(((CollapsiblePanel)this.Control).Content, "Content");
			}
		}
	}
}
