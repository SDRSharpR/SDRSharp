namespace SDRSharp.Radio
{
	public class SyndromeDetector
	{
		protected enum BlockSequence
		{
			GotA,
			GotB,
			GotC,
			GotD,
			WaitBitSync,
			GotBitSync
		}

		private const int MaxCorrectableBits = 5;

		private const int CheckwordBitsCount = 10;

		private readonly RdsDumpGroups _dumpGroups;

		private readonly ushort[] _blocks = new ushort[4];

		private bool _useFec = Utils.GetBooleanSetting("RdsUseFec");

		private BlockSequence _sequence = BlockSequence.WaitBitSync;

		private ushort _syndrome;

		private uint _raw;

		private RdsFrame _frame;

		private int _count;

		public bool UseFEC
		{
			get
			{
				return this._useFec;
			}
			set
			{
				this._useFec = value;
			}
		}

		public event RdsFrameAvailableDelegate FrameAvailable;

		public SyndromeDetector(RdsDumpGroups dumpGroups)
		{
			this._dumpGroups = dumpGroups;
		}

		public void Clock(bool b)
		{
			this._raw <<= 1;
			this._raw |= (uint)(b ? 1 : 0);
			this._count++;
			if (this._sequence == BlockSequence.WaitBitSync)
			{
				this._syndrome = SyndromeDetector.BuildSyndrome(this._raw);
				this._syndrome ^= 984;
				this._sequence = ((this._syndrome != 0) ? BlockSequence.WaitBitSync : BlockSequence.GotA);
				this._blocks[0] = (ushort)(this._raw >> 10 & 0xFFFF);
				this._count = 0;
			}
			if (this._count == 26)
			{
				this.ProcessSyndrome();
				if (this._sequence == BlockSequence.GotD)
				{
					this._frame.GroupA = this._blocks[0];
					this._frame.GroupB = this._blocks[1];
					this._frame.GroupC = this._blocks[2];
					this._frame.GroupD = this._blocks[3];
					this._frame.Filter = false;
					RdsFrameAvailableDelegate frameAvailable = this.FrameAvailable;
					if (frameAvailable != null)
					{
						frameAvailable(ref this._frame);
					}
					if (!this._frame.Filter)
					{
						this._dumpGroups.AnalyseFrames(ref this._frame);
					}
					this._sequence = BlockSequence.GotBitSync;
				}
				this._count = 0;
			}
		}

		private void ProcessSyndrome()
		{
			this._syndrome = SyndromeDetector.BuildSyndrome(this._raw);
			switch (this._sequence)
			{
			case BlockSequence.GotBitSync:
				this._syndrome ^= 984;
				this._sequence = BlockSequence.GotA;
				break;
			case BlockSequence.GotA:
				this._syndrome ^= 980;
				this._sequence = BlockSequence.GotB;
				break;
			case BlockSequence.GotB:
				this._syndrome ^= (ushort)(((this._blocks[1] & 0x800) == 0) ? 604 : 972);
				this._sequence = BlockSequence.GotC;
				break;
			case BlockSequence.GotC:
				this._syndrome ^= 600;
				this._sequence = BlockSequence.GotD;
				break;
			}
			int sequence = (int)this._sequence;
			if (this._syndrome != 0)
			{
				if (this._useFec)
				{
					int num = this.ApplyFEC();
					if (this._syndrome != 0 || num > 5)
					{
						this._sequence = BlockSequence.WaitBitSync;
					}
					else
					{
						this._blocks[sequence] = (ushort)(this._raw & 0xFFFF);
					}
				}
				else
				{
					this._sequence = BlockSequence.WaitBitSync;
				}
			}
			else
			{
				this._blocks[sequence] = (ushort)(this._raw >> 10 & 0xFFFF);
			}
		}

		private int ApplyFEC()
		{
			uint num = 33554432u;
			int num2 = 0;
			for (int i = 0; i < 16; i++)
			{
				bool flag = (this._syndrome & 0x200) == 512;
				bool flag2 = (this._syndrome & 0x20) == 0;
				this._raw ^= ((flag & flag2) ? num : 0);
				this._syndrome <<= 1;
				this._syndrome ^= (ushort)((flag && !flag2) ? 1465 : 0);
				num2 += ((flag & flag2) ? 1 : 0);
				num >>= 1;
			}
			this._syndrome &= 1023;
			return num2;
		}

		private static ushort BuildSyndrome(uint raw)
		{
			ushort[] array = new ushort[16]
			{
				732,
				366,
				183,
				647,
				927,
				787,
				853,
				886,
				443,
				513,
				988,
				494,
				247,
				679,
				911,
				795
			};
			uint num = raw & 0x3FFFFFF;
			ushort num2 = (ushort)(num >> 16);
			for (int i = 0; i < 16; i++)
			{
				num2 = (ushort)(num2 ^ (((num & 0x8000) == 32768) ? array[i] : 0));
				num <<= 1;
			}
			return num2;
		}
	}
}
