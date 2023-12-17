
using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.LocalObjects;
using DefconNull.Rendering.UILayout.GameLayout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.Rendering;

public static class RenderSystem
{
	public static GraphicsDevice GraphicsDevice = null!;
	public static void Init(GraphicsDevice graphicsdevice)
	{

		GraphicsDevice = graphicsdevice;
	}

	public static List<Tuple<Color, List<Vector2Int>>> debugPaths = new List<Tuple<Color, List<Vector2Int>>>();
	static List<IDrawable> objs = new List<IDrawable>();
	static List<IDrawable> unsortedObjs = new List<IDrawable>();
	public static void Draw(SpriteBatch spriteBatch)
	{
		List<WorldTile> allTiles = WorldManager.Instance.GetAllTiles();
		objs.Clear();
		unsortedObjs.Clear();
		
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

			if (tile.UnitAtLocation != null)
			{
				objs.Add(tile.UnitAtLocation.WorldObject);
			}
			if (tile.UnitAtLocation != null)
			{
				objs.Add(tile.UnitAtLocation);
			}

			foreach (var item in tile.ObjectsAtLocation)
			{
				objs.Add(item);
			}
		

		}

		objs.AddRange(LocalObject.Objects);

		objs.Sort(new DrawableSort());

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
		int count = 0;
		foreach (var pp in debugPaths)
		{
			break;
			count++;

				for (int i = 0; i < pp.Item2.Count - 1; i++)
				{
					var path =  pp.Item2[i];
					if (path.X < 0 || path.Y < 0) break;
					var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));
					var nextpos = Utility.GridToWorldPos((Vector2) pp.Item2[i + 1] + new Vector2(0.5f, 0.5f));

					float mul = (float) WorldManager.Instance.GetTileAtGrid(pp.Item2[i + 1]).TraverseCostFrom(path);


					spriteBatch.DrawLine(pos, nextpos, pp.Item1, count);
				}
			
		}
		foreach (var obj in unsortedObjs)
		{
			
			if(obj == null)continue;
			var texture = obj.GetTexture();
			var transform = obj.GetDrawTransform();

			//if (!obj.IsVisible())//hide tileobjects
			//{
			//	continue;
			//}
			
			spriteBatch.Draw(texture,transform.Position,obj.GetColor());
			
			
		}
		spriteBatch.End();
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix());
		count = 0;

			foreach (var moves in GameLayout.PreviewMoves.Reverse())
			{
				Color c = Color.White;
				switch (count)
				{
					case 0:
						c = Color.Red;
						break;
					case 1:
						c = Color.Orange;
						break;
					case 2:
						c = Color.Yellow;
						break;
					case 3:
						c = Color.GreenYellow;
						break;
					case 4:
						c = Color.Green;
						break;
					case 5:
						c = Color.LightGreen;
						break;
					default:
						c = Color.White;
						break;

				}
				spriteBatch.DrawOutline(moves, c, 4f);
				foreach (var path in moves)
				{	
					
					var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

					var tile = WorldManager.Instance.GetTileAtGrid(path);
					if (tile.Surface != null)
					{
						Texture2D sprite = tile.Surface.GetTexture();
						spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, c * 0.2f);
					}
				}

				count++;
			

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
				//	if (PathFinding.PathfindingCache.ContainsKey(new Vector2Int(x, y)))
					//{
				//		spriteBatch.DrawText($"{PathFinding.PathfindingCache[new Vector2Int(x, y)].Item1:F1}",Utility.GridToWorldPos(new Vector2(x,y) + new Vector2(0.4f,0.4f)),2,Color.Yellow);
					//	spriteBatch.DrawLine(Utility.GridToWorldPos(new Vector2Int(x, y)+new Vector2(0.5f,0.5f)),Utility.GridToWorldPos( PathFinding.PathfindingCache[new Vector2Int(x, y)].Item2+new Vector2(0.5f,0.5f)), Color.Yellow, 2);
					//}

				
					if(GameLayout.AIMoveCache.GetLength(2)<2) continue;
					if(GameLayout.AIMoveCache[x,y,0] == 0 && GameLayout.AIMoveCache[x,y,1] == 0) continue;
					spriteBatch.DrawText(GameLayout.AIMoveCache[x,y,0].ToString(),Utility.GridToWorldPos(new Vector2(x,y) + new Vector2(0.4f,0.4f)),Color.Green);
					spriteBatch.DrawText(GameLayout.AIMoveCache[x,y,1].ToString(),Utility.GridToWorldPos(new Vector2(x,y) + new Vector2(0.6f,0.6f)),Color.Red);
				}
			}
		}
		spriteBatch.End();
		

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred);
		foreach (var obj in objs)
		{
			var texture = obj.GetTexture();
			var transform = obj.GetDrawTransform();

			//if (!obj.IsVisible())//hide tileobjects
			//{
			//	continue;
			//}

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
		*/
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
			

