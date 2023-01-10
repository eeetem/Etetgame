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
		public static List<Controllable> MyUnits = new List<Controllable>();


		public static bool IsMyTurn()
		{
			return IsPlayer1 == IsPlayer1Turn;
		}

		public static void SetData(GameDataPacket data)
		{
			IsPlayer1Turn = data.IsPlayer1Turn;
			IsPlayer1 = data.IsPlayerOne;
			score = data.Score;
			GameStarted = data.GameStarted;
			if (GameStarted)//skip setup
			{
				StartGame();
				UI.SetUI(UI.GameUi);
			}

			WorldManager.Instance.MakeFovDirty();

		}

		public static void StartGame()
		{
			if(intated)return;
			intated = true;
			
		}

		public static void CountMyUnits()
		{
			foreach (var obj in WorldManager.Instance.WorldObjects.Values)
			{
				if (obj.ControllableComponent != null && obj.ControllableComponent.IsPlayerOneTeam == IsPlayer1)
				{
					MyUnits.Add(obj.ControllableComponent);
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