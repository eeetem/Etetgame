using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno;

public static class RenderSystem
{
	private static SpriteBatch spriteBatch;
	private static GraphicsDevice graphicsDevice;
	public static void Init(GraphicsDevice graphicsdevice)
	{

		graphicsDevice = graphicsdevice;
		spriteBatch = new SpriteBatch(graphicsDevice);
	}

	



	public static void Draw()
	{

		List<WorldTile> allTiles = WorldManager.Instance.GetAllTiles();
		List<IDrawable> Objs = new List<IDrawable>();
		
		foreach (var tile in allTiles)
		{
			if (tile.Surface != null)
			{ 
				Objs.Add(tile.Surface);
			}

			if (tile.NorthEdge != null)
			{
				Objs.Add(tile.NorthEdge);
			}

			if (tile.WestEdge != null)
			{
				Objs.Add(tile.WestEdge);
			}

			if (tile.ObjectAtLocation != null)
			{
				Objs.Add(tile.ObjectAtLocation);
			}
			//tileObjs.Sort(new DrawLayerSort());

		}
		Objs.AddRange(LocalObject.Objects);
		
		Objs.Sort(new DrawableSort());
		
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			foreach (var obj in Objs)
			{
				if(obj == null)continue;
				var sprite = obj.GetSprite();
				var transform = obj.GetDrawTransform();
				var worldPos = obj.GetWorldPos();
				WorldTile? worldTile = null;
				if (WorldManager.IsPositionValid(worldPos))
				{
					worldTile =WorldManager.Instance.GetTileAtGrid( worldPos);

				}

				

				if (worldTile == null || !worldTile.IsVisible)
				{
					sprite.Color = Color.DarkGray;
					if (!obj.IsAlwaysVisible())//hide tileobjects
					{
						continue;
					}
				}

				if (obj.IsTransparentUnderMouse())
				{
					sprite.Color *= 0.4f;
				}
						
					
		
				spriteBatch.Draw(sprite, transform.Position,  transform.Rotation,  transform.Scale);
			}
			spriteBatch.End();
		
		

		
	}
	
	public class DrawableSort : Comparer<IDrawable>
	{
//draws "top" ones first

		public override int Compare(IDrawable x, IDrawable y)
		{


			return x.GetDrawOrder().CompareTo(y.GetDrawOrder());
		}
	}
			
	public class WorldTileDrawOrderCompare : Comparer<WorldTile>
	{
//draws "top" ones first

		public override int Compare(WorldTile x, WorldTile y)
		{

			int xpos = x.Position.X + x.Position.Y;
			int ypos = y.Position.X + y.Position.Y;
			return xpos.CompareTo(ypos);
		}
	}
	public class DrawLayerSort : Comparer<WorldObject>
	{
//draws "top" ones first

		public override int Compare(WorldObject x, WorldObject y)
		{

			int xpos = x.GetDrawLayer();
			int ypos = y.GetDrawLayer();
			return xpos.CompareTo(ypos);
		}
	}
}