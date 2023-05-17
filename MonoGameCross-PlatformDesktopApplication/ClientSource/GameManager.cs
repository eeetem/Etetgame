﻿using System;
using System.Collections.Generic;
using System.Linq;
using MultiplayerXeno;
using MultiplayerXeno.UILayouts;

namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1;
		public static bool intated = false;
		public static bool spectating = false;
		public static List<Controllable> _myUnits = new List<Controllable>();
		private static PreGameDataPacket preGameData = new();
		public static Dictionary<string,string> MapList = new Dictionary<string, string>();
		public static Dictionary<string,string> CustomMapList = new Dictionary<string, string>();
		public static PreGameDataPacket PreGameData
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

		public static void SetData(GameDataPacket data)
		{
			IsPlayer1Turn = data.IsPlayer1Turn;
			if (data.IsPlayerOne == null)
			{
				spectating = true;
			}
			else
			{
				IsPlayer1 = (bool)data.IsPlayerOne;
			}
			Console.WriteLine("IsPlayer1: " + IsPlayer1);

			
			score = data.Score;
			GameState = data.GameState;

			try
			{
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
						UI.SetUI(new SquadCompBuilderLayout());
						break;
					case GameState.Playing:
						StartGame();
						break;
				}
			}
			catch(Exception e){
				Console.WriteLine(e);
			}

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

			GameActionPacket packet = new GameActionPacket(-1,null,ActionType.EndTurn);
			Networking.serverConnection.Send(packet);
			Action.SetActiveAction(null);

		}

		public static void ResetGame()
		{
			intated = false;
			WorldManager.Instance.WipeGrid();
		}


	}
}