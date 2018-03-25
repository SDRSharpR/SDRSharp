using System.Collections.Generic;

namespace SDRSharp.Radio
{
	public class HookManager
	{
		private readonly List<IRealProcessor> _filteredAudioProcessors = new List<IRealProcessor>();

		private readonly List<IRealProcessor> _demodulatorOutputProcessors = new List<IRealProcessor>();

		private readonly List<IIQProcessor> _rawIQProcessors = new List<IIQProcessor>();

		private readonly List<IIQProcessor> _decimatedAndFilteredIQProcessors = new List<IIQProcessor>();

		private readonly List<IRealProcessor> _fmMPXProcessors = new List<IRealProcessor>();

		private readonly List<IRdsBitStreamProcessor> _rdsBitStreamProcessors = new List<IRdsBitStreamProcessor>();

		public void RegisterStreamHook(object hook, ProcessorType processorType)
		{
			switch (processorType)
			{
			case ProcessorType.RawIQ:
				lock (this._rawIQProcessors)
				{
					this._rawIQProcessors.Add((IIQProcessor)hook);
				}
				break;
			case ProcessorType.DecimatedAndFilteredIQ:
				lock (this._decimatedAndFilteredIQProcessors)
				{
					this._decimatedAndFilteredIQProcessors.Add((IIQProcessor)hook);
				}
				break;
			case ProcessorType.DemodulatorOutput:
				lock (this._demodulatorOutputProcessors)
				{
					this._demodulatorOutputProcessors.Add((IRealProcessor)hook);
				}
				break;
			case ProcessorType.FilteredAudioOutput:
				lock (this._filteredAudioProcessors)
				{
					this._filteredAudioProcessors.Add((IRealProcessor)hook);
				}
				break;
			case ProcessorType.FMMPX:
				lock (this._fmMPXProcessors)
				{
					this._fmMPXProcessors.Add((IRealProcessor)hook);
				}
				break;
			case ProcessorType.RDSBitStream:
				lock (this._rdsBitStreamProcessors)
				{
					this._rdsBitStreamProcessors.Add((IRdsBitStreamProcessor)hook);
				}
				break;
			}
		}

		public void UnregisterStreamHook(object hook)
		{
			if (hook != null)
			{
				if (hook is IIQProcessor)
				{
					IIQProcessor item = (IIQProcessor)hook;
					lock (this._rawIQProcessors)
					{
						this._rawIQProcessors.Remove(item);
					}
					lock (this._decimatedAndFilteredIQProcessors)
					{
						this._decimatedAndFilteredIQProcessors.Remove(item);
					}
				}
				if (hook is IRealProcessor)
				{
					IRealProcessor item2 = (IRealProcessor)hook;
					lock (this._demodulatorOutputProcessors)
					{
						this._demodulatorOutputProcessors.Remove(item2);
					}
					lock (this._filteredAudioProcessors)
					{
						this._filteredAudioProcessors.Remove(item2);
					}
					lock (this._fmMPXProcessors)
					{
						this._fmMPXProcessors.Remove(item2);
					}
				}
				if (hook is IRdsBitStreamProcessor)
				{
					IRdsBitStreamProcessor item3 = (IRdsBitStreamProcessor)hook;
					lock (this._rdsBitStreamProcessors)
					{
						this._rdsBitStreamProcessors.Remove(item3);
					}
				}
			}
		}

		public void SetProcessorSampleRate(ProcessorType processorType, double sampleRate)
		{
			switch (processorType)
			{
			case ProcessorType.RawIQ:
				this.SetSampleRate(this._rawIQProcessors, sampleRate);
				break;
			case ProcessorType.DecimatedAndFilteredIQ:
				this.SetSampleRate(this._decimatedAndFilteredIQProcessors, sampleRate);
				break;
			case ProcessorType.DemodulatorOutput:
				this.SetSampleRate(this._demodulatorOutputProcessors, sampleRate);
				break;
			case ProcessorType.FilteredAudioOutput:
				this.SetSampleRate(this._filteredAudioProcessors, sampleRate);
				break;
			case ProcessorType.FMMPX:
				this.SetSampleRate(this._fmMPXProcessors, sampleRate);
				break;
			}
		}

		public unsafe void ProcessRawIQ(Complex* buffer, int length)
		{
			this.ProcessHooks(this._rawIQProcessors, buffer, length);
		}

		public unsafe void ProcessDecimatedAndFilteredIQ(Complex* buffer, int length)
		{
			this.ProcessHooks(this._decimatedAndFilteredIQProcessors, buffer, length);
		}

		public unsafe void ProcessDemodulatorOutput(float* buffer, int length)
		{
			this.ProcessHooks(this._demodulatorOutputProcessors, buffer, length);
		}

		public unsafe void ProcessFilteredAudioOutput(float* buffer, int length)
		{
			this.ProcessHooks(this._filteredAudioProcessors, buffer, length);
		}

		public unsafe void ProcessFMMPX(float* buffer, int length)
		{
			this.ProcessHooks(this._fmMPXProcessors, buffer, length);
		}

		public void ProcessRdsBitStream(ref RdsFrame frame)
		{
			this.ProcessHooks(this._rdsBitStreamProcessors, ref frame);
		}

		private void SetSampleRate(List<IIQProcessor> processors, double sampleRate)
		{
			lock (processors)
			{
				for (int i = 0; i < processors.Count; i++)
				{
					processors[i].SampleRate = sampleRate;
				}
			}
		}

		private void SetSampleRate(List<IRealProcessor> processors, double sampleRate)
		{
			lock (processors)
			{
				for (int i = 0; i < processors.Count; i++)
				{
					processors[i].SampleRate = sampleRate;
				}
			}
		}

		private unsafe void ProcessHooks(List<IIQProcessor> processors, Complex* buffer, int length)
		{
			lock (processors)
			{
				for (int i = 0; i < processors.Count; i++)
				{
					if (processors[i].Enabled)
					{
						processors[i].Process(buffer, length);
					}
				}
			}
		}

		private unsafe void ProcessHooks(List<IRealProcessor> processors, float* buffer, int length)
		{
			lock (processors)
			{
				for (int i = 0; i < processors.Count; i++)
				{
					if (processors[i].Enabled)
					{
						processors[i].Process(buffer, length);
					}
				}
			}
		}

		private void ProcessHooks(List<IRdsBitStreamProcessor> processors, ref RdsFrame frame)
		{
			lock (processors)
			{
				for (int i = 0; i < processors.Count; i++)
				{
					if (processors[i].Enabled)
					{
						processors[i].Process(ref frame);
					}
				}
			}
		}
	}
}
