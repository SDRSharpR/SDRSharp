using System.Windows.Forms;

namespace SDRSharp.Radio
{
	public interface IFloatingConfigDialogProvider
	{
		void ShowSettingGUI(IWin32Window parent);

		void HideSettingGUI();
	}
}
