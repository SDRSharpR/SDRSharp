namespace SDRSharp.Radio
{
	public class FloatCircularBuffer : CircularBuffer
	{
		public FloatCircularBuffer(int bufferSize, int maxBufferCount)
			: base(bufferSize, 4, maxBufferCount)
		{
		}

		public unsafe bool Write(float* buffer, int len, bool block)
		{
			return base.Write((byte*)buffer, len, block);
		}

		public unsafe bool Write(float* buffer, int len)
		{
			return base.Write((byte*)buffer, len, true);
		}

		public unsafe float* Acquire()
		{
			return (float*)base.AcquireRawBuffer(true);
		}

		public unsafe float* Acquire(bool block)
		{
			return (float*)base.AcquireRawBuffer(block);
		}
	}
}
