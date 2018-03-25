using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace SDRSharp.Radio
{
	public static class Utils
	{
		private const string Libc = "msvcrt.dll";

		public unsafe static void ManagedMemcpy(void* dest, void* src, int len)
		{
			byte* ptr = (byte*)dest;
			byte* ptr2 = (byte*)src;
			if (len >= 16)
			{
				do
				{
					*(int*)ptr = *(int*)ptr2;
					*(int*)(ptr + 4) = *(int*)(ptr2 + 4);
					*(int*)(ptr + 2L * 4L) = *(int*)(ptr2 + 2L * 4L);
					*(int*)(ptr + 3L * 4L) = *(int*)(ptr2 + 3L * 4L);
					ptr += 16;
					ptr2 += 16;
				}
				while ((len -= 16) >= 16);
			}
			if (len > 0)
			{
				if ((len & 8) != 0)
				{
					*(int*)ptr = *(int*)ptr2;
					*(int*)(ptr + 4) = *(int*)(ptr2 + 4);
					ptr += 8;
					ptr2 += 8;
				}
				if ((len & 4) != 0)
				{
					*(int*)ptr = *(int*)ptr2;
					ptr += 4;
					ptr2 += 4;
				}
				if ((len & 2) != 0)
				{
					*(short*)ptr = *(short*)ptr2;
					ptr += 2;
					ptr2 += 2;
				}
				if ((len & 1) != 0)
				{
					*ptr = *ptr2;
				}
			}
		}

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memmove")]
		public unsafe static extern void* Memmove(void* dest, void* src, int len);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy")]
		public unsafe static extern void* Memcpy(void* dest, void* src, int len);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset")]
		public unsafe static extern void* Memset(void* dest, int value, int len);

		[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
		public static extern uint TimeBeginPeriod(uint uMilliseconds);

		[DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
		public static extern uint TimeEndPeriod(uint uMilliseconds);

		public static double GetDoubleSetting(string name, double defaultValue)
		{
			double result;
			if (double.TryParse(ConfigurationManager.AppSettings[name], NumberStyles.Number, (IFormatProvider)CultureInfo.InvariantCulture, out result))
			{
				return result;
			}
			return defaultValue;
		}

		public static bool GetBooleanSetting(string name)
		{
			return Utils.GetBooleanSetting(name, false);
		}

		public static bool GetBooleanSetting(string name, bool defaultValue)
		{
			string text;
			try
			{
				text = (ConfigurationManager.AppSettings[name] ?? string.Empty);
			}
			catch
			{
				return defaultValue;
			}
			if (string.IsNullOrEmpty(text))
			{
				return defaultValue;
			}
			return "YyTt".IndexOf(text[0]) >= 0;
		}

		public static Color GetColorSetting(string name, Color defaultColor)
		{
			return Utils.GetColorSetting(name, defaultColor, byte.MaxValue);
		}

		public static Color GetColorSetting(string name, Color defaultColor, byte alpha)
		{
			try
			{
				string text = ConfigurationManager.AppSettings[name];
				int red = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
				int green = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
				int blue = int.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
				return Color.FromArgb(alpha, red, green, blue);
			}
			catch
			{
				return defaultColor;
			}
		}

		public static ColorBlend GetGradientBlend(int alpha, string settingName)
		{
			ColorBlend colorBlend = new ColorBlend();
			string text;
			try
			{
				text = (ConfigurationManager.AppSettings[settingName] ?? string.Empty);
			}
			catch
			{
				text = string.Empty;
			}
			string[] array = text.Split(',');
			if (array.Length < 2)
			{
				colorBlend.Colors = new Color[6]
				{
					Color.White,
					Color.LightBlue,
					Color.DodgerBlue,
					Color.FromArgb(0, 0, 80),
					Color.Black,
					Color.Black
				};
				for (int i = 0; i < colorBlend.Colors.Length; i++)
				{
					colorBlend.Colors[i] = Color.FromArgb(alpha, colorBlend.Colors[i]);
				}
			}
			else
			{
				colorBlend.Colors = new Color[array.Length];
				for (int j = 0; j < array.Length; j++)
				{
					string obj2 = array[j];
					int red = int.Parse(obj2.Substring(0, 2), NumberStyles.HexNumber);
					int green = int.Parse(obj2.Substring(2, 2), NumberStyles.HexNumber);
					int blue = int.Parse(obj2.Substring(4, 2), NumberStyles.HexNumber);
					colorBlend.Colors[j] = Color.FromArgb(red, green, blue);
				}
			}
			float[] array2 = new float[colorBlend.Colors.Length];
			float num = 1f / (float)(array2.Length - 1);
			for (int k = 0; k < array2.Length; k++)
			{
				byte r = colorBlend.Colors[k].R;
				byte g = colorBlend.Colors[k].G;
				byte b = colorBlend.Colors[k].B;
				colorBlend.Colors[k] = Color.FromArgb(alpha, r, g, b);
				array2[k] = (float)k * num;
			}
			colorBlend.Positions = array2;
			return colorBlend;
		}

		public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
		{
			int num = radius * 2;
			SizeF size = new SizeF((float)num, (float)num);
			RectangleF rect = new RectangleF(bounds.Location, size);
			GraphicsPath graphicsPath = new GraphicsPath();
			if (radius == 0)
			{
				graphicsPath.AddRectangle(bounds);
				return graphicsPath;
			}
			graphicsPath.AddArc(rect, 180f, 90f);
			rect.X = bounds.Right - (float)num;
			graphicsPath.AddArc(rect, 270f, 90f);
			rect.Y = bounds.Bottom - (float)num;
			graphicsPath.AddArc(rect, 0f, 90f);
			rect.X = bounds.Left;
			graphicsPath.AddArc(rect, 90f, 90f);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}

		public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, int cornerRadius)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException("graphics");
			}
			if (pen == null)
			{
				throw new ArgumentNullException("pen");
			}
			using (GraphicsPath path = Utils.RoundedRect(bounds, cornerRadius))
			{
				graphics.DrawPath(pen, path);
			}
		}

		public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, int cornerRadius)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException("graphics");
			}
			if (brush == null)
			{
				throw new ArgumentNullException("brush");
			}
			using (GraphicsPath path = Utils.RoundedRect(bounds, cornerRadius))
			{
				graphics.FillPath(brush, path);
			}
		}

		public static string GetFrequencyDisplay(long frequency, bool appendHz)
		{
			string str = (frequency != 0L) ? ((Math.Abs(frequency) < 1000000000) ? ((Math.Abs(frequency) < 1000000) ? ((Math.Abs(frequency) < 1000) ? string.Format("{0} ", frequency) : string.Format("{0:#,#.###} k", (double)frequency / 1000.0)) : string.Format("{0:#,0.000#} M", (double)frequency / 1000000.0)) : string.Format("{0:#,0.000 000} G", (double)frequency / 1000000000.0)) : "0";
			return str + ((frequency == 0L || !appendHz) ? string.Empty : "Hz");
		}

		public static int GetIntSetting(string name, int defaultValue)
		{
			int result;
			if (int.TryParse(ConfigurationManager.AppSettings[name], out result))
			{
				return result;
			}
			return defaultValue;
		}

		public static long GetLongSetting(string name, long defaultValue)
		{
			long result;
			if (long.TryParse(ConfigurationManager.AppSettings[name], out result))
			{
				return result;
			}
			return defaultValue;
		}

		public static string IntArrayToString(params int[] values)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (int value in values)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(',');
			}
			return stringBuilder.ToString().TrimEnd(',');
		}

		public static string ColorToString(Color color)
		{
			return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
		}

		public static Color StringToColor(string code, byte defaultTransparency = byte.MaxValue)
		{
			if (string.IsNullOrEmpty(code))
			{
				return Color.Empty;
			}
			int argb;
			if (int.TryParse(code, NumberStyles.HexNumber, (IFormatProvider)null, out argb))
			{
				return Color.FromArgb(argb);
			}
			Color baseColor = Color.FromName(code);
			if (!baseColor.IsKnownColor)
			{
				return Color.Empty;
			}
			return Color.FromArgb(defaultTransparency, baseColor);
		}

		public static int[] GetIntArraySetting(string name, int[] defaultValue)
		{
			try
			{
				string text = ConfigurationManager.AppSettings[name];
				if (string.IsNullOrEmpty(text))
				{
					return defaultValue;
				}
				string[] array = text.Split(',');
				if (defaultValue != null && defaultValue.Length != array.Length)
				{
					return defaultValue;
				}
				int[] array2 = new int[array.Length];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = int.Parse(array[i]);
				}
				return array2;
			}
			catch
			{
				return defaultValue;
			}
		}

		public static string GetStringSetting(string name, string defaultValue)
		{
			string text = ConfigurationManager.AppSettings[name];
			if (string.IsNullOrEmpty(text))
			{
				return defaultValue;
			}
			return text;
		}

		public static void SaveSetting(string key, object value)
		{
			string value2 = Convert.ToString(value, CultureInfo.InvariantCulture);
			Utils.SaveSetting(key, value2);
		}

		public static void SaveSetting(string key, string value)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			configuration.AppSettings.Settings.Remove(key);
			configuration.AppSettings.Settings.Add(key, value);
			configuration.Save(ConfigurationSaveMode.Full);
			ConfigurationManager.RefreshSection("appSettings");
		}
	}
}
