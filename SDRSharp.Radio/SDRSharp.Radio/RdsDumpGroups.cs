using System;
using System.Linq;
using System.Text;

namespace SDRSharp.Radio
{
	public class RdsDumpGroups
	{
		private StringBuilder _radioTextSB = new StringBuilder("                                                                        ");

		private StringBuilder _programServiceSB = new StringBuilder("                                                                        ");

		private string _radioText = string.Empty;

		private string _programService = "        ";

		private ushort _piCode;

		private bool _radioTextABFlag;

		public string RadioText
		{
			get
			{
				return this._radioText;
			}
		}

		public string ProgramService
		{
			get
			{
				return this._programService;
			}
		}

		public ushort PICode
		{
			get
			{
				return this._piCode;
			}
		}

		public void Reset()
		{
			lock (this)
			{
				this._radioTextSB = new StringBuilder("                                                                        ");
				this._programServiceSB = new StringBuilder("                                                                        ");
				this._radioText = string.Empty;
				this._programService = "        ";
				this._piCode = 0;
				this._radioTextABFlag = false;
			}
		}

		public bool AnalyseFrames(ref RdsFrame frame)
		{
			bool result = false;
			if ((frame.GroupB & 0xF800) == 8192)
			{
				int num = (frame.GroupB & 0xF) * 4;
				bool flag = (frame.GroupB >> 4 & 1) == 1;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append((char)(frame.GroupC >> 8));
				stringBuilder.Append((char)(frame.GroupC & 0xFF));
				stringBuilder.Append((char)(frame.GroupD >> 8));
				stringBuilder.Append((char)(frame.GroupD & 0xFF));
				if (stringBuilder.ToString().Any(delegate(char ch)
				{
					if (ch >= ' ')
					{
						return ch > '\u007f';
					}
					return true;
				}))
				{
					return false;
				}
				lock (this)
				{
					if (flag != this._radioTextABFlag)
					{
						for (int i = 0; i < this._radioTextSB.Length; i++)
						{
							this._radioTextSB[i] = ' ';
						}
						this._radioTextABFlag = flag;
					}
					else
					{
						this._radioTextSB.Remove(num, 4);
					}
					this._radioTextSB.Insert(num, stringBuilder.ToString());
					this._radioText = this._radioTextSB.ToString().Trim();
					this._piCode = frame.GroupA;
				}
				result = true;
			}
			if ((frame.GroupB & 0xF800) == 0)
			{
				int num2 = (frame.GroupB & 3) * 2;
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.Append((char)(frame.GroupD >> 8));
				stringBuilder2.Append((char)(frame.GroupD & 0xFF));
				if (stringBuilder2.ToString().Any(delegate(char ch)
				{
					if (ch >= ' ')
					{
						return ch > '\u007f';
					}
					return true;
				}))
				{
					return false;
				}
				lock (this)
				{
					this._programServiceSB.Remove(num2, 2);
					this._programServiceSB.Insert(num2, stringBuilder2.ToString());
					this._programService = this._programServiceSB.ToString().Substring(0, 8);
					this._piCode = frame.GroupA;
				}
				result = true;
			}
			return result;
		}

		private static string Dump4A(ushort blockB, ushort block3, ushort block4)
		{
			int num = block4 & 0x1F;
			if ((block4 & 0x20) != 0)
			{
				num *= -1;
			}
			int minute = block4 >> 6 & 0x3F;
			int hour = (block4 >> 12 & 0xF) | (block3 << 4 & 0x10);
			int num2 = block3 >> 1 | (blockB << 15 & 0x18000);
			int num3 = (int)(((double)num2 - 15078.2) / 365.25);
			int num4 = (int)(((double)num2 - 14956.1 - (double)(int)((double)num3 * 365.25)) / 30.6001);
			int day = num2 - 14956 - (int)((double)num3 * 365.25) - (int)((double)num4 * 30.6001);
			int num5 = 0;
			if (num4 == 14 || num4 == 15)
			{
				num5 = 1;
			}
			num3 = num3 + num5 + 1900;
			num4 = num4 - 1 - num5 * 12;
			try
			{
				DateTime d = new DateTime(num3, num4, day, hour, minute, 0);
				TimeSpan t = new TimeSpan(num / 2, num * 30 % 60, 0);
				d += t;
				return "4A " + d.ToLongDateString() + " " + d.ToLongTimeString();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
