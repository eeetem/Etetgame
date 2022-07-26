using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno.Structs;


namespace MultiplayerXeno
{
	public static partial class WorldObjectManager
	{
		private static List<int>[,] gridData = new List<int>[100,100];

		private static Dictionary<int,WorldObject> WorldObjects = new Dictionary<int,WorldObject>();

		private static int NextId = 0;


		public static void Init()
		{
			WipeGrid();
			

		}



		public static int GetNextId()
		{
			
			while (WorldObjects.ContainsKey(NextId))//skip all the server-side force assinged IDs
			{
				NextId++;
			}

			return NextId;
		}

		public static WorldObject MakeWorldObject(string prefabName, Vector2Int position, Direction facing = Direction.North, int id = -1)
		{
			WorldObjectData data = new WorldObjectData(prefabName, id, position);
			data.Facing = facing;
			return MakeWorldObject(data);
		}

		public static WorldObject MakeWorldObject(WorldObjectData data)
		{
			if (data.Position.X < 0 || data.Position.Y < 0)
			{
				throw new IndexOutOfRangeException();
			}


			WorldObjectType type = PrefabManager.Prefabs[data.Prefab];
			WorldObject WO = new WorldObject(data.Position,type,data.Id);
			WO.Face(data.Facing);

			if (type.controllable != null && data.ControllableData != null)
			{
				Controllable component = type.controllable.Instantiate(WO,data.ControllableData.Value.Team1);
				WO.ControllableComponent = component;
			}




			if (data.Id == -1)
			{
				data.Id = GetNextId();

			}
		

			
			WorldObjects.EnsureCapacity(data.Id+1);			
			WorldObjects[data.Id] = WO;

			return WO;
	

		}

		public static void DeleteWorldObject(WorldObject obj)
		{
			DeleteWorldObject(obj.Id);
		}

		public static void DeleteWorldObject(int id)
		{
			WorldObject Obj = WorldObjects[id];
			gridData[Obj.Position.X, Obj.Position.Y].Remove(id);
			//obj.dispose()
			WorldObjects.Remove(id);

		}
		
	
		public static List<int> GetEntitiesAtGrid(Vector2Int pos)
		{
			return gridData[pos.X, pos.Y];
		}

		private static void WipeGrid(bool deleteAll = false)
		{
			if (deleteAll)
			{
				foreach (var obj in WorldObjects)
				{
					DeleteWorldObject(obj.Key);
				}
			}

			
			gridData = new List<int>[100,100];
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					gridData[x, y] = new List<int>();
				}
			}
		}
	


		public static void Update(float gameTime)
		{

			foreach (var obj in WorldObjects.Values)
			{
				obj.Update();
			}

			WipeGrid();
			foreach (var obj in WorldObjects.Values)
			{
				
				gridData[obj.Position.X,obj.Position.Y].Add(obj.Id);
					
			}
		}


		private const int SIZE = 180;
		public static Vector2 GridToWorldPos(Vector2 gridpos) {
                    
			Matrix2 isometricTransform = Matrix2.Multiply(Matrix2.CreateRotationZ((float) (Math.PI / 4)), Matrix2.CreateScale(1, 0.5f));
	
			Vector2 transformVector = Vector2.Transform( new Vector2(gridpos.X * SIZE, gridpos.Y *SIZE), isometricTransform);

			return transformVector;
		}
		public static Vector2Int WorldPostoGrid(Vector2 worldPos) {
                    
			Matrix2 isometricTransform = Matrix2.Multiply(Matrix2.CreateRotationZ((float) (Math.PI / 4)), Matrix2.CreateScale(1, 0.5f));
			isometricTransform = Matrix2.Invert(isometricTransform);
	
			Vector2 transformVector = Vector2.Transform( worldPos, isometricTransform);

			Vector2Int gridPos = new Vector2Int((int)Math.Floor(transformVector.X/SIZE),(int)Math.Floor(transformVector.Y/SIZE));

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

	


		public static void SaveData()
		{

			List<WorldObjectData> prefabData = new List<WorldObjectData>();

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					foreach (var ID in gridData[x, y])
					{
						WorldObject obj = WorldObjects[ID];
						string prefab = obj.Type.TypeName;
						WorldObjectData worldObjectData = new WorldObjectData(prefab,ID,new Vector2Int(x,y));
						worldObjectData.Facing = obj.Facing;
						prefabData.Add(worldObjectData);
					}
				
				}
			}
			using(FileStream stream = File.Open("map.mapdata", FileMode.Create))
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
				List<WorldObjectData> prefabData = bformatter.Deserialize(dataStream) as List<WorldObjectData>;
				WipeGrid(true);

				if (prefabData != null)
					foreach (var worldObjectDatadata in prefabData)
					{
						MakeWorldObject(worldObjectDatadata);
					}
			}
				
			

		
						
					
				
			
			
		
			
		}

	
	
		
		
	}




}