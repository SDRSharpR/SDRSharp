using System;

namespace SDRSharp.Radio
{
	public abstract class OverlapAddProcessor
	{
		private readonly int _fftSize;

		private readonly int _halfSize;

		private int _inputPos;

		private int _outputPos;

		private UnsafeBuffer _fftBuffer;

		private unsafe Complex* _fftPtr;

		private UnsafeBuffer _queuepBuffer;

		private unsafe Complex* _queuePtr;

		private UnsafeBuffer _outputBuffer;

		private unsafe Complex* _outputPtr;

		private UnsafeBuffer _overlapBuffer;

		private unsafe Complex* _overlapPtr;

		private UnsafeBuffer _windowBuffer;

		private unsafe float* _windowPtr;

		public int FFTSize
		{
			get
			{
				return this._fftSize;
			}
		}

		public unsafe OverlapAddProcessor(int fftSize)
		{
			this._fftSize = fftSize;
			this._halfSize = this._fftSize / 2;
			this._inputPos = this._halfSize;
			this._queuepBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._queuePtr = (Complex*)(void*)this._queuepBuffer;
			this._windowBuffer = UnsafeBuffer.Create(this._fftSize, 4);
			this._windowPtr = (float*)(void*)this._windowBuffer;
			this._fftBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._fftPtr = (Complex*)(void*)this._fftBuffer;
			this._outputBuffer = UnsafeBuffer.Create(this._halfSize, sizeof(Complex));
			this._outputPtr = (Complex*)(void*)this._outputBuffer;
			this._overlapBuffer = UnsafeBuffer.Create(this._halfSize, sizeof(Complex));
			this._overlapPtr = (Complex*)(void*)this._overlapBuffer;
			double num = 1.5707963267948966 / (double)(this._halfSize - 1);
			for (int i = 0; i < this._halfSize; i++)
			{
				double a = (double)i * num;
				this._windowPtr[i] = (float)Math.Sin(a);
				this._windowPtr[this._fftSize - 1 - i] = this._windowPtr[i];
			}
		}

		public unsafe virtual void Process(Complex* buffer, int length)
		{
			while (length > 0)
			{
				int num = Math.Min(this._fftSize - this._inputPos, length);
				Utils.Memcpy(this._queuePtr + this._inputPos, buffer, num * sizeof(Complex));
				Utils.Memcpy(buffer, this._outputPtr + this._outputPos, num * sizeof(Complex));
				buffer += num;
				this._inputPos += num;
				this._outputPos += num;
				length -= num;
				if (this._inputPos == this._fftSize)
				{
					this.OverlapAdd();
					this._inputPos = this._halfSize;
					this._outputPos = 0;
				}
			}
		}

		public unsafe virtual void Process(float* buffer, int length, int step = 1)
		{
			for (int i = 0; i < length; i += step)
			{
				this._queuePtr[this._inputPos++] = buffer[i];
				buffer[i] = this._outputPtr[this._outputPos++].Real;
				if (this._inputPos == this._fftSize)
				{
					this.OverlapAdd();
					this._inputPos = this._halfSize;
					this._outputPos = 0;
				}
			}
		}

		private unsafe void OverlapAdd()
		{
			for (int i = 0; i < this._fftSize; i++)
			{
				this._fftPtr[i] = this._queuePtr[i] * this._windowPtr[i];
			}
			Fourier.ForwardTransform(this._fftPtr, this._fftSize, false);
			this.ProcessFft(this._fftPtr, this._fftSize);
			Fourier.InverseTransform(this._fftPtr, this._fftSize);
			for (int j = 0; j < this._halfSize; j++)
			{
				this._outputPtr[j] = this._overlapPtr[j] * this._windowPtr[this._halfSize + j] + this._fftPtr[j] * this._windowPtr[j];
			}
			Utils.Memcpy(this._overlapPtr, this._fftPtr + this._halfSize, this._halfSize * sizeof(Complex));
			Utils.Memcpy(this._queuePtr, this._queuePtr + this._halfSize, this._halfSize * sizeof(Complex));
		}

		protected unsafe abstract void ProcessFft(Complex* buffer, int length);
	}
}
