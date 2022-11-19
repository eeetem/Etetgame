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
		public static readonly List<WorldObject> T1SpawnPoints = new List<WorldObject>();
		public static readonly List<WorldObject> T2SpawnPoints = new List<WorldObject>();
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
			foreach (var spawn in T1SpawnPoints)
			{
				WorldManager.Instance.MakeWorldObject("Human", spawn.TileLocation.Position,controllableData:cdata);
			}

			cdata = new ControllableData(false);
			foreach (var spawn in T2SpawnPoints)
			{
				WorldManager.Instance.MakeWorldObject("Human", spawn.TileLocation.Position,controllableData:cdata);
			}
			NextTurn();

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