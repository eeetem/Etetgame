using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MultiplayerXeno;

using Network;
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
			WorldManager.MakeWorld();
			UpdateLoop();
			
		}
		static void UpdateLoop()
		{
			while (true)
			{
				stopWatch.Restart();


				
				WorldManager.World.Update(_gameTime);
				
				Console.WriteLine(_gameTime.ElapsedGameTime);

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
				

			}
		}
	}
};