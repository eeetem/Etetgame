using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerXeno.Structs;

namespace MultiplayerXeno
{
	public static class WorldEditSystem 
	{

		
		private static bool enabled = false;
		public static void Init()
		{
			enabled = true;

		}


		public static string ActivePrefab = "basicFloor";
		public static Direction ActiveDir = Direction.North;
		public static void GenerateUI()
		{
			if(!enabled) return;
			UI.EditorMenu();
			
			
			
		}
		

		private static MouseState lastState;
		public static void Update(GameTime gameTime)
		{
			var mouseState = Mouse.GetState();
			if (mouseState.MiddleButton == ButtonState.Released && lastState.MiddleButton == ButtonState.Pressed)
			{
				ActiveDir += 1;
			}


			Vector2Int gridClick = WorldObjectManager.WorldPostoGrid(Camera.GetMouseWorldPos());
			
			if (mouseState.LeftButton == ButtonState.Released && lastState.LeftButton == ButtonState.Pressed)
			{

				
				WorldObjectManager.MakeWorldObject(ActivePrefab,gridClick,ActiveDir);
		
				
			}
			if (mouseState.RightButton == ButtonState.Released && lastState.RightButton == ButtonState.Pressed)
			{

				if(WorldObjectManager.GetEntitiesAtGrid(gridClick).Count > 0)
				{
					
					WorldObjectManager.DeleteWorldObject(WorldObjectManager.GetEntitiesAtGrid(gridClick)[0]);
			
				}

				
			}


			lastState = mouseState;

		}
	}
}