using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;

namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1;
		public static bool intated = false;
		public static bool spectating = false;
		public static List<Controllable> _myUnits = new List<Controllable>();
		private static PreGameDataPacket _preGameData;
		public static PreGameDataPacket PreGameData
		{
			get => _preGameData;
			set
			{
				_preGameData = value;
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
		
			
				switch (GameState)
				{
					case GameState.Lobby:
						UI.SetUI(UI.PreGameLobby);
						break;
					case GameState.Setup:
						if (spectating)
						{
							break;
						}

						UI.SetUI(UI.SetupUI);
						break;
					case GameState.Playing:
						StartGame();
						break;
				}
			

		}

		public static void StartGame()
		{
			if(intated)return;
			intated = true;
			CountMyUnits();
			WorldManager.Instance.MakeFovDirty();
			UI.SetUI(UI.GameUi);
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


	}
}