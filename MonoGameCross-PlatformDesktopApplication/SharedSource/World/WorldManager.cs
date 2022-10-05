using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using CommonData;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno.Pathfinding;


namespace MultiplayerXeno
{
	public static partial class WorldManager
	{
		private static WorldTile[,] gridData = new WorldTile[100,100];

		private static Dictionary<int,WorldObject> WorldObjects = new Dictionary<int,WorldObject>();

		private static int NextId = 0;


		public static void Init()
		{
			MakeGrid();


		}

		public static WorldObject GetObject(int id)
		{
			return WorldObjects[id];
		}

		public static bool IsPositionValid(Vector2Int pos)
		{
			return (pos.X > 0 && pos.X < 100 && pos.Y >0 && pos.Y <100);
		}

		public static Vector2Int DirToVec2(Direction dir)
		{
			dir = Utility.NormaliseDir(dir);
			switch (dir)
			{
				case Direction.East:
					return new Vector2Int(1, 0);
				case Direction.North:
					return new Vector2Int(0, -1);
				case Direction.NorthEast:
					return new Vector2Int(1, -1);
				case Direction.West:
					return new Vector2Int(-1, 0);
				case Direction.South:
					return new Vector2Int(0, 1);
				case Direction.SouthWest:
					return new Vector2Int(-1, 1);
				case Direction.SouthEast:
					return new Vector2Int(1, 1);
				case Direction.NorthWest:
					return new Vector2Int(-1, -1);
				
			}

			throw new Exception("impossible direction");
		}
		public static Direction Vec2ToDir(Vector2Int vec2)
		{

			switch (vec2)
			{
				case (1, 0):
					return Direction.East;
				case (0, -1):
					return Direction.North;
				case (1, -1):
					return Direction.NorthEast;
				case (-1, 0):
					return Direction.West;
				case (0, 1):
					return Direction.South;
				case (-1, 1):
					return Direction.SouthWest;
				case (1, 1):
					return Direction.SouthEast;
				case (-1, -1):
					return Direction.NorthWest;
			}

			throw new Exception("incorrect vector");
		}

		public static void ResetControllables(bool teamOneTurn)
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

		
		public static int GetNextId()
		{
			
			while (WorldObjects.ContainsKey(NextId))//skip all the server-side force assinged IDs
			{
				NextId++;
			}

			return NextId;
		}

		public static WorldTile LoadWorldTile(WorldTileData data)
		{
			WorldTile tile = GetTileAtGrid(data.position);
			tile.Wipe();
			if (data.Surface != null)
			{
				MakeWorldObject((WorldObjectData)data.Surface, tile);
			}
			if (data.NorthEdge != null)
			{
				MakeWorldObject((WorldObjectData)data.NorthEdge, tile);
			}
			if (data.ObjectAtLocation != null)
			{
				MakeWorldObject((WorldObjectData)data.ObjectAtLocation, tile);
			}
			if (data.WestEdge != null)
			{
				MakeWorldObject((WorldObjectData)data.WestEdge, tile);
			}


		#if SERVER
			Networking.SendTileUpdate(tile);
		#endif
			return tile;
		}

		public static WorldObject MakeWorldObjectPublically(string prefabName, Vector2Int position, Direction facing = Direction.North, int id = -1, ControllableData? controllableData = null)
		{
			WorldObjectData data = new WorldObjectData(prefabName);
			data.Id = id;
			data.Facing = facing;
			data.ControllableData = controllableData;
			return MakeWorldObject(data,GetTileAtGrid(position),true);
		}

	

		private static WorldObject MakeWorldObject(WorldObjectData data,WorldTile tile, bool UpdateTile = false)
		{
		

			if (data.Id == -1)
			{
				data.Id = GetNextId();

			}
			else
			{
				DeleteWorldObject(data.Id);//delete existing object with same id, most likely caused by server updateing a specific entity
			}


			WorldObjectType type = PrefabManager.Prefabs[data.Prefab];
			WorldObject WO = new WorldObject(type,data.Id,tile);
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
						newTile = GetTileAtGrid(tile.Position + DirToVec2(Direction.East));
						newTile.WestEdge = WO;
						WO.TileLocation = newTile;
						break;
					case Direction.South:
						newTile = GetTileAtGrid(tile.Position + DirToVec2(Direction.South));
						newTile.NorthEdge = WO;
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
				Controllable component = type.Controllable.Instantiate(WO,data.ControllableData.Value.Team1);
				WO.ControllableComponent = component;
			}


		

			
			WorldObjects.EnsureCapacity(data.Id+1);			
			WorldObjects[data.Id] = WO;
#if SERVER
			//this is resulted when a singlar object is created outside the world manager(the world manager deals with full tiles exclusively)
			//this could cause issues if multiple objects are cerated on a tile in quick succsesion - but other than loading the map(which happends tile by tile rather than object by object) it shouldnt happen
			if (UpdateTile)
			{
				Networking.SendTileUpdate(WO.TileLocation);
			}
#endif
			

			return WO;
	

		}

		

		public struct RayCastOutcome
		{
			public  List<Vector2> CollisionPoint;
			public  Vector2 StartPoint;
			public Vector2 EndPoint;
			public  Vector2 VectorToCenter;

			public bool hit;

			public RayCastOutcome(Vector2 start, Vector2 end)
			{
				this.hit = false;
				VectorToCenter = new Vector2(0,0);
				EndPoint = end;
				StartPoint = start;
				CollisionPoint = new List<Vector2>();
			}

			
		}



		public static RayCastOutcome Raycast(Vector2Int startcell, Vector2Int endcell)
		{

			Vector2 startPos = (Vector2)startcell + new Vector2(0.5f,0.5f);
			Vector2 endPos = (Vector2)endcell + new Vector2(0.5f,0.5f);
			RayCastOutcome result = new RayCastOutcome(startPos,endPos);
			if (startcell == endcell)
			{
				result.hit = false;
				return result;
			}


			
			Vector2Int checkingSquare = new Vector2Int(startcell.X,startcell.Y);



			Vector2 dir = endPos - startPos;
			dir.Normalize();
		




			Vector2Int step = new Vector2Int(dir.X > 0 ? 1 : -1, dir.Y > 0 ? 1 : -1);
			

			float slope = dir.Y / dir.X;
			float inverseSlope = dir.X / dir.Y;


			Vector2 scalingFactor = new Vector2((float) Math.Sqrt(1 + slope * slope), (float) Math.Sqrt(1 + inverseSlope * inverseSlope));

			Vector2 lenght = new Vector2Int(0,0);



			lenght.X = 0.5f *scalingFactor.X;
			lenght.Y =  0.5f *scalingFactor.Y;


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
				result.CollisionPoint.Add(collisionPoint);
				//todo proper colision check
				Vector2 collisionVector = (((Vector2)tile.Position + new Vector2(0.5f,0.5f))) - collisionPoint;
				collisionVector.Normalize();
				
				

				float angle = collisionVector.ToAngle() * (float)(180/Math.PI);
				

				angle = (float)Math.Round(angle / 45) * 45;

			
				Vector2 normalcollisionvector = new Vector2((float) Math.Sin(angle*(Math.PI/180)), (float) Math.Cos(angle*(Math.PI/180)));
				normalcollisionvector.Normalize();
				
				normalcollisionvector.Round();

				//take a step back and check cover in the right direction
				Direction direc = Vec2ToDir(normalcollisionvector);
				WorldTile tilefrom = GetTileAtGrid(tile.Position + (Vector2Int)normalcollisionvector);
				
				Cover c = tilefrom.GetCover(direc+=4);
				if (c == Cover.Full)
				{  
					System.Console.WriteLine(c);
					
					System.Console.WriteLine(collisionPoint);
					System.Console.WriteLine(normalcollisionvector);
					var exactcoordinate = WorldManager.WorldPostoGrid(Camera.GetMouseWorldPos(),false);
					Console.WriteLine(exactcoordinate);	
					//result.CollisionPoint = collisionPoint;
					result.VectorToCenter = collisionVector;
					result.hit = true;
					return result;
				}


				if (endcell == checkingSquare)
				{
					result.hit = false;
					return result;
				}
			}

		}

		
		public static void DeleteWorldObject(WorldObject obj)
		{
		
			
			DeleteWorldObject(obj.Id);
		}

		public static void DeleteWorldObject(int id)
		{
			if(!WorldObjects.ContainsKey(id)) return;

			if (id < NextId)
			{
				NextId = id;//reuse IDs
			}

			WorldObject Obj = WorldObjects[id];
			Obj.TileLocation.Remove(id);
			//obj.dispose()
			WorldObjects.Remove(id);
			


		}
		
	
		public static WorldTile GetTileAtGrid(Vector2Int pos)
		{
			return gridData[pos.X, pos.Y];
		}

		private static void WipeGrid()
		{
			foreach (var worldObject in WorldObjects.Values)
			{
				DeleteWorldObject(worldObject);
			}
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					gridData[x, y].Wipe();
				}
			}
		}

		private static void MakeGrid()
		{
			foreach (var worldObject in WorldObjects.Values)
			{
				DeleteWorldObject(worldObject);
			}
			gridData = new WorldTile[100,100];
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					gridData[x, y] = new WorldTile(new Vector2Int(x, y));
				}
			}
			
		}



		public static void Update(float gameTime)
		{

			foreach (var obj in new List<WorldObject>(WorldObjects.Values))
			{
				obj.Update(gameTime);
			}
			
			
			#if CLIENT
			if (Controllable.Selected != null)
			{


				Vector2Int currentPos = WorldManager.WorldPostoGrid(Camera.GetMouseWorldPos());
				if (LastMousePos !=currentPos)
				{
					PreviewPath = PathFinding.GetPath(Controllable.Selected.Parent.TileLocation.Position, currentPos);
					LastMousePos = currentPos;
				}
			}
#endif

		//	WipeGrid();
		//	foreach (var obj in new List<WorldObject>(WorldObjects.Values))
		//	{
				
		//		gridData[obj.Position.X,obj.Position.Y].Add(obj.Id);
					
	//		}
		}


		private const int SIZE = 180;
		public static Vector2 GridToWorldPos(Vector2 gridpos) {
                    
			Matrix2 isometricTransform = Matrix2.Multiply(Matrix2.CreateRotationZ((float) (Math.PI / 4)), Matrix2.CreateScale(1, 0.5f));
	
			Vector2 transformVector = Vector2.Transform( new Vector2(gridpos.X * SIZE, gridpos.Y *SIZE), isometricTransform);

			return transformVector;
		}
		public static Vector2 WorldPostoGrid(Vector2 worldPos, bool clamp = true) {
                    
			Matrix2 isometricTransform = Matrix2.Multiply(Matrix2.CreateRotationZ((float) (Math.PI / 4)), Matrix2.CreateScale(1, 0.5f));
			isometricTransform = Matrix2.Invert(isometricTransform);
	
			Vector2 transformVector = Vector2.Transform( worldPos, isometricTransform);

			Vector2 gridPos;

			if (clamp)
			{
				gridPos	= new Vector2((int)Math.Floor(transformVector.X/SIZE),(int)Math.Floor(transformVector.Y/SIZE));
			}
			else
			{
				gridPos	= new Vector2(transformVector.X/SIZE,transformVector.Y/SIZE);
			}

			if (gridPos.X < 0)
			{
				gridPos.X = 0;
			}

			if (gridPos.Y < 0)
			{
				gridPos.Y = 0;
			}
			return gridPos;
		}


	


		public static void SaveData(string path)
		{

			List<WorldTileData> prefabData = new List<WorldTileData>();

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					prefabData.Add(gridData[x, y].GetData());
					
				}
			}
			using(FileStream stream = File.Open(path, FileMode.Create))
			{
		//		TextWriter textWriter = new StreamWriter(stream);
//				JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		//		JsonSerializer jsonSerializer = new JsonSerializer();
				//bformatter.Serialize(stream, prefabData);	
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(stream,prefabData);
		//		jsonSerializer.Serialize(textWriter, prefabData);
				
			}
			

			
			
		}
		
		
		
		
		public static void LoadData(byte[] data)
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