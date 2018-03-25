using System;

namespace SDRSharp.Radio
{
	public abstract class OverlapSaveProcessor
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

		public int FFTSize
		{
			get
			{
				return this._fftSize;
			}
		}

		public unsafe OverlapSaveProcessor(int fftSize)
		{
			this._fftSize = fftSize;
			this._halfSize = this._fftSize / 2;
			this._inputPos = this._halfSize;
			this._queuepBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._queuePtr = (Complex*)(void*)this._queuepBuffer;
			this._fftBuffer = UnsafeBuffer.Create(this._fftSize, sizeof(Complex));
			this._fftPtr = (Complex*)(void*)this._fftBuffer;
			this._outputBuffer = UnsafeBuffer.Create(this._halfSize, sizeof(Complex));
			this._outputPtr = (Complex*)(void*)this._outputBuffer;
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
					this.OverlapSave();
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
					this.OverlapSave();
					this._inputPos = this._halfSize;
					this._outputPos = 0;
				}
			}
		}

		private unsafe void OverlapSave()
		{
			Utils.Memcpy(this._fftPtr, this._queuePtr, this._fftSize * sizeof(Complex));
			Fourier.ForwardTransform(this._fftPtr, this._fftSize, false);
			this.ProcessFft(this._fftPtr, this._fftSize);
			Fourier.InverseTransform(this._fftPtr, this._fftSize);
			Utils.Memcpy(this._outputPtr, this._fftPtr + this._halfSize, this._halfSize * sizeof(Complex));
			Utils.Memcpy(this._queuePtr, this._queuePtr + this._halfSize, this._halfSize * sizeof(Complex));
		}

		protected unsafe abstract void ProcessFft(Complex* buffer, int length);
	}
}
