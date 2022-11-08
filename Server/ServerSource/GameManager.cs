using System;
using System.Threading;
using CommonData;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;



		public static bool GameStarted = false;
		public static void StatGame()
		{

			if (Player1 == null || Player2 == null)
			{
				return;
			}
			
			if (GameStarted)
			{
				return;
			}

			GameStarted = true;

			
			//not a fan of this, should probably be made a single function
			ControllableData cdata = new ControllableData(true);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(10, 15),controllableData:cdata);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(11, 15),controllableData:cdata);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(12, 15),controllableData:cdata);


			cdata = new ControllableData(false);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(4, 4),controllableData:cdata);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(5, 4),controllableData:cdata);
			WorldManager.Instance.MakeWorldObject("Human", new Vector2Int(6, 4),controllableData:cdata);

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