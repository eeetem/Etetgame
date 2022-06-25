using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno // Note: actual namespace depends on the project name.
{
	public static class Program
	{
	
		static int tickrate = 16;
		static float MSperTick = 1000 / tickrate;
		static Stopwatch stopWatch = new Stopwatch();

		private static readonly GameTime _gameTime = new GameTime();

		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			Networking.Start();
			

			WorldObjectManager.Init();

			
			UpdateLoop();
			
		}
		static void UpdateLoop()
		{
			_gameTime.ElapsedGameTime = new TimeSpan((long)MSperTick);
			while (true)
			{
				stopWatch.Restart();


				WorldObjectManager.Update(_gameTime);
		

				stopWatch.Stop();

				TimeSpan ts = stopWatch.Elapsed;
				if (ts.Milliseconds >= MSperTick)
				{
					Console.WriteLine("WARNING: SERVER CAN'T KEEP UP WITH TICK");
					Console.WriteLine(ts.Milliseconds);
				}
				else
				{
					// Console.WriteLine("update took: "+ts.Milliseconds);
					Thread.Sleep((int)(MSperTick - ts.Milliseconds));
				}

				_gameTime.TotalGameTime += new TimeSpan((long)MSperTick);
			}
		}
	}
};