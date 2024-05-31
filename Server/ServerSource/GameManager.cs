using System.Collections.Concurrent;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Riptide;
using Action = DefconNull.WorldObjects.Units.Actions.Action;


namespace DefconNull;

public static partial class GameManager
{
	public static ClientInstance? Player1;
	public static ClientInstance? Player2;
	public static List<ClientInstance> Spectators = new();
	public static PreGameDataStruct PreGameData = new();
	
	

	
	
	public static ClientInstance? GetPlayer(bool isPlayer1)
	{
		if (isPlayer1)
		{
			return Player1;
		}
		return Player2;
	}
	public static ClientInstance? GetPlayer(Connection c)
	{
		if (Player1?.Connection == c)
		{
			return Player1;
		}
		if (Player2?.Connection == c)
		{
			return Player2;
		}
		return null;
	}


	public static void StartSetup()
	{
		if (GameState != GameState.Lobby) return;
		if (Player1 == null || Player2 == null) return;
		if(T1SpawnPoints.Count == 0 || T2SpawnPoints.Count == 0)
		{
			return;
		}
		GameState = GameState.Setup;


		NetworkingManager.SendGameData();
	}


	public static void StartGame()
	{
		if (GameState != GameState.Setup)
		{
			return;
		}

		GameState = GameState.Playing;
		Log.Message("GAME","Initiating starting game....");
		//not a fan of this, should probably be made a single function
		int i = 0;
		if (Player1?.SquadComp == null || Player1.SquadComp.Count == 0)
		{
			var newComp = new List<SquadMember>();
			for (int j = 0; j < PrefabManager.UnitPrefabs.Keys.Count; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = PrefabManager.UnitPrefabs.Keys.ToList()[j];
				Vector2Int pos = new Vector2Int(0, 0);
				do
				{
					pos = T1SpawnPoints[Random.Shared.Next(T1SpawnPoints.Count)];
				}while(!newComp.Find((s) => s.Position == pos).Equals(default(SquadMember)));
				sq.Position = pos;
				newComp.Add(sq);
			}
			for (int j = 0; j < WorldManager.Instance.CurrentMap.unitCount-PrefabManager.UnitPrefabs.Keys.Count; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = PrefabManager.UnitPrefabs.Keys.ToList()[Random.Shared.Next(PrefabManager.UnitPrefabs.Count)];
				Vector2Int pos = new Vector2Int(0, 0);
				do
				{
					pos = T1SpawnPoints[Random.Shared.Next(T1SpawnPoints.Count)];
				}while(!newComp.Find((s) => s.Position == pos).Equals(default(SquadMember)));
				sq.Position = pos;
				newComp.Add(sq);
			}
			Player1.SetSquadComp(newComp);
		
		}
		foreach (var spawn in Player1!.SquadComp!)
		{
			Unit.UnitData cdata = new Unit.UnitData(true);
			if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
			if (T1SpawnPoints.Contains(spawn.Position))
			{
				var objMake = WorldObjectManager.MakeWorldObject.Make(spawn.Prefab, spawn.Position, Direction.North,false, cdata);
				SequenceManager.AddSequence(objMake);
				T1Units.Add(objMake.data.ID);
				i++;

			}
		}

		
		if (Player2.SquadComp == null || Player2.SquadComp.Count == 0)
		{
			var newComcp = new List<SquadMember>();
			for (int j = 0; j < PrefabManager.UnitPrefabs.Keys.Count; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = PrefabManager.UnitPrefabs.Keys.ToList()[j];
				Vector2Int pos = new Vector2Int(0, 0);
				do
				{
					pos = T2SpawnPoints[Random.Shared.Next(T2SpawnPoints.Count)];
				}while(!newComcp.Find((s) => s.Position == pos).Equals(default(SquadMember)));
				sq.Position = pos;
				newComcp.Add(sq);
			}
			for (int j = 0; j < WorldManager.Instance.CurrentMap.unitCount-PrefabManager.UnitPrefabs.Keys.Count; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = PrefabManager.UnitPrefabs.Keys.ToList()[Random.Shared.Next(PrefabManager.UnitPrefabs.Count)];
				Vector2Int pos = new Vector2Int(0, 0);
				do
				{
					pos = T2SpawnPoints[Random.Shared.Next(T2SpawnPoints.Count)];
				}while(!newComcp.Find((s) => s.Position == pos).Equals(default(SquadMember)));
				sq.Position = pos;
				newComcp.Add(sq);
			}
			Player2.SetSquadComp(newComcp);
		}

		i = 0;
		foreach (var spawn in Player2!.SquadComp!)
		{
			Unit.UnitData cdata = new Unit.UnitData(false);
			if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
			if (T2SpawnPoints.Contains(spawn.Position))
			{
				var objMake = WorldObjectManager.MakeWorldObject.Make(spawn.Prefab, spawn.Position, Direction.North,false, cdata);
				SequenceManager.AddSequence(objMake);
				T2Units.Add(objMake.data.ID);
				i++;

			}

		}
		TimeTillNextTurn = PreGameData.TurnTime*1000;
		var t = new Task(delegate
		{
			Log.Message("GAME","Starting game");
			Task.Run(() =>
			{
				while (SequenceManager.SequenceRunning || !Player1!.HasDeliveredAllMessages || !Player2!.HasDeliveredAllMessages)
				{
					Thread.Sleep(500);
				}

				Thread.Sleep(1000); //just in case
				foreach (var u in T1Units)
				{
					Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
					if (!GetPlayer(true)!.KnownUnitPositions.ContainsKey(u))
					{
						GetPlayer(true)!.KnownUnitPositions.Add(u,(unit.WorldObject.TileLocation.Position,unit.WorldObject.GetData()));
					}

				
				}
				foreach (var u in T2Units)
				{
					Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
					if (!GetPlayer(false)!.KnownUnitPositions.ContainsKey(u))
					{
						GetPlayer(false)!.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
					}
				}
				//PlayerUnitPositionsDirty = true;
				var rng = Random.Shared.Next(100);
				NextTurn();
				Console.WriteLine("turn rng: "+rng);
				if (rng > 50 || Player2!.IsAI)
				{
					NextTurn();
				}
				NetworkingManager.SendGameData();
				WorldManager.Instance.MakeFovDirty();	
				NetworkingManager.SendAllSeenUnitPositions();
			});
		});
		SequenceManager.RunNextAfterFrames(t,5);//let units be created and sent before we swtich to playing


		
	}
	public static void FinishTurnWithAI()
	{
		if (IsPlayer1Turn)
		{
			List<Unit> units = new List<Unit>();
			foreach (var u in T1Units)
			{
				units.Add(WorldObjectManager.GetObject(u)!.UnitComponent ?? throw new Exception("team unit ids are not actual units"));
			}
			AI.AI.DoAITurn(units);
		}
		else
		{
			List<Unit> units = new List<Unit>();
			foreach (var u in T2Units)
			{
				units.Add(WorldObjectManager.GetObject(u)!.UnitComponent ?? throw new Exception("team unit ids are not actual units"));
			}
			AI.AI.DoAITurn(units);
		}
	}

	public static bool PlayerUnitPositionsDirty = false;

	private static void UpdatePlayerSideUnitPositionsForPlayer(bool player1,bool newTurn = false)
	{
		ClientInstance c = GetPlayer(player1)!;
		if(c == null) return;
		foreach (var pos in new Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)>(c.KnownUnitPositions))
		{
			var t = WorldManager.Instance.GetTileAtGrid(pos.Value.Item1);
			Visibility minVis = Visibility.Partial;
			if(pos.Value.Item2.UnitData!.Value.Crouching) minVis = Visibility.Full;
			
			
			if(!t.IsVisible(minVis,player1) && pos.Value.Item2.UnitData.Value.Team1 != player1) continue;//if they cant see the tile and the unit is from another team dont check anything
			if (t.UnitAtLocation == null)
			{
				c.KnownUnitPositions.Remove(pos.Key);
				Log.Message("UNITPOSITIONS","P1 unit at pos: "+pos.Value.Item1+" is null");
				PlayerUnitPositionsDirty = true;
			}
			else if(t.UnitAtLocation.WorldObject.ID != pos.Key){
				c.KnownUnitPositions.Remove(pos.Key);
				c.KnownUnitPositions.Remove(t.UnitAtLocation.WorldObject.ID);
				c.KnownUnitPositions.Add(t.UnitAtLocation.WorldObject.ID,(pos.Value.Item1,t.UnitAtLocation.WorldObject.GetData()));
				Log.Message("UNITPOSITIONS","P1 unit at pos: "+pos.Value.Item1+" is not the same as the one at the tile");
				PlayerUnitPositionsDirty = true;
			}
			else
			{
				var unitData = (pos.Value.Item1, t.UnitAtLocation!.WorldObject.GetData());
				if (c.KnownUnitPositions.ContainsKey(pos.Key) && !c.KnownUnitPositions[pos.Key].Equals(unitData))
				{
					c.KnownUnitPositions[pos.Key] = unitData;
					Log.Message("UNITPOSITIONS","P1 unit at pos: "+pos.Value.Item1+" has different data");
					PlayerUnitPositionsDirty = true;
				}
			}
		}

		if (newTurn)
		{
			//remove overwatch from all the unit datas
			foreach (var key in c.KnownUnitPositions.Keys.ToList())
			{

				var data =  c.KnownUnitPositions[key];
				if (data.Item2.UnitData!.Value.Team1 == IsPlayer1Turn)
				{
					data.Item2.UnitData = data.Item2.UnitData!.Value with {Overwatch = (false, -1)};
					c.KnownUnitPositions[key] = data;
				}
			}
		}
	}
	public static void UpdatePlayerSideUnitPositions(bool newTurn = false)
	{
		Log.Message("UNITPOSITIONS","updating unit positions");
		
		UpdatePlayerSideUnitPositionsForPlayer(true,newTurn);
		UpdatePlayerSideUnitPositionsForPlayer(false,newTurn);
	}

	public static void ShowUnitToEnemy(Unit spotedUnit)
	{
		ClientInstance c = GetPlayer(!spotedUnit.IsPlayer1Team)!;
		
		if (!c.KnownUnitPositions.ContainsKey(spotedUnit.WorldObject.ID))
		{
			c.KnownUnitPositions.Add(spotedUnit.WorldObject.ID,(spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData()));
			PlayerUnitPositionsDirty = true;

		}else if (c.KnownUnitPositions[spotedUnit.WorldObject.ID].Item1 != spotedUnit.WorldObject.TileLocation.Position)
		{
			c.KnownUnitPositions[spotedUnit.WorldObject.ID] = (spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData());
			PlayerUnitPositionsDirty = true;
		}
		
	}

	public static void SequenceFinished(Connection c)
	{
		var p = GetPlayer(c);
		if(p == null) return;
		p.IsReadyForNextSequence = true;
	}
	
	public class ClientInstance
	{
		public Connection? Connection { get; private set; }
		public string Name;
		public bool IsAI;
		public List<SquadMember>?  SquadComp { get; private set; }
		public bool IsPracticeOpponent { get; set; }
	
		public bool HasDeliveredAllMessages => MessagesToBeDelivered.Count == 0 || PlayerUnitPositionsDirty;
	
		public ConcurrentQueue<NetworkingManager.SequencePacket> SequenceQueue = new ConcurrentQueue<NetworkingManager.SequencePacket>();
	
		private List<ushort> MessagesToBeDelivered = new List<ushort>();
		public bool IsReadyForNextSequence = false;
		public bool ReadyForNextSequence => IsReadyForNextSequence || IsAI || IsPracticeOpponent;
		public readonly Dictionary<Vector2Int, WorldTile.WorldTileData> WorldState = new(); 
		public Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> KnownUnitPositions = new();

		public ClientInstance(string name,Connection? con)
		{
			IsReadyForNextSequence = true;
			Name = name;
			Connection = con;
			if (Connection != null) Connection.ReliableDelivered += ProcessDelivery;
		}
		public void Reconnect(Connection? con)
		{
			MessagesToBeDelivered.Clear();
			IsReadyForNextSequence = true;
			Connection = con;
			if (Connection != null) Connection.ReliableDelivered += ProcessDelivery;
		}
		public void RegisterMessageToBeDelivered(ushort id)
		{
			MessagesToBeDelivered.Add(id);
		}
		private void ProcessDelivery(ushort id)
		{
			MessagesToBeDelivered.Remove(id);
		}
	
		public void SetSquadComp(List<SquadMember> squad)
		{
			SquadComp = squad;
		}

		public void Disconnect()
		{
			Connection = null;
			MessagesToBeDelivered.Clear();
		}

		
	}

	public static void UpdatePlayerSideEnvironment()
	{
		ClientInstance? p = Player1;
		bool team1 = true;
		for (int i = 0; i < 2; i++)
		{
			if (i == 1)
			{
				p = Player2;
				team1 = false;
			}
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					var tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
				
					if(tile.Surface == null) return;//ignore empty tiles
					WorldTile.WorldTileData worldTileData = tile.GetData();
					p.WorldState.TryAdd(tile.Position,worldTileData);

					if (tile.IsVisible(team1:team1))
					{
						if (!p.WorldState[tile.Position]!.Equals(worldTileData))
						{
							p.WorldState[tile.Position] = worldTileData;
						}

					}
					
				
				}
			}
		}
	}


	public static bool tutorial = false;
	public static void StartTutorial()
	{
		tutorial = true;
		WorldManager.Instance.LoadMap("/Maps/Special/tutorial.mapdata");
       
		PracticeMode(Player1.Connection);
		NetworkingManager.SendMapData(Player1!.Connection!);
		var t = new Task(delegate
		{

			Unit.UnitData cdata = new Unit.UnitData(true);
			var objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(16, 35), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);


			WorldObject.WorldObjectData data = new WorldObject.WorldObjectData("Grunt");
			data.UnitData = new Unit.UnitData(false);
			data.Health = 5;
			data.Facing = Direction.NorthWest;
			data.JustSpawned = false;
			objMake = WorldObjectManager.MakeWorldObject.Make(data, new Vector2Int(23, 42));
			SequenceManager.AddSequence(objMake);
			
			
			
			data = new WorldObject.WorldObjectData("Heavy");
			cdata = new Unit.UnitData(false);
			//cdata.Determination = 0;
			data.UnitData = cdata;
			data.JustSpawned = false;
			data.Health = 100;
			objMake = WorldObjectManager.MakeWorldObject.Make(data, new Vector2Int(33, 36));
			SequenceManager.AddSequence(objMake);
			GameState = GameState.Playing;

			while (SequenceManager.SequenceRunning) Thread.Sleep(100);


			foreach (var u in T1Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				Player1.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
			}
			foreach (var u in T2Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				Player2.KnownUnitPositions.Add(u,(unit.WorldObject.TileLocation.Position,unit.WorldObject.GetData()));
			}
			NetworkingManager.SendGameData();
			WorldManager.Instance.MakeFovDirty();
			NetworkingManager.SendAllSeenUnitPositions();
			NetworkingManager.SendMapData(Player1!.Connection!);
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}

		
			
			var del = WorldObjectManager.DeleteWorldObject.Make(WorldManager.Instance.GetTileAtGrid(new Vector2Int(33, 42)).UnitAtLocation!.WorldObject);
			SequenceManager.AddSequence(del);

			
			cdata = new Unit.UnitData(true);
			cdata.Determination = 2;
			objMake = WorldObjectManager.MakeWorldObject.Make("Grunt", new Vector2Int(18, 43), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			
			cdata = new Unit.UnitData(false);
			objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(39, 45), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);

			cdata = new Unit.UnitData(false);
			objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(39, 44), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			
			while (SequenceManager.SequenceRunning) Thread.Sleep(100);
			
			foreach (var u in T1Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				Player1.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
			}

			foreach (var u in T2Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				if (!Player2.KnownUnitPositions.ContainsKey(u))
				{
					Player2.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
				}
			}

			NetworkingManager.SendAllSeenUnitPositions();
			SetEndTurn();
			
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}

			foreach (var u in T2Units)
			{
				var d = WorldObjectManager.DeleteWorldObject.Make(u);
				SequenceManager.AddSequence(d);
			}
			cdata = new Unit.UnitData(true);
			objMake = WorldObjectManager.MakeWorldObject.Make("Heavy", new Vector2Int(20, 45), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			cdata = new Unit.UnitData(false);
			objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(28, 40), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			cdata = new Unit.UnitData(false);
			objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(29, 40), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			cdata = new Unit.UnitData(false);
			objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(30, 40), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			do
			{
				Thread.Sleep(1500);
			} while (SequenceManager.SequenceRunning);
			foreach (var u in T2Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				if (!Player2.KnownUnitPositions.ContainsKey(u))
				{
					Player2.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
				}
			}
			
			foreach (var u in T1Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				if (!Player1.KnownUnitPositions.ContainsKey(u))
				{
					Player1.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
				}
			}
			List<Vector2Int> poses = new List<Vector2Int>();
			poses.Add(new Vector2Int(28, 45));
			poses.Add(new Vector2Int(29, 44));
			poses.Add(new Vector2Int(29, 43));

			foreach (var u in Player2.KnownUnitPositions.ToList())
			{
				if (!u.Value.Item2.UnitData!.Value.Team1)
				{

					WorldObjectManager.GetObject(u.Key)!.UnitComponent!.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(poses[0]));
					poses.RemoveAt(0);
					do
					{
						Thread.Sleep(1500);
					} while (SequenceManager.SequenceRunning);

				}
			}
			SetEndTurn();

			do
			{
				Thread.Sleep(2000);
			} while (IsPlayer1Turn);
			
			foreach (var u in T1Units)
			{
				var d = WorldObjectManager.DeleteWorldObject.Make(u);
				SequenceManager.AddSequence(d);
			}
			foreach (var u in T2Units)
			{
				var d = WorldObjectManager.DeleteWorldObject.Make(u);
				SequenceManager.AddSequence(d);
			}
			do
			{
				Thread.Sleep(1500);
			} while (SequenceManager.SequenceRunning);

	
			cdata = new Unit.UnitData(true);
			objMake = WorldObjectManager.MakeWorldObject.Make("Officer", new Vector2Int(35, 43), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			
			
			cdata = new Unit.UnitData(true);
			objMake = WorldObjectManager.MakeWorldObject.Make("Specialist", new Vector2Int(35, 45), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);
			do
			{
				Thread.Sleep(1500);
			} while (SequenceManager.SequenceRunning);
						
			foreach (var u in T1Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				if (!Player1.KnownUnitPositions.ContainsKey(u))
				{
					Player1.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
				}
			}
			NetworkingManager.SendAllSeenUnitPositions();
			SetEndTurn();
		});
		t.Start();


	}

	public static void PracticeMode(Connection con)
	{
		Player2 = new ClientInstance("Practice Mode",con);
		Player2.IsPracticeOpponent = true;
		NetworkingManager.SendPreGameInfo();
	}
}