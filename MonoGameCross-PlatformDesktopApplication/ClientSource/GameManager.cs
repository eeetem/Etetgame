﻿using System;
using CommonData;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;

namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1;
		public static bool intated = false;


		public static bool IsMyTurn()
		{
			return IsPlayer1 == IsPlayer1Turn;
		}

		public static void SetData(GameDataPacket data)
		{
			intated = true;
			IsPlayer1Turn = data.IsPlayer1Turn;
			IsPlayer1 = data.IsPlayerOne;
			WorldManager.Instance.CalculateFov();

		}

		public static void EndTurn()
		{
			if (IsPlayer1 != IsPlayer1Turn) return;

			GameActionPacket packet = new GameActionPacket();
			Networking.serverConnection.Send(packet);
			Controllable.Selected = null;
			

		}


	}
}