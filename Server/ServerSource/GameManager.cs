﻿using System;
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
		public static readonly List<int> T1Units = new List<int>();
		public static readonly List<int> T2Units = new List<int>();

		public static void StartGame()
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
				int id = WorldManager.Instance.GetNextId();
				T1Units.Add(id);
				if (i < Player1.StartData.Soldiers)
				{
					WorldManager.Instance.MakeWorldObject("Gunner", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				else if (i < Player1.StartData.Soldiers+ Player1.StartData.Heavies)
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
				if (i < Player2.StartData.Soldiers)
				{
					WorldManager.Instance.MakeWorldObject("Gunner", spawn.TileLocation.Position,Direction.North, id, controllableData: cdata);
				}
				else if (i < Player2.StartData.Soldiers+ Player2.StartData.Heavies)
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

			Networking.StartGame();
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