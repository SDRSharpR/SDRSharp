namespace SDRSharp.Radio
{
	public struct RdsFrame
	{
		public ushort GroupA;

		public ushort GroupB;

		public ushort GroupC;

		public ushort GroupD;

		public bool Filter;

		public RdsFrame(ushort groupA, ushort groupB, ushort groupC, ushort groupD)
		{
			this.GroupA = groupA;
			this.GroupB = groupB;
			this.GroupC = groupC;
			this.GroupD = groupD;
			this.Filter = false;
		}
	}
}
