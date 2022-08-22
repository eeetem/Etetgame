using System;
using CommonData;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1;


		public static void SetData(GameDataPacket data)
		{

			IsPlayer1Turn = data.IsPlayer1Turn;
			IsPlayer1 = data.IsPlayerOne;


		}

		public static void EndTurn()
		{
			if (IsPlayer1 != IsPlayer1Turn) return;

			GameActionPacket packet = new GameActionPacket();
			Networking.serverConnection.Send(packet);

		}


	}
}