using System;
using System.Threading;
using CommonData;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;



		public static bool gameStarted = false;
		public static void StatGame()
		{

			if (Player1 == null || Player2 == null)
			{
				return;
			}
			
			if (gameStarted)
			{
				return;
			}

			gameStarted = true;

			
			//not a fan of this, should probably be made a single function
			ControllableData cdata = new ControllableData(true);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(10, 5),controllableData:cdata);


			cdata = new ControllableData(false);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(15, 5),controllableData:cdata);

		}

		public static void SendData()
		{
			GameDataPacket packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = true
			};
			

			//packet.
			Player1?.Connection.Send(packet);
			
			packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = false
			};
			Player2?.Connection.Send(packet);
		}
	}
}