using System;
using System.IO;

namespace MultiplayerXeno
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{
			
			AppDomain currentDomain = default(AppDomain);
			currentDomain = AppDomain.CurrentDomain;
			// Handler for unhandled exceptions.
			currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;

			using (var game = new Game1())
				game.Run();
			
			
			
			
		}
		
		private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
		{

			DateTime date = DateTime.Now;
			
			File.WriteAllText("CrashDump"+date.ToFileTime()+".txt", e.ExceptionObject.ToString());
	
		}
	}
}