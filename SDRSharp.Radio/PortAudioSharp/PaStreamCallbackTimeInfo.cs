namespace PortAudioSharp
{
	internal struct PaStreamCallbackTimeInfo
	{
		public double inputBufferAdcTime;

		public double currentTime;

		public double outputBufferDacTime;

		public override string ToString()
		{
			return "[" + ((object)this).GetType().Name + "]\ncurrentTime: " + this.currentTime + "\ninputBufferAdcTime: " + this.inputBufferAdcTime + "\noutputBufferDacTime: " + this.outputBufferDacTime;
		}
	}
}
