using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	internal struct PaHostApiInfo
	{
		public int structVersion;

		public PaHostApiTypeId type;

		[MarshalAs(UnmanagedType.LPStr)]
		public string name;

		public int deviceCount;

		public int defaultInputDevice;

		public int defaultOutputDevice;

		public override string ToString()
		{
			return "[" + ((object)this).GetType().Name + "]\nstructVersion: " + this.structVersion + "\ntype: " + this.type + "\nname: " + this.name + "\ndeviceCount: " + this.deviceCount + "\ndefaultInputDevice: " + this.defaultInputDevice + "\ndefaultOutputDevice: " + this.defaultOutputDevice;
		}
	}
}
