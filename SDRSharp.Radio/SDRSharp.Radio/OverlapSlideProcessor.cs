using System;

namespace SDRSharp.Radio
{
	public abstract class OverlapSlideProcessor
	{
		private readonly int _fftSize;

		private readonly int _outputSize;

		private readonly float _overlapRatio;

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

		public int FFTSize
		{
			get
			{
				return this._fftSize;
			}
		}

		public float OverlapRatio
		{
			get
			{
				return this._overlapRatio;
			}
		}

		public OverlapSlideProcessor(int fftSize)
			: this(fftSize, 0.75f)
		{
		}

		public unsafe OverlapSlideProcessor(int fftSize, float overlapRatio)
		{
			if (overlapRatio < 0.75f)
			{
				throw new ArgumentException("Overlap ratio must be greater than or equal to 0.75", "overlapRatio");
			}
			if (overlapRatio > 1f)
			{
				throw new ArgumentException("Overlap ratio must be less than 1.0", "overlapRatio");
			}
			this._fftSize = fftSize;
			this._outputSize = (int)Math.Round((double)((float)this._fftSize * (1f - overlapRatio)));
			if (this._outputSize < 3)
			{
				this._outputSize = 3;
			}
			this._overlapRatio = 1f - (float)this._outputSize / (float)this._fftSize;
			this._inputPos = this._fftSize - this._outputSize;
			this._queuepBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._queuePtr = (Complex*)(void*)this._queuepBuffer;
			this._fftBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._fftPtr = (Complex*)(void*)this._fftBuffer;
			this._outputBuffer = UnsafeBuffer.Create(this._outputSize, sizeof(Complex));
			this._outputPtr = (Complex*)(void*)this._outputBuffer;
			this._overlapBuffer = UnsafeBuffer.Create(this._outputSize, sizeof(Complex));
			this._overlapPtr = (Complex*)(void*)this._overlapBuffer;
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
					this._inputPos = this._fftSize - this._outputSize;
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
					this._inputPos = this._fftSize - this._outputSize;
					this._outputPos = 0;
				}
			}
		}

		private unsafe void OverlapAdd()
		{
			Utils.Memcpy(this._fftPtr, this._queuePtr, this._fftSize * sizeof(Complex));
			Fourier.ForwardTransform(this._fftPtr, this._fftSize, false);
			this.ProcessFft(this._fftPtr, this._fftSize);
			Fourier.InverseTransform(this._fftPtr, this._fftSize);
			Complex* ptr = this._fftPtr + this._fftSize / 2;
			float num = 1f / (float)(this._outputSize - 1);
			for (int i = 0; i < this._outputSize; i++)
			{
				float num2 = num * (float)i;
				this._outputPtr[i] = this._overlapPtr[i] * (1f - num2) + ptr[i] * num2;
			}
			Utils.Memmove(this._overlapPtr, ptr + this._outputSize, this._outputSize * sizeof(Complex));
			Utils.Memmove(this._queuePtr, this._queuePtr + this._outputSize, (this._fftSize - this._outputSize) * sizeof(Complex));
		}

		protected unsafe abstract void ProcessFft(Complex* buffer, int length);
	}
}
