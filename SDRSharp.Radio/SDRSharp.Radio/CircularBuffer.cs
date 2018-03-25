using System;
using System.Collections.Generic;

namespace SDRSharp.Radio
{
	public class CircularBuffer : IDisposable
	{
		private readonly int _bufferSize;

		private readonly int _elementSize;

		private readonly int _maxBufferCount;

		private readonly SharpEvent _readEvent = new SharpEvent(false);

		private readonly SharpEvent _writeEvent = new SharpEvent(false);

		private int _count;

		private int _len;

		private int _head;

		private int _tail;

		private bool _closed;

		private List<UnsafeBuffer> _buffers = new List<UnsafeBuffer>();

		public int BufferSize
		{
			get
			{
				return this._bufferSize;
			}
		}

		public int BufferCount
		{
			get
			{
				return this._maxBufferCount;
			}
		}

		public int AvailableCount
		{
			get
			{
				return this._count;
			}
		}

		protected CircularBuffer(int bufferSize, int elementSize, int maxBufferCount)
		{
			this._bufferSize = bufferSize;
			this._elementSize = elementSize;
			this._maxBufferCount = maxBufferCount;
			for (int i = 0; i < this._maxBufferCount; i++)
			{
				this._buffers.Add(UnsafeBuffer.Create(this._bufferSize, this._elementSize));
			}
		}

		~CircularBuffer()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			this.Close();
			GC.SuppressFinalize(this);
		}

		protected unsafe bool Write(byte* buffer, int len, bool block)
		{
			int num = len;
			while (num > 0 && !this._closed)
			{
				if (block)
				{
					while (this._count >= this._maxBufferCount && !this._closed)
					{
						this._writeEvent.WaitOne();
					}
				}
				else if (this._count >= this._maxBufferCount)
				{
					return false;
				}
				if (this._closed)
				{
					return false;
				}
				byte* ptr = (byte*)(void*)this._buffers[this._head];
				int num2 = Math.Min(this._bufferSize - this._len, num);
				int num3 = num2 * this._elementSize;
				Utils.Memcpy(ptr + this._len * this._elementSize, buffer, num3);
				buffer += num3;
				this._len += num2;
				num -= num2;
				if (this._len == this._bufferSize)
				{
					this._len = 0;
					this._head = (this._head + 1) % this._maxBufferCount;
					lock (this)
					{
						this._count++;
						this._readEvent.Set();
					}
				}
			}
			return true;
		}

		protected unsafe void* AcquireRawBuffer(bool block)
		{
			if (this._closed)
			{
				return null;
			}
			if (block)
			{
				while (this._count == 0 && !this._closed)
				{
					this._readEvent.WaitOne();
				}
			}
			if (!this._closed && this._count != 0)
			{
				return this._buffers[this._tail];
			}
			return null;
		}

		public void Release()
		{
			if (!this._closed && this._count != 0)
			{
				this._tail = (this._tail + 1) % this._maxBufferCount;
				lock (this)
				{
					this._count--;
					this._writeEvent.Set();
				}
			}
		}

		public void Close()
		{
			this._closed = true;
			this._readEvent.Set();
			this._writeEvent.Set();
			this._head = 0;
			this._tail = 0;
			this._count = 0;
			this._len = 0;
			lock (this)
			{
				foreach (UnsafeBuffer buffer in this._buffers)
				{
					buffer.Dispose();
				}
				this._buffers.Clear();
			}
		}
	}
}
