using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Pathfinding;

namespace MultiplayerXeno // Note: actual namespace depends on the project name.
{
	public static class Program
	{
	
		static int tickrate = 16;
		static float MSperTick = 1000 / tickrate;
		static Stopwatch stopWatch = new Stopwatch();
		
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			PrefabManager.MakePrefabs();
			WorldManager.Instance.LoadData(File.ReadAllBytes("map.mapdata"));
			PathFinding.GenerateNodes();
			
			Networking.Start();
			
			UpdateLoop();
			
		}
		static void UpdateLoop()
		{

			while (true)
			{
				stopWatch.Restart();


				WorldManager.Instance.Update(MSperTick);
		

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