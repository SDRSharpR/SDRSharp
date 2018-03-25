using System;
using System.Collections.Generic;

namespace SDRSharp.Radio
{
	public sealed class FloatFifoStream : IDisposable
	{
		private const int BlockSize = 16384;

		private const int MaxBlocksInCache = 128;

		private int _size;

		private int _readPos;

		private int _writePos;

		private bool _terminated;

		private readonly int _maxSize;

		private readonly SharpEvent _writeEvent;

		private readonly SharpEvent _readEvent;

		private readonly Stack<UnsafeBuffer> _usedBlocks = new Stack<UnsafeBuffer>();

		private readonly List<UnsafeBuffer> _blocks = new List<UnsafeBuffer>();

		public int Length
		{
			get
			{
				return this._size;
			}
		}

		public FloatFifoStream()
			: this(BlockMode.None)
		{
		}

		public FloatFifoStream(BlockMode blockMode)
			: this(blockMode, 0)
		{
		}

		public FloatFifoStream(BlockMode blockMode, int maxSize)
		{
			if (blockMode == BlockMode.BlockingRead || blockMode == BlockMode.BlockingReadWrite)
			{
				this._readEvent = new SharpEvent(false);
			}
			if (blockMode == BlockMode.BlockingWrite || blockMode == BlockMode.BlockingReadWrite)
			{
				if (maxSize <= 0)
				{
					throw new ArgumentException("MaxSize should be greater than zero when in blocking write mode", "maxSize");
				}
				this._writeEvent = new SharpEvent(false);
			}
			this._maxSize = maxSize;
		}

		~FloatFifoStream()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			this.Close();
			GC.SuppressFinalize(this);
		}

		private UnsafeBuffer AllocBlock()
		{
			if (this._usedBlocks.Count <= 0)
			{
				return UnsafeBuffer.Create(16384, 4);
			}
			return this._usedBlocks.Pop();
		}

		private void FreeBlock(UnsafeBuffer block)
		{
			if (this._usedBlocks.Count < 128)
			{
				this._usedBlocks.Push(block);
			}
		}

		private UnsafeBuffer GetWBlock()
		{
			UnsafeBuffer unsafeBuffer;
			if (this._writePos < 16384 && this._blocks.Count > 0)
			{
				unsafeBuffer = this._blocks[this._blocks.Count - 1];
			}
			else
			{
				unsafeBuffer = this.AllocBlock();
				this._blocks.Add(unsafeBuffer);
				this._writePos = 0;
			}
			return unsafeBuffer;
		}

		public void Open()
		{
			this._terminated = false;
		}

		public void Close()
		{
			this._terminated = true;
			this.Flush();
		}

		public void Flush()
		{
			lock (this)
			{
				foreach (UnsafeBuffer block in this._blocks)
				{
					this.FreeBlock(block);
				}
				this._blocks.Clear();
				this._readPos = 0;
				this._writePos = 0;
				this._size = 0;
			}
			if (this._writeEvent != null)
			{
				this._writeEvent.Set();
			}
			if (this._readEvent != null)
			{
				this._readEvent.Set();
			}
		}

		public unsafe int Read(float* buf, int ofs, int count)
		{
			if (this._readEvent != null)
			{
				while (this._size == 0 && !this._terminated)
				{
					this._readEvent.WaitOne();
				}
				if (this._terminated)
				{
					return 0;
				}
			}
			int num = default(int);
			lock (this)
			{
				num = this.DoPeek(buf, ofs, count);
				this.DoAdvance(num);
			}
			if (this._writeEvent != null)
			{
				this._writeEvent.Set();
			}
			return num;
		}

		public unsafe int Read(float* buf, int count)
		{
			return this.Read(buf, 0, count);
		}

		public unsafe void Write(float* buf, int ofs, int count)
		{
			if (this._writeEvent != null)
			{
				while (this._size >= this._maxSize && !this._terminated)
				{
					this._writeEvent.WaitOne();
				}
				if (this._terminated)
				{
					return;
				}
			}
			lock (this)
			{
				int num2;
				for (int num = count; num > 0; num -= num2)
				{
					num2 = Math.Min(16384 - this._writePos, num);
					float* ptr = (float*)(void*)this.GetWBlock();
					Utils.Memcpy(ptr + this._writePos, buf + ofs + count - num, num2 * 4);
					this._writePos += num2;
				}
				this._size += count;
			}
			if (this._readEvent != null)
			{
				this._readEvent.Set();
			}
		}

		public unsafe void Write(float* buf, int count)
		{
			this.Write(buf, 0, count);
		}

		private int DoAdvance(int count)
		{
			int num = count;
			while (num > 0 && this._size > 0)
			{
				if (this._readPos == 16384)
				{
					this._readPos = 0;
					this.FreeBlock(this._blocks[0]);
					this._blocks.RemoveAt(0);
				}
				int num2 = (this._blocks.Count == 1) ? Math.Min(this._writePos - this._readPos, num) : Math.Min(16384 - this._readPos, num);
				this._readPos += num2;
				num -= num2;
				this._size -= num2;
			}
			return count - num;
		}

		public int Advance(int count)
		{
			if (this._readEvent != null)
			{
				while (this._size == 0 && !this._terminated)
				{
					this._readEvent.WaitOne();
				}
				if (this._terminated)
				{
					return 0;
				}
			}
			int result = default(int);
			lock (this)
			{
				result = this.DoAdvance(count);
			}
			if (this._writeEvent != null)
			{
				this._writeEvent.Set();
			}
			return result;
		}

		private unsafe int DoPeek(float* buf, int ofs, int count)
		{
			int num = count;
			int num2 = this._readPos;
			int num3 = this._size;
			int num4 = 0;
			while (num > 0 && num3 > 0)
			{
				if (num2 == 16384)
				{
					num2 = 0;
					num4++;
				}
				int num5 = Math.Min(((num4 < this._blocks.Count - 1) ? 16384 : this._writePos) - num2, num);
				float* ptr = (float*)(void*)this._blocks[num4];
				Utils.Memcpy(buf + ofs + count - num, ptr + num2, num5 * 4);
				num -= num5;
				num2 += num5;
				num3 -= num5;
			}
			return count - num;
		}

		public unsafe int Peek(float* buf, int ofs, int count)
		{
			lock (this)
			{
				return this.DoPeek(buf, ofs, count);
			}
		}
	}
}
