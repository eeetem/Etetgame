using System;
using Packets;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;

		
		public static void NextTurn()
		{
			IsPlayer1Turn = !IsPlayer1Turn;

			SendData();
		}

		public static void StatGame()
		{
			if (Player1 == null || Player2 == null)
			{
				return;
			}

		
		}

		public static void SendData()
		{
			GameDataPacket packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				
			};
			Console.WriteLine("turn: "+IsPlayer1Turn);

			packet.IsPlayerOne = true;
			Player1?.Connection.Send(packet);
			packet.IsPlayerOne = false;
			Player2?.Connection.Send(packet);
		}
	}
}