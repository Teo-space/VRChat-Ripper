using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamLib
{
	public static class Debug
	{

		private static readonly object console = new object();
		public static void LogLine(string message, ConsoleColor color = ConsoleColor.White,
			bool messageIsUnsafe = false, bool beep = false, int freq = 800, int duration = 200)
		{
			if (messageIsUnsafe)
				message = ToolBox.MakeConsoleSafe(message);

			lock (console)
			{
				if (beep) System.Console.Beep();
				System.Console.ForegroundColor = color;
				System.Console.WriteLine(message);
				System.Console.ResetColor();
			}
		}
		public static void LogText(string message, ConsoleColor color = ConsoleColor.White,
			bool messageIsUnsafe = false, bool beep = false, int freq = 800, int duration = 200)
		{
			if (messageIsUnsafe)
				message = ToolBox.MakeConsoleSafe(message);

			lock (console)
			{
				if (beep) System.Console.Beep();
				System.Console.ForegroundColor = color;
				System.Console.Write(message);
				System.Console.ResetColor();
			}
		}
	}
}
