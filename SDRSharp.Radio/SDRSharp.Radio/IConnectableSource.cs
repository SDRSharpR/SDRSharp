namespace SDRSharp.Radio
{
	public interface IConnectableSource
	{
		bool Connected
		{
			get;
		}

		void Connect();

		void Disconnect();
	}
}
