using SDRSharp.Radio;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SDRSharp
{
	public static class Program
	{
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += Program.CurrentDomain_UnhandledException;
			if (Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				Process currentProcess = Process.GetCurrentProcess();
				currentProcess.PriorityBoostEnabled = true;
				currentProcess.PriorityClass = (ProcessPriorityClass)Utils.GetIntSetting("processPriority", 256);
				Utils.TimeBeginPeriod(1u);
			}
			DSPThreadPool.Initialize();
			Control.CheckForIllegalCrossThreadCalls = false;
			Application.EnableVisualStyles();
			Application.Run(new MainForm());
			if (Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				Utils.TimeEndPeriod(1u);
			}
			DSPThreadPool.Terminate();
			Application.Exit();
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Exception ex = (Exception)e.ExceptionObject;
			StackTrace stackTrace = new StackTrace(ex);
			StringBuilder stringBuilder = new StringBuilder();
			StackFrame[] frames = stackTrace.GetFrames();
			foreach (StackFrame stackFrame in frames)
			{
				stringBuilder.AppendLine("at " + stackFrame.GetMethod().Module.Name + "." + stackFrame.GetMethod().ReflectedType.Name + "." + stackFrame.GetMethod().Name + "  (IL offset: 0x" + stackFrame.GetILOffset().ToString("x") + ")");
			}
			File.WriteAllText("crash.txt", ex.Message + Environment.NewLine + stringBuilder);
		}
	}
}
