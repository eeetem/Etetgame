using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace MultiplayerXeno
{
	public static class Camera
	{

		private static OrthographicCamera Cam { get; set; }

		private static Vector2 velocity = new Vector2();
		private static float ZoomVelocity = 0;

		public static void Init(GraphicsDevice graphicsDevice, GameWindow window)
		{
			var viewportAdapter = new BoxingViewportAdapter(window, graphicsDevice, 1920, 1080);
			Cam = new OrthographicCamera(viewportAdapter);
			Cam.MinimumZoom = 0.1f;
			Cam.MaximumZoom = 10;
		}

		public static Vector2 GetPos()
		{
			return Cam.Center;
		}

		public static Matrix GetViewMatrix()
		{
			return Cam.GetViewMatrix();
		}

		 static Vector2 MoveTarget;
		 static bool forceMoving = false;
		public static void SetPos(Vector2 vec)
		{
			vec = Utility.GridToWorldPos(vec);
			//vec.X -= Cam.BoundingRectangle.Width / 2;
		//	vec.Y -= Cam.BoundingRectangle.Height / 2;
			MoveTarget = vec-Cam.Origin;
			forceMoving = true;


		}


		private static Vector2 GetMovementDirection()
		{
			if (forceMoving)
			{
				Vector2 difference = MoveTarget - Cam.Position;
				if (difference.Length() < 20)
				{
					forceMoving = false;
				}

				return  Vector2.Clamp(difference/1500f,new Vector2(-3,-3),new Vector2(3,3));
			}

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
			return Vector2.Transform(new Vector2(state.Position.X, state.Position.Y), MultiplayerXeno.Camera.Cam.GetInverseViewMatrix()) + new Vector2(0, 0);
		}
		private static int lastScroll;
		public static void Update(GameTime gameTime)
		{
			
			
			var state = Mouse.GetState();
			float diff = (float)(state.ScrollWheelValue - lastScroll)/1000*Cam.Zoom;
			lastScroll = state.ScrollWheelValue;
			ZoomVelocity += diff*gameTime.GetElapsedSeconds()*25f;
			Cam.ZoomIn(ZoomVelocity);
			ZoomVelocity *= gameTime.GetElapsedSeconds()*45;
			

			float movementSpeed = 200*(Cam.MaximumZoom/Cam.Zoom);

			Vector2 move = GetMovementDirection();
			velocity += move*gameTime.GetElapsedSeconds()*25f;
			Cam.Move(velocity * movementSpeed * gameTime.GetElapsedSeconds());
			velocity *= gameTime.GetElapsedSeconds()*45;


		}
	}
}