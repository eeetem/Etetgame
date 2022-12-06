using System;
using System.Collections.Generic;
using CommonData;
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
		
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred);
			foreach (var obj in Objs)
			{
				if(obj == null)continue;
				var sprite = obj.GetSprite();
				var transform = obj.GetDrawTransform();
				var worldPos = obj.GetWorldPos();
				WorldTile worldTile = null;
				if (WorldManager.IsPositionValid(worldPos))
				{
					worldTile =WorldManager.Instance.GetTileAtGrid( worldPos);

				}
				else
				{
					continue;
				}

				if (!obj.IsVisible())//hide tileobjects
				{
					continue;
				}


				if (worldTile.Visible == Visibility.None)
				{
					sprite.Color = Color.DimGray;
					
				}else if (worldTile.Visible == Visibility.Partial)
				{
					sprite.Color = Color.LightPink;
				}


				if (obj.IsTransparentUnderMouse())
				{
					sprite.Color *= 0.3f;
				}
						
					
		
				spriteBatch.Draw(sprite, transform.Position,  transform.Rotation,  transform.Scale);
			//	spriteBatch.DrawString(Game1.SpriteFont," "+worldTile.Visible,  transform.Position,Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			//	spriteBatch.DrawString(Game1.SpriteFont,""+Math.Round(Pathfinding.PathFinding.NodeCache[worldPos.X,worldPos.Y].CurrentCost,2),  transform.Position,Color.Black, 0, Vector2.Zero, 2, new SpriteEffects(), 0);
				
				
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