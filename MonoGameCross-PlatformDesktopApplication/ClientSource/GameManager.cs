using System;
using System.Collections.Generic;
using System.Linq;
using CommonData;
using MultiplayerXeno.UILayouts;

namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1;
		public static bool intated = false;
		public static bool spectating = false;
		public static List<Controllable> _myUnits = new List<Controllable>();
		private static PreGameDataPacket _preGameData;
		public static Dictionary<string,string> MapList = new Dictionary<string, string>();
		public static Dictionary<string,string> CustomMapList = new Dictionary<string, string>();
		public static PreGameDataPacket PreGameData
		{
			get => _preGameData;
			set
			{
				_preGameData = value;
				GenerateMapList();
				UI.SetUI(null);
			}
		}

		public static List<Controllable> MyUnits
		{
			get {
				if (_myUnits.Count == 0)
				{
					CountMyUnits();
				}

				return _myUnits;
			}
			set { _myUnits = value; }
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
						if (spectating) break;

						UI.SetUI(new GameSetupLayout());
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
			foreach (var mapPath in GameManager.PreGameData.MapList)
			{
				MapList.Add(mapPath.Split("/").Last().Split(".").First(),mapPath);
			}
			CustomMapList.Clear();
			foreach (var mapPath in GameManager.PreGameData.CustomMapList)
			{
				CustomMapList.Add(mapPath.Split("/").Last().Split(".").First(),mapPath);
			}
			
		}

		public static void StartGame()
		{
			if(intated)return;
			intated = true;
			Audio.PlayCombat();
			CountMyUnits();
			WorldManager.Instance.MakeFovDirty();
			UI.SetUI(new GameLayout());
		}

		public static void CountMyUnits()
		{
			_myUnits.Clear();
			foreach (var obj in UI.Controllables)
			{
				if (obj.IsPlayerOneTeam == IsPlayer1)
				{
					_myUnits.Add(obj);
				}
			}

		}

		public static void EndTurn()
		{
			if (IsPlayer1 != IsPlayer1Turn) return;

			GameActionPacket packet = new GameActionPacket(-1,null,ActionType.EndTurn);
			Networking.serverConnection.Send(packet);
			UI.SelectControllable(null);
			Action.SetActiveAction(null);

		}

		public static void ResetGame()
		{
			intated = false;
			WorldManager.Instance.WipeGrid();
			MyUnits.Clear();
			UI.Controllables.Clear();
			WorldEditSystem.enabled = false;
		}


	}
}