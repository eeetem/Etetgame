﻿using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;


namespace DefconNull;

public static partial class GameManager
{
	public static ClientInstance? Player1;
	public static ClientInstance? Player2;
	public static List<ClientInstance> Spectators = new();
	public static PreGameDataStruct PreGameData = new();
	
	
	public static Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> Player1UnitPositions = new();
	public static Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> Player2UnitPositions = new();
	

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
				var objMake = WorldObjectManager.MakeWorldObject.Make(spawn.Prefab, spawn.Position, Direction.North, cdata);
				SequenceManager.AddSequence(objMake);
				NetworkingManager.SendSequence(objMake);
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
				var objMake = WorldObjectManager.MakeWorldObject.Make(spawn.Prefab, spawn.Position, Direction.North, cdata);
				SequenceManager.AddSequence(objMake);
				NetworkingManager.SendSequence(objMake);
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
			NetworkingManager.SendGameData();
			WorldManager.Instance.MakeFovDirty();	
			

		});
		WorldManager.Instance.RunNextAfterFrames(t,5);//let units be created and sent before we swtich to playing


		
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
	public static void UpdatePlayerSideUnitPositions()
	{
		foreach (var pos in Player1UnitPositions)
		{
			var t = WorldManager.Instance.GetTileAtGrid(pos.Value.Item1);
			if(!t.IsVisible(team1:true)) continue;//if they cant see the tile - dont do any checks
			if (t.UnitAtLocation == null)
			{
				Player1UnitPositions.Remove(pos.Key);
				PlayerUnitPositionsDirty = true;
			}
			else if(t.UnitAtLocation.WorldObject.ID != pos.Key){
				Player1UnitPositions.Remove(pos.Key);
				Player1UnitPositions.Add(t.UnitAtLocation.WorldObject.ID,pos.Value);
				PlayerUnitPositionsDirty = true;
			}
			if(Player1UnitPositions.ContainsKey(pos.Key)){
				Player1UnitPositions[pos.Key] = (pos.Value.Item1,t.UnitAtLocation!.WorldObject.GetData());
			}
		}
		foreach (var pos in Player2UnitPositions)
		{
			var t = WorldManager.Instance.GetTileAtGrid(pos.Value.Item1);
			if(!t.IsVisible(team1:false)) continue;//if they cant see the tile - dont do any checks
			if (t.UnitAtLocation == null)
			{
				Player2UnitPositions.Remove(pos.Key);
				PlayerUnitPositionsDirty = true;
			}
			else if(t.UnitAtLocation.WorldObject.ID != pos.Key){
				Player2UnitPositions.Remove(pos.Key);
				Player2UnitPositions.Add(t.UnitAtLocation.WorldObject.ID,pos.Value);
				PlayerUnitPositionsDirty = true;
			}
			if(Player2UnitPositions.ContainsKey(pos.Key)){
				Player2UnitPositions[pos.Key] = (pos.Value.Item1,t.UnitAtLocation!.WorldObject.GetData());
			}
		}
	}

	public static void EnsureUnitSpoted(Unit spotedUnit)
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
}