using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DefconNull.AI;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldActions.UnitAbility;
using DefconNull.WorldObjects;

namespace DefconNull;

public static class PseudoWorldManager
{
	public static readonly ConcurrentDictionary<int, ConcurrentDictionary<int,WorldObject>> PseudoWorldObjects = new ();
	private static readonly List<PseudoTile?[,]> PseudoGrids = new List<PseudoTile?[,]>();
	private static readonly List<bool> PseudoGridsInUse = new List<bool>();
	public static readonly ConcurrentDictionary<int,Tuple<List<AIAction.PotentialAbilityActivation>,List<AIAction.PotentialAbilityActivation>>> CachedAttacks = new ();


	public static WorldObject? GetObject(int id, int dimension = -1)
	{
		if(dimension != -1)
		{
			if(PseudoWorldObjects.TryGetValue(dimension, out var dimDIct) && dimDIct.TryGetValue(id, out var obj))
				return obj;//return if present, otherwise return the real object since there's no pseudo analogue
		}

		return WorldObjectManager.GetObject(id);
	}
	public static IWorldTile GetTileAtGrid(Vector2Int pos, int pseudoGrid)
	{
		if(pseudoGrid != -1)
		{
			var grid = PseudoGrids[pseudoGrid];
			if(grid[pos.X, pos.Y] == null)
			{
				grid[pos.X, pos.Y] = new PseudoTile(WorldManager.Instance.GetTileAtGrid(pos));
			}
			return grid[pos.X, pos.Y] ?? throw new InvalidOperationException();
				
		}
		return WorldManager.Instance.GetTileAtGrid(pos);
	}
	public static void AddUnitToPseudoWorld(Unit realunit, Vector2Int tilePosition, out Unit pseudoUnit, int dimension)
	{
		
		//		Console.WriteLine("placing unit at: " + tilePosition + " in dimension: " + dimension + " with ID " + realunit.WorldObject.ID);
		if (PseudoWorldObjects.ContainsKey(dimension) && PseudoWorldObjects[dimension].ContainsKey(realunit.WorldObject.ID))
		{
			//	Console.WriteLine("unit already in pseudo world at " + realunit.WorldObject.TileLocation.Position);
			pseudoUnit = PseudoWorldObjects[dimension][realunit.WorldObject.ID].UnitComponent!;
			return;
		}

		WorldObject.WorldObjectData data = realunit.WorldObject.GetData();
		WorldObject pseudoObj = new WorldObject(realunit.WorldObject.Type, GetTileAtGrid(tilePosition, dimension), data);
		pseudoObj.UnitComponent = new Unit(pseudoObj, realunit.Type, realunit.GetData(),false);
		realunit.Abilities.ForEach(extraAction => { pseudoObj.UnitComponent.Abilities.Add((UnitAbility) extraAction.Clone()); });
				
		pseudoUnit = pseudoObj.UnitComponent;
				

		if (!PseudoWorldObjects[dimension].TryAdd(pseudoObj.ID, pseudoObj))
		{
			throw new Exception("failed to create pseudo object");
					
		}
		GetTileAtGrid(tilePosition, dimension).UnitAtLocation = pseudoUnit;
		//	Console.WriteLine("adding unit with id: " + realunit.WorldObject.ID + " to dimension: " + dimension);
				
		if (realunit.WorldObject.TileLocation.Position != tilePosition)
		{
			((PseudoTile) GetTileAtGrid(realunit.WorldObject.TileLocation.Position, dimension)).ForceNoUnit = true; //remove old position from world
		}
			
	}

	public static int CreatePseudoWorld()
	{
		return ReverseNextFreePseudoDimension();
	}
	public static int CreatePseudoWorldWithUnit(Unit realunit, Vector2Int tilePosition, out Unit pseudoUnit, int copyDimension = -1)
	{

		int dimension= ReverseNextFreePseudoDimension();
		
	
		if (!PseudoWorldObjects.ContainsKey(dimension))
		{
			PseudoWorldObjects.TryAdd(dimension, new ConcurrentDictionary<int, WorldObject>());

		}

	
			
		//	Console.WriteLine("Creating pseudo world with unit at: "+tilePosition+" in dimension: "+dimension +" with ID "+realunit.WorldObject.ID);
		WorldObject.WorldObjectData data = realunit.WorldObject.GetData();
		WorldObject pseudoObj = new WorldObject(realunit.WorldObject.Type, GetTileAtGrid(tilePosition,dimension), data);
		pseudoObj.UnitComponent = new Unit(pseudoObj,realunit.Type,realunit.GetData(),false);
		realunit.Abilities.ForEach(extraAction => { pseudoObj.UnitComponent.Abilities.Add((UnitAbility) extraAction.Clone()); });
				
		pseudoUnit = pseudoObj.UnitComponent;

				
				
		
		if (!PseudoWorldObjects[dimension].TryAdd(pseudoObj.ID, pseudoObj))
		{
					
			throw new Exception("failed to create pseudo object");
					
		}

				
		GetTileAtGrid(tilePosition, dimension).UnitAtLocation = pseudoUnit;
		//Console.WriteLine("adding unit with id: " + realunit.WorldObject.ID + " to dimension: " + dimension);
				
		if (realunit.WorldObject.TileLocation.Position != tilePosition)
		{
			((PseudoTile) GetTileAtGrid(realunit.WorldObject.TileLocation.Position, dimension)).ForceNoUnit = true; //remove old position from world

		}
		if(copyDimension != -1){
			foreach (var otherdem in PseudoWorldObjects[copyDimension])
			{
				if (otherdem.Value.UnitComponent != null) AddUnitToPseudoWorld(otherdem.Value.UnitComponent, otherdem.Value.TileLocation.Position, out _, dimension);
			}
			
			CacheAttacksInDimension(CachedAttacks[copyDimension].Item2,dimension,true);
			CacheAttacksInDimension(CachedAttacks[copyDimension].Item1,dimension,false);
		}
		else
		{
			CacheAttacksInDimension(new List<AIAction.PotentialAbilityActivation>(),dimension,true);
			CacheAttacksInDimension(new List<AIAction.PotentialAbilityActivation>(),dimension,false);
		}


		return dimension;
			
	}
	
	public static void CacheAttacksInDimension(List<AIAction.PotentialAbilityActivation> attacks, int dimension, bool mainUnit)
	{
		//Console.WriteLine("Cashing attacks in " + dimension);
		if (!CachedAttacks.ContainsKey(dimension))
		{
			List<AIAction.PotentialAbilityActivation> enemyAttacks;
			List<AIAction.PotentialAbilityActivation> mainAttacks;
			if (mainUnit)
			{
				mainAttacks = new List<AIAction.PotentialAbilityActivation>(attacks);
				enemyAttacks = new List<AIAction.PotentialAbilityActivation>();
			}
			else
			{
				enemyAttacks = new List<AIAction.PotentialAbilityActivation>(attacks);
				mainAttacks = new List<AIAction.PotentialAbilityActivation>();
			}

			CachedAttacks.TryAdd(dimension, new Tuple<List<AIAction.PotentialAbilityActivation>, List<AIAction.PotentialAbilityActivation>>(enemyAttacks,mainAttacks));
			return;
		}

		if (mainUnit)
		{
			CachedAttacks[dimension].Item2.AddRange(attacks);
		}
		else
		{
			CachedAttacks[dimension].Item1.AddRange(attacks);
		}


	}

	public static bool GetCachedAttacksInDimension(ref List<AIAction.PotentialAbilityActivation> attacks, int dimension, bool mainUnit)
	{
		if(!CachedAttacks.ContainsKey(dimension)) return false;

		List<AIAction.PotentialAbilityActivation> list;
		if (mainUnit)
		{
			list = CachedAttacks[dimension].Item2;
		}
		else
		{
			list = CachedAttacks[dimension].Item1;
		}

		if (list.Count == 0) return false;
	

		attacks.AddRange(list);
		return true;
	}

	public static readonly object PseudoGenLock = new object();
	private static int ReverseNextFreePseudoDimension()
	{
		lock (PseudoGenLock)
		{
			int dimension = 0;
			while (true)
			{
				if (PseudoGridsInUse.Count <= dimension)
				{
					GenerateGridAtDimension(dimension);
					return dimension;
				}

				if (PseudoGridsInUse[dimension] == false)
				{
					GenerateGridAtDimension(dimension);
					return dimension;
				}

				dimension++;
			}
		}
	}

	private static void GenerateGridAtDimension(int d)
	{
		while(PseudoGridsInUse.Count <= d)
		{
			PseudoGridsInUse.Add(false);
		}
		PseudoGridsInUse[d] = true;

		while(PseudoGrids.Count <= d)
		{
			PseudoGrids.Add(new PseudoTile?[100,100]);
		}
			
			
	}

	/*	public void DeletePseudoUnit(int id, int dimension)
		{

			Console.WriteLine("removing unit with id: " + id + " from dimension: " + dimension);
			if (PseudoWorldObjects.ContainsKey(id) && PseudoWorldObjects[dimension].ContainsKey(id)){
				PseudoWorldObjects[dimension][id].TileLocation.UnitAtLocation = null;
				PseudoWorldObjects[dimension].Remove(id);
			}

		}*/
	public static void WipePseudoLayer(int dimension, bool NoAttackWipe = false)
	{
		lock (PseudoGenLock)
		{
			//	Console.WriteLine("Wiping grid in " + dimension);
		
			Array.Clear(PseudoGrids[dimension]);
			PseudoWorldObjects[dimension].Clear();
			///Console.WriteLine("Wiping grid in " + dimension);

			if (!NoAttackWipe)
			{
				CachedAttacks[dimension].Item1.ForEach(y => y.Consequences.ForEach(z => z.Return()));
				CachedAttacks[dimension].Item2.ForEach(y => y.Consequences.ForEach(z => z.Return()));
			}
			
			
			CachedAttacks[dimension].Item1.Clear();
			CachedAttacks[dimension].Item2.Clear();
			PseudoGridsInUse[dimension] = false;
		}
	}

}