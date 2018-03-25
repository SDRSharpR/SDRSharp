using System.Windows.Forms;

namespace SDRSharp.Radio
{
	public interface IConfigurationPanelProvider
	{
		UserControl Gui
		{
			get;
		}
	}
}
