﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using DefconNull.Rendering.UILayout.GameLayout;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;

namespace DefconNull;

public static partial class GameManager
{

	public static bool IsPlayer1;
	public static bool intated;
	public static bool spectating;
	private static PreGameDataStruct preGameData = new();
	public static Dictionary<string,string> MapList = new Dictionary<string, string>();
	public static Dictionary<string,string> CustomMapList = new Dictionary<string, string>();
	public static PreGameDataStruct PreGameData
	{
		get => preGameData;
		set
		{
			preGameData = value;
			GenerateMapList();
			UI.SetUI(null);
		}
	}



	public static bool IsMyTurn()
	{
		return IsPlayer1 == IsPlayer1Turn;
	}




	private static void GenerateMapList()
	{
		MapList.Clear();
		foreach (var mapPath in PreGameData.MapList)
		{
			MapList.Add(mapPath.Split("/").Last().Split(".").First(),mapPath);
		}
		CustomMapList.Clear();
		foreach (var mapPath in PreGameData.CustomMapList)
		{
			CustomMapList.Add(mapPath.Split("/").Last().Split(".").First(),mapPath);
		}
			
	}

	public static void StartGame()
	{
		if(intated)return;
		intated = true;
		TimeTillNextTurn = PreGameData.TurnTime*1000;
		WorldManager.Instance.MakeFovDirty();

		
	}


	public static void EndTurn()
	{
		if (IsPlayer1 != IsPlayer1Turn) return;

		foreach (var unit in GetTeamUnits(IsPlayer1))
		{
			if (unit.MovePoints > 0)
			{
				UI.OptionMessage("Are you sure?", "You have units with unspent move points\nTAB to cycle units with unspent points", "no", (a,b)=> {  }, "yes", (a, b) =>
				{
						
					NetworkingManager.EndTurn();
				});
				return;
			}
		}

		NetworkingManager.EndTurn();
	
	}

	public static void ResetGame()
	{
		if (intated)
		{
			Console.WriteLine("Resetting Game");
		}
		intated = false;
		spectating = false;
		WorldManager.Instance.WipeGrid();
	}


	public static void SetData(GameStateData data)
	{
		IsPlayer1Turn = data.IsPlayer1Turn;
		if (data.IsPlayerOne == null)
		{
			spectating = true;
		}
		else
		{
			IsPlayer1 = data.IsPlayerOne.Value;
		}
		Console.WriteLine("IsPlayer1: " + IsPlayer1);
		Console.WriteLine("IsPlayer1Turn: " + IsPlayer1Turn);
			
		score = data.Score;
		GameState = data.GameState;

		Audio.OnGameStateChange(GameState);
		switch (GameState)
		{
			case GameState.Lobby:
				UI.SetUI(new PreGameLobbyLayout());
				break;
			case GameState.Setup:
				if (spectating)
				{
					UI.SetUI(new PreGameLobbyLayout());
					break;//todo specating
				}

				Task.Run(delegate
				{
					var mySpawnPoints = IsPlayer1 ? T1SpawnPoints : T2SpawnPoints;
					UI.SetUI(new SquadCompBuilderLayout());
					do
					{
						mySpawnPoints = IsPlayer1 ? T1SpawnPoints : T2SpawnPoints;
						Thread.Sleep(1000);

					} while (mySpawnPoints.Count == 0);
					//UI.SetUI(new SquadCompBuilderLayout());

				});

				break;
			case GameState.Playing:
				StartGame();
				Task.Run(delegate
				{
					UI.SetUI(new GameLayout());
				});
				break;
		}
			

	}

	static Process? localServerProcess = null;
	public static void StartLocalServer()
	{
		Console.WriteLine("Starting local server:");
		string name = "LocalServer";
		string pass = "";
		int port = 52233;
		Console.WriteLine("Port: " + port); //ddos or spam protection is needed
		if(localServerProcess != null)
			localServerProcess.Kill();
		localServerProcess = new Process();
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			Console.WriteLine("Filename: ./Server");
			localServerProcess.StartInfo.FileName = "./Server";
		}
		else
		{
			Console.WriteLine("Filename: ./Server.exe");
			localServerProcess.StartInfo.FileName = "./Server.exe";
		}
				
		List<string> args = new List<string>();
		args.Add(port.ToString());
		args.Add("true");
		args.Add(pass);
		localServerProcess.StartInfo.Arguments = string.Join(" ", args);
		localServerProcess.ErrorDataReceived += (a, b) => { Console.WriteLine("ERROR - Server(" + port + "):" + b.Data?.ToString()); };
		localServerProcess.Exited += (a, b) => { Console.WriteLine("Server(" + port + ") Exited"); };

		Console.WriteLine("starting...");

		localServerProcess.Start();
		Console.WriteLine("process started with id: "+localServerProcess.Id);
		
		Thread.Sleep(1000);
		bool res = NetworkingManager.Connect("localhost:" + port,"Player");
		if (!res)
			UI.ShowMessage("Failed to connect to local server", "Failed to connect to local server");
	}

	public static void UpdateUnitPositions(Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> recievedUnitPositions, bool fullUpdate)
	{

		List<int> justCreated = new List<int>();

		Log.Message("UNITS","updating unit positions");
		//remove all units that we are aware of that we shouldnt be (if this is a full update)
		var allUnits = GetAllUnits();
		if (fullUpdate)
		{
			foreach (var u in allUnits)
			{
				if (!recievedUnitPositions.ContainsKey(u.WorldObject.ID))
				{
					Log.Message("UNITS","deleting non-existant unit: "+u.WorldObject.ID);
					WorldObjectManager.DeleteWorldObject.Make(u.WorldObject.ID).GenerateTask().RunTaskSynchronously(); //queue up deletion
				}
			}
		}
		//create new units that we didnt know of before
		foreach (var u in recievedUnitPositions)
		{
			var obj = WorldObjectManager.GetObject(u.Key);
			if (obj == null)
			{
				Log.Message("UNITS","creating new unit: "+u.Key);
				WorldObjectManager.MakeWorldObject.Make(u.Value.Item2,u.Value.Item1).GenerateTask().RunTaskSynchronously();
				justCreated.Add(u.Key);
			}
		}
		
		//assign teams to new units
		foreach (var u in recievedUnitPositions)
		{
			if (u.Value.Item2.UnitData!.Value.Team1)
			{
				if (!T1Units.Contains(u.Key))
				{
					T1Units.Add(u.Key);
					Log.Message("UNITS","adding unit to team 1: "+u.Key);
				}
			}
			else
			{
				if (!T2Units.Contains(u.Key))
				{
					T2Units.Add(u.Key);
					Log.Message("UNITS","adding unit to team 2: "+u.Key);
				}
			}	
		}
		
		//update existing units
		foreach (var u in new List<int>(T1Units))
		{
			if (recievedUnitPositions.TryGetValue(u, out var data))
			{
				if (!data.Item2.UnitData!.Value.Team1)//this unit is for other team
				{
					throw new Exception("Unit on wrong team");
				}	
				
				//move units to known positions
				
				var obj = WorldObjectManager.GetObject(u);
				if(obj!.IsVisible() && obj.GetData().Equals(data.Item2) && !justCreated.Contains(u)) continue;//ignore units that are visible since they are fully updated with sequence actions
				if (obj.TileLocation.Position != data.Item1 && WorldManager.Instance.GetTileAtGrid(data.Item1).UnitAtLocation != null)
				{
					WorldObjectManager.DeleteWorldObject.Make(WorldManager.Instance.GetTileAtGrid(data.Item1).UnitAtLocation!.WorldObject.ID).GenerateTask().RunTaskSynchronously();
				}
				Log.Message("UNITS","moving unit to known position and loading data: "+u + " " + data.Item1);
				obj!.SetData(data.Item2);
				obj.UnitComponent!.MoveTo(data.Item1);
			}
		}
		foreach (var u in new List<int>(T2Units))
		{
			if (recievedUnitPositions.TryGetValue(u, out var data))
			{
				if (data.Item2.UnitData!.Value.Team1)//this unit is for other team
				{
					throw new Exception("Unit on wrong team");
				}	
				//move units to known positions
				
				var obj = WorldObjectManager.GetObject(u);
				if(obj!.IsVisible() && obj.GetData().Equals(data.Item2) && !justCreated.Contains(u)) continue;//ignore units that are visible since they are fully updated with sequence actions
				if (obj.TileLocation.Position != data.Item1 && WorldManager.Instance.GetTileAtGrid(data.Item1).UnitAtLocation != null)
				{
					WorldObjectManager.DeleteWorldObject.Make(WorldManager.Instance.GetTileAtGrid(data.Item1).UnitAtLocation!.WorldObject.ID).GenerateTask().RunTaskSynchronously();
				}
				Log.Message("UNITS","moving unit to known position and loading data: "+u + " " + data.Item1);
				obj!.SetData(data.Item2);
				obj.UnitComponent!.MoveTo(data.Item1);
			}
		}
		
		
		if(GameLayout.SelectedUnit== null) GameLayout.SelectUnit(null);
	
	}
}