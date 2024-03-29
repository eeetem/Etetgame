using System.Collections.Concurrent;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Riptide;


namespace DefconNull;

public static partial class GameManager
{
	public static ClientInstance? Player1;
	public static ClientInstance? Player2;
	public static List<ClientInstance> Spectators = new();
	public static PreGameDataStruct PreGameData = new();
	
	
	public static Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> Player1UnitPositions = new();
	public static Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> Player2UnitPositions = new();
	
	
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

		GameState = GameState.Setup;


		NetworkingManager.SendGameData();
	}


	public static void StartGame()
	{
		if (GameState != GameState.Setup)
		{
			return;
		}
		
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
		
		var t = new Task(delegate
		{
			GameState = GameState.Playing;
		
			var rng = Random.Shared.Next(100);
			NextTurn();
			Console.WriteLine("turn rng: "+rng);
			if (rng > 50 || Player2!.IsAI)
			{
				NextTurn();
			}

			Thread.Sleep(1000); //just in case
			foreach (var u in T1Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				Player1UnitPositions.Add(u,(unit.WorldObject.TileLocation.Position,unit.WorldObject.GetData()));
			}
			foreach (var u in T2Units)
			{
				Unit unit = WorldObjectManager.GetObject(u)!.UnitComponent!;
				Player2UnitPositions.Add(u,(unit.WorldObject.TileLocation.Position,unit.WorldObject.GetData()));
			}
			//PlayerUnitPositionsDirty = true;
			
			NetworkingManager.SendGameData();
			WorldManager.Instance.MakeFovDirty();	
			NetworkingManager.SendAllSeenUnitPositions();

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
	public static void UpdatePlayerSideUnitPositions(bool newTurn = false)
	{
		foreach (var pos in new Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)>(Player1UnitPositions))
		{
			var t = WorldManager.Instance.GetTileAtGrid(pos.Value.Item1);
			Visibility minVis = Visibility.Partial;
			if(pos.Value.Item2.UnitData!.Value.Crouching) minVis = Visibility.Full;
			if(!t.IsVisible(minVis,team1:true) && !pos.Value.Item2.UnitData.Value.Team1) continue;//if they cant see the tile and the unit is from another team dont check anything
			if (t.UnitAtLocation == null)
			{
				Player1UnitPositions.Remove(pos.Key);
				PlayerUnitPositionsDirty = true;
			}
			else if(t.UnitAtLocation.WorldObject.ID != pos.Key){
				Player1UnitPositions.Remove(pos.Key);
				Player1UnitPositions.Remove(t.UnitAtLocation.WorldObject.ID);
				Player1UnitPositions.Add(t.UnitAtLocation.WorldObject.ID,(pos.Value.Item1,t.UnitAtLocation.WorldObject.GetData()));
				PlayerUnitPositionsDirty = true;
			}
			else
			{
				var unitData = (pos.Value.Item1, t.UnitAtLocation!.WorldObject.GetData());
				if (Player1UnitPositions.ContainsKey(pos.Key) && !Player1UnitPositions[pos.Key].Equals(unitData))
				{
					Player1UnitPositions[pos.Key] = unitData;
					PlayerUnitPositionsDirty = true;
				}
			}
		}
		foreach (var pos  in new Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)>(Player2UnitPositions))
		{
			var t = WorldManager.Instance.GetTileAtGrid(pos.Value.Item1);
			Visibility minVis = Visibility.Partial;
			if(pos.Value.Item2.UnitData!.Value.Crouching) minVis = Visibility.Full;
			if(!t.IsVisible(minVis,team1:false) && pos.Value.Item2.UnitData!.Value.Team1) continue;//if they cant see the tile - dont do any checks
			if (t.UnitAtLocation == null)
			{
				Player2UnitPositions.Remove(pos.Key);
				PlayerUnitPositionsDirty = true;
			}
			else if(t.UnitAtLocation.WorldObject.ID != pos.Key){
				Player2UnitPositions.Remove(pos.Key);
				Player2UnitPositions.Remove(t.UnitAtLocation.WorldObject.ID);
				Player2UnitPositions.Add(t.UnitAtLocation.WorldObject.ID,(pos.Value.Item1,t.UnitAtLocation.WorldObject.GetData()));
				PlayerUnitPositionsDirty = true;
			}
			else
			{

				var unitData = (pos.Value.Item1, t.UnitAtLocation!.WorldObject.GetData());
				if (Player2UnitPositions.ContainsKey(pos.Key) && !Player2UnitPositions[pos.Key].Equals(unitData))
				{
					Player2UnitPositions[pos.Key] = unitData;
					PlayerUnitPositionsDirty = true;
				}
			}
		}

		if (newTurn)
		{
			//remove overwatch from all the unit datas
			foreach (var key in Player1UnitPositions.Keys.ToList())
			{
				
				var data = Player1UnitPositions[key];
				if (data.Item2.UnitData!.Value.Team1 == IsPlayer1Turn)
				{
					data.Item2.UnitData = data.Item2.UnitData!.Value with {Overwatch = (false, -1)};
					Player1UnitPositions[key] = data;
				}
			}

			foreach (var key in Player2UnitPositions.Keys.ToList())
			{
				var data = Player2UnitPositions[key];
				if (data.Item2.UnitData!.Value.Team1 == IsPlayer1Turn)
				{
					data.Item2.UnitData = data.Item2.UnitData!.Value with {Overwatch = (false, -1)};
					Player2UnitPositions[key] = data;
				}
			}
			
		}
	}

	public static void ShowUnitToEnemy(Unit spotedUnit)
	{
	
		if(spotedUnit.IsPlayer1Team)
		{
			if (!Player2UnitPositions.ContainsKey(spotedUnit.WorldObject.ID))
			{
				Player2UnitPositions.Add(spotedUnit.WorldObject.ID,(spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData()));
				PlayerUnitPositionsDirty = true;

			}else if (Player2UnitPositions[spotedUnit.WorldObject.ID].Item1 != spotedUnit.WorldObject.TileLocation.Position)
			{
				Player2UnitPositions[spotedUnit.WorldObject.ID] = (spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData());
				PlayerUnitPositionsDirty = true;
			}
			
		}
		else
		{
			if (!Player1UnitPositions.ContainsKey(spotedUnit.WorldObject.ID))
			{
				Player1UnitPositions.Add(spotedUnit.WorldObject.ID,(spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData()));
				PlayerUnitPositionsDirty = true;
			}else if (Player1UnitPositions[spotedUnit.WorldObject.ID].Item1 != spotedUnit.WorldObject.TileLocation.Position)
			{
				Player1UnitPositions[spotedUnit.WorldObject.ID] = (spotedUnit.WorldObject.TileLocation.Position,spotedUnit.WorldObject.GetData());
				PlayerUnitPositionsDirty = true;
			}
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
}