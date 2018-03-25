namespace SDRSharp.FrontEnds.SpyServer
{
	public struct MessageHeader
	{
		public uint ProtocolID;

		public MessageType MessageType;

		public StreamType StreamType;

		public uint SequenceNumber;

		public uint BodySize;
	}
}
