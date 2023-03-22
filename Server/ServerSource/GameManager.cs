using System;
using System.Threading;
using CommonData;
namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;
		public static List<Client> Spectators = new();




		public static readonly List<int> T1Units = new();
		public static readonly List<int> T2Units = new();

		public static void StartSetup()
		{
			if (GameState != GameState.Lobby) return;
			if(Player1==null || Player2==null)return;

			GameState = GameState.Setup;
			
			Networking.SendMapData(Player1.Connection);
			Networking.SendMapData(Player2.Connection);
			SendData();
		}
	

		public static void StartGame()
		{
			if (GameState != GameState.Setup)
			{
				return;
			}

			GameState = GameState.Playing;
			
				
			//not a fan of this, should probably be made a single function
			ControllableData cdata = new ControllableData(true);
			
			int i = 0;
			foreach (var spawn in T1SpawnPoints)
			{
				int id = WorldManager.Instance.GetNextId();
				T1Units.Add(id);
				if (i < Player1.SquadComp.Soldiers)
				{
					WorldManager.Instance.MakeWorldObject("Gunner", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				else if (i < Player1.SquadComp.Soldiers+ Player1.SquadComp.Heavies)
				{
					WorldManager.Instance.MakeWorldObject("Heavy", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				else
				{
					WorldManager.Instance.MakeWorldObject("Scout", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				
				i++;
			}

			cdata = new ControllableData(false);
			i = 0;
			foreach (var spawn in T2SpawnPoints)
			{
				int id = WorldManager.Instance.GetNextId();
				T2Units.Add(id);
				if (i < Player2.SquadComp.Soldiers)
				{
					WorldManager.Instance.MakeWorldObject("Gunner", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				else if (i < Player2.SquadComp.Soldiers+ Player2.SquadComp.Heavies)
				{
					WorldManager.Instance.MakeWorldObject("Heavy", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				else
				{
					WorldManager.Instance.MakeWorldObject("Scout", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}


				i++;
			}

			if (Random.Shared.Next(100) > 50)
			{
				NextTurn();
			}

		
			Thread.Sleep(1000);//let the clients process spawns
			SendData();
		}

		public static void SendData()
		{
			GameDataPacket packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = true,
				Score = score,
				GameState = GameState
			};
			

			//packet.
			Player1?.Connection.Send(packet);
			
			packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = false,
				Score = score,
				GameState = GameState
			};
			Player2?.Connection.Send(packet);
			packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = null,
				Score = score,
				GameState = GameState
			};
			foreach (var spectator in Spectators)
			{
				spectator?.Connection.Send(packet);
			}
			Program.InformMasterServer();
		}
	}
}