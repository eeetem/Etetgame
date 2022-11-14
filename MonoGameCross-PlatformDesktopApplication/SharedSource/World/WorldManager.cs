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

		private Dictionary<int, WorldObject> WorldObjects = new Dictionary<int, WorldObject>();
	
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



		public WorldObject GetObject(int id)
		{
			return WorldObjects[id];
		}

		public bool IsPositionValid(Vector2Int pos)
		{
			return (pos.X > 0 && pos.X < 100 && pos.Y > 0 && pos.Y < 100);
		}

		public void ResetControllables(bool teamOneTurn)
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
		}

		public int GetNextId()
		{
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

		public WorldObject MakeWorldObject(string prefabName, Vector2Int position, Direction facing = Direction.North, int id = -1, ControllableData? controllableData = null)
		{
			WorldObjectData data = new WorldObjectData(prefabName);
			data.Id = id;
			data.Facing = facing;
			data.ControllableData = controllableData;
			return MakeWorldObjectFromData(data, GetTileAtGrid(position),true);
		}

		private WorldObject MakeWorldObjectFromData(WorldObjectData data, WorldTile tile, bool UpdateTile = false)
		{
			if (data.Id == -1)
			{
				data.Id = GetNextId();
			}
			else
			{
				DeleteWorldObject(data.Id); //delete existing object with same id, most likely caused by server updateing a specific entity
			}

			WorldObjectType type = PrefabManager.Prefabs[data.Prefab];
			WorldObject WO = new WorldObject(type, data.Id, tile);
			WO.fliped = data.fliped;
			WO.Face(data.Facing);

			WorldTile newTile;
			if (type.Surface)
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
			}

			lock (syncobj)
			{
				
			
				WorldObjects.EnsureCapacity(data.Id + 1);
				WorldObjects[data.Id] = WO;
#if SERVER
			//this is resulted when a singlar object is created outside the world manager(the world manager deals with full tiles exclusively)
			//this could cause issues if multiple objects are cerated on a tile in quick succsesion - but other than loading the map(which happends tile by tile rather than object by object) it shouldnt happen
			if (UpdateTile)
			{
				Networking.SendTileUpdate(WO.TileLocation);
			}
#endif
			}
			return WO;
		}

	

	

		public RayCastOutcome Raycast(Vector2Int startcell, Vector2Int endcell,Cover minHitCover,bool ignoreControllables = false)
		{

			Vector2 startPos = (Vector2) startcell + new Vector2(0.5f, 0.5f);
			Vector2 endPos = (Vector2) endcell + new Vector2(0.5f, 0.5f);
			RayCastOutcome result = new RayCastOutcome(startPos, endPos);
			if (startcell == endcell)
			{
				result.hit = false;
				return result;
			}
			

			Vector2Int checkingSquare = new Vector2Int(startcell.X, startcell.Y);

			Vector2 dir = endPos - startPos;
			dir.Normalize();

			Vector2Int step = new Vector2Int(dir.X > 0 ? 1 : -1, dir.Y > 0 ? 1 : -1);

			float slope = dir.Y / dir.X;
			float inverseSlope = dir.X / dir.Y;

			Vector2 scalingFactor = new Vector2((float) Math.Sqrt(1 + slope * slope), (float) Math.Sqrt(1 + inverseSlope * inverseSlope));

			Vector2 lenght = new Vector2Int(0, 0);

			lenght.X = 0.5f * scalingFactor.X;
			lenght.Y = 0.5f * scalingFactor.Y;

			float totalLenght = 0f;

			while (true)
			{
				if (lenght.X < lenght.Y)
				{
					checkingSquare.X += step.X;
					totalLenght = lenght.X;
					lenght.X += scalingFactor.X;
				}
				else
				{
					checkingSquare.Y += step.Y;
					totalLenght = lenght.Y;
					lenght.Y += scalingFactor.Y;
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

				//result.CollisionPoint.Add(collisionPoint);

				Vector2 collisionVector = (Vector2) tile.Position + new Vector2(0.5f, 0.5f) - collisionPoint;

				
				Direction direc = Utility.ToClampedDirection(collisionVector);
				WorldTile tilefrom = GetTileAtGrid(tile.Position + Utility.DirToVec2(direc));
				//Console.WriteLine("Direction: "+ direc +" Reverse Dir: "+ (direc+4));

				WorldObject hitobj = tilefrom.GetCoverObj(direc + 4,ignoreControllables);
				if (hitobj.Id != -1 && !( hitobj.TileLocation.Position == startcell && (hitobj.TileLocation.ObjectAtLocation == hitobj || hitobj.GetCover() != Cover.Full)))//this is super hacky and convoluted
				{
					Cover c = hitobj.GetCover();
					if (c >= minHitCover)
					{
						result.CollisionPoint = collisionPoint;
						result.VectorToCenter = collisionVector;
						result.hit = true;
						result.hitObjID = hitobj.Id;
						return result;
					}
				}

			

				if (endcell == checkingSquare)
				{
					result.hit = false;
					return result;
				}
			}
		}

		public void DeleteWorldObject(WorldObject? obj)
		{
			if (obj == null) return;

			DeleteWorldObject(obj.Id);
			
		}

		public void DeleteWorldObject(int id)
		{
			if (!WorldObjects.ContainsKey(id)) return;

			if (id < NextId)
			{
				NextId = id; //reuse IDs
			}

			WorldObject Obj = WorldObjects[id];
			Obj.TileLocation.Remove(id);
			//obj.dispose()
			WorldObjects.Remove(id);
			#if CLIENT
			CalculateFov();
			#endif
		}

		public  WorldTile GetTileAtGrid(Vector2Int pos)
		{

			return _gridData[pos.X, pos.Y];
	
		}

		private  void WipeGrid()
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

		public static readonly object syncobj = new object();


		public void Update(float gameTime)
		{
			//Console.WriteLine(GridData[15,5].WestEdge);
			lock (syncobj)
			{

				foreach (var obj in WorldObjects.Values)
				{
					obj.Update(gameTime);
				}
			}


			//	WipeGrid();
			//	foreach (var obj in new List<WorldObject>(WorldObjects.Values))
			//	{

			//		gridData[obj.Position.X,obj.Position.Y].Add(obj.Id);

			//		}
		}


		public void SaveData(string path)
		{
			List<WorldTileData> prefabData = new List<WorldTileData>();

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
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