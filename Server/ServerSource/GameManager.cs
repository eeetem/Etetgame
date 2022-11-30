using System;
using System.Threading;
using CommonData;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;



		public static readonly List<WorldObject> T1SpawnPoints = new List<WorldObject>();
		public static readonly List<WorldObject> T2SpawnPoints = new List<WorldObject>();
		public static void StatGame()
		{

			if (Player1 == null || Player2 == null)
			{
				return;
			}
			
	

		

		}

		public static void SpawnCharacters()
		{
			if (GameStarted)
			{
				return;
			}

			GameStarted = true;
				
			//not a fan of this, should probably be made a single function
			ControllableData cdata = new ControllableData(true);
			
			int i = 0;
			foreach (var spawn in T1SpawnPoints)
			{
				
				if (i < Player1.StartData.Soldiers)
				{
					WorldManager.Instance.MakeWorldObject("Soldier", spawn.TileLocation.Position, controllableData: cdata);
				}
				else
				{
					WorldManager.Instance.MakeWorldObject("Scout", spawn.TileLocation.Position, controllableData: cdata);
				}

				i++;
			}

			cdata = new ControllableData(false);
			i = 0;
			foreach (var spawn in T2SpawnPoints)
			{
				if (i < Player2.StartData.Soldiers)
				{
					WorldManager.Instance.MakeWorldObject("Soldier", spawn.TileLocation.Position, controllableData: cdata);
				}
				else
				{
					WorldManager.Instance.MakeWorldObject("Scout", spawn.TileLocation.Position, controllableData: cdata);
				}

				i++;
			}
		}

		public static void SendData()
		{
			GameDataPacket packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = true,
				Score = score,
				GameStarted = GameStarted
			};
			

			//packet.
			Player1?.Connection.Send(packet);
			
			packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = false,
				Score = score,
				GameStarted = GameStarted
			};
			Player2?.Connection.Send(packet);
		}
	}
}