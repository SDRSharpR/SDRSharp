using System.Threading;

namespace SDRSharp.Radio
{
	public static class DSPThreadPool
	{
		private static SharpThreadPool _threadPool;

		public static void Initialize()
		{
			if (DSPThreadPool._threadPool == null)
			{
				DSPThreadPool._threadPool = new SharpThreadPool();
			}
		}

		public static void Initialize(int threadCount)
		{
			if (DSPThreadPool._threadPool == null)
			{
				DSPThreadPool._threadPool = new SharpThreadPool(threadCount);
			}
		}

		public static void QueueUserWorkItem(WaitCallback callback)
		{
			if (DSPThreadPool._threadPool == null)
			{
				DSPThreadPool._threadPool = new SharpThreadPool();
			}
			DSPThreadPool._threadPool.QueueUserWorkItem(callback);
		}

		public static void QueueUserWorkItem(WaitCallback callback, object parameter)
		{
			if (DSPThreadPool._threadPool == null)
			{
				DSPThreadPool._threadPool = new SharpThreadPool();
			}
			DSPThreadPool._threadPool.QueueUserWorkItem(callback, parameter);
		}

		public static void Terminate()
		{
			if (DSPThreadPool._threadPool != null)
			{
				DSPThreadPool._threadPool.Dispose();
				DSPThreadPool._threadPool = null;
			}
		}
	}
}
