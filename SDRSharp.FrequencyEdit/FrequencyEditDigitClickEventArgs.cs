using System;
using System.Windows.Forms;

namespace SDRSharp.FrequencyEdit
{
	public class FrequencyEditDigitClickEventArgs
	{
		public FrequencyEditDigitClickEventArgs(bool isUpperHalf, MouseButtons button)
		{
			this.IsUpperHalf = isUpperHalf;
			this.Button = button;
		}

		public bool IsUpperHalf;

		public MouseButtons Button;
	}
}
