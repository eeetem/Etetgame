using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public class WorldEditSystem : UpdateSystem
	{
		private SpriteBatch _spriteBatch { get; set; }

		private GraphicsDevice _graphicsDevice { get; set; }
		private ComponentMapper<Sprite> _spriteMapper { get; set; }

		private ComponentMapper<Transform2> _transformMapper { get; set; }
		private ComponentMapper<WorldObject> _gridMapper { get; set; }
		
		
		
		public WorldEditSystem() : base()
		{
			
			
		}

		public override void Initialize(World world)
		{
			
			base.Initialize(world);
		}

		private static string ActivePrefab = "basicFloor";
		public static void GenerateUI()
		{
			int xpos = 5;
			int ypos = 5;
			foreach (var prefabDictElement in WorldObjectManager.Prefabs)
			{
				var entity = Game1.World.CreateEntity();
				var uiobj = new UiObject(10, 10, xpos, ypos);
				//uiobj.Click += () => System.Console.WriteLine(prefabDictElement.Key);
				uiobj.Click += () => ActivePrefab = prefabDictElement.Key;
				entity.Attach(uiobj);
				entity.Attach(new Sprite(prefabDictElement.Value.Sprite));
				xpos += 10;
				//ypos += 5;
			}
			var buttoin = Game1.World.CreateEntity();
			var uiObj = new UiObject(10, 10, xpos, ypos);
			//uiobj.Click += () => System.Console.WriteLine(prefabDictElement.Key);
			uiObj.Click += () => WorldObjectManager.SaveData();
			buttoin.Attach(uiObj);
			buttoin.Attach(new Text("save"));
			xpos += 10;
			//ypos += 5;
			buttoin = Game1.World.CreateEntity(); 
			uiObj = new UiObject(10, 10, xpos, ypos);
			//uiobj.Click += () => System.Console.WriteLine(prefabDictElement.Key);
			uiObj.Click += () => WorldObjectManager.LoadData();
			buttoin.Attach(uiObj);
			buttoin.Attach(new Text("load"));
			xpos += 10;
			//ypos += 5;
			
			
		}

		private bool LastMousePressed = true;
		public override void Update(GameTime gameTime)
		{
			var mouseState = Mouse.GetState();
			
			Vector2Int gridClick = WorldObjectManager.WorldPostoGrid(CameraSystem.GetMouseWorldPos());
			
			if (mouseState.LeftButton == ButtonState.Released && LastMousePressed)
			{

				
					WorldObjectManager.MakeGridEntity(ActivePrefab,gridClick);
				
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{

				if(WorldObjectManager.GetEntitiesAtGrid(gridClick).Count > 0)
				{
					
					WorldObjectManager.DeleteEntity(WorldObjectManager.GetEntitiesAtGrid(gridClick)[0]);
			
				}

				
			}
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				LastMousePressed = true;
			}
			else
			{
				LastMousePressed = false;
			}

		}
	}
}