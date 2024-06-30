using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DefconNull.LocalObjects;
using DefconNull.Rendering.UILayout.GameLayout;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.Rendering;

public static class RenderSystem
{
	public static GraphicsDevice GraphicsDevice = null!;
	public static void Init(GraphicsDevice graphicsdevice, ContentManager c)
	{
		GraphicsDevice = graphicsdevice;
		tracerEffect = c.Load<Effect>("CompressedContent/shaders/tracer");
	}


	static List<IDrawable> objs = new List<IDrawable>();
	static List<IDrawable> unsortedObjs = new List<IDrawable>();
	static List<IDrawable> invisibleTiles = new List<IDrawable>();
	static Effect tracerEffect = null!;
	public static void Draw(SpriteBatch spriteBatch)
	{
		List<WorldTile> allTiles = WorldManager.Instance.GetAllTiles();
		objs.Clear();
		invisibleTiles.Clear();
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
			//if (tile.UnitAtLocation != null)
			//{
			//	objs.Add(tile.UnitAtLocation);
			//}

			foreach (var item in tile.ObjectsAtLocation)
			{
				objs.Add(item);
			}
		

		}

		objs.AddRange(Particle.Objects);
		

		objs.Sort(new DrawableSort());

		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
		
		foreach (var obj in unsortedObjs)
		{
			
			if(obj == null)continue;
			var texture = obj.GetTexture();
			var transform = obj.GetDrawTransform();

			if (obj is WorldObject)
			{
				if (!((WorldTile)((WorldObject)obj).TileLocation).IsVisible())
				{
					invisibleTiles.Add(obj);
					continue;
				}
			}
			if (!obj.IsVisible())//hide tileobjects
			{
				invisibleTiles.Add(obj);
				continue;
			}
			
			spriteBatch.Draw(texture,transform.Position,obj.GetColor());
			
			
		}
		spriteBatch.End();
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix());
		
		var selectedglow = TextureManager.GetTexture("selectionglow"); 
		if (GameLayout.SelectedUnit != null)
		{
			var unitposition = Utility.GridToWorldPos(GameLayout.SelectedUnit.WorldObject.TileLocation.Position +
			                                          new Vector2(-1.5f,-0.5f));
			Color cc = Color.White; 
			spriteBatch.Draw(selectedglow,unitposition , cc);

		}
		spriteBatch.End();

		foreach (var tracer in new List<Tracer>(Tracer.Tracers))
		{
			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred, effect:tracerEffect);
			
			tracerEffect.Parameters["lifeTime"]?.SetValue(tracer.Lifetime/Tracer.TotalLife);
		
			var texture = tracer.GetTexture();
			var transform = tracer.GetDrawTransform();
			var color = tracer.GetColor();

			
			spriteBatch.Draw(texture, transform.Position,null,color, transform.Rotation,Vector2.Zero, transform.Scale, new SpriteEffects(), 0);
			spriteBatch.End();
		}
		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred);
		
		
		
		foreach (var obj in invisibleTiles)
		{
			
			if(obj == null)continue;
			var texture = obj.GetTexture();
			var transform = obj.GetDrawTransform();

			
			spriteBatch.Draw(texture,transform.Position,obj.GetColor());
			
			
		}
		int count = 0;

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
		
				
		foreach (var obj in objs)
		{
			var texture = obj.GetTexture();
			var transform = obj.GetDrawTransform();
			spriteBatch.Draw(texture, transform.Position+new Vector2(texture.Width/2f,texture.Height/2f),null,obj.GetColor(), transform.Rotation,new Vector2(texture.Width/2f,texture.Height/2f), transform.Scale, new SpriteEffects(), 0);

		}
		spriteBatch.End();
		


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