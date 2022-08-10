using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerXeno
{
	public static class MouseManager
	{
		public delegate void MouseClick(Vector2Int gridPos);

		public static event MouseClick RightClick;
		public static event MouseClick LeftClick;

		private static MouseState lastState;
		public static void Update(GameTime gameTime)
		{
			
			
			var mouseState = Mouse.GetState();


			Vector2Int gridClick = WorldManager.WorldPostoGrid(Camera.GetMouseWorldPos());
			
			if (mouseState.LeftButton == ButtonState.Released && lastState.LeftButton == ButtonState.Pressed)
			{

				LeftClick?.Invoke(gridClick);
				

			
		
				
			}
			if (mouseState.RightButton == ButtonState.Released && lastState.RightButton == ButtonState.Pressed)
			{

				RightClick?.Invoke(gridClick);

				
			}


			lastState = mouseState;

		}
	}
}