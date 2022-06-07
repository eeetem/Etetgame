using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.ViewportAdapters;

namespace MultiplayerXeno
{
	public class CameraSystem : UpdateSystem
	{

		public static OrthographicCamera Camera;
		
		public CameraSystem(GraphicsDevice graphicsDevice, GameWindow window) : base()
		{
			var viewportAdapter = new BoxingViewportAdapter(window, graphicsDevice, 1920, 1080);
			Camera = new OrthographicCamera(viewportAdapter);
			Camera.MinimumZoom = 0.1f;
			Camera.MaximumZoom = 10;
		}
		
	

		private Vector2 GetMovementDirection()
		{
			var movementDirection = Vector2.Zero;
			var state = Keyboard.GetState();
			if (state.IsKeyDown(Keys.S))
			{
				movementDirection += Vector2.UnitY;
			}
			if (state.IsKeyDown(Keys.W))
			{
				movementDirection -= Vector2.UnitY;
			}
			if (state.IsKeyDown(Keys.A))
			{
				movementDirection -= Vector2.UnitX;
			}
			if (state.IsKeyDown(Keys.D))
			{
				movementDirection += Vector2.UnitX;
			}
			return movementDirection;
		}

		public static Vector2 GetMouseWorldPos()
		{
			var state = Mouse.GetState();
			return Vector2.Transform(new Vector2(state.Position.X, state.Position.Y), CameraSystem.Camera.GetInverseViewMatrix()) + new Vector2(0, 64);
		}
		private int lastScroll;
		public override void Update(GameTime gameTime)
		{
			
			
			var state = Mouse.GetState();
			float diff = ((float)(state.ScrollWheelValue - lastScroll)/1000)*Camera.Zoom;
			lastScroll = state.ScrollWheelValue;
			
			Camera.ZoomIn(diff);

			float movementSpeed = 200*(Camera.MaximumZoom/Camera.Zoom);
			Camera.Move(GetMovementDirection() * movementSpeed * gameTime.GetElapsedSeconds());
			
	
		}
	}
}