using System.Diagnostics;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Action = DefconNull.WorldObjects.Units.Actions.Action;

namespace DefconNull; // Note: actual namespace depends on the project name.

public static class Program
{
	
    static int tickrate = 16;
    static float MSperTick = 1000 / tickrate;
    static Stopwatch stopWatch = new Stopwatch();
		
    static void Main(string[] args)
    { 
	
	    AppDomain currentDomain = default(AppDomain);
	    currentDomain = AppDomain.CurrentDomain;
	    currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
		
        Console.WriteLine("Hello World!");
        string port = "52233";
        bool allowSinglePlayer = true;
        if (args.Length > 1)
        {
            port = args[0];
            allowSinglePlayer = args[1] == "true";
        }
        Log.Init();
        SequenceAction.InitialisePools();
        PrefabManager.MakePrefabs();
        Action.Init();
        Utility.Init();
        WorldManager.Instance.Init();
        //AI.AI.Init();
        //	Console.WriteLine("Enter Port:");
        //string port = Console.ReadLine();
			



        NetworkingManager.Start(ushort.Parse(port),allowSinglePlayer);
        Console.WriteLine("informing masterserver");
        InformMasterServer();
			
        UpdateLoop();

			
    }
    static void UpdateLoop()
    {

        while (true)
        {
            stopWatch.Restart();

            NetworkingManager.Update();
            GameManager.Update(MSperTick);
            WorldManager.Instance.Update(MSperTick);
            SequenceManager.Update();
				

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            if (ts.Milliseconds >= MSperTick)
            {
                //	Console.WriteLine("WARNING: SERVER CAN'T KEEP UP WITH TICK");
                //	Console.WriteLine(ts.Milliseconds);
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
        msg += "[PLAYERCOUNT]" +((GameManager.Player1 != null ? 1 : 0) + (GameManager.Player2 != null ? 1 : 0)) + "[/PLAYERCOUNT]";
        msg += "[SPECTATORS]" +GameManager.Spectators.Count + "[/SPECTATORS]";
        msg += "[STATE]" +GameManager.GameState + "[/STATE]";
        msg += "[MAP]" +WorldManager.Instance.CurrentMap.Name + "[/MAP]";
        Console.WriteLine(msg);

    }
    private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {

	    Log.Crash(e.ExceptionObject);
    }
}