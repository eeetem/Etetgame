using System;
using System.Threading;
using CommonData;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;

		
	

		public static void StatGame()
		{
			if (Player1 == null || Player2 == null)
			{
				return;
			}

			
			//not a fan of this, should probably be made a single function
			ControllableData cdata = new ControllableData(true);
			WorldManager.MakeWorldObjectPublically("human", new Vector2Int(1, 1),controllableData:cdata);


			cdata = new ControllableData(false);
			WorldManager.MakeWorldObjectPublically("human", new Vector2Int(5, 5),controllableData:cdata);

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