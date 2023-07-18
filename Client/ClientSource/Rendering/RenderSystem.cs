
using System;
using System.Collections.Generic;
using System.Linq;

using DefconNull.LocalObjects;
using DefconNull.Rendering.UILayout;
using DefconNull.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.Rendering;

public static class RenderSystem
{
	private static GraphicsDevice graphicsDevice = null!;
	public static void Init(GraphicsDevice graphicsdevice)
	{

		graphicsDevice = graphicsdevice;
	}

	public static List<Tuple<Color, List<Vector2Int>>> debugPaths = new List<Tuple<Color, List<Vector2Int>>>();
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

			if (tile.UnitAtLocation != null)
			{
				objs.Add(tile.UnitAtLocation.WorldObject);
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

			if (!obj.IsVisible())//hide tileobjects
			{
				continue;
			}
			
			spriteBatch.Draw(texture,transform.Position,obj.GetColor());
			
			
		}
		spriteBatch.End();
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix());
		count = 0;
		if (Action.ActiveAction == null || Action.ActiveAction.Type == Action.ActionType.Move)
		{

			foreach (var moves in GameLayout.previewMoves.Reverse())
			{
				foreach (var path in moves)
				{


					if (path.X < 0 || path.Y < 0) break;
					var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

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
					spriteBatch.DrawPolygon(pos,new Polygon(new List<Vector2> {Utility.GridToWorldPos(new(-0.5f,-0.5f)),Utility.GridToWorldPos(new(0.5f,-0.5f)),Utility.GridToWorldPos(new(0.5f,0.5f)),Utility.GridToWorldPos(new(-0.5f,0.5f))}),c,2.5f,0);
				
				}

				count++;
			}
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
			

