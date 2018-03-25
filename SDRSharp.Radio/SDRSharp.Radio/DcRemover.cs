using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	[StructLayout(LayoutKind.Sequential, Pack = 16)]
	public struct DcRemover
	{
		private float _average;

		private float _ratio;

		public float Offset
		{
			get
			{
				return this._average;
			}
		}

		public DcRemover(float ratio)
		{
			this._ratio = ratio;
			this._average = 0f;
		}

		public void Init(float ratio)
		{
			this._ratio = ratio;
			this._average = 0f;
		}

		public unsafe void Process(float* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				this._average += this._ratio * (buffer[i] - this._average);
				buffer[i] -= this._average;
			}
		}

		public unsafe void ProcessInterleaved(float* buffer, int length)
		{
			length *= 2;
			for (int i = 0; i < length; i += 2)
			{
				this._average += this._ratio * (buffer[i] - this._average);
				buffer[i] -= this._average;
			}
		}

		public void Reset()
		{
			this._average = 0f;
		}
	}
}
