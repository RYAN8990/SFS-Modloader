using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using SFS.IO;
using System.Collections.Generic;

namespace ModLoader
{

    public class ModConsole
    {
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("Kernel32.dll")]
		private static extern bool AllocConsole();

		// in this files all logs will be saved
		private FilePath logFile;

		private const int SW_HIDE = 0;

		private const int SW_SHOW = 5;

		// this indicate if the console is visible or not
		private bool visible = false;

		// the log queue 
		private Queue<Log> logs = new Queue<Log>();


		public ModConsole()
		{
			ModConsole.AllocConsole();
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
			{
				AutoFlush = true
			});

			this.visible = true;
			DateTime current = new DateTime();
			this.logFile = FileLocations.BaseFolder.Extend("logs").CreateFolder().ExtendToFile(current.Year + "-" + current.Month + "-" + current.Day+".txt");
		}

		/// <summary>
		/// This functions cahnge visible status of console
		/// </summary>
		public void toggleConsole()
		{
			if (this.visible)
			{
				ModConsole.ShowWindow(ModConsole.GetConsoleWindow(), SW_HIDE);
			}
			else
			{
				ModConsole.ShowWindow(ModConsole.GetConsoleWindow(), SW_SHOW);
			}
			this.visible = !this.visible;
		}

		/// <summary>
		/// If you need log a Esception in your mod
		/// </summary>
		/// <param name="e">Escpetion object</param>
		public void logError(Exception e)
		{
			StackTrace stackTrace = new StackTrace(e, true);
			StackFrame frame = stackTrace.GetFrame(0);
			int fileColumnNumber = frame.GetFileColumnNumber();
			int fileLineNumber = frame.GetFileLineNumber();
			string fileName = frame.GetFileName();
			this.log("##[ERROR]##", "ErrorReporter", LogType.Error);
			this.log(e.Message , "ErrorReporter", LogType.Error);
			this.log(e.StackTrace, "ErrorReporter", LogType.Error);
			this.log(fileLineNumber+":"+ fileColumnNumber + "@" + fileName, "ErrorReporter", LogType.Error);
			this.log("##[ERROR]##", "ErrorReporter", LogType.Error);
		}

		/// <summary>
		/// Call this method if you need log a text in runtime
		/// </summary>
		/// <param name="message">Message of your log</param>
		/// <param name="tag">Who is logging?</param>
		/// <param name="type">what type of log is this?</param>
		public void log(string message, string tag, LogType type = LogType.Log)
		{
			this.logs.Enqueue(new Log(message, tag, type));
		}

		/// <summary>
		/// The simplest way to log something
		/// </summary>
		/// <param name="msg">Your log message</param>
		public void log(string msg)
		{
			this.log(msg, "Unkwn");
		}

		/// <summary>
		/// Execute this method if you want to proccess log queue. ONLY IS USED FOR MODLOADER, so you don't need call in your mod
		/// </summary>
		public void proccessQueue()
		{
			Queue<Log> obj = this.logs;
			lock (obj)
			{
				while (this.logs.Count > 0)
				{
					Log log = this.logs.Dequeue();
					this.logFile.AppendText(log.getLogMessage() + "\n");
					Console.WriteLine(log.getLogMessage());
				}
			}
		}

		

	}

	/// <summary>
	/// This class is used to handler log queue
	/// </summary>
	class Log
	{

		private string message;
		private string tag;
		private LogType type;

		public Log(string message,string tag, LogType type)
		{
			this.message = message;
			this.tag = tag;
			this.type = type;
		}

		/// <summary>
		/// Get the log messafe whit format
		/// </summary>
		/// <returns>log message</returns>
		public string getLogMessage()
		{
			string[] logText = { "[", this.type.ToString(), "] [", this.tag, "]: ", this.message };
			return string.Concat(logText);
		}
	}
}
