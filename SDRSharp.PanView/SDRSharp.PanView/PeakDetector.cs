using System;
using System.Collections.Generic;

namespace SDRSharp.PanView
{
	public sealed class PeakDetector
	{
		private const byte Threshold = 20;

		public static void GetPeaks(byte[] buffer, List<int> peaks, int windowSize)
		{
			windowSize |= 1;
			int halfSize = windowSize / 2;
			float num = 1f / (float)windowSize;
			peaks.Clear();
			for (int i = 0; i < buffer.Length; i++)
			{
				int num2 = 0;
				int max_index = i;
				for (int j = 0; j < windowSize; j++)
				{
					int num3 = i + j - halfSize;
					if (num3 < 0)
					{
						num3 = 0;
					}
					if (num3 >= buffer.Length)
					{
						num3 = buffer.Length - 1;
					}
					if (buffer[num3] >= buffer[max_index])
					{
						max_index = num3;
					}
					num2 += buffer[num3];
				}
				float num4 = (float)num2 * num;
				if ((float)(int)buffer[max_index] - num4 > 20f && !peaks.Exists(delegate(int x)
				{
					if (Math.Abs(max_index - x) <= halfSize)
					{
						return buffer[x] > buffer[max_index];
					}
					return false;
				}))
				{
					peaks.RemoveAll((int x) => Math.Abs(max_index - x) <= halfSize);
					peaks.Add(max_index);
				}
			}
		}
	}
}
