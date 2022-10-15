using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Network;

namespace MultiplayerXeno
{
	public partial class WorldManager
	{
		
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		public static void Init(GraphicsDevice graphicsdevice)
		{

			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			MouseManager.LeftClick += SelectAtPosition;
			Init();
		}

		private static Vector2Int LastMousePos;
		public static List<Vector2Int> PreviewPath;
		private static void SelectAtPosition(Vector2Int position)
		{
			var Tile = GetTileAtGrid(position);

			WorldObject obj = Tile.ObjectAtLocation;
			if (obj!=null&&obj.ControllableComponent != null) { 
				obj.ControllableComponent.Select(); 
				return;
			}
			
			
			//if nothing was selected then it's a click on an empty tile
			
			Controllable.StartOrder(position);

		}


		public static List<RayCastOutcome> RecentFOVRaycasts = new List<RayCastOutcome>();
		public static void CalculateFov()
		{
			RecentFOVRaycasts = new List<RayCastOutcome>();
			foreach (var tile in gridData)
			{
				tile.IsVisible = false;
			}


	

			foreach (var obj in WorldObjects.Values)
			{
			
				if (obj.ControllableComponent is not null && obj.ControllableComponent.IsMyTeam())
				{
					Vector2Int pos = obj.TileLocation.Position;

					int itteration = 0;

					List<Vector2Int> positionsToCheck = new List<Vector2Int>();
					while (itteration < obj.ControllableComponent.GetSightRange())
					{
						
						positionsToCheck.Add(pos);
						Vector2Int offset;
						Vector2Int invoffset;
						if (DirToVec2(obj.Facing).Magnitude() > 1)//diagonal
						{
							offset = DirToVec2(obj.Facing+3);
							invoffset = DirToVec2(obj.Facing-3);
						}
						else
						{
							offset = DirToVec2(obj.Facing+2);
							invoffset = DirToVec2(obj.Facing-2);
						}

					
						for (int x = 0; x < itteration; x++)
						{
							positionsToCheck.Add(pos + invoffset*(x+1));
							positionsToCheck.Add(pos + offset*(x+1));
						}

						pos += DirToVec2(obj.Facing);
						itteration++;
					}
		
					

					foreach (var tile in positionsToCheck)
					{
						if(!IsPositionValid(tile)) continue;
						RayCastOutcome cast = Raycast(obj.TileLocation.Position, tile);
						RecentFOVRaycasts.Add(cast);
						if (cast.hit) continue;
						GetTileAtGrid(tile).IsVisible = true;
					}
				}
			}
		}

		


		public static void Draw(GameTime gameTime)
		{
			List<WorldObject>[] DrawOrderSortedEntities = new List<WorldObject>[5];
			foreach (var WO in new List<WorldObject>(WorldObjects.Values))
			{
				if (DrawOrderSortedEntities[WO.Type.DrawLayer] == null)
				{
					DrawOrderSortedEntities[WO.Type.DrawLayer] = new List<WorldObject>();
				}
				DrawOrderSortedEntities[WO.Type.DrawLayer].Add(WO);
			
			}


			foreach (var list in DrawOrderSortedEntities)
			{
				if (list == null) continue;
				list.Sort(new EntityDrawOrderCompare());
				spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
				foreach (var worldObject in list)
				{
					var sprite = worldObject.GetSprite();
					var transform = worldObject.Type.Transform;
					Color c = Color.White;
					if (!worldObject.TileLocation.IsVisible)
					{
						c = Color.DarkGray;
						if (worldObject.TileLocation.ObjectAtLocation == worldObject)
						{
							continue;
						}
					}
				

					sprite.Color = c;
					
					spriteBatch.Draw(sprite, transform.Position + GridToWorldPos(worldObject.TileLocation.Position),transform.Rotation, transform.Scale);
				}
				spriteBatch.End();


			
			}

			
		}
		public static Vector2 RadianToVector2(float radian)
		{
			return new Vector2((float) Math.Cos(radian), (float) Math.Sin(radian));
		}
		public static Vector2 RadianToVector2(float radian, float length)
		{
			return RadianToVector2(radian) * length;
		}
		public static Vector2 DegreeToVector2(float degree)
		{
			return RadianToVector2(degree * (MathF.PI/180));
		}
		public static Vector2 DegreeToVector2(float degree, float length)
		{
			return RadianToVector2(degree * (MathF.PI/180)) * length;
		}
		
			
		
		public class EntityDrawOrderCompare : Comparer<WorldObject>
		{


			public override int Compare(WorldObject x, WorldObject y)
			{

				Rectangle rectx = Utility.GetSmallestRectangleFromTexture(x.GetSprite().TextureRegion.Texture);
				Rectangle recty = Utility.GetSmallestRectangleFromTexture(y.GetSprite().TextureRegion.Texture);
				



				return (x.Type.Transform.Position + new Vector2(rectx.Center.X,rectx.Center.Y) + GridToWorldPos(x.TileLocation.Position)).Y.CompareTo((y.Type.Transform.Position + new Vector2(recty.Center.X,recty.Center.Y)+ GridToWorldPos(y.TileLocation.Position)).Y);
			}
		}
	}
}