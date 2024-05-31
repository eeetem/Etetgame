using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.AI;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldActions.UnitAbility;
using DefconNull.WorldObjects;
using MD5Hash;
using Microsoft.Xna.Framework;
using Riptide;
#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif


namespace DefconNull;

public  partial class WorldManager
{

	private readonly WorldTile[,] _gridData;
	private static WorldManager instance;
	public static WorldManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new WorldManager();
			}
			return instance;
		}
	}
		
		

	private WorldManager()
	{
		CurrentMap = new MapData();
		_gridData = new WorldTile[100, 100];
		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				_gridData[x, y] = new WorldTile(new Vector2Int(x, y));
			}
		}
	}
	private static readonly List<WorldTile> AllTiles = new List<WorldTile>();
	public List<WorldTile> GetAllTiles()
	{
		AllTiles.Clear();

		foreach (var tile  in Instance._gridData)
		{
			AllTiles.Add(tile);
		}

		return AllTiles;
	}



	public static bool IsPositionValid(Vector2Int pos)
	{
		return pos.X >= 0 && pos.X < 100 && pos.Y >= 0 && pos.Y < 100;
	}

	public void NextTurn(bool teamOneTurn)
	{

		foreach (var tile in _gridData)
		{
			tile.NextTurn(teamOneTurn);
		}
	}


	public void LoadWorldTile(WorldTile.WorldTileData data, bool forceUpdateEverything = false)
	{
       

		WorldTile tile = (WorldTile) GetTileAtGrid(data.Position);
		Log.Message("WORLDMANAGER","Loading tile at " + data.Position);
		if(tile.GetData().Equals(data))return;
		
		LoadTileObject(data.Surface,tile.Surface, tile,forceUpdateEverything);
		LoadTileObject(data.NorthEdge,tile.NorthEdge, tile,forceUpdateEverything);
		LoadTileObject(data.WestEdge,tile.WestEdge, tile,forceUpdateEverything);
		LoadTileObject(data.EastEdge,tile.EastEdge,  GetTileAtGrid(data.Position+new Vector2(1,0)),forceUpdateEverything);
		LoadTileObject(data.SouthEdge,tile.SouthEdge, GetTileAtGrid(data.Position+new Vector2(0,1)),forceUpdateEverything);
		//	LoadTileObject(data.UnitAtLocation, tile.UnitAtLocation?.WorldObject, tile,forceUpdateEverything);
	



		foreach (var obj in tile.ObjectsAtLocation)
		{
			SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(obj.ID));
		}
	
		foreach (var obj in data.ObjectsAtLocation)
		{
			SequenceManager.AddSequence(WorldObjectManager.MakeWorldObject.Make(obj, tile));
		}



	}

	private static void LoadTileObject(WorldObject.WorldObjectData? data, WorldObject? tileObject, WorldTile tile, bool forceUpdateEverything = false)
	{
		
		if (data.HasValue)
		{
			var obj = WorldObjectManager.GetObject(data.Value.ID);
			if (forceUpdateEverything || obj == null || !obj.IsVisible()) //dont update if we can see it since that causes weird stuff and we should already have all the info through sequence actionss
			{
				if (tileObject is null)
				{
					Log.Message("WORLD MANAGER","desired location is null making obj: " + data.Value.ID);
					WorldObjectManager.MakeWorldObject.Make(data.Value, tile).GenerateTask().RunTaskSynchronously();
				}
				else if (tileObject.ID != data.Value.ID)
				{
					Log.Message("WORLD MANAGER","desired location is has an object with a different id(" + tileObject.ID + ")deleting and  making obj: " + data.Value.ID);
					WorldObjectManager.DeleteWorldObject.Make(tileObject.ID).GenerateTask().RunTaskSynchronously();
					WorldObjectManager.MakeWorldObject.Make(data.Value, tile).GenerateTask().RunTaskSynchronously();
				}
				else if (tileObject.GetData().GetHash() != data.Value.GetHash())
				{
					Log.Message("WORLD MANAGER","desired location is has an object with a different hash remaking obj: " + data.Value.ID);
					WorldObjectManager.MakeWorldObject.Make(data.Value, tile).GenerateTask().RunTaskSynchronously();
				}
			}
		}
		else if (tileObject is not null)
		{
			WorldObjectManager.DeleteWorldObject.Make(tileObject.ID).GenerateTask().RunTaskSynchronously();;
		}
	}


	private readonly List<Tuple<WorldObject.WorldObjectData, WorldTile>> _createdObjects = new List<Tuple<WorldObject.WorldObjectData, WorldTile>>();

	public Visibility CanSee(Unit unit, Vector2Int to)
	{
		if (unit.VisibleTiles.TryGetValue(to, out var see))
		{
			return see;
		}

		return Visibility.None;

	}

	public Visibility CanTeamSee(Vector2 position, bool team1)
	{
		return GetTileAtGrid(position).GetVisibility(team1);
	}
	public bool FovDirty { get; private set; }
	public void MakeFovDirty()
	{
		Log.Message("WORLD MANAGER","FOV made dirty");
		FovDirty = true;
	}
		
	private void CalculateFov()
	{
		Log.Message("WORLD MANAGER", "calculating FOV");

		foreach (var tile in _gridData)
		{
#if CLIENT
            tile.TileVisibility = Visibility.None;
#elif SERVER
			tile.TileVisibility = new ValueTuple<Visibility, Visibility>(Visibility.None, Visibility.None);
#endif
		}

		List<Unit> units = new List<Unit>();

		units = GameManager.GetAllUnits();

		Parallel.ForEach(units, seeingUnit =>
		{
			var unitSee = GetVisibleTiles(seeingUnit.WorldObject.TileLocation.Position, seeingUnit.WorldObject.Facing, seeingUnit.GetSightRange(), seeingUnit.Crouching);
            
			seeingUnit.VisibleTiles = unitSee;
#if CLIENT
            if(!seeingUnit.IsMyTeam())return;//enemy units dont update our FOV
#endif
			foreach (var visTuple in unitSee)
			{

               
				if(GetTileAtGrid(visTuple.Key).GetVisibility(seeingUnit.IsPlayer1Team) < visTuple.Value)
				{
					GetTileAtGrid(visTuple.Key).SetVisibility(seeingUnit.IsPlayer1Team,visTuple.Value);

					var spotedUnit = GetTileAtGrid(visTuple.Key).UnitAtLocation;

					if(spotedUnit == null) continue;

#if SERVER
					if (spotedUnit.IsPlayer1Team != seeingUnit.IsPlayer1Team && spotedUnit.WorldObject.GetMinimumVisibility() <= visTuple.Value)
					{
						GameManager.ShowUnitToEnemy(spotedUnit);
					}
#endif               
				}
				
			}
            
		});
		
#if CLIENT
        foreach (var tile in _gridData)
        {
            tile.CalcWatchLevel();
        }
#endif    
        
#if SERVER
		Log.Message("WORLD MANAGER","sending FOV tile updates");


		GameManager.UpdatePlayerSideUnitPositions();
		GameManager.UpdatePlayerSideEnvironment();
#endif
		FovDirty = false;
	}
			
	public ConcurrentDictionary<Vector2Int,Visibility> GetVisibleTiles(Vector2Int pos, Direction dir, int range,bool crouched)
	{
		while (SequenceManager.SequenceRunningRightNow)
		{
			Thread.Sleep(100);
		}

		int itteration = 0;

		List<Vector2Int> positionsToCheck = new List<Vector2Int>();
		ConcurrentDictionary<Vector2Int, Visibility> resullt = new ConcurrentDictionary<Vector2Int, Visibility>();
		Vector2Int initialpos = pos;
		while (itteration < range+2)
		{
			if (IsPositionValid(pos))
			{
				positionsToCheck.Add(pos);
			}
				
			Vector2Int offset;
			Vector2Int invoffset;
			if (Utility.DirToVec2(dir).Magnitude() > 1) //diagonal
			{
				offset = Utility.DirToVec2(dir + 3);
				invoffset = Utility.DirToVec2(dir - 3);
			}
			else
			{
				offset = Utility.DirToVec2(dir+ 2);
				invoffset = Utility.DirToVec2(dir - 2);
			}


			for (int x = 0; x < itteration; x++)
			{
					
				if (IsPositionValid(pos + invoffset * (x + 1)))
				{

					positionsToCheck.Add(pos + invoffset * (x + 1));
					
				}

				if (IsPositionValid(pos + offset * (x + 1)))
				{

					positionsToCheck.Add(pos + offset * (x + 1));
					
				}
			}

			pos += Utility.DirToVec2(dir);
			itteration++;
		}
		
		Parallel.ForEach(positionsToCheck, (position) =>
		{
			var vis = VisibilityCast(initialpos, position, range, crouched);
			if (vis != Visibility.None)
				resullt.AddOrUpdate(position, vis, (a, b) =>
				{
					if (vis > b) return vis;
					return b;
				});
		});
		
		return resullt;
	}
	
	
	public Visibility VisibilityCast(Vector2Int From,Vector2Int to, int sightRange, bool crouched)
	{
		if(Vector2.Distance(From, to) > sightRange)
		{
			return Visibility.None;
		}
		RayCastOutcome?[] fullCasts;
		RayCastOutcome?[] partalCasts;
		if (crouched)
		{
			fullCasts = MultiCornerCast(From, to, Cover.High, true);//full vsson does not go past high cover and no partial sigh
			partalCasts = Array.Empty<RayCastOutcome?>();
		}
		else
		{
			fullCasts = MultiCornerCast(From, to, Cover.High,  true,Cover.Full);//full vission does not go past high cover
			partalCasts  = MultiCornerCast(From, to, Cover.Full,  true);//partial visson over high cover
							
		}

		try
		{
			foreach (var cast in fullCasts)
			{
				if (!cast.HasValue) continue;
				if (!cast.Value.hit)
				{
					return Visibility.Full;
				}

				var hitObj = WorldObjectManager.GetObject(cast.Value.HitObjId);
				if(!hitObj.Type.Edge && hitObj.TileLocation.Position == to)//if we hit a non edge object then we can still spot the tile it's on. this is used for smoke
				{
					return Visibility.Partial;
				}
                
				//i think this is for seeing over distant cover while crouched, dont quote me on that tho
				if (Vector2.Floor(cast.Value.CollisionPointLong) == Vector2.Floor(cast.Value.EndPoint) && cast.Value.HitObjId != -1 && WorldObjectManager.GetObject(cast.Value.HitObjId).GetCover(true) != Cover.Full)
				{
					return Visibility.Partial;
				}
			}

			foreach (var cast in partalCasts)
			{
				if (!cast.HasValue) continue;
				if (!cast.Value.hit)
				{
					return Visibility.Partial;
				}
				var hitObj = WorldObjectManager.GetObject(cast.Value.HitObjId);
				if(!hitObj.Type.Edge && hitObj.TileLocation.Position == to)//if we hit a non edge object then we can still spot the tile it's on. this is used for smoke
				{
					return Visibility.Partial;
				}

			}
		}
		finally
		{
			ArrayPool<RayCastOutcome?>.Shared.Return(fullCasts);
			ArrayPool<RayCastOutcome?>.Shared.Return(partalCasts);
		}

		return Visibility.None;
	}

	public struct RayCastOutcome : IMessageSerializable
	{
		public bool Equals(RayCastOutcome other)
		{
			return CollisionPointLong.Equals(other.CollisionPointLong) && CollisionPointShort.Equals(other.CollisionPointShort) && StartPoint.Equals(other.StartPoint) && EndPoint.Equals(other.EndPoint) &&Path is null == other.Path is null&& ( (Path is null && other.Path is null) ||Enumerable.SequenceEqual(Path!, other.Path!)) && HitObjId == other.HitObjId && hit == other.hit;
		}

		public override bool Equals(object? obj)
		{
			return obj is RayCastOutcome other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = CollisionPointLong.GetHashCode();
				hashCode = (hashCode * 397) ^ CollisionPointShort.GetHashCode();
				hashCode = (hashCode * 397) ^ StartPoint.GetHashCode();
				hashCode = (hashCode * 397) ^ EndPoint.GetHashCode();
				hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ HitObjId;
				hashCode = (hashCode * 397) ^ hit.GetHashCode();
				return hashCode;
			}
		}

		public Vector2 CollisionPointLong= new Vector2(0, 0);
		public Vector2 CollisionPointShort= new Vector2(0, 0);
		public Vector2 StartPoint;
		public Vector2 EndPoint;
		public List<Vector2Int>? Path;

		public int HitObjId{ get; set; }

		public bool hit{ get; set; }

		public RayCastOutcome(Vector2 start, Vector2 end, bool MakePath = false)
		{
			CollisionPointLong = new Vector2(0, 0);
			CollisionPointShort = new Vector2(0, 0);
			hit = false;
			EndPoint = end;
			HitObjId = -1;
			StartPoint = start;
			if (MakePath)
			{
				Path = new List<Vector2Int>(Math.Max(2, (int) (Vector2.Distance(start, end) * 32)));
			}
		}

		public void Serialize(Message message)
		{
			message.Add(CollisionPointLong.X);
			message.Add(CollisionPointLong.Y);
			message.Add(CollisionPointShort.X);
			message.Add(CollisionPointShort.Y);
			message.Add(EndPoint.X);
			message.Add(EndPoint.Y);
			message.Add(StartPoint.X);
			message.Add(StartPoint.Y);
			message.Add(HitObjId);
			message.Add(hit);
			if (Path != null)
			{
				message.Add(Path.ToArray());
			}
			else
			{
				message.Add(Array.Empty<Vector2Int>());
			}
		}

		public void Deserialize(Message message)
		{
			CollisionPointLong.X = message.GetFloat();
			CollisionPointLong.Y = message.GetFloat();
			CollisionPointShort.X = message.GetFloat();
			CollisionPointShort.Y = message.GetFloat();
			EndPoint.X = message.GetFloat();
			EndPoint.Y = message.GetFloat();
			StartPoint.X = message.GetFloat();
			StartPoint.Y = message.GetFloat();
			HitObjId = message.GetInt();
			hit = message.GetBool();
			Path = message.GetSerializables<Vector2Int>().ToList();
			if(Path.Count() == 0)
			{
				Path = null;
			}
		}
	}
	
	public RayCastOutcome?[] MultiCornerCast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover , bool visibilityCast = false,Cover? minHitCoverSameTile = null)
	{

		RayCastOutcome?[] result = ArrayPool<RayCastOutcome?>.Shared.Rent(4);
		Vector2 startPos =startcell+new Vector2(0.5f,0.5f);
		Vector2 Dir = Vector2.Normalize(startcell - endcell);
		startPos += Dir / new Vector2(2.5f, 2.5f);
		
		for (int i = 0; i < result.Length; i++)
		{
			result[i] = null;
		}
		result[0] = Raycast(startPos, endcell, minHitCover, visibilityCast, false, minHitCoverSameTile);
		result[1] = Raycast(startPos, endcell+ new Vector2(0f, 0.99f), minHitCover,visibilityCast ,false,minHitCoverSameTile);
		result[2] = Raycast(startPos,  endcell + new Vector2(0.99f, 0f), minHitCover,visibilityCast ,false,minHitCoverSameTile);
		result[3] = Raycast(startPos, endcell + new Vector2(0.99f, 0.99f), minHitCover,visibilityCast ,false,minHitCoverSameTile);
		
		
		return result;
	}


	public RayCastOutcome CenterToCenterRaycast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover,  bool visibilityCast = false, bool ignoreControllables = false,bool makePath = false)
	{
		Vector2 startPos = (Vector2) startcell + new Vector2(0.5f, 0.5f);
		Vector2 endPos = (Vector2) endcell + new Vector2(0.5f, 0.5f);
		return Raycast(startPos, endPos, minHitCover, visibilityCast,ignoreControllables,makePath:makePath);
	}


	public RayCastOutcome Raycast(Vector2 startPos, Vector2 endPos, Cover minHitCover, bool visibilityCast = false, bool ignoreControllables = false, Cover? minHtCoverSameTile = null, int pseudoLayer = -1, bool makePath = false)
	{
		if (minHtCoverSameTile == null)
		{
			minHtCoverSameTile = minHitCover;
		}

		Vector2Int startcell = new Vector2Int((int)Math.Floor(startPos.X), (int)Math.Floor(startPos.Y));
		Vector2Int endcell = new Vector2Int((int)Math.Floor(endPos.X), (int)Math.Floor(endPos.Y));


		RayCastOutcome result = new RayCastOutcome(startPos, endPos, makePath);
		if (startPos == endPos)
		{
			result.hit = false;
			return result;
		}

		Vector2Int checkingSquare = new Vector2Int(startcell.X, startcell.Y);
		Vector2Int lastCheckingSquare;

		Vector2 dir = endPos - startPos;
		dir.Normalize();

		float slope = dir.Y / dir.X;
		float inverseSlope = dir.X / dir.Y;
		Vector2 scalingFactor = new Vector2((float) Math.Sqrt(1 + slope * slope), (float) Math.Sqrt(1 + inverseSlope * inverseSlope));

			
		Vector2Int step = new Vector2Int(0,0);
		Vector2 lenght = new Vector2Int(0, 0);
		if (dir.X < 0)
		{
			step.X = -1;
			lenght.X = (startPos.X - startcell.X) * scalingFactor.X;
		}
		else
		{
			step.X = 1;
			lenght.X = (startcell.X + 1 - startPos.X) * scalingFactor.X;
		}

		if (dir.Y < 0)
		{
			step.Y = -1;
			lenght.Y = (startPos.Y - startcell.Y) * scalingFactor.Y;
		}
		else
		{
			step.Y = 1;
			lenght.Y = (startcell.Y+1 - startPos.Y) * scalingFactor.Y;
		}
			
			
			
		if (float.IsNaN(lenght.X))
		{
			lenght.X = 0;
		}
		if (float.IsNaN(lenght.Y))
		{
			lenght.Y = 0;
		}

		float totalLenght = 0.05f;
		int smokeLayers = 0;
		while (true)
		{
			if (makePath)
			{
				result.Path!.Add(checkingSquare);
			}

			lastCheckingSquare = new Vector2Int(checkingSquare.X,checkingSquare.Y);
			if (lenght.X > lenght.Y)
			{
				checkingSquare.Y += step.Y;
				totalLenght = lenght.Y;
				lenght.Y += scalingFactor.Y;
			}
			else
			{
				checkingSquare.X += step.X;
				totalLenght = lenght.X;
				lenght.X += scalingFactor.X;
					
			}

	
			IWorldTile tile;
			
			Vector2 collisionPointlong = (totalLenght+0.05f) * dir + startPos;
			Vector2 collisionPointshort = (totalLenght-0.1f) * dir + startPos;
			if (IsPositionValid(checkingSquare))
			{
				tile = PseudoWorldManager.GetTileAtGrid(checkingSquare,pseudoLayer);
			}
			else
			{
				result.CollisionPointLong = collisionPointlong;
				result.CollisionPointShort = collisionPointshort;
				result.hit = false;
				return result;
			}
			Vector2 collisionVector = (Vector2) tile.Position + new Vector2(0.5f, 0.5f) - collisionPointlong;

			if (IsPositionValid(lastCheckingSquare))
			{
				WorldObject hitobj = GetCoverObj(lastCheckingSquare,Utility.Vec2ToDir(checkingSquare - lastCheckingSquare), visibilityCast, ignoreControllables,lastCheckingSquare.Equals(startcell),pseudoLayer);

				if (visibilityCast)
				{
					foreach (var obj in tile.ObjectsAtLocation)
					{
						smokeLayers += obj.Type.VisibilityObstructFactor;
						if (smokeLayers >= 10)
						{
							result.CollisionPointLong = collisionPointlong;
							result.CollisionPointShort = collisionPointshort;
							result.hit = true;
							result.HitObjId = obj.ID;
							//if this is true then we're hitting a controllable form behind
							// if (GetTileAtGrid(lastCheckingSquare).UnitAtLocation != null)
							// {
							//     result.CollisionPointLong += -0.3f * dir;
							//     result.CollisionPointShort += -0.3f * dir;
							// }

							return result;
						}
					}

                  
				}

				if (hitobj.ID != -1 )
				{
						
					Cover c = hitobj.GetCover(visibilityCast);
					if (Utility.IsClose(hitobj, startcell))
					{
						if (c >= minHtCoverSameTile)
						{
							result.CollisionPointLong = collisionPointlong;
							result.CollisionPointShort = collisionPointshort;
							result.hit = true;
							result.HitObjId = hitobj.ID;
							//if this is true then we're hitting a controllable form behind
							// if (GetTileAtGrid(lastCheckingSquare).UnitAtLocation != null)
							// {
							//     result.CollisionPointLong += -0.1f * dir;
							//     result.CollisionPointShort += -0.1f * dir;
							// }
							return result;
								
						}

					}
					else
					{
						if (c >= minHitCover)
						{
							result.CollisionPointLong = collisionPointlong;
							result.CollisionPointShort = collisionPointshort;
							result.hit = true;
							result.HitObjId = hitobj.ID;
							// if (GetTileAtGrid(lastCheckingSquare).UnitAtLocation != null)
							// {
							//     result.CollisionPointLong += -0.1f * dir;
							//     result.CollisionPointShort += -0.1f * dir;
							// }
							return result;
						}
					}
				}

			}

			if (endcell.Equals(checkingSquare))
			{
				result.CollisionPointLong = endcell + new Vector2(0.5f, 0.5f);
				result.CollisionPointShort = endcell + new Vector2(0.5f, 0.5f);
				result.hit = false;
				return result;
			}
		}
	}



		
	public Cover GetCover(Vector2Int loc, Direction dir, bool visibilityCover = false, bool ignoreControllables = false, bool ignoreObjectsAtLoc = true,int pseudoLayer = -1)
	{
		return GetCoverObj(loc,dir,visibilityCover,ignoreControllables,ignoreObjectsAtLoc,pseudoLayer).GetCover();
	}

	private static WorldObject nullWorldObject;
	
	public WorldObject GetCoverObj(Vector2Int loc,Direction dir, bool visibilityCover = false,bool ignoreContollables = false, bool ignoreObjectsAtLoc = true, int pseudoLayer = -1)
	{
		while (SequenceManager.SequenceRunningRightNow)
		{
			Thread.Sleep(100);
		}
		dir = Utility.NormaliseDir(dir);
		WorldObject biggestCoverObj = nullWorldObject;
		IWorldTile tileAtPos = PseudoWorldManager.GetTileAtGrid(loc,pseudoLayer);
		foreach (var obj in tileAtPos.ObjectsAtLocation)
		{
			if(obj.GetCover(false)>biggestCoverObj.GetCover(false))
			{
				biggestCoverObj = obj;
			}
		}
		IWorldTile? tileInDir =null;
		if(IsPositionValid(tileAtPos.Position + Utility.DirToVec2(dir)))
		{
			tileInDir = PseudoWorldManager.GetTileAtGrid(tileAtPos.Position + Utility.DirToVec2(dir),pseudoLayer);
		}
            

		WorldObject coverObj;
		switch (dir)
		{
			case Direction.East:
				if(tileInDir?.WestEdge != null && tileInDir.WestEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileInDir.WestEdge;
				}
				break;
			case Direction.North:
				if(tileAtPos.NorthEdge != null && tileAtPos.NorthEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileAtPos.NorthEdge;
				}
				break;
				
			case Direction.West:
				if(tileAtPos.WestEdge != null && tileAtPos.WestEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileAtPos.WestEdge;
				}
				break;
			case Direction.South:
				if(tileInDir?.NorthEdge != null && tileInDir.NorthEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileInDir.NorthEdge;
				}
				break;
			case Direction.SouthWest:
				coverObj = GetCoverObj(loc,Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(loc,Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}
				coverObj = GetCoverObj(tileInDir.Position,Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = GetCoverObj(tileInDir.Position,Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
					

				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				break;
			case Direction.SouthEast:
				coverObj = GetCoverObj(loc,Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(loc,Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}
				coverObj = GetCoverObj(tileInDir.Position,Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = GetCoverObj(tileInDir.Position,Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
					

				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				break;
			case Direction.NorthWest:
				coverObj = GetCoverObj(loc,Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(loc,Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}

				coverObj = GetCoverObj(tileInDir.Position,Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = GetCoverObj(tileInDir.Position,Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

					
				break;
			case Direction.NorthEast:
				coverObj = GetCoverObj(loc,Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(loc,Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}
				coverObj = GetCoverObj(tileInDir.Position,Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = GetCoverObj(tileInDir.Position,Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc,pseudoLayer);
					

				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
					
				break;
				
		}

		if (!ignoreObjectsAtLoc)
		{
			if (!ignoreContollables)
			{
				if (tileAtPos.UnitAtLocation != null && tileAtPos.UnitAtLocation.WorldObject.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileAtPos.UnitAtLocation.WorldObject.Facing == dir || tileAtPos.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir + 1) || tileAtPos.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir - 1)))
				{

					biggestCoverObj = tileAtPos.UnitAtLocation.WorldObject;

				}
			}

			foreach (var obj in tileAtPos.ObjectsAtLocation)
			{
				if (obj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = obj;
				}
			}
		}
			
        
		//this code is broken but unutill objs at loc can provide cover it doesnt matter
		if (tileInDir != null)
		{

			if (!ignoreContollables && tileInDir.UnitAtLocation != null)
			{
				if (tileInDir.UnitAtLocation.WorldObject.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileInDir.UnitAtLocation.WorldObject.Facing == dir || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir + 1) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir - 1)))
				{

					biggestCoverObj = tileInDir.UnitAtLocation.WorldObject;

				}

				Direction inverseDir = Utility.NormaliseDir(dir - 4);
				if (tileInDir.UnitAtLocation.WorldObject.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileInDir.UnitAtLocation.WorldObject.Facing == inverseDir || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir + 1) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir + 2) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir - 2) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir - 1))) //only hit people from the front
				{

					biggestCoverObj = tileInDir.UnitAtLocation.WorldObject;

				}
			}
			foreach (var obj in tileInDir.ObjectsAtLocation)
			{

				if (obj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = obj;
				}
			}
            

		}

		if (biggestCoverObj == null) throw new Exception("Biggest cover obj cannot be null");
		return biggestCoverObj;
		
	}



	
	public WorldTile GetTileAtGrid(Vector2Int pos)
	{
		return _gridData[pos.X, pos.Y];
	}

	public List<IWorldTile> GetTilesAround(Vector2Int pos, int range = 1, int alternateDimension = -1, Cover? lineOfSight = null)
	{
		int x = pos.X;
		int y = pos.Y;
		List<IWorldTile> tiles = new List<IWorldTile>(range*range);

		var topLeft = new Vector2Int(x - range, y - range);
		var bottomRight = new Vector2Int(x + range, y + range);
		for (int i = topLeft.X; i < bottomRight.X; i++)
		{
			for (int j = topLeft.Y; j < bottomRight.Y; j++)
			{
				if(Math.Pow(i-x,2) + Math.Pow(j-y,2) < Math.Pow(range,2)){
					if (IsPositionValid(new Vector2Int(i,j)))
					{
						tiles.Add(PseudoWorldManager.GetTileAtGrid(new Vector2Int(i,j),alternateDimension));
					}
				}
			}
		}

		if (lineOfSight != null)
		{
			foreach (var tile in new List<IWorldTile>(tiles))
			{
				if (CenterToCenterRaycast(pos, tile.Position, (Cover)lineOfSight, visibilityCast: true,ignoreControllables: true).hit)
				{
					tiles.Remove(tile);
				}
			}
		}

		return tiles;
	}

	public void WipeGrid()
	{

		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				_gridData[x, y].Wipe();
			}
		}
		
	}

	public void Update(float gameTime)
	{
		foreach (var tile in _gridData)
		{
			tile.Update(gameTime);
					
		}

		if (FovDirty)
		{
			CalculateFov();
		}

		

	}
    


	public MapData CurrentMap { get; set; }
	public bool Maploading { get; set; }


	[Serializable]
	public class MapData
	{

		public string Name = "New Map";
		public string Author = "Unknown";
		public int unitCount;
		public List<WorldTile.WorldTileData> Data = new List<WorldTile.WorldTileData>();


		public static MapData FromJSON(string json)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(json) ?? throw new InvalidOperationException();
		}

		public string ToJSON()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(this);
		}

	}
	public void SaveCurrentMapTo(string path)
	{
		List<WorldTile.WorldTileData> prefabData = new List<WorldTile.WorldTileData>();
		Vector2Int biggestPos = new Vector2Int(0, 0);//only save the bit of the map that has stuff
		Vector2Int smallestPos = new Vector2Int(100, 100);//only save the bit of the map that has stuff
		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				if (_gridData[x, y].NorthEdge != null || _gridData[x, y].WestEdge!=null)
				{
					if (x > biggestPos.X) {
						biggestPos.X = x;
					}
					if(x<smallestPos.X){
						smallestPos.X = x;
					}
					if (y > biggestPos.Y) {
						biggestPos.Y = y;
					}
					if(y<smallestPos.Y){
						smallestPos.Y = y;
					}
				}
			}
		}
			
		for (int x = smallestPos.X; x <= biggestPos.X; x++)
		{
			for (int y = smallestPos.Y; y <= biggestPos.Y; y++)
			{
	
				prefabData.Add(_gridData[x, y].GetData(true));
			}
		}

		CurrentMap.Data = prefabData;
		string json = CurrentMap.ToJSON();
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		if(path.Contains("/")){
			Directory.CreateDirectory(path.Remove(path.LastIndexOf("/")));
		}
		using (FileStream stream = File.Open(path, FileMode.Create))
		{
			using (StreamWriter writer = new StreamWriter(stream))
			{
				writer.Write(json);
			}
		}
	}


	public void LoadMap(string path)
	{
		if(!File.Exists(path))
			path =  Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)+ "/" + path;
		MapData mapData = MapData.FromJSON(File.ReadAllText(path)); 
		if(mapData.Name != CurrentMap.Name)
			LoadMap(mapData);
	}

	public void LoadMap(MapData mapData)
	{

		WipeGrid();
		CurrentMap = mapData;
		foreach (var worldTileData in mapData.Data)
		{
			LoadWorldTile(worldTileData,true);
		}

	}




	
	public void Init()
	{
		var data = new WorldObject.WorldObjectData();
		data.ID = -1;
		nullWorldObject = new WorldObject(null,null,data);
	}

	public string GetMapHash()
	{
		while (SequenceManager.SequenceRunning)
		{
			Thread.Sleep(100);
		}
		string hash = "";

		foreach (var tile in _gridData)
		{
                
			hash += tile.GetData().GetHash();
		}
			
        
	
		var md5 = hash.GetMD5();
		return md5;
	}
}