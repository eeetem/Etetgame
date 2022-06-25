using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Newtonsoft.Json;


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

		public static WorldObject MakeWorldObject(string prefabName, Vector2Int position, WorldObject.Direction facing = WorldObject.Direction.North, int id = -1)
		{
			if (position.X < 0 || position.Y < 0)
			{
				throw new IndexOutOfRangeException();
			}


			WorldObjectPrefab prefab = Prefabs[prefabName];

			
			if (id == -1)
			{
				id = GetNextId();

			}


			WorldObject WO = new WorldObject(position,prefab.Faceable,prefabName,id);
			WO.Face(facing);
			
#if CLIENT
			WO.Transform = new Transform2(prefab.Offset);
			WO.DrawLayer = prefab.DrawLayer;
		//	entity.Attach(new Transform2(prefab.Offset));

#endif
			
			WO.SetCovers(prefab.Covers);

			WorldObjects.EnsureCapacity(id+1);			
			WorldObjects[id] = WO;
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
	


		public static void Update(GameTime gameTime)
		{
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

		public static Dictionary<string, WorldObjectPrefab> Prefabs = new Dictionary<string, WorldObjectPrefab>();
		public static void MakePrefabs()
		{
			XmlDocument xmlDoc= new XmlDocument();
			xmlDoc.Load("ObjectData.xml"); 


			foreach (XmlElement xmlObj in xmlDoc.GetElementsByTagName("object"))
			{

				string name = xmlObj.GetElementsByTagName("name")[0]?.InnerText;
				XmlNode cover = xmlObj.GetElementsByTagName("cover")[0];

				WorldObject.Cover eastCover = WorldObject.Cover.None;
				WorldObject.Cover westCover = WorldObject.Cover.None;
				WorldObject.Cover southCover = WorldObject.Cover.None;
				WorldObject.Cover northCover = WorldObject.Cover.None;
				WorldObject.Cover northeastCover = WorldObject.Cover.None;
				WorldObject.Cover northwestCover =WorldObject.Cover.None;
				WorldObject.Cover southeastCover = WorldObject.Cover.None;
				WorldObject.Cover southwestCover = WorldObject.Cover.None;
				
				
				if (cover != null && cover.Attributes != null)
				{
					
					 eastCover = (WorldObject.Cover) int.Parse(cover.Attributes["E"]?.InnerText ?? "0");
					 westCover = (WorldObject.Cover)int.Parse(cover.Attributes["W"]?.InnerText ??  "0");
					 southCover = (WorldObject.Cover)int.Parse(cover.Attributes["S"]?.InnerText ??  "0");
					 northCover = (WorldObject.Cover)int.Parse(cover.Attributes["N"]?.InnerText ??  "0");
					 northeastCover = (WorldObject.Cover)int.Parse(cover.Attributes["NE"]?.InnerText ?? "0");
					 northwestCover = (WorldObject.Cover)int.Parse(cover.Attributes["NW"]?.InnerText ??  "0");
					 southeastCover = (WorldObject.Cover)int.Parse(cover.Attributes["SE"]?.InnerText ??  "0");
					 southwestCover = (WorldObject.Cover)int.Parse(cover.Attributes["SW"]?.InnerText ??  "0");

					
					
				}

				
				
				WorldObjectPrefab prefab = new WorldObjectPrefab(1);


				bool faceable = true;
				if (xmlObj.HasAttributes && xmlObj.Attributes["Faceable"] != null)
				{
					 faceable = bool.Parse(xmlObj?.Attributes?["Faceable"].InnerText);
				}

				prefab.Faceable = faceable;


#if CLIENT
				string stringoffset = xmlObj.GetElementsByTagName("sprite")[0].Attributes["offset"].InnerText;
				float x = float.Parse(stringoffset.Substring(0, stringoffset.IndexOf(",")));
				float y = float.Parse(stringoffset.Substring(stringoffset.IndexOf(",")+1, stringoffset.Length - stringoffset.IndexOf(",")-1));
				Vector2 Offset = new Vector2(x, y);
				int drawlayer = int.Parse(xmlObj.GetElementsByTagName("sprite")[0].Attributes["layer"].InnerText);
#endif
			
				
				
				

#if CLIENT

				prefab.Offset = GridToWorldPos(Offset+ new Vector2(-0.5f,-0.5f));
				
				
				
#endif

				Dictionary<WorldObject.Direction, WorldObject.Cover> covers = new Dictionary<WorldObject.Direction, WorldObject.Cover>();


				covers.Add(WorldObject.Direction.North, northCover);
				covers.Add(WorldObject.Direction.South, southCover);
				covers.Add(WorldObject.Direction.East, eastCover);
				covers.Add(WorldObject.Direction.West, westCover);
				covers.Add(WorldObject.Direction.SouthEast, southeastCover);
				covers.Add(WorldObject.Direction.SouthWest, southwestCover);
				covers.Add(WorldObject.Direction.NorthEast, northeastCover);
				covers.Add(WorldObject.Direction.NorthWest, northwestCover);
				
				
				
				
				
				
				
				
				prefab.Covers = covers;
				prefab.DrawLayer = drawlayer;
				Prefabs.Add(name,prefab);
				
#if CLIENT
				GenerateSpriteSheet(name, Game1.Textures[name]);
#endif


			}
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
						string prefab = obj.PrefabName;
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

				foreach (var worldObjectDatadata in prefabData)
				{
					MakeWorldObject(worldObjectDatadata.Prefab, worldObjectDatadata.Position,worldObjectDatadata.Facing,worldObjectDatadata.Id);
				}
			}
				
			

		
						
					
				
			
			
		
			
		}

		public partial struct WorldObjectPrefab
		{
#if CLIENT
			public Vector2 Offset;
			public int DrawLayer;
#endif

			public bool Faceable;

			
			
			public Dictionary<WorldObject.Direction, WorldObject.Cover> Covers;

			public WorldObjectPrefab(int foo)
			{
				
#if CLIENT
				DrawLayer = 0;
				Offset = Vector2.Zero;
#endif
				Faceable = false;
				Covers = new Dictionary<WorldObject.Direction, WorldObject.Cover>();
			}
		}
		[Serializable]
		public partial struct WorldObjectData
		{
			public WorldObject.Direction Facing;
			public int Id;
			//health
			public string Prefab;
			public Vector2Int Position;
			public WorldObjectData(string prefab, int id, Vector2Int position)
			{
				this.Prefab = prefab;
				this.Id = id;
				Position = position;
				Facing = WorldObject.Direction.North;
			}
		}
		
		
	}




}