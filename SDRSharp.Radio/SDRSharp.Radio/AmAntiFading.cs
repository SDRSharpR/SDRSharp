namespace SDRSharp.Radio
{
	public class AmAntiFading : OverlapAddProcessor
	{
		public AmAntiFading()
			: base(4096)
		{
		}

		protected unsafe override void ProcessFft(Complex* buffer, int length)
		{
			for (int i = 1; i < length / 2 - 1; i++)
			{
				int num = i;
				int num2 = length - i;
				float num3 = buffer[num].ModulusSquared();
				float num4 = buffer[num2].ModulusSquared();
				if (num3 > num4)
				{
					buffer[num2] = buffer[num].Conjugate();
				}
				else
				{
					buffer[num] = buffer[num2].Conjugate();
				}
			}
		}
	}
}
