namespace SDRSharp.Radio
{
	public class RdsDetectorBank
	{
		private readonly RdsDumpGroups _dumpGroups;

		private readonly SyndromeDetector _detector;

		public string RadioText
		{
			get
			{
				return this._dumpGroups.RadioText;
			}
		}

		public string ProgramService
		{
			get
			{
				return this._dumpGroups.ProgramService;
			}
		}

		public ushort PICode
		{
			get
			{
				return this._dumpGroups.PICode;
			}
		}

		public bool UseFEC
		{
			get
			{
				return this._detector.UseFEC;
			}
			set
			{
				this._detector.UseFEC = value;
			}
		}

		public event RdsFrameAvailableDelegate FrameAvailable;

		public RdsDetectorBank()
		{
			this._dumpGroups = new RdsDumpGroups();
			this._detector = new SyndromeDetector(this._dumpGroups);
			this._detector.FrameAvailable += this.FrameAvailableHandler;
		}

		public void Process(bool b)
		{
			this._detector.Clock(b);
		}

		public void Reset()
		{
			this._dumpGroups.Reset();
		}

		private void FrameAvailableHandler(ref RdsFrame frame)
		{
			RdsFrameAvailableDelegate frameAvailable = this.FrameAvailable;
			if (frameAvailable != null)
			{
				frameAvailable(ref frame);
			}
		}
	}
}
