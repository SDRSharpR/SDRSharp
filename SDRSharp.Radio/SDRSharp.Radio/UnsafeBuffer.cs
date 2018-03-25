using System;
using System.Runtime.InteropServices;

namespace SDRSharp.Radio
{
	public sealed class UnsafeBuffer : IDisposable
	{
		private readonly GCHandle _handle;

		private unsafe void* _ptr;

		private int _length;

		private Array _buffer;

		public unsafe void* Address
		{
			get
			{
				return this._ptr;
			}
		}

		public int Length
		{
			get
			{
				return this._length;
			}
		}

		private unsafe UnsafeBuffer(Array buffer, int realLength, bool aligned)
		{
			this._buffer = buffer;
			this._handle = GCHandle.Alloc(this._buffer, GCHandleType.Pinned);
			this._ptr = (void*)this._handle.AddrOfPinnedObject();
			if (aligned)
			{
				this._ptr = (void*)((long)this._ptr + 15 & -16);
			}
			this._length = realLength;
		}

		~UnsafeBuffer()
		{
			this.Dispose();
		}

		public unsafe void Dispose()
		{
			GCHandle handle = this._handle;
			if (handle.IsAllocated)
			{
				handle = this._handle;
				handle.Free();
			}
			this._buffer = null;
			this._ptr = null;
			this._length = 0;
			GC.SuppressFinalize(this);
		}

		public void Clear()
		{
			Array.Clear(this._buffer, 0, this._buffer.Length);
		}

		public unsafe static implicit operator void*(UnsafeBuffer unsafeBuffer)
		{
			return unsafeBuffer.Address;
		}

		public static UnsafeBuffer Create(int size)
		{
			return UnsafeBuffer.Create(size, 1, true);
		}

		public static UnsafeBuffer Create(int length, int sizeOfElement)
		{
			return UnsafeBuffer.Create(length, sizeOfElement, true);
		}

		public static UnsafeBuffer Create(int length, int sizeOfElement, bool aligned)
		{
			return new UnsafeBuffer(new byte[length * sizeOfElement + (aligned ? 16 : 0)], length, aligned);
		}

		public static UnsafeBuffer Create(Array buffer)
		{
			return new UnsafeBuffer(buffer, buffer.Length, false);
		}
	}
}
