using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;

using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
#if CLIENT
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.World
{
	public  partial class WorldManager
	{
		private readonly WorldTile[,] _gridData;
		private readonly List<PseudoTile?[,]> _pseudoGrids = new List<PseudoTile?[,]>();
		private readonly List<bool> _pseudoGridsInUse = new List<bool>();
		

		public readonly Dictionary<int, WorldObject> WorldObjects = new Dictionary<int, WorldObject>(){};
		public readonly Dictionary<int, Dictionary<int,WorldObject>> PseudoWorldObjects = new ();
	
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
			try
			{
				if(dimension != -1)
				{
					if(PseudoWorldObjects.TryGetValue(dimension, out var dimDIct) && dimDIct.TryGetValue(id, out var obj))
						return obj;//return if present, otherwise return the real object since there's no pseudo analogue
				}
				return WorldObjects[id];
			}
			catch (Exception)
			{
				Console.WriteLine("failed to access object with id:"+id);
				return null;
			}

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
			lock (createSync)
			{
				if (data.ID != -1)//if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
				{
					DeleteWorldObject(data.ID); //delete existing object with same id, most likely caused by server updateing a specific entity
				}
				_createdObjects.Add(new Tuple<WorldObject.WorldObjectData, WorldTile>(data,tile));
			}

			return;
		}

		private readonly List<Tuple<WorldObject.WorldObjectData, WorldTile>> _createdObjects = new List<Tuple<WorldObject.WorldObjectData, WorldTile>>();

		public Visibility CanSee(Unit unit, Vector2 to, bool ignoreRange = false)
		{
			if (ignoreRange)
			{
				return CanSee(unit.WorldObject.TileLocation.Position, to, 200, unit.Crouching);
			}
			return CanSee(unit.WorldObject.TileLocation.Position, to, unit.GetSightRange(), unit.Crouching);
		}

		public Visibility CanTeamSee(Vector2 Position, bool Team1)
		{
			Visibility vis = Visibility.None;
			foreach (var u in GameManager.GetTeamUnits(Team1))
			{
				var tempVis = CanSee(u, Position);
				if (tempVis > vis)
				{
					vis = tempVis;
				}
			}

			return vis;
		}

		public Visibility CanSee(Vector2Int From,Vector2Int to, int sightRange, bool crouched)
		{
			if(Vector2.Distance(From, to) > sightRange)
			{
				return Visibility.None;
			}
			RayCastOutcome[] FullCasts;
			RayCastOutcome[] PartalCasts;
			if (crouched)
			{
				FullCasts = MultiCornerCast(From, to, Cover.High, true);//full vsson does not go past high cover and no partial sigh
				PartalCasts = Array.Empty<RayCastOutcome>();
			}
			else
			{
				FullCasts = MultiCornerCast(From, to, Cover.High, true,Cover.Full);//full vission does not go past high cover
				PartalCasts  = MultiCornerCast(From, to, Cover.Full, true);//partial visson over high cover
							
			}

			foreach (var cast in FullCasts)
			{
				if (!cast.hit)
				{
					return Visibility.Full;
				}
				//i think this is for seeing over distant cover while crouched, dont quote me on that tho
				if (Vector2.Floor(cast.CollisionPointLong) == Vector2.Floor(cast.EndPoint) && cast.HitObjId != -1 && GetObject(cast.HitObjId).GetCover(true) != Cover.Full)
				{
					return Visibility.Partial;
				}
			}
			foreach (var cast in PartalCasts)
			{
				if (!cast.hit)
				{
					return Visibility.Partial;
				}
			}

			return Visibility.None;
		}

		public struct RayCastOutcome//fuck the network library holy moly
		{
			public Vector2 CollisionPointLong= new Vector2(0, 0);
			public Vector2 CollisionPointShort= new Vector2(0, 0);
			public Vector2 StartPoint;
			public Vector2 EndPoint;
			public Vector2 VectorToCenter;
			public List<Vector2Int> Path;
			public int HitObjId{ get; set; }

			public bool hit{ get; set; }

			public RayCastOutcome(Vector2 start, Vector2 end)
			{
				CollisionPointLong = new Vector2(0, 0);
				CollisionPointShort = new Vector2(0, 0);
				hit = false;
				EndPoint = end;
				HitObjId = -1;
				StartPoint = start;
				Path = new List<Vector2Int>();
	
			}
		}
		public RayCastOutcome[] MultiCornerCast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover,bool visibilityCast = false,Cover? minHitCoverSameTile = null)
		{
			RayCastOutcome[] result = new RayCastOutcome[4];
			Vector2 startPos =startcell+new Vector2(0.5f,0.5f);
			Vector2 Dir = Vector2.Normalize(startcell - endcell);
			startPos += Dir / new Vector2(2.5f, 2.5f);
			var t1 = Task.Run(() =>
			{
				result[0] = Raycast(startPos, endcell, minHitCover,visibilityCast ,false,minHitCoverSameTile);
			});
			var t2 = Task.Run(() =>
			{
				result[1] = Raycast(startPos, endcell+ new Vector2(0f, 0.99f), minHitCover,visibilityCast ,false,minHitCoverSameTile);
			});
			var t3 = Task.Run(() =>
			{
				result[2] = Raycast(startPos,  endcell + new Vector2(0.99f, 0f), minHitCover,visibilityCast ,false,minHitCoverSameTile);
			});
			var t4 = Task.Run(() =>
			{
				result[3] = Raycast(startPos, endcell + new Vector2(0.99f, 0.99f), minHitCover,visibilityCast ,false,minHitCoverSameTile);
			});
			
			Task.WaitAll(t1, t2, t3, t4);
			return result;
		}


		public RayCastOutcome CenterToCenterRaycast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover, bool visibilityCast = false, bool ignoreControllables = false)
		{
			Vector2 startPos = (Vector2) startcell + new Vector2(0.5f, 0.5f);
			Vector2 endPos = (Vector2) endcell + new Vector2(0.5f, 0.5f);
			return Raycast(startPos, endPos, minHitCover, visibilityCast,ignoreControllables);
		}


		public RayCastOutcome Raycast(Vector2 startPos, Vector2 endPos, Cover minHitCover, bool visibilityCast = false, bool ignoreControllables = false, Cover? minHtCoverSameTile = null, int pseudoLayer = -1)
		{
			if (minHtCoverSameTile == null)
			{
				minHtCoverSameTile = minHitCover;
			}

			Vector2Int startcell = new Vector2Int((int)Math.Floor(startPos.X), (int)Math.Floor(startPos.Y));
			Vector2Int endcell = new Vector2Int((int)Math.Floor(endPos.X), (int)Math.Floor(endPos.Y));


			RayCastOutcome result = new RayCastOutcome(startPos, endPos);
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
				result.Path.Add(new Vector2Int(checkingSquare.X,checkingSquare.Y));
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
					WorldObject hitobj = GetCoverObj(lastCheckingSquare,Utility.Vec2ToDir(checkingSquare - lastCheckingSquare), visibilityCast, ignoreControllables,lastCheckingSquare == startcell,pseudoLayer);

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
							result.VectorToCenter = collisionVector;
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
								result.VectorToCenter = collisionVector;
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
						else
						{
							if (c >= minHitCover)
							{
								result.CollisionPointLong = collisionPointlong;
								result.CollisionPointShort = collisionPointshort;
								result.VectorToCenter = collisionVector;
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

				if (endcell == checkingSquare)
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
	

		public WorldObject GetCoverObj(Vector2Int loc,Direction dir, bool visibilityCover = false,bool ignoreContollables = false, bool ignoreObjectsAtLoc = true, int pseudoLayer = -1)
		{
			var data = new WorldObject.WorldObjectData();
			data.ID = -1;
			WorldObject biggestCoverObj = new WorldObject(null,null,data);
			dir = Utility.NormaliseDir(dir);

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
			List<IWorldTile> tiles = new List<IWorldTile>();

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
					if (CenterToCenterRaycast(pos, tile.Position, (Cover)lineOfSight, false,true).hit)
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
#if CLIENT
					if (GameManager.GameState == GameState.Playing)
					{
						MakeFovDirty();
					}

#endif
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
					//	Console.WriteLine("creating: " + WO.Item2 + " at " + WO.Item1);
					CreateWorldObj(WO);
#if CLIENT
					if (GameManager.GameState == GameState.Playing)
					{
						MakeFovDirty();
					}
#endif
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
#if CLIENT
			if(fovDirty){
				CalculateFov();
			}
#endif
				
		

			if (_currentSequenceTasks.Count == 0)
			{
				if (SequenceQueue.Count > 0)
				{
					var task = SequenceQueue.Dequeue();
					while (!task.ShouldDo())
					{
						if(SequenceQueue.Count == 0){
							return;
						}
						task = SequenceQueue.Dequeue();
					}
					Console.WriteLine("runnin sequnce task: "+task.SqcType);
					_currentSequenceTasks.Add(task.GenerateTask());
					_currentSequenceTasks.Last().Start();
					

					
					//batch tile updates and other things
					while (true)
					{
						if (SequenceQueue.Count == 0)
						{
							break;
						}
				
						if(!SequenceQueue.Peek().CanBatch || !SequenceQueue.Peek().ShouldDo()) break;
						
                        
						_currentSequenceTasks.Add(SequenceQueue.Dequeue().GenerateTask());
						_currentSequenceTasks.Last().Start();
					} 

				}
			}
			else if (_currentSequenceTasks.TrueForAll((t) => t.Status != TaskStatus.Running))
			{
				foreach (var t in _currentSequenceTasks)
				{
					if (t.Status == TaskStatus.RanToCompletion)
					{
					//	Console.WriteLine("sequence task finished");
					}
					else if (t.Status == TaskStatus.Faulted)
					{
						Console.WriteLine("Sequence task failed");
						Console.WriteLine(t.Status);
						throw t.Exception!;
					}else{
						Console.WriteLine("undefined sequence task state");
					}
				}
				_currentSequenceTasks.Clear();
			}
		}




		private List<Task> _currentSequenceTasks = new List<Task>();
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
					prefabData.Add(_gridData[x, y].GetData());
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


		private static readonly Queue<SequenceAction> SequenceQueue = new Queue<SequenceAction>();

		public bool SequenceRunning => SequenceQueue.Count > 0 || _currentSequenceTasks.Count > 0;
		public void AddSequence(SequenceAction action)
		{
			SequenceQueue.Enqueue(action);
			Console.WriteLine("adding action "+action.SqcType+" to sequence");
		}

		public void AddSequence(IEnumerable<SequenceAction> actions)
		{
			foreach (var a in actions)
			{
				AddSequence(a);
			}
		}


		public static readonly object PseudoGenLock = new object();
		public int PlaceUnitInPseudoWorld(Unit realunit, Vector2Int tilePosition, out Unit pseudoUnit)
		{
			lock (PseudoGenLock)
			{
			
				int dimension = GetNextFreePseudoDimension();
		
				Console.WriteLine("placing unit at: "+tilePosition+" in dimension: "+dimension +" with ID "+realunit.WorldObject.ID);
				WorldObject.WorldObjectData data = realunit.WorldObject.GetData();
				WorldObject pseudoObj = new WorldObject(realunit.WorldObject.Type, GetTileAtGrid(tilePosition,dimension), data);
				pseudoObj.UnitComponent = new Unit(pseudoObj,realunit.Type,realunit.GetData());
				realunit.Abilities.ForEach(extraAction => { pseudoObj.UnitComponent.Abilities.Add((IUnitAbility) extraAction.Clone()); });
				pseudoUnit = pseudoObj.UnitComponent;

				if (!PseudoWorldObjects.ContainsKey(dimension))
				{
					PseudoWorldObjects.Add(dimension, new Dictionary<int, WorldObject>());
				}
				PseudoWorldObjects[dimension].Add(pseudoObj.ID,pseudoObj);
			
				((PseudoTile)GetTileAtGrid(realunit.WorldObject.TileLocation.Position, dimension)).ForceNoUnit = true;//remove old position from world

			
		
				return dimension;
			}
		}

		private int GetNextFreePseudoDimension()
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

		public void WipePseudoLayer(int dimension)
		{
			lock (PseudoGenLock)
			{
				_pseudoGridsInUse[dimension] = false;
				_pseudoGrids[dimension] = new PseudoTile?[100, 100];
				PseudoWorldObjects[dimension].Clear();
			}
		}
	}
}