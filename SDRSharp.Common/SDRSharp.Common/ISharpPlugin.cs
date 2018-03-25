using System.Windows.Forms;

namespace SDRSharp.Common
{
	public interface ISharpPlugin
	{
		UserControl Gui
		{
			get;
		}

		string DisplayName
		{
			get;
		}

		void Initialize(ISharpControl control);

		void Close();
	}
}
