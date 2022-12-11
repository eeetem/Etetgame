using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using CommonData;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno.Pathfinding;

namespace MultiplayerXeno
{
	public  partial class WorldManager
	{
		private readonly WorldTile[,] _gridData;

		private readonly Dictionary<int, WorldObject> _worldObjects = new Dictionary<int, WorldObject>();
	
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

			foreach (var tile  in WorldManager.Instance._gridData)
			{
				_allTiles.Add(tile);
			}

			return _allTiles;
		}

		public WorldObject? GetObject(int id)
		{
			try
			{
				return _worldObjects[id];
			}
			catch (Exception e)
			{
				Console.WriteLine("failed to access object with id:"+id);
				return null;
			}

		}

		public static bool IsPositionValid(Vector2Int pos)
		{
			return (pos.X > 0 && pos.X < 100 && pos.Y > 0 && pos.Y < 100);
		}

		public void ResetControllables(bool teamOneTurn)
		{
			foreach (var obj in _worldObjects.Values)
			{
				if (obj.ControllableComponent != null && obj.ControllableComponent.IsPlayerOneTeam == teamOneTurn)
				{
					obj.ControllableComponent.StartTurn();
				}
			}

			foreach (var obj in _worldObjects.Values)
			{
				if (obj.ControllableComponent != null && obj.ControllableComponent.IsPlayerOneTeam != teamOneTurn)
				{
					obj.ControllableComponent.EndTurn();
				}
			}
		}

		public int GetNextId()
		{
			while (_worldObjects.ContainsKey(NextId)) //skip all the server-side force assinged IDs
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

			if (data.ObjectAtLocation != null)
			{
				MakeWorldObjectFromData((WorldObjectData) data.ObjectAtLocation, tile);
			}

			if (data.WestEdge != null)
			{
				MakeWorldObjectFromData((WorldObjectData) data.WestEdge, tile);
			}

#if SERVER
			Networking.SendTileUpdate(tile);
#endif
			return tile;
		}

		public int MakeWorldObject(string prefabName, Vector2Int position, Direction facing = Direction.North, int id = -1, ControllableData? controllableData = null)
		{
			WorldObjectData data = new WorldObjectData(prefabName);
			data.Id = id;
			data.Facing = facing;
			data.ControllableData = controllableData;
			return MakeWorldObjectFromData(data, GetTileAtGrid(position));
		}

		private int MakeWorldObjectFromData(WorldObjectData data, WorldTile tile)
		{
			lock (syncobj)
			{
				if (data.Id != -1)//if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
				{
					DeleteWorldObject(data.Id); //delete existing object with same id, most likely caused by server updateing a specific entity
				}


			
				createdObjects.Add(new Tuple<WorldObjectData, WorldTile>(data,tile));
			}

			return data.Id;
		}

		private List<Tuple<WorldObjectData, WorldTile>> createdObjects = new List<Tuple<WorldObjectData, WorldTile>>();


		public RayCastOutcome[] MultiCornerCast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover, bool ignoreControllables = false,Cover ignoreEdgesOnSameTilenCover = Cover.None)
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
					result[index] = Raycast(startPos+Dir/new Vector2(2.5f,2.5f), endpos, minHitCover, ignoreControllables,ignoreEdgesOnSameTilenCover);
					index++;

				}
				
			



			return result;
		}


		public RayCastOutcome CenterToCenterRaycast(Vector2Int startcell, Vector2Int endcell, Cover minHitCover, bool ignoreControllables = false)
		{
			Vector2 startPos = (Vector2) startcell + new Vector2(0.5f, 0.5f);
			Vector2 endPos = (Vector2) endcell + new Vector2(0.5f, 0.5f);
			return Raycast(startPos, endPos, minHitCover, ignoreControllables);
		}


		public RayCastOutcome Raycast(Vector2 startPos, Vector2 endPos,Cover minHitCover,bool ignoreControllables = false,Cover ignoreEdgesOnSameTilenCover = Cover.None)
		{
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

			float totalLenght = 0f;

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
				if (IsPositionValid(checkingSquare))
				{
					tile = GetTileAtGrid(checkingSquare);
				}
				else
				{
					result.hit = false;
					return result;
				}

				Vector2 collisionPoint = (totalLenght * dir) + (startPos);
				Vector2 collisionVector = (Vector2) tile.Position + new Vector2(0.5f, 0.5f) - collisionPoint;

				
				Direction direc = Utility.ToClampedDirection(collisionVector);

			
				if (IsPositionValid(lastCheckingSquare))
				{
					WorldTile tilefrom = GetTileAtGrid(lastCheckingSquare);
					//Console.WriteLine("Direction: "+ direc +" Reverse Dir: "+ (direc+4));

					 WorldObject hitobj = tilefrom.GetCoverObj(Utility.Vec2ToDir(checkingSquare - lastCheckingSquare), ignoreControllables);

					 Cover c = hitobj.GetCover();
					if (hitobj.Id != -1 && (c <= ignoreEdgesOnSameTilenCover|| !Utility.DoesEdgeBorderTile(hitobj,startcell))) //this is super hacky and convoluted
					{
						
						if (c >= minHitCover)
						{
							result.CollisionPoint = collisionPoint;
							result.VectorToCenter = collisionVector;
							result.hit = true;
							result.hitObjID = hitobj.Id;
							return result;
						}
					}

				}

				if (endcell == checkingSquare)
				{
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
			if (!_worldObjects.ContainsKey(id)) return;

			if (id < NextId)
			{
				NextId = id; //reuse IDs
			}

			WorldObject Obj = _worldObjects[id];
#if  CLIENT
			if (Obj.ControllableComponent != null)
			{
				UI.Controllables.Remove(Obj.ControllableComponent);
			}
#endif
			

			Obj.TileLocation.Remove(id);
			//obj.dispose()
			_worldObjects.Remove(id);

		}

		public  WorldTile GetTileAtGrid(Vector2Int pos)
		{

			return _gridData[pos.X, pos.Y];
	
		}

		private  void WipeGrid()
		{
			foreach (var worldObject in _worldObjects.Values)
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

		public static readonly object syncobj = new object();


		public void Update(float gameTime)
		{
			//Console.WriteLine(GridData[15,5].WestEdge);
			lock (syncobj)
			{
				
				foreach (var obj in objsToDel)
				{
#if CLIENT
					MakeFovDirty();
#endif
					DestroyWorldObject(obj);
					Console.WriteLine("deleting: "+obj);
				}
				objsToDel.Clear();

				foreach (var obj in _worldObjects.Values)
				{
					obj.Update(gameTime);
				}

				foreach (var WO in createdObjects)
				{
					#if CLIENT
					MakeFovDirty();
#endif
					var obj = CreateWorldObj(WO);
					Console.WriteLine("creatinig: "+obj.Id);
#if SERVER
			//this is resulted when a singlar object is created outside the world manager(the world manager deals with full tiles exclusively)
			//this could cause issues if multiple objects are cerated on a tile in quick succsesion - but other than loading the map(which happends tile by tile rather than object by object) it shouldnt happen
			
				Networking.SendTileUpdate(WO.Item2);

#endif
				}		
				createdObjects.Clear();
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
				
			
			WorldObjectType type = PrefabManager.Prefabs[data.Prefab];
			WorldObject WO = new WorldObject(type, data.Id, tile);
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
			else
			{
				tile.ObjectAtLocation = WO;
			}

			if (type.Controllable != null && data.ControllableData != null)
			{
				Controllable component = type.Controllable.Instantiate(WO, data.ControllableData.Value);
				WO.ControllableComponent = component;
#if CLIENT
				UI.Controllables.Add(component);
#endif
				
			}

			_worldObjects.EnsureCapacity(WO.Id + 1);
			_worldObjects[WO.Id] = WO;

			return WO;
		}


		public void SaveData(string path)
		{
			List<WorldTileData> prefabData = new List<WorldTileData>();
			Vector2Int biggestPos = new Vector2Int(0, 0);//only save the big of the map that has stuff
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					if (_gridData[x, y].NorthEdge != null || _gridData[x, y].WestEdge!=null || _gridData[x, y].ObjectAtLocation != null)
					{
						if (x > biggestPos.X)
						{
							biggestPos.X = x;
						}
						
						if (y > biggestPos.Y)
						{
							biggestPos.Y = y;
						}
					}
				}
			}
			
			for (int x = 0; x <= biggestPos.X; x++)
			{
				for (int y = 0; y <= biggestPos.Y; y++)
				{
					prefabData.Add(_gridData[x, y].GetData());
				}
			}

			using (FileStream stream = File.Open(path, FileMode.Create))
			{
				//		TextWriter textWriter = new StreamWriter(stream);
//				JsonWriter jsonWriter = new JsonTextWriter(textWriter);
				//		JsonSerializer jsonSerializer = new JsonSerializer();
				//bformatter.Serialize(stream, prefabData);	
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(stream, prefabData);
				//		jsonSerializer.Serialize(textWriter, prefabData);
			}
		}

		public void LoadData(byte[] data)
		{
			using (Stream dataStream = new MemoryStream(data))
			{
				BinaryFormatter bformatter = new BinaryFormatter();
				List<WorldTileData> prefabData = bformatter.Deserialize(dataStream) as List<WorldTileData>;
				WipeGrid();

				if (prefabData != null)
					foreach (var worldTileData in prefabData)
					{
						LoadWorldTile(worldTileData);
					}
			}
		}
	}
}