using System.Collections;
using System.Windows.Forms.Design;

namespace SDRSharp.CollapsiblePanel
{
	public class ContentPanelDesigner : ScrollableControlDesigner
	{
		protected override void PreFilterProperties(IDictionary properties)
		{
			properties.Remove("Dock");
			properties.Remove("AutoSize");
			properties.Remove("AutoSizeMode");
			base.PreFilterProperties(properties);
		}
	}
}
