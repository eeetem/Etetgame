using System;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace MultiplayerXeno
{
	public static class Camera
	{

		private static OrthographicCamera Cam { get; set; }
		public static AudioListener AudioListener{ get; private set; }

		private static Vector2 velocity = new Vector2();
		private static float ZoomVelocity = 0;
		private static float scale = 1;

		public static void Init(GraphicsDevice graphicsDevice, GameWindow window)
		{
			var viewportAdapter = new BoxingViewportAdapter(window, graphicsDevice, window.ClientBounds.Width, window.ClientBounds.Height);
			Cam = new OrthographicCamera(viewportAdapter);
			Cam.MinimumZoom = window.ClientBounds.Width / 50000f;
			Cam.MaximumZoom =  window.ClientBounds.Width/1000f;
			AudioListener = new AudioListener();
			Cam.Position = MoveTarget;
		}

		
		public static Vector2 GetPos()
		{
			return Cam.Center;
		}

		//function that checks if a specfic position is visible
		public static bool IsOnScreen(Vector2Int vec)
		{
			vec = Utility.GridToWorldPos(vec);
			return Cam.BoundingRectangle.Contains((Vector2)vec);
		}

		public static Matrix GetViewMatrix()
		{
			return Cam.GetViewMatrix();
		}

		 static Vector2 MoveTarget;
		 static bool forceMoving = false;
		public static void SetPos(Vector2Int vec)
		{
			vec = Utility.GridToWorldPos(vec);
			//vec.X -= Cam.BoundingRectangle.Width / 2;
		//	vec.Y -= Cam.BoundingRectangle.Height / 2;
			MoveTarget = vec-(Vector2Int)Cam.Origin;
			forceMoving = true;


		}


		private static Vector2 lastMousePos;
		private static Vector2 GetMovementDirection()
		{

			
			var state = Keyboard.GetState();
			var mouseState = Mouse.GetState();
			if (mouseState.MiddleButton == ButtonState.Pressed)
			{
				var lastpos = lastMousePos;
				lastMousePos = new Vector2(mouseState.Position.X,mouseState.Position.Y);
				if (lastpos != new Vector2(0, 0))
				{
					return Vector2.Clamp((lastpos - new Vector2(mouseState.Position.X, mouseState.Position.Y)) / 30f, new Vector2(-3, -3), new Vector2(3, 3));
				}
			}
			else
			{
				lastMousePos = new Vector2(0,0);
			}

			var movementDirection = Vector2.Zero;
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

			if (movementDirection.Length() != 0)
			{
				forceMoving = false;
				return movementDirection;
			}//overrideforcemove

			if (forceMoving)
			{
				Vector2 difference = MoveTarget - Cam.Position;
				if (difference.Length() < 20)
				{
					forceMoving = false;
				}

				return  Vector2.Clamp(difference/1500f,new Vector2(-3,-3),new Vector2(3,3));
			}
			
			
			return movementDirection;
		}

		public static Vector2 GetMouseWorldPos()
		{
			var state = Mouse.GetState();
			return Vector2.Transform(new Vector2(state.Position.X, state.Position.Y), Cam.GetInverseViewMatrix());
		}
		private static int lastScroll;
		public static void Update(GameTime gameTime)
		{
			
			
			var state = Mouse.GetState();
			float diff = (float) (state.ScrollWheelValue - lastScroll)*(Cam.Zoom/3000);
			lastScroll = state.ScrollWheelValue;
			ZoomVelocity += diff*gameTime.GetElapsedSeconds()*25f;
			Cam.ZoomIn(ZoomVelocity);
			ZoomVelocity *= gameTime.GetElapsedSeconds()*45;

			float movementSpeed = 400*(Cam.MaximumZoom/Cam.Zoom);
			Vector2 move = GetMovementDirection();
			velocity += move*gameTime.GetElapsedSeconds()*25f* movementSpeed;
			Cam.Move(velocity  * gameTime.GetElapsedSeconds());
			Cam.Position = Vector2.Clamp(Cam.Position, new Vector2(-15000, -1000), new Vector2(15000, 12000));
			velocity *= gameTime.GetElapsedSeconds()*45;


			AudioListener.Position =  new Vector3(Cam.Center/80f,0);
			AudioListener.Velocity = new Vector3(velocity/80f,10);

		}
	}
}