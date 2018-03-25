using System;
using System.Drawing;

namespace SDRSharp.PanView
{
	public class CustomPaintEventArgs : EventArgs
	{
		public Graphics Graphics
		{
			get;
			private set;
		}

		public Point CursorPosition
		{
			get;
			private set;
		}

		public string CustomTitle
		{
			get;
			set;
		}

		public bool Cancel
		{
			get;
			set;
		}

		public CustomPaintEventArgs(Graphics graphics, Point cursorPosition)
		{
			this.Graphics = graphics;
			this.CursorPosition = cursorPosition;
		}
	}
}
