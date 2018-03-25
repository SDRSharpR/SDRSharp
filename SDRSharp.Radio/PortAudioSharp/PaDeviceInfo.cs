using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	internal struct PaDeviceInfo
	{
		public int structVersion;

		[MarshalAs(UnmanagedType.LPStr)]
		public string name;

		public int hostApi;

		public int maxInputChannels;

		public int maxOutputChannels;

		public double defaultLowInputLatency;

		public double defaultLowOutputLatency;

		public double defaultHighInputLatency;

		public double defaultHighOutputLatency;

		public double defaultSampleRate;

		public override string ToString()
		{
			return "[" + ((object)this).GetType().Name + "]\nname: " + this.name + "\nhostApi: " + this.hostApi + "\nmaxInputChannels: " + this.maxInputChannels + "\nmaxOutputChannels: " + this.maxOutputChannels + "\ndefaultLowInputLatency: " + this.defaultLowInputLatency + "\ndefaultLowOutputLatency: " + this.defaultLowOutputLatency + "\ndefaultHighInputLatency: " + this.defaultHighInputLatency + "\ndefaultHighOutputLatency: " + this.defaultHighOutputLatency + "\ndefaultSampleRate: " + this.defaultSampleRate;
		}
	}
}
