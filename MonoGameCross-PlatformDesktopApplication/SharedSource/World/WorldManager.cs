using System;
using MultiplayerXeno;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
#if CLIENT
using MultiplayerXeno.UILayouts;
#endif
namespace MultiplayerXeno
{
	public  partial class WorldManager
	{
		private readonly WorldTile[,] _gridData;

		public readonly Dictionary<int, WorldObject> WorldObjects = new Dictionary<int, WorldObject>(){};
	
		private int NextId = 0;

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
			
			_gridData = new WorldTile[100, 100];
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					_gridData[x, y] = new WorldTile(new Vector2Int(x, y));
				}
			}
		}
		private static readonly List<WorldTile> _allTiles = new List<WorldTile>();
		public List<WorldTile> GetAllTiles()
		{
			_allTiles.Clear();

			foreach (var tile  in Instance._gridData)
			{
				_allTiles.Add(tile);
			}

			return _allTiles;
		}

		public WorldObject? GetObject(int id)
		{
			try
			{
				return WorldObjects[id];
			}
			catch (Exception e)
			{
				Console.WriteLine("failed to access object with id:"+id);
				return null;
			}

		}

		public static bool IsPositionValid(Vector2Int pos)
		{
			return (pos.X >= 0 && pos.X < 100 && pos.Y >= 0 && pos.Y < 100);
		}

		public void NextTurn(bool teamOneTurn)
		{
			foreach (var obj in WorldObjects.Values)
			{
				if (obj.ControllableComponent != null && obj.ControllableComponent.IsPlayerOneTeam == teamOneTurn)
				{
					obj.ControllableComponent.StartTurn();
				}
			}

			foreach (var obj in WorldObjects.Values)
			{
				if (obj.ControllableComponent != null && obj.ControllableComponent.IsPlayerOneTeam != teamOneTurn)
				{
					obj.ControllableComponent.EndTurn();
				}
			}

			foreach (var tile in _gridData)
			{
				tile.NextTurn();
			}
		}

		public int GetNextId()
		{
			
			NextId++;
			while (WorldObjects.ContainsKey(NextId)) //skip all the server-side force assinged IDs
			{
				NextId++;
			}

			return NextId;
		}

		public WorldTile LoadWorldTile(WorldTileData data)
		{

			

			WorldTile tile = GetTileAtGrid(data.position);
			tile.Wipe();
			if (data.Surface != null)
			{
				MakeWorldObjectFromData((WorldObjectData) data.Surface, tile);
			}

			if (data.NorthEdge != null)
			{
				MakeWorldObjectFromData((WorldObjectData) data.NorthEdge, tile);
			}


			if (data.WestEdge != null)
			{
				MakeWorldObjectFromData((WorldObjectData) data.WestEdge, tile);
			}
			if (data.ControllableAtLocation != null)
			{
				
				MakeWorldObjectFromData((WorldObjectData)data.ControllableAtLocation , tile);
				
			}

			if (data.ObjectsAtLocation != null)
			{
				foreach (var itm in data.ObjectsAtLocation)
				{
					MakeWorldObjectFromData(itm, tile);
				}
			}

			return tile;
		
		}
		

		public void MakeWorldObject(string prefabName, Vector2Int position, Direction facing = Direction.North, int id = -1, ControllableData? controllableData = null)
		{
			WorldObjectData data = new WorldObjectData(prefabName);
			data.Id = id;
			data.Facing = facing;
			data.ControllableData = controllableData;
			MakeWorldObjectFromData(data, GetTileAtGrid(position));
			return;
		}

		private void MakeWorldObjectFromData(WorldObjectData data, WorldTile tile)
		{
			lock (syncobj)
			{
				if (data.Id != -1)//if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
				{
					DeleteWorldObject(data.Id); //delete existing object with same id, most likely caused by server updateing a specific entity
				}
				createdObjects.Add(new Tuple<WorldObjectData, WorldTile>(data,tile));
			}

			return;
		}

		private List<Tuple<WorldObjectData, WorldTile>> createdObjects = new List<Tuple<WorldObjectData, WorldTile>>();

		public Visibility CanSee(Controllable controllable, Vector2 to, bool ignoreRange = false)
		{
			if (ignoreRange)
			{
				return CanSee(controllable.worldObject.TileLocation.Position, to, 200, controllable.Crouching);
			}
			return CanSee(controllable.worldObject.TileLocation.Position, to, controllable.GetSightRange(), controllable.Crouching);
		}

		public Visibility CanSee(Vector2Int From,Vector2Int to, int sightRange, bool crouched)//if truesight is false it will do a proper raycast otherwise you will only collide with already visible objects
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
				//i think this is for seeing over cover while crouched, dont quote me on that tho
				if (Vector2.Floor(cast.CollisionPointLong) == Vector2.Floor(cast.EndPoint) && cast.hitObjID != -1 && GetObject(cast.hitObjID).GetCover(true) != Cover.Full)
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

		public RayCastOutcome[] MultiCornerCast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover,bool visibilityCast = false,Cover? minHitCoverSameTile = null)
		{

			RayCastOutcome[] result = new RayCastOutcome[4];
			Vector2 startPos =startcell+new Vector2(0.5f,0.5f);
			Vector2 endpos = endcell;
			int index = 0;
			
			for (int j = 0; j < 4; j++)
			{
				
				switch (j)
				{
					case 0:
						endpos = endcell;
						break;
					case 1:
						endpos = endcell + new Vector2(0f, 0.99f);
						break;
					case 2:
						endpos = endcell + new Vector2(0.99f, 0f);
						break;
					case 3:
						endpos = endcell + new Vector2(0.99f, 0.99f);
						break;
					
				}
					
				Vector2 Dir = Vector2.Normalize(startcell - endcell);
				result[index] = Raycast(startPos+Dir/new Vector2(2.5f,2.5f), endpos, minHitCover,visibilityCast ,false,minHitCoverSameTile);
				index++;

			}
				
			



			return result;
		}


		public RayCastOutcome CenterToCenterRaycast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover, bool visibilityCast = false, bool ignoreControllables = false)
		{
			Vector2 startPos = (Vector2) startcell + new Vector2(0.5f, 0.5f);
			Vector2 endPos = (Vector2) endcell + new Vector2(0.5f, 0.5f);
			return Raycast(startPos, endPos, minHitCover, visibilityCast,ignoreControllables);
		}


		public RayCastOutcome Raycast(Vector2 startPos, Vector2 endPos,Cover minHitCover,bool visibilityCast = false,bool ignoreControllables = false,Cover? minHtCoverSameTile = null)
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
				lenght.X = ((startcell.X + 1) - startPos.X) * scalingFactor.X;
			}

			if (dir.Y < 0)
			{
				step.Y = -1;
				lenght.Y = (startPos.Y - startcell.Y) * scalingFactor.Y;
			}
			else
			{
				step.Y = 1;
				lenght.Y = ((startcell.Y+1) - startPos.Y) * scalingFactor.Y;
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

	
				WorldTile tile;
				result.Path.Add(new Vector2Int(checkingSquare.X,checkingSquare.Y));
				Vector2 collisionPointlong = ((totalLenght+0.05f) * dir) + (startPos);
				Vector2 collisionPointshort = ((totalLenght-0.1f) * dir) + (startPos);
				if (IsPositionValid(checkingSquare))
				{
					tile = GetTileAtGrid(checkingSquare);
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
					WorldTile tilefrom = GetTileAtGrid(lastCheckingSquare);

					WorldObject hitobj = tilefrom.GetCoverObj(Utility.Vec2ToDir(checkingSquare - lastCheckingSquare), visibilityCast, ignoreControllables,(lastCheckingSquare == startcell));

					foreach (var obj in tile.ObjectsAtLocation)
					{
						smokeLayers+=obj.Type.VisibilityObstructFactor;
					}

					if (smokeLayers > 10)
					{
						result.CollisionPointLong = collisionPointlong;
						result.CollisionPointShort = collisionPointshort;
						result.VectorToCenter = collisionVector;
						result.hit = true;
						result.hitObjID = hitobj.Id;
						//if this is true then we're hitting a controllable form behind
						if (GetTileAtGrid(lastCheckingSquare).ControllableAtLocation?.ControllableComponent != null)
						{
							result.CollisionPointLong += -0.3f * dir;
							result.CollisionPointShort += -0.3f * dir;
						}
						return result;
					}

					if (hitobj.Id != -1 )
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
								result.hitObjID = hitobj.Id;
								//if this is true then we're hitting a controllable form behind
								if (GetTileAtGrid(lastCheckingSquare).ControllableAtLocation?.ControllableComponent != null)
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
								result.hitObjID = hitobj.Id;
								if (GetTileAtGrid(lastCheckingSquare).ControllableAtLocation?.ControllableComponent != null)
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

			DeleteWorldObject(obj.Id);
			
		}
		public void DeleteWorldObject(int id)
		{
			
			
			lock (syncobj)
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
			if (Obj.ControllableComponent != null)
			{
				GameLayout.UnRegisterContollable(Obj.ControllableComponent);
			}
#endif
			Obj.TileLocation.Remove(id);
			WorldObjects.Remove(id);
			

		}

		public  WorldTile GetTileAtGrid(Vector2Int pos)
		{
			return _gridData[pos.X, pos.Y];
		}

		public List<WorldTile> GetTilesAround(Vector2Int pos, int range = 1, Cover? lineOfSight = null)
		{
			int x = pos.X;
			int y = pos.Y;
			List<WorldTile> tiles = new List<WorldTile>();

			var topLeft = new Vector2Int(x - range, y - range);
			var bottomRight = new Vector2Int(x + range, y + range);
			for (int i = topLeft.X; i < bottomRight.X; i++)
			{
				for (int j = topLeft.Y; j < bottomRight.Y; j++)
				{
					if(Math.Pow(i-x,2) + Math.Pow(j-y,2) < Math.Pow(range,2)){
						if (IsPositionValid(new Vector2Int(i,j)))
						{
							tiles.Add(GetTileAtGrid(new Vector2Int(i,j)));
						}
					}
				}
			}

			if (lineOfSight != null)
			{
				foreach (var tile in new List<WorldTile>(tiles))
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
			lock (syncobj)
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

		public static readonly object syncobj = new object();


		public void Update(float gameTime)
		{
			//Console.WriteLine(GridData[15,5].WestEdge);
			lock (syncobj)
			{
#if SERVER
				HashSet<WorldTile> tilesToUpdate = new HashSet<WorldTile>();
#endif
				foreach (var obj in objsToDel)
				{
#if CLIENT
					MakeFovDirty();
#endif
#if SERVER
					if (WorldObjects.ContainsKey(obj))
					{
						tilesToUpdate.Add(WorldObjects[obj].TileLocation);
					}
#endif
					DestroyWorldObject(obj);
					//	Console.WriteLine("deleting: "+obj);
				}
				objsToDel.Clear();

				foreach (var tile in _gridData)
				{
					tile.Update(gameTime);
					
				}

				foreach (var obj in WorldObjects.Values)
				{
					obj.Update(gameTime);
				}

				foreach (var WO in createdObjects)
				{
					var obj = CreateWorldObj(WO);
#if CLIENT
					MakeFovDirty();

					if (GameManager.GameState == GameState.Lobby)
					{
						if(obj.ControllableComponent == null){
	
							Camera.SetPos(WO.Item2.Position);
						}
					}
#endif
					
#if SERVER
					tilesToUpdate.Add(WO.Item2);
#endif
				}

				
				createdObjects.Clear();
#if SERVER
				//no need to update if we're loading an entire map
				if (!Loading)
				{
					foreach (var tile in tilesToUpdate)
					{
						Networking.SendTileUpdate(tile);
					}
				}

				tilesToUpdate.Clear();
#endif
				Loading = false;
#if CLIENT
				if(fovDirty){
					CalculateFov();
				}
#endif
				
				
			}


			//	WipeGrid();
			//	foreach (var obj in new List<WorldObject>(WorldObjects.Values))
			//	{

			//		gridData[obj.Position.X,obj.Position.Y].Add(obj.Id);

			//		}
		}

		private WorldObject CreateWorldObj(Tuple<WorldObjectData, WorldTile> obj)
		{
			var data = obj.Item1;
			var tile = obj.Item2;
			if(data.Id == -1){
				data.Id = GetNextId();
			}
				
			
			WorldObjectType type = PrefabManager.WorldObjectPrefabs[data.Prefab];
			WorldObject WO = new WorldObject(type, tile, data);
			WO.fliped = data.fliped;
			WO.Face(data.Facing,false);
			WorldTile newTile;
			if (WO.Type.Surface)
			{
				tile.Surface = WO;
			}
			else if (type.Edge)
			{
				switch (data.Facing)
				{
					case Direction.North:
						tile.NorthEdge = WO;
						break;
					case Direction.West:
						tile.WestEdge = WO;
						break;
					case Direction.East:
						newTile = GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.East));
						newTile.WestEdge = WO;
						WO.Face(Direction.West);
						WO.fliped = true;
						WO.TileLocation = newTile;
						break;
					case Direction.South:
						newTile = GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.South));
						newTile.NorthEdge = WO;
						WO.Face(Direction.North);
						WO.fliped = true;
						WO.TileLocation = newTile;
						break;
					default:
						throw new Exception("edge cannot be cornerfacing");
				}
			}
			else if(type.Controllable != null && data.ControllableData != null)
			{
				Controllable component = type.Controllable.Instantiate(WO, data.ControllableData.Value);
				WO.ControllableComponent = component;
#if CLIENT
				GameLayout.RegisterContollable(component);
#endif
				tile.ControllableAtLocation = WO;

				if (data.ControllableData.Value.JustSpawned)//bad spot for this but whatever for now
				{
					type.Controllable.SpawnEffect?.Apply(tile.Position);
				}
			}
			else
			{
				tile.PlaceObject(WO);
			}

			WorldObjects.EnsureCapacity(WO.Id + 1);
			WorldObjects[WO.Id] = WO;

			return WO;
		}

		public MapData CurrentMap { get; set; }
		
		public void SaveCurrentMapTo(string path)
		{
			List<WorldTileData> prefabData = new List<WorldTileData>();
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
			string json = CurrentMap.Serialise();
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

		private static bool Loading = false;

		public bool LoadMap(string path)
		{
			if (File.Exists(path))
			{
				MapData mapData = MapData.Deserialse(File.ReadAllText(path));
				return LoadMap(mapData);
			}
			else
			{
				return false;
			}

			
		}

		public bool LoadMap(MapData mapData)
		{
			if (Loading)
			{
				return false;
			}

			Loading = true;

			WipeGrid();
			CurrentMap = mapData;
			foreach (var worldTileData in mapData.Data)
			{
				Loading = true;
				LoadWorldTile(worldTileData);
			}
				
				

#if SERVER
				Thread.Sleep(2000);
				if (GameManager.Player1?.Connection != null) Networking.SendMapData(GameManager.Player1.Connection);
				if (GameManager.Player2?.Connection != null)  Networking.SendMapData(GameManager.Player2.Connection);
			foreach (var spec in GameManager.Spectators)
			{
				Networking.SendMapData(spec.Connection);
			}
			
#endif
			return true;
		}
	}
}