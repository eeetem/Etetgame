using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.AI;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;

using DefconNull.World.WorldObjects.Units.ReplaySequence;

using MD5Hash;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Riptide;
#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif


namespace DefconNull.World;

public  partial class WorldManager
{
	private readonly WorldTile[,] _gridData;
	private readonly List<PseudoTile?[,]> _pseudoGrids = new List<PseudoTile?[,]>();
	private readonly List<bool> _pseudoGridsInUse = new List<bool>();
		

	public readonly Dictionary<int, WorldObject> WorldObjects = new Dictionary<int, WorldObject>(){};
	public readonly ConcurrentDictionary<int, ConcurrentDictionary<int,WorldObject>> PseudoWorldObjects = new ();
	public readonly ConcurrentDictionary<int,Tuple<List<AIAction.PotentialAbilityActivation>,List<AIAction.PotentialAbilityActivation>>> CachedAttacks = new ();
	
	private int NextId;

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

	public WorldObject? GetObject(int id, int dimension = -1)
	{
		if(dimension != -1)
		{
			if(PseudoWorldObjects.TryGetValue(dimension, out var dimDIct) && dimDIct.TryGetValue(id, out var obj))
				return obj;//return if present, otherwise return the real object since there's no pseudo analogue
		}
		if(WorldObjects.TryGetValue(id, out var obj2))
			return obj2;

		return null;
	}

	public static bool IsPositionValid(Vector2Int pos)
	{
		return pos.X >= 0 && pos.X < 100 && pos.Y >= 0 && pos.Y < 100;
	}

	public void NextTurn(bool teamOneTurn)
	{
		foreach (var obj in WorldObjects.Values)
		{
			if (obj.UnitComponent != null && obj.UnitComponent.IsPlayer1Team == teamOneTurn)
			{
				obj.UnitComponent.StartTurn();
			}
		}

		foreach (var obj in WorldObjects.Values)
		{
			if (obj.UnitComponent != null && obj.UnitComponent.IsPlayer1Team != teamOneTurn)
			{
				obj.UnitComponent.EndTurn();
			}
		}

		foreach (var tile in _gridData)
		{
			tile.NextTurn();
		}
	}
	public static readonly object IdAquireLock = new object();
	public int GetNextId()
	{
		lock (IdAquireLock)
		{
			NextId++;
			while (WorldObjects.ContainsKey(NextId)) //skip all the server-side force assinged IDs
			{
				NextId++;
			}
		}

		return NextId;
	}

	public void LoadWorldTile(WorldTile.WorldTileData data)
	{
		//	Console.WriteLine("Loading tile at " + data.position);

		WorldTile tile = (WorldTile) GetTileAtGrid(data.position);
		tile.Wipe();
		if (data.Surface != null)
		{
			MakeWorldObjectFromData((WorldObject.WorldObjectData) data.Surface, tile);
		}

		if (data.NorthEdge != null)
		{
			MakeWorldObjectFromData((WorldObject.WorldObjectData) data.NorthEdge, tile);
		}


		if (data.WestEdge != null)
		{
			MakeWorldObjectFromData((WorldObject.WorldObjectData) data.WestEdge, tile);
		}

		if (data.UnitAtLocation != null)
		{

			MakeWorldObjectFromData((WorldObject.WorldObjectData) data.UnitAtLocation, tile);

		}

		if (data.ObjectsAtLocation != null)
		{
			foreach (var itm in data.ObjectsAtLocation)
			{
				MakeWorldObjectFromData(itm, tile);
			}
		}

	}
		

	public void MakeWorldObject(string? prefabName, Vector2Int position, Direction facing = Direction.North, int id = -1, Unit.UnitData? unitData = null)
	{
		WorldObject.WorldObjectData data = new WorldObject.WorldObjectData(prefabName);
		data.ID = id;
		data.Facing = facing;
		data.UnitData = unitData;
		MakeWorldObjectFromData(data, (WorldTile)GetTileAtGrid(position));
		return;
	}

	public void MakeWorldObjectFromData(WorldObject.WorldObjectData data, WorldTile tile)
	{
		
		if (data.ID != -1)//if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
		{
			DeleteWorldObject(data.ID); //delete existing object with same id, most likely caused by server updateing a specific entity
		}
		lock (createSync)
		{
			_createdObjects.Add(new Tuple<WorldObject.WorldObjectData, WorldTile>(data,tile));
		}

		return;
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


	private bool fovDirty;
	public void MakeFovDirty()
	{
		fovDirty = true;
	}

		
	private void CalculateFov()
	{
		fovDirty = false;
		
				

		foreach (var tile in _gridData)
		{
#if CLIENT
			tile.TileVisibility = Visibility.None;
#else
			tile.TileVisibility = new ValueTuple<Visibility, Visibility>(Visibility.None, Visibility.None);
#endif
		}

		List<Unit> units;
#if SERVER
		units = GameManager.GetAllUnits();
#else
		units = GameManager.GetTeamUnits(GameManager.IsPlayer1);
#endif
		Parallel.ForEach(units, unit =>
		{
			var unitSee = GetVisibleTiles(unit.WorldObject.TileLocation.Position, unit.WorldObject.Facing, unit.GetSightRange(), unit.Crouching);
			foreach (var visTuple in unitSee)
			{
				if(GetTileAtGrid(visTuple.Key).GetVisibility(unit.IsPlayer1Team) < visTuple.Value)
				{
					GetTileAtGrid(visTuple.Key).SetVisibility(unit.IsPlayer1Team,visTuple.Value);
#if CLIENT
					GetTileAtGrid(visTuple.Key).UnitAtLocation?.Spoted();
#endif
				}
				
			}
			unit.VisibleTiles = unitSee;
		});
		
	}
			
	public ConcurrentDictionary<Vector2Int,Visibility> GetVisibleTiles(Vector2Int pos, Direction dir, int range,bool crouched)
	{

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

				//i think this is for seeing over distant cover while crouched, dont quote me on that tho
				if (Vector2.Floor(cast.Value.CollisionPointLong) == Vector2.Floor(cast.Value.EndPoint) && cast.Value.HitObjId != -1 && GetObject(cast.Value.HitObjId).GetCover(true) != Cover.Full)
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
				tile = GetTileAtGrid(checkingSquare,pseudoLayer);
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
					}

					if (smokeLayers > 10)
					{
						result.CollisionPointLong = collisionPointlong;
						result.CollisionPointShort = collisionPointshort;
						result.hit = true;
						result.HitObjId = hitobj.ID;
						//if this is true then we're hitting a controllable form behind
						if (GetTileAtGrid(lastCheckingSquare).UnitAtLocation != null)
						{
							result.CollisionPointLong += -0.3f * dir;
							result.CollisionPointShort += -0.3f * dir;
						}

						return result;
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
							if (GetTileAtGrid(lastCheckingSquare).UnitAtLocation != null)
							{
								result.CollisionPointLong += -0.3f * dir;
								result.CollisionPointShort += -0.2f * dir;
							}
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
							if (GetTileAtGrid(lastCheckingSquare).UnitAtLocation != null)
							{
								result.CollisionPointLong += -0.3f * dir;
								result.CollisionPointShort += -0.3f * dir;
							}
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

	private List<int> objsToDel = new List<int>();
	public void DeleteWorldObject(WorldObject? obj)
	{
		if (obj == null) return;

		DeleteWorldObject(obj.ID);
			
	}
	public void DeleteWorldObject(int id)
	{
		lock (deleteSync)
		{
			objsToDel.Add(id);
		}
	}


	private void DestroyWorldObject(int id)
	{
		if (!WorldObjects.ContainsKey(id)) return;

		if (id < NextId)
		{
			NextId = id; //reuse IDs
		}

		WorldObject Obj = WorldObjects[id];
			
		GameManager.Forget(Obj);

#if  CLIENT
		if (Obj.UnitComponent != null)
		{
			GameLayout.UnRegisterUnit(Obj.UnitComponent);
		}
#endif		
		(Obj.TileLocation as WorldTile)?.Remove(id);
	
		WorldObjects.Remove(id);
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
		IWorldTile tileAtPos = GetTileAtGrid(loc,pseudoLayer);
		IWorldTile? tileInDir =null;
		if(IsPositionValid(tileAtPos.Position + Utility.DirToVec2(dir)))
		{
			tileInDir = Instance.GetTileAtGrid(tileAtPos.Position + Utility.DirToVec2(dir),pseudoLayer);
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
        
		return biggestCoverObj;
		
	}



	public IWorldTile GetTileAtGrid(Vector2Int pos, int pseudoGrid)
	{
		if(pseudoGrid != -1)
		{
			var grid = _pseudoGrids[pseudoGrid];
			if(grid[pos.X, pos.Y] == null)
			{
				grid[pos.X, pos.Y] = new PseudoTile(GetTileAtGrid(pos));
			}
			return grid[pos.X, pos.Y] ?? throw new InvalidOperationException();
				
		}
		return GetTileAtGrid(pos);
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
						tiles.Add(GetTileAtGrid(new Vector2Int(i,j),alternateDimension));
					}
				}
			}
		}

		if (lineOfSight != null)
		{
			foreach (var tile in new List<IWorldTile>(tiles))
			{
				if (CenterToCenterRaycast(pos, tile.Position, (Cover)lineOfSight, visibilityCast: false,ignoreControllables: true).hit)
				{
					tiles.Remove(tile);
				}
			}
		}

		return tiles;
	}

	public void WipeGrid()
	{
		lock (deleteSync)
		{
				
			
			foreach (var worldObject in WorldObjects.Values)
			{
				DeleteWorldObject(worldObject);
			}

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					_gridData[x, y].Wipe();
				}
			}
		}
	}

	public static readonly object deleteSync = new object();

	public static readonly object createSync = new object();
	public static readonly object TaskSync = new object();

		
	private static List<Tuple<Task,int>> NextFrameTasks = new List<Tuple<Task,int>>();

	public void RunNextAfterFrames(Task t, int frames = 1)
	{
		Task.Factory.StartNew(() =>
		{
			lock (TaskSync)
			{
				NextFrameTasks.Add(new Tuple<Task, int>(t, frames));
			}
		});
	}
	HashSet<WorldTile> tilesToUpdate = new HashSet<WorldTile>();
	public void Update(float gameTime)
	{
		
		lock (TaskSync)
		{
			List<Tuple<Task, int>> updatedList = new List<Tuple<Task, int>>();	
			foreach (var task in NextFrameTasks)
			{
				if (task.Item2 > 1)
				{
					updatedList.Add(new Tuple<Task, int>(task.Item1, task.Item2 - 1));
				}
				else
				{
					task.Item1.RunSynchronously();
				}
			}

			NextFrameTasks = updatedList;
		}

			

		lock (deleteSync)
		{


			foreach (var obj in objsToDel)
			{

				if (GameManager.GameState == GameState.Playing)
				{
					MakeFovDirty();
				}

#if SERVER
				if (GameManager.GameState != GameState.Playing)
				{
					if (WorldObjects.TryGetValue(obj, out var o))
					{
						tilesToUpdate.Add((WorldTile)o.TileLocation);
					}
				}
#endif
				DestroyWorldObject(obj);
				//	Console.WriteLine("deleting: "+obj);
			}

			objsToDel.Clear();

		}

		lock (createSync)
		{
				
			
			foreach (var WO in _createdObjects)
			{
				CreateWorldObj(WO);
				if (GameManager.GameState == GameState.Playing)
				{
					MakeFovDirty();
				}

				if (GameManager.GameState != GameState.Playing)
				{
						
#if SERVER
					//	Task t = new Task(delegate
					//	{
					tilesToUpdate.Add(WO.Item2);
					//});
					//RunNextAfterFrames(t,5);
#endif
						
				}
			}

				
			_createdObjects.Clear();
#if SERVER
				
			
			foreach (var tile in tilesToUpdate)
			{
				NetworkingManager.SendTileUpdate(tile);
			}
				

			tilesToUpdate.Clear();
#endif
		}
		foreach (var tile in _gridData)
		{
			tile.Update(gameTime);
					
		}

		foreach (var obj in WorldObjects.Values)
		{
			obj.Update(gameTime);
		}

		if (fovDirty)
		{
			CalculateFov();
		}


	}




	private void CreateWorldObj(Tuple<WorldObject.WorldObjectData, WorldTile> obj)
	{
		var data = obj.Item1;
		var tile = obj.Item2;
		if(data.ID == -1){
			data.ID = GetNextId();
		}
		if(WorldObjects.ContainsKey(data.ID)){ 
			DeleteWorldObject(WorldObjects[data.ID]);
			Task t = new Task(delegate
			{
				MakeWorldObjectFromData(obj.Item1, obj.Item2);
			});
			RunNextAfterFrames(t);
			return;
		}
			
		WorldObjectType type = PrefabManager.WorldObjectPrefabs[data.Prefab];
		WorldObject WO = new WorldObject(type, tile, data);
		WO.fliped = data.Fliped;
			
		type.Place(WO,tile,data);
			
		WorldObjects.EnsureCapacity(WO.ID + 1);
		WorldObjects[WO.ID] = WO;

		return;
	}

	public MapData CurrentMap { get; set; }
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


	public bool LoadMap(string path)
	{
		if (File.Exists(path))
		{
			MapData mapData = MapData.FromJSON(File.ReadAllText(path));
			return LoadMap(mapData);
		}

		return false;


	}

	public bool LoadMap(MapData mapData)
	{

		WipeGrid();
		CurrentMap = mapData;
		foreach (var worldTileData in mapData.Data)
		{
			LoadWorldTile(worldTileData);
		}

		return true;
	}




	public static readonly object PseudoGenLock = new object();
		
	public void AddUnitToPseudoWorld(Unit realunit, Vector2Int tilePosition, out Unit pseudoUnit, int dimension)
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

	public int CreatePseudoWorldWithUnit(Unit realunit, Vector2Int tilePosition, out Unit pseudoUnit, int copyDimension = -1)
	{

		int dimension= GetNextFreePseudoDimension();
		
	
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

	private int GetNextFreePseudoDimension()
	{
		lock (PseudoGenLock)
		{
			int dimension = 0;
			while (true)
			{
				if (_pseudoGridsInUse.Count <= dimension)
				{
					GenerateGridAtDimension(dimension);
					return dimension;
				}

				if (_pseudoGridsInUse[dimension] == false)
				{
					GenerateGridAtDimension(dimension);
					return dimension;
				}

				dimension++;
			}
		}
	}

	private void GenerateGridAtDimension(int d)
	{
		while(_pseudoGridsInUse.Count <= d)
		{
			_pseudoGridsInUse.Add(false);
		}
		_pseudoGridsInUse[d] = true;

		while(_pseudoGrids.Count <= d)
		{
			_pseudoGrids.Add(new PseudoTile?[100,100]);
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
	public void WipePseudoLayer(int dimension, bool NoAttackWipe = false)
	{
		lock (PseudoGenLock)
		{
			//	Console.WriteLine("Wiping grid in " + dimension);
		
			Array.Clear(_pseudoGrids[dimension]);
			PseudoWorldObjects[dimension].Clear();
			///Console.WriteLine("Wiping grid in " + dimension);

			if (!NoAttackWipe)
			{
				CachedAttacks[dimension].Item1.ForEach(y => y.Consequences.ForEach(z => z.Return()));
				CachedAttacks[dimension].Item2.ForEach(y => y.Consequences.ForEach(z => z.Return()));
			}
			
			
			CachedAttacks[dimension].Item1.Clear();
			CachedAttacks[dimension].Item2.Clear();
			_pseudoGridsInUse[dimension] = false;
		}
	}

	public void Init()
	{
		var data = new WorldObject.WorldObjectData();
		data.ID = -1;
		nullWorldObject = new WorldObject(null,null,data);
	}

	public void CacheAttacksInDimension(List<AIAction.PotentialAbilityActivation> attacks, int dimension, bool mainUnit)
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

	public bool GetCachedAttacksInDimension(ref List<AIAction.PotentialAbilityActivation> attacks, int dimension, bool mainUnit)
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

	public string GetMapHash()
	{
		string hash = "";
		lock (createSync)
		lock (deleteSync)
		{
			foreach (var tile in _gridData)
			{
				hash += tile.GetHash();
			}
			
		}
	
		var md5 = hash.GetMD5();
		return md5;
	}
}