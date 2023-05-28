using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public static class RenderSystem
{
	private static GraphicsDevice graphicsDevice = null!;
	public static void Init(GraphicsDevice graphicsdevice)
	{

		graphicsDevice = graphicsdevice;
	}
	
	public static void Draw(SpriteBatch spriteBatch)
	{

		List<WorldTile> allTiles = WorldManager.Instance.GetAllTiles();
		List<IDrawable> objs = new List<IDrawable>();
		List<IDrawable> unsortedObjs = new List<IDrawable>();

		foreach (var tile in allTiles)
		{
			if (tile.Surface != null)
			{
				unsortedObjs.Add(tile.Surface);
			}

			if (tile.NorthEdge != null)
			{
				objs.Add(tile.NorthEdge);
			}

			if (tile.WestEdge != null)
			{
				objs.Add(tile.WestEdge);
			}

			if (tile.ControllableAtLocation != null)
			{
				objs.Add(tile.ControllableAtLocation);
			}

			foreach (var item in tile.ObjectsAtLocation)
			{
				objs.Add(item);
			}
		

		}

		objs.AddRange(LocalObject.Objects);

		objs.Sort(new DrawableSort());

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
		foreach (var obj in unsortedObjs)
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
			
			//spriteBatch.DrawString(Game1.SpriteFont,""+Math.Round(Pathfinding.PathFinding.NodeCache[(int) Utility.WorldPostoGrid(transform.Position).X,(int) Utility.WorldPostoGrid(transform.Position).Y].CurrentCost,2),  transform.Position,Color.Black, 0, Vector2.Zero, 2, new SpriteEffects(), 0);

			
		}
		spriteBatch.End();

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred);
		foreach (var obj in objs)
		{
			if(obj == null)continue;
			var texture = obj.GetTexture();
			var transform = obj.GetDrawTransform();

			if (!obj.IsVisible())//hide tileobjects
			{
				continue;
			}

			spriteBatch.Draw(texture, transform.Position,null,obj.GetColor(), transform.Rotation,Vector2.Zero, transform.Scale, new SpriteEffects(), 0);

		}
		spriteBatch.End();
		
		/*
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
		foreach (var obj in UnsortedObjs)
		{
			if(obj == null)continue;
			var transform = obj.GetDrawTransform();
			spriteBatch.DrawString(Game1.SpriteFont,""+Math.Round(Pathfinding.PathFinding.NodeCache[(int) Utility.WorldPostoGrid(transform.Position).X,(int) Utility.WorldPostoGrid(transform.Position).Y].CurrentCost,2),  transform.Position,Color.Wheat, 0, Vector2.Zero, 3, new SpriteEffects(), 0);

		}
		spriteBatch.End();
*///pathfinddebug
		
	}
	
	public class DrawableSort : Comparer<IDrawable>
	{
//draws "top" ones first

		public override int Compare(IDrawable? x, IDrawable? y)
		{
			float orderX = 0;
			if (x != null)
			{
				orderX = x.GetDrawOrder();
			}
			float orderY =0;
			if (y != null)
			{
				orderY  = y.GetDrawOrder();
			}
			
			return orderX.CompareTo(orderY);
		}
	}
}
			

