namespace SDRSharp.Radio
{
	public class Oscillator
	{
		private Complex _vector;

		private Complex _rotation;

		private double _sampleRate;

		private double _frequency;

		public double SampleRate
		{
			get
			{
				return this._sampleRate;
			}
			set
			{
				if (this._sampleRate != value)
				{
					this._sampleRate = value;
					this.Configure();
				}
			}
		}

		public double Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				if (this._frequency != value)
				{
					this._frequency = value;
					this.Configure();
				}
			}
		}

		public Complex Phase
		{
			get
			{
				return this._vector;
			}
			set
			{
				this._vector = value;
			}
		}

		public float Real
		{
			get
			{
				return this._vector.Real;
			}
			set
			{
				this._vector.Real = value;
			}
		}

		public float Imag
		{
			get
			{
				return this._vector.Imag;
			}
			set
			{
				this._vector.Imag = value;
			}
		}

		private void Configure()
		{
			if (this._vector.Real == 0f && this._vector.Imag == 0f)
			{
				this._vector.Real = 1f;
			}
			if (this._sampleRate != 0.0)
			{
				double angle = 6.2831853071795862 * this._frequency / this._sampleRate;
				this._rotation = Complex.FromAngle(angle);
			}
		}

		public void Tick()
		{
			this._vector *= this._rotation;
			this._vector = this._vector.NormalizeFast();
		}
	}
}
