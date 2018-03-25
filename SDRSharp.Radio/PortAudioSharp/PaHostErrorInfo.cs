using System.Runtime.InteropServices;

namespace PortAudioSharp
{
	internal struct PaHostErrorInfo
	{
		public PaHostApiTypeId hostApiType;

		public int errorCode;

		[MarshalAs(UnmanagedType.LPStr)]
		public string errorText;

		public override string ToString()
		{
			return "[" + ((object)this).GetType().Name + "]\nhostApiType: " + this.hostApiType + "\nerrorCode: " + this.errorCode + "\nerrorText: " + this.errorText;
		}
	}
}
