using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SDRSharp.CollapsiblePanel.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("SDRSharp.CollapsiblePanel.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static Bitmap CollapsedIcon
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("CollapsedIcon", Resources.resourceCulture);
			}
		}

		internal static string ExpandedHeigth
		{
			get
			{
				return Resources.ResourceManager.GetString("ExpandedHeigth", Resources.resourceCulture);
			}
		}

		internal static Bitmap ExpandedIcon
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("ExpandedIcon", Resources.resourceCulture);
			}
		}

		internal static Bitmap titleBackground
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("titleBackground", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}
