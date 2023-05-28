using System.Diagnostics;
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
			Action.Init();
			PathFinding.GenerateNodes();
		//	Console.WriteLine("Enter Port:");
			//string port = Console.ReadLine();
			string port = "52233";
			if (args.Length > 0)
			{
				port = args[0];
			}

		
			Networking.Start(Int32.Parse(port));

			InformMasterServer();
			UpdateLoop();
			
		}
		static void UpdateLoop()
		{

			while (true)
			{
				stopWatch.Restart();


				WorldManager.Instance.Update(MSperTick);
				GameManager.Update(MSperTick);

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

		public static void InformMasterServer()
		{
			string msg = "[UPDATE]";
			msg += "[PLAYERCOUNT]" +((GameManager.Player1 != null ? 0 : 1) + (GameManager.Player2 != null ? 0 : 1)) + "[/PLAYERCOUNT]";
			msg += "[SPECTATORS]" +GameManager.Spectators.Count + "[/SPECTATORS]";
			msg += "[STATE]" +GameManager.GameState + "[/STATE]";
			msg += "[MAP]" +WorldManager.Instance.CurrentMap.Name + "[/MAP]";
			Console.WriteLine(msg);

			

		}
	}
};