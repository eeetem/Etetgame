﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using DefconNull.World;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

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
		Audio.PlayCombat();
		WorldManager.Instance.MakeFovDirty();
		UI.SetUI(new GameLayout());
	}


	public static void EndTurn()
	{
		if (IsPlayer1 != IsPlayer1Turn) return;

		foreach (var unit in GameLayout.MyUnits)
		{
			if (unit.MovePoints > 0)
			{
				UI.OptionMessage("Are you sure?", "You have units with unspent move points", "no", (a,b)=> {  }, "yes", (a, b) =>
				{
						
					NetworkingManager.EndTurn();
					Action.SetActiveAction(null);

				});
				return;
			}
		}

		NetworkingManager.EndTurn();
		Action.SetActiveAction(null);
	
	}

	public static void ResetGame()
	{
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

			
		score = data.Score;
		GameState = data.GameState;

			
		switch (GameState)
		{
			case GameState.Lobby:
				Audio.PlayMenu();
				UI.SetUI(new PreGameLobbyLayout());
				break;
			case GameState.Setup:
				if (spectating)
				{
					Audio.PlayMenu();
					UI.SetUI(new PreGameLobbyLayout());
					break;//todo specating
				}

				Task.Run(delegate
				{
					var mySpawnPoints = IsPlayer1 ? T1SpawnPoints : T2SpawnPoints;
					do
					{
						mySpawnPoints = IsPlayer1 ? T1SpawnPoints : T2SpawnPoints;
						Thread.Sleep(1000);

					} while (mySpawnPoints.Count == 0);
					UI.SetUI(new SquadCompBuilderLayout());

				});
		
				break;
			case GameState.Playing:
				StartGame();
				break;
		}
			

	}
	
	public static void StartLocalServer()
	{
		Console.WriteLine("Starting local server:");
		string name = "LocalServer";
		string pass = "";
		int port = 52233;
		Console.WriteLine("Port: " + port); //ddos or spam protection is needed
		var process = new Process();
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			Console.WriteLine("Filename: ./Server");
			process.StartInfo.FileName = "./Server";
		}
		else
		{
			Console.WriteLine("Filename: ./Server.exe");
			process.StartInfo.FileName = "./Server.exe";
		}
				
		List<string> args = new List<string>();
		args.Add(port.ToString());
		args.Add(pass);
		process.StartInfo.Arguments = string.Join(" ", args);
		process.ErrorDataReceived += (a, b) => { Console.WriteLine("ERROR - Server(" + port + "):" + b.Data?.ToString()); };
		process.Exited += (a, b) => { Console.WriteLine("Server(" + port + ") Exited"); };

		Console.WriteLine("starting...");
		try
		{
			process.Start();
			Console.WriteLine("process started with id: "+process.Id);
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			LobbyData lobbyData = new LobbyData(name, port);
			if (pass != "")
			{
				lobbyData.HasPassword = true;
			}

			Thread.Sleep(1000);
			NetworkingManager.Connect("localhost:" + port,"Player");

		}catch(Exception e)
		{
			Console.WriteLine(e);
		}
	}

}