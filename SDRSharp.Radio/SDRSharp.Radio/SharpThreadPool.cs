using System;
using System.Collections.Generic;
using System.Threading;

namespace SDRSharp.Radio
{
	public class SharpThreadPool
	{
		private struct WorkItem
		{
			private readonly WaitCallback _callback;

			private readonly object _parameter;

			public WorkItem(WaitCallback callback, object parameter)
			{
				this._callback = callback;
				this._parameter = parameter;
			}

			public void Invoke()
			{
				this._callback(this._parameter);
			}
		}

		private readonly Queue<WorkItem> _jobQueue = new Queue<WorkItem>();

		private readonly Thread[] _workerThreads;

		private int _threadsWaiting;

		private bool _terminated;

		public SharpThreadPool()
			: this(Environment.ProcessorCount)
		{
		}

		public SharpThreadPool(int threadCount)
		{
			this._workerThreads = new Thread[threadCount];
			for (int i = 0; i < this._workerThreads.Length; i++)
			{
				this._workerThreads[i] = new Thread(this.DispatchLoop);
				this._workerThreads[i].Priority = ThreadPriority.Highest;
				this._workerThreads[i].Start();
			}
		}

		public void QueueUserWorkItem(WaitCallback callback)
		{
			this.QueueUserWorkItem(callback, null);
		}

		public void QueueUserWorkItem(WaitCallback callback, object parameter)
		{
			WorkItem item = new WorkItem(callback, parameter);
			lock (this._jobQueue)
			{
				this._jobQueue.Enqueue(item);
				if (this._threadsWaiting > 0)
				{
					Monitor.Pulse(this._jobQueue);
				}
			}
		}

		private void DispatchLoop()
		{
			while (true)
			{
				WorkItem workItem;
				lock (this._jobQueue)
				{
					if (!this._terminated)
					{
						while (this._jobQueue.Count == 0)
						{
							this._threadsWaiting++;
							try
							{
								Monitor.Wait(this._jobQueue);
							}
							finally
							{
								this._threadsWaiting--;
							}
							if (this._terminated)
							{
								return;
							}
						}
						workItem = this._jobQueue.Dequeue();
						goto end_IL_0009;
					}
					return;
					end_IL_0009:;
				}
				workItem.Invoke();
			}
		}

		public void Dispose()
		{
			this._terminated = true;
			lock (this._jobQueue)
			{
				Monitor.PulseAll(this._jobQueue);
			}
			for (int i = 0; i < this._workerThreads.Length; i++)
			{
				this._workerThreads[i].Join();
			}
		}
	}
}
