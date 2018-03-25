using System;

namespace SDRSharp.Radio
{
	public abstract class OverlapCrossfadeProcessor
	{
		private readonly int _fftSize;

		private readonly int _halfSize;

		private readonly int _outputSize;

		private readonly int _crossFadingSize;

		private int _inputPos;

		private int _outputPos;

		private UnsafeBuffer _fftBuffer;

		private unsafe Complex* _fftPtr;

		private UnsafeBuffer _queuepBuffer;

		private unsafe Complex* _queuePtr;

		private UnsafeBuffer _outputBuffer;

		private unsafe Complex* _outputPtr;

		private UnsafeBuffer _crossFadingBuffer;

		private unsafe Complex* _crossFadingPtr;

		private UnsafeBuffer _windowBuffer;

		private unsafe float* _windowPtr;

		public int FFTSize
		{
			get
			{
				return this._fftSize;
			}
		}

		public unsafe OverlapCrossfadeProcessor(int fftSize, float crossFadingRatio = 0f)
		{
			this._fftSize = fftSize;
			this._halfSize = this._fftSize / 2;
			this._crossFadingSize = (int)((float)this._halfSize * crossFadingRatio);
			this._outputSize = this._halfSize - this._crossFadingSize;
			this._inputPos = this._halfSize + this._crossFadingSize;
			this._queuepBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._queuePtr = (Complex*)(void*)this._queuepBuffer;
			this._windowBuffer = UnsafeBuffer.Create(this._crossFadingSize, 4);
			this._windowPtr = (float*)(void*)this._windowBuffer;
			this._fftBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._fftPtr = (Complex*)(void*)this._fftBuffer;
			this._outputBuffer = UnsafeBuffer.Create(this._outputSize, sizeof(Complex));
			this._outputPtr = (Complex*)(void*)this._outputBuffer;
			this._crossFadingBuffer = UnsafeBuffer.Create(this._crossFadingSize, sizeof(Complex));
			this._crossFadingPtr = (Complex*)(void*)this._crossFadingBuffer;
			double num = 1.5707963267948966 / (double)(this._crossFadingSize - 1);
			for (int i = 0; i < this._crossFadingSize; i++)
			{
				double a = (double)i * num;
				this._windowPtr[i] = (float)Math.Pow(Math.Sin(a), 2.0);
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
					this.OverlapCrossfade();
					this._inputPos = this._halfSize + this._crossFadingSize;
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
					this.OverlapCrossfade();
					this._inputPos = this._halfSize + this._crossFadingSize;
					this._outputPos = 0;
				}
			}
		}

		private unsafe void OverlapCrossfade()
		{
			Utils.Memcpy(this._fftPtr, this._queuePtr, this._fftSize * sizeof(Complex));
			Fourier.ForwardTransform(this._fftPtr, this._fftSize, false);
			this.ProcessFft(this._fftPtr, this._fftSize);
			Fourier.InverseTransform(this._fftPtr, this._fftSize);
			int num = 0;
			int num2 = this._crossFadingSize - 1;
			int num3 = this._halfSize;
			while (num < this._crossFadingSize)
			{
				this._outputPtr[num] = this._fftPtr[num3] * this._windowPtr[num] + this._crossFadingPtr[num] * this._windowPtr[num2];
				num++;
				num2--;
				num3++;
			}
			Utils.Memcpy(this._outputPtr + this._crossFadingSize, this._fftPtr + this._halfSize + this._crossFadingSize, (this._outputSize - this._crossFadingSize) * sizeof(Complex));
			Utils.Memcpy(this._crossFadingPtr, this._fftPtr + this._halfSize + this._outputSize, this._crossFadingSize * sizeof(Complex));
			Utils.Memcpy(this._queuePtr, this._queuePtr + this._outputSize, (this._fftSize - this._outputSize) * sizeof(Complex));
		}

		protected unsafe abstract void ProcessFft(Complex* buffer, int length);
	}
}
