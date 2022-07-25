using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public partial class WorldObjectManager
	{
		
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		public static void Init(GraphicsDevice graphicsdevice)
		{

			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			Init();
		}

	
		
		public static void Draw(GameTime gameTime)
		{
			List<WorldObject>[] DrawOrderSortedEntities = new List<WorldObject>[5];
			foreach (var WO in WorldObjects.Values)
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
					var transform = worldObject.Transform;
					
					spriteBatch.Draw(sprite, transform.Position + WorldObjectManager.GridToWorldPos(worldObject.Position),transform.Rotation, transform.Scale);
				}
				spriteBatch.End();
			}

			
		}

		
			
		
		public class EntityDrawOrderCompare : Comparer<WorldObject>
		{


			public override int Compare(WorldObject x, WorldObject y)
			{

				Rectangle rectx = Utility.GetSmallestRectangleFromTexture(x.GetSprite().TextureRegion.Texture);
				Rectangle recty = Utility.GetSmallestRectangleFromTexture(y.GetSprite().TextureRegion.Texture);
				



				return (x.Transform.Position + new Vector2(rectx.Center.X,rectx.Center.Y) + GridToWorldPos(x.Position)).Y.CompareTo((y.Transform.Position + new Vector2(recty.Center.X,recty.Center.Y)+ GridToWorldPos(y.Position)).Y);
			}
		}
	}
}