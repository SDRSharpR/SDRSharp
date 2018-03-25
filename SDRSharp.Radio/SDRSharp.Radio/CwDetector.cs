namespace SDRSharp.Radio
{
	public sealed class CwDetector
	{
		private Oscillator _bfo = new Oscillator();

		public double SampleRate
		{
			get
			{
				return this._bfo.SampleRate;
			}
			set
			{
				this._bfo.SampleRate = value;
			}
		}

		public int BfoFrequency
		{
			get
			{
				return (int)this._bfo.Frequency;
			}
			set
			{
				this._bfo.Frequency = (double)value;
			}
		}

		public unsafe void Demodulate(Complex* iq, float* audio, int length)
		{
			for (int i = 0; i < length; i++)
			{
				this._bfo.Tick();
				audio[i] = (iq[i] * this._bfo.Phase).Real;
			}
		}
	}
}
