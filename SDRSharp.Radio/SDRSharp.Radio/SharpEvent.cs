using System;
using System.Threading;

namespace SDRSharp.Radio
{
	public sealed class SharpEvent
	{
		private bool _state;

		private bool _waiting;

		public SharpEvent(bool initialState)
		{
			this._state = initialState;
		}

		public SharpEvent()
			: this(false)
		{
		}

		~SharpEvent()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			this.Set();
			GC.SuppressFinalize(this);
		}

		public void Set()
		{
			lock (this)
			{
				this._state = true;
				if (this._waiting)
				{
					Monitor.Pulse(this);
				}
			}
		}

		public void WaitOne()
		{
			lock (this)
			{
				if (!this._state)
				{
					this._waiting = true;
					try
					{
						Monitor.Wait(this);
					}
					finally
					{
						this._waiting = false;
					}
				}
				this._state = false;
			}
		}

		public void Reset()
		{
			lock (this)
			{
				this._state = false;
			}
		}
	}
}
