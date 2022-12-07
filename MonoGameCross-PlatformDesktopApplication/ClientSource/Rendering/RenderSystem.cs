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
		List<IDrawable> UnsortedObjs = new List<IDrawable>();

		foreach (var tile in allTiles)
		{
			if (tile.Surface != null)
			{
				UnsortedObjs.Add(tile.Surface);
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

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
		foreach (var obj in UnsortedObjs)
		{
			
				if(obj == null)continue;
				var texture = obj.GetTexture();
				var transform = obj.GetDrawTransform();

				if (!obj.IsVisible())//hide tileobjects
				{
					continue;
				}
			
				spriteBatch.Draw(texture,transform.Position,obj.GetColor());
				
				
				//spriteBatch.Draw(texture, transform.Position,  transform.Rotation,  transform.Scale,Color.Wheat,);
				//	spriteBatch.DrawString(Game1.SpriteFont," "+worldTile.Visible,  transform.Position,Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
				//	spriteBatch.DrawString(Game1.SpriteFont,""+Math.Round(Pathfinding.PathFinding.NodeCache[worldPos.X,worldPos.Y].CurrentCost,2),  transform.Position,Color.Black, 0, Vector2.Zero, 2, new SpriteEffects(), 0);
				
				
			
		}
		spriteBatch.End();

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred);
			foreach (var obj in Objs)
			{
				if(obj == null)continue;
				var texture = obj.GetTexture();
				var transform = obj.GetDrawTransform();

				if (!obj.IsVisible())//hide tileobjects
				{
					continue;
				}
			
				spriteBatch.Draw(texture,transform.Position,obj.GetColor());
				
				
				//spriteBatch.Draw(texture, transform.Position,  transform.Rotation,  transform.Scale,Color.Wheat,);
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

}