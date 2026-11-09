using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using nodesCatchNext.Forms;

namespace nodesCatchNext;

internal static class Program
{
	private static Mutex mutex;

	[STAThread]
	private static void Main()
	{
		Application.ThreadException += (s, e) =>
		{
			try
			{
				File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
					$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ThreadException: {e.Exception}\r\n\r\n");
			}
			catch { }
			MessageBox.Show($"程序发生未处理的异常：\n{e.Exception.Message}\n\n详情已记录到 crash.log", "程序异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
		};

		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			try
			{
				File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"),
					$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UnhandledException: {e.ExceptionObject}\r\n\r\n");
			}
			catch { }
		};

		bool createdNew = false;
		try
		{
			mutex = new Mutex(initiallyOwned: true, "Global\\nodesCatchNext_SingleInstance_Mutex_3F8A9D2B", out createdNew);
			if (!createdNew)
			{
				try
				{
					if (mutex.WaitOne(500, false))
					{
						createdNew = true;
					}
				}
				catch (AbandonedMutexException)
				{
					createdNew = true;
				}
			}
		}
		catch
		{
			createdNew = true;
		}

		if (!createdNew)
		{
			MessageBox.Show("nodesCatchNext 已经在运行中！\n\n请检查任务栏或系统托盘。", "程序已运行", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}

		try
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new MainForm());
		}
		finally
		{
			if (mutex != null)
			{
				try { mutex.ReleaseMutex(); } catch { }
				mutex.Dispose();
			}
		}
	}
}
