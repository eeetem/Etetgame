using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public class WorldObjectManager : EntityUpdateSystem
	{
		private static List<int>[,] gridData = new List<int>[100,100];

		private ComponentMapper<WorldObject> _gridMapper { get; set; }

		public static Entity MakeGridEntity(string prefabName, Vector2Int position)
		{
			if (position.X < 0 || position.Y < 0)
			{
				throw new IndexOutOfRangeException();
			}

			WorldObjectPrefab prefab = Prefabs[prefabName];
			var entity = Game1.World.CreateEntity();
			entity.Attach(new Transform2(prefab.Offset));
			entity.Attach(new Sprite(prefab.Sprite));
			
			WorldObject component = new WorldObject(position,prefab.drawLayer,prefabName);
			component.EastCover = prefab.EastCover;
			component.WestCover = prefab.WestCover;
			component.NorthCover = prefab.NorthCover;
			component.SouthCover = prefab.SouthCover;
			entity.Attach(component);
			
			gridData[position.X,position.Y].Add(entity.Id);
			
			return entity;
		}

		public static void DeleteEntity(int id)//this isnt very efficent but for now it'll do
		{
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					if (gridData[x, y].Remove(id))
					{
						Game1.World.DestroyEntity(id);
						return;
					}
				}
			}


		}
		
	
		public static List<int> GetEntitiesAtGrid(Vector2Int pos)
		{
			return gridData[pos.X, pos.Y];
		}

		private static void WipeGrid()
		{
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					if (gridData[x, y] != null)
					{
						foreach (var entity in gridData[x, y] )
						{
							Game1.World.DestroyEntity(entity);
						}
					}
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
		public WorldObjectManager( ) : base(Aspect.All(typeof(WorldObject)))
		{
			WipeGrid();


		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_gridMapper = mapperService.GetMapper<WorldObject>();
		}

		public override void Update(GameTime gameTime)
		{
			foreach (var entity in ActiveEntities)
			{

				WorldObject grid = _gridMapper.Get(entity);
				if (grid.DesiredPosition != grid.Position)
				{
					gridData[grid.Position.X, grid.Position.Y].Remove(entity);
					gridData[grid.DesiredPosition.X, grid.DesiredPosition.Y].Add(entity);
					grid.UpdatePosition();

				}
					
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
			WorldObjectPrefab prefab = new WorldObjectPrefab(Game1.Floors[0]);
			Prefabs.Add("basicFloor",prefab);
			
			prefab = new WorldObjectPrefab(Game1.Walls[0]);
			prefab.Offset = GridToWorldPos(new Vector2(0,-0.5f));
			prefab.EastCover = WorldObject.Cover.Full;
			prefab.drawLayer = 1;
			Prefabs.Add("baiscWallE",prefab);
			
			prefab = new WorldObjectPrefab(Game1.Walls[7]);
			prefab.Offset = GridToWorldPos(new Vector2(-0.5f,0f));
			prefab.EastCover = WorldObject.Cover.Full;
			prefab.drawLayer = 1;
			Prefabs.Add("baiscWallS",prefab);

			prefab = new WorldObjectPrefab(Game1.Walls[1]);
			prefab.Offset = GridToWorldPos(new Vector2(-0.5f,-1f));
			prefab.EastCover = WorldObject.Cover.Full;
			prefab.drawLayer = 1;
			Prefabs.Add("baiscWallN",prefab);
			
			prefab = new WorldObjectPrefab(Game1.Walls[6]);
			prefab.Offset = GridToWorldPos(new Vector2(-1f,-0.5f));
			prefab.EastCover = WorldObject.Cover.Full;
			prefab.drawLayer = 1;
			Prefabs.Add("baiscWallW",prefab);
			WorldEditSystem.GenerateUI();

		}


		public static void SaveData()
		{

			List<string>[,] prefabData = new List<string>[100,100];

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					foreach (var entity in gridData[x, y])
					{
						string prefab = Game1.World.GetEntity(entity).Get<WorldObject>().PrefabName;
						if (prefabData[x,y] == null)
						{
							prefabData[x,y] = new List<string>();
						}
						prefabData[x,y].Add(prefab);
					}
				
				}
			}
			using(Stream stream = File.Open("map.xml", FileMode.Create))
			{
				BinaryFormatter bformatter = new BinaryFormatter();
				bformatter.Serialize(stream, prefabData);
			}

			
			
		}
		
		
		
		
		public static void LoadData()
		{
			using(Stream stream = File.Open("map.xml", FileMode.Open))
			{
				BinaryFormatter bformatter = new BinaryFormatter();
				List<string>[,] prefabData =bformatter.Deserialize(stream) as List<string>[,] ;
				WipeGrid();
				for (int x = 0; x < 100; x++)
				{
					for (int y = 0; y < 100; y++)
					{
						if (prefabData[x, y] != null)
						{
							foreach (var entity in prefabData[x, y])
							{
								MakeGridEntity(entity, new Vector2Int(x, y));
							}
						}
					}
				}
			}
			
		
			
		}

		public struct WorldObjectPrefab
		{
			public int drawLayer;//perhaps make this an enum
			public Vector2 Offset;
			public Texture2D Sprite;
			public WorldObject.Cover SouthCover;
			public WorldObject.Cover NorthCover;
			public WorldObject.Cover EastCover;
			public WorldObject.Cover WestCover;

			public WorldObjectPrefab(Texture2D sprite)
			{
				drawLayer = 0;
				this.Offset = Vector2.Zero;
				this.Sprite = sprite;
				SouthCover = WorldObject.Cover.None;
				NorthCover = WorldObject.Cover.None;
				EastCover = WorldObject.Cover.None;
				WestCover = WorldObject.Cover.None;
			}
		}
	}




}