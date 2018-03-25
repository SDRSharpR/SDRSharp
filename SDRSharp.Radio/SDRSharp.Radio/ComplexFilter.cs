namespace SDRSharp.Radio
{
	public class ComplexFilter : OverlapSaveProcessor
	{
		private UnsafeBuffer _kernelBuffer;

		private unsafe Complex* _kernelPtr;

		private int _actualKernelLength;

		public int KernelSize
		{
			get
			{
				return this._actualKernelLength;
			}
		}

		public unsafe ComplexFilter(Complex[] kernel)
			: base(ComplexFilter.GetFFTSize(kernel.Length))
		{
			this._actualKernelLength = kernel.Length;
			this._kernelBuffer = UnsafeBuffer.Create(base.FFTSize, sizeof(Complex));
			this._kernelPtr = (Complex*)(void*)this._kernelBuffer;
			this.SetKernel(kernel);
		}

		private static int GetFFTSize(int length)
		{
			int num;
			for (num = 1; num < length; num <<= 1)
			{
			}
			return num << 1;
		}

		public bool IsKernelLengthSupported(int length)
		{
			return length < base.FFTSize / 2;
		}

		public unsafe void SetKernel(Complex[] kernel)
		{
			if (this.IsKernelLengthSupported(kernel.Length))
			{
				fixed (Complex* src = kernel)
				{
					Utils.Memcpy(this._kernelPtr, src, kernel.Length * sizeof(Complex));
				}
				for (int i = kernel.Length; i < base.FFTSize; i++)
				{
					this._kernelPtr[i] = 0f;
				}
				Fourier.ForwardTransform(this._kernelPtr, base.FFTSize, false);
			}
		}

		protected unsafe override void ProcessFft(Complex* buffer, int length)
		{
			for (int i = 0; i < length; i++)
			{
				Complex* intPtr = buffer + i;
				*intPtr *= this._kernelPtr[i];
			}
		}
	}
}
