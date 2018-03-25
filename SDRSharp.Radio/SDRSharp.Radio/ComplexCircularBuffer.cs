namespace SDRSharp.Radio
{
	public class ComplexCircularBuffer : CircularBuffer
	{
		public unsafe ComplexCircularBuffer(int bufferSize, int maxBufferCount)
			: base(bufferSize, sizeof(Complex), maxBufferCount)
		{
		}

		public unsafe bool Write(Complex* buffer, int len, bool block)
		{
			return base.Write((byte*)buffer, len, block);
		}

		public unsafe bool Write(Complex* buffer, int len)
		{
			return base.Write((byte*)buffer, len, true);
		}

		public unsafe Complex* Acquire()
		{
			return this.Acquire(true);
		}

		public unsafe Complex* Acquire(bool block)
		{
			return (Complex*)base.AcquireRawBuffer(block);
		}
	}
}
