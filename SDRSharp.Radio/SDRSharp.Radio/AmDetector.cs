using System;

namespace SDRSharp.Radio
{
	public sealed class AmDetector
	{
		private float _avg;

		private float _powerThreshold;

		private int _squelchThreshold;

		private bool _isSquelchOpen;

		public int SquelchThreshold
		{
			get
			{
				return this._squelchThreshold;
			}
			set
			{
				if (this._squelchThreshold != value)
				{
					this._squelchThreshold = value;
					this._powerThreshold = ((float)this._squelchThreshold / 100f - 1f) * 100f - 50f;
				}
			}
		}

		public bool IsSquelchOpen
		{
			get
			{
				return this._isSquelchOpen;
			}
		}

		public unsafe void Demodulate(Complex* iq, float* audio, int length)
		{
			for (int i = 0; i < length; i++)
			{
				float num = iq[i].Modulus();
				if (this._squelchThreshold == 0)
				{
					audio[i] = num;
				}
				else
				{
					float num2 = (float)(20.0 * Math.Log10(1E-60 + (double)num));
					this._avg = 0.99f * this._avg + 0.01f * num2;
					this._isSquelchOpen = (this._avg > this._powerThreshold);
					audio[i] = num;
					if (!this._isSquelchOpen)
					{
						audio[i] *= 1E-15f;
					}
				}
			}
		}
	}
}
