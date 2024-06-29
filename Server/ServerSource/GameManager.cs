using System.Collections.Concurrent;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using DefconNull.WorldObjects.Units.Actions;
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
				Thread.Sleep(1000);
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

	public static bool ShouldUpdateUnitPositions = false;
	public static bool ShouldRecalculateUnitPositions=  false;

	public static void SpotUnit(ClientInstance p, int unitId,(Vector2Int, WorldObject.WorldObjectData) spotedUnit)
	{
		p.KnownUnitPositions[unitId] = spotedUnit;
	}

	private static void UpdatePlayerSideUnitPositionsForPlayer(bool player1,bool newTurn = false)
	{
		ClientInstance? c = GetPlayer(player1);
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
				ShouldUpdateUnitPositions = true;
			}
			else if(t.UnitAtLocation.WorldObject.ID != pos.Key){
				c.KnownUnitPositions.Remove(pos.Key);
				c.KnownUnitPositions.Remove(t.UnitAtLocation.WorldObject.ID);
				c.KnownUnitPositions.Add(t.UnitAtLocation.WorldObject.ID,(pos.Value.Item1,t.UnitAtLocation.WorldObject.GetData()));
				Log.Message("UNITPOSITIONS","P1 unit at pos: "+pos.Value.Item1+" is not the same as the one at the tile");
				ShouldUpdateUnitPositions = true;
			}
			else
			{
				var unitData = (pos.Value.Item1, t.UnitAtLocation!.WorldObject.GetData());
				if (c.KnownUnitPositions.ContainsKey(pos.Key) && !c.KnownUnitPositions[pos.Key].Equals(unitData))
				{
					c.KnownUnitPositions[pos.Key] = unitData;
					Log.Message("UNITPOSITIONS","P1 unit at pos: "+pos.Value.Item1+" has different data");
					
					ShouldUpdateUnitPositions = true;
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
		Log.Message("UNIT","updating unit positions");
		GameManager.ShouldRecalculateUnitPositions = false;
		UpdatePlayerSideUnitPositionsForPlayer(true,newTurn);
		UpdatePlayerSideUnitPositionsForPlayer(false,newTurn);
	}

	public static void ShowUnitToEnemy(Unit spotedUnit)
	{
		ClientInstance c = GetPlayer(!spotedUnit.IsPlayer1Team)!;
		
		if (!c.KnownUnitPositions.ContainsKey(spotedUnit.WorldObject.ID))
		{
			c.KnownUnitPositions.Add(spotedUnit.WorldObject.ID,(spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData()));
			ShouldUpdateUnitPositions = true;

		}else if (c.KnownUnitPositions[spotedUnit.WorldObject.ID].Item1 != spotedUnit.WorldObject.TileLocation.Position)
		{
			c.KnownUnitPositions[spotedUnit.WorldObject.ID] = (spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData());
			ShouldUpdateUnitPositions = true;
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
		public Connection? Connection { get;  set; }
		public bool IsConnected => Connection != null && (Connection.IsConnected || Connection.IsConnecting || Connection.IsPending);
		public string Name;
		public bool IsAI;
		public List<SquadMember>?  SquadComp { get; private set; }
		public bool IsPracticeOpponent { get; set; }
	
		public bool HasDeliveredAllMessages => MessagesToBeDelivered.Count == 0 || ShouldUpdateUnitPositions;
		
		private readonly ConcurrentQueue<NetworkingManager.SequencePacket> _sequenceQueue = new ConcurrentQueue<NetworkingManager.SequencePacket>();
		public bool HasSequencesToSend => _sequenceQueue.Count > 0;
		public NetworkingManager.SequencePacket GetNextSequence()
		{
			if (_sequenceQueue.TryDequeue(out var seq))
			{
				return seq;
			}
			throw new Exception("No sequence to get");
		}
		public void AddToSequenceQueue(NetworkingManager.SequencePacket packet)
		{
			if(this.IsAI) return;
			if(!this.IsConnected) return;
			_sequenceQueue.Enqueue(packet);
		}
		
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
			_sequenceQueue.Clear();
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
			MessagesToBeDelivered.Clear();
			_sequenceQueue.Clear();
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
			if (p == null) continue;
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					var tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
				
					if(tile.Surface == null) continue;//ignore empty tiles
					WorldTile.WorldTileData worldTileData = tile.GetData();
					if (!p.WorldState.ContainsKey(tile.Position))
					{
						p.WorldState.TryAdd(tile.Position,worldTileData);
					}
					

					if (tile.IsVisible(team1:team1))
					{
						if (!p.WorldState[tile.Position]!.Equals(worldTileData))
						{
							p.WorldState[tile.Position] = worldTileData;
							NetworkingManager.AddSequenceToSendQueue(TileUpdate.Make(tile.Position,true,false));
						}
						

					}
					
				
				}
			}
		}
	}


	public static bool tutorial = false;
	public static void StartBasicTutorial()
	{
		tutorial = true;
		WorldManager.Instance.LoadMap("/Maps/Special/BasicTutorialMap.mapdata");
       
		PracticeMode(Player1.Connection);
		NetworkingManager.SendMapData(Player1!.Connection!);
		var t = new Task(delegate
		{

			Unit.UnitData cdata = new Unit.UnitData(true);
			var objMake = WorldObjectManager.MakeWorldObject.Make("Scout", new Vector2Int(19, 25), Direction.East, false, cdata);
			SequenceManager.AddSequence(objMake);


			WorldObject.WorldObjectData data = new WorldObject.WorldObjectData("Grunt");
			data.UnitData = new Unit.UnitData(false);
			data.Health = 5;
			data.Facing = Direction.NorthWest;
			data.JustSpawned = false;
			objMake = WorldObjectManager.MakeWorldObject.Make(data, new Vector2Int(25, 33));
			SequenceManager.AddSequence(objMake);
			
			
			
			data = new WorldObject.WorldObjectData("Heavy");
			cdata = new Unit.UnitData(false);
			cdata.Determination = 1;
			data.UnitData = cdata;
			data.JustSpawned = false;
			data.Health = 7;
			objMake = WorldObjectManager.MakeWorldObject.Make(data, new Vector2Int(28, 26));
			SequenceManager.AddSequence(objMake);
			GameState = GameState.Playing;

			while (SequenceManager.SequenceRunning) Thread.Sleep(100);


			foreach (var u in T1Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				Player1.KnownUnitPositions.Add(u, (unit.WorldObject.TileLocation.Position, unit.WorldObject.GetData()));
			}

			NetworkingManager.SendGameData();
			WorldManager.Instance.MakeFovDirty();
			NetworkingManager.SendAllSeenUnitPositions();
			NetworkingManager.SendMapData(Player1!.Connection!);
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}

			Unit grunt = null!;
			foreach (var u in T2Units)
			{
				
				if (WorldObjectManager.GetObject(u)!.Type.Name == "Grunt")
				{
					grunt = WorldObjectManager.GetObject(u)!.UnitComponent!;
					break;
				}
			}
			grunt.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(new Vector2Int(22, 31)));
			do
			{
				Thread.Sleep(300);
			}while (SequenceManager.SequenceRunning);
			grunt.DoAction(Action.ActionType.Face, new Action.ActionExecutionParamters(new Vector2Int(20,34)));
			
			SetEndTurn();
			
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			
			Unit heavy = null!;
			foreach (var u in T2Units)
			{
				
				if (WorldObjectManager.GetObject(u)!.Type.Name == "Heavy")
				{
					heavy = WorldObjectManager.GetObject(u)!.UnitComponent!;
					break;
				}
			}
			foreach (var u in T2Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				if (unit.Type.Name == "Heavy")
				{
					unit.Determination.Current = 1;
					break;
				} 
			}
			heavy.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(new Vector2Int(27, 26)));
			do
			{
				Thread.Sleep(300);
			}while (SequenceManager.SequenceRunning);
			heavy.DoAction(Action.ActionType.Face, new Action.ActionExecutionParamters(new Vector2Int(26,31)));
			
			SetEndTurn();
			
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(100);
			}

			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			
			foreach (var u in T2Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				if (unit.Type.Name == "Heavy")
				{
					unit.Determination.Current = 1;
					break;
				} 
			}
			heavy.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(new Vector2Int(27, 28)));

			SetEndTurn();
			
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(100);
			}

			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			
			heavy.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(new Vector2Int(25, 28)));
			
			SetEndTurn();
			
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(100);
			}

			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			
			SetEndTurn();
			
			while (!IsPlayer1Turn)
			{
				Thread.Sleep(100);
			}

			while (IsPlayer1Turn)
			{
				Thread.Sleep(1000);
			}
			
			/*
			 
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
			SetEndTurn(); */
			
			
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