namespace PortAudioSharp
{
	internal struct PaStreamInfo
	{
		public int structVersion;

		public double inputLatency;

		public double outputLatency;

		public double sampleRate;

		public override string ToString()
		{
			return "[" + ((object)this).GetType().Name + "]\nstructVersion: " + this.structVersion + "\ninputLatency: " + this.inputLatency + "\noutputLatency: " + this.outputLatency + "\nsampleRate: " + this.sampleRate;
		}
	}
}
