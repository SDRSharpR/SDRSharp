using System;

namespace PortAudioSharp
{
	internal struct PaStreamParameters
	{
		public int device;

		public int channelCount;

		public PaSampleFormat sampleFormat;

		public double suggestedLatency;

		public IntPtr hostApiSpecificStreamInfo;

		public override string ToString()
		{
			return "[" + ((object)this).GetType().Name + "]\ndevice: " + this.device + "\nchannelCount: " + this.channelCount + "\nsampleFormat: " + this.sampleFormat + "\nsuggestedLatency: " + this.suggestedLatency;
		}
	}
}
