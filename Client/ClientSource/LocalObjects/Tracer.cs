using System;
using System.Collections.Generic;
using DefconNull.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using IDrawable = DefconNull.Rendering.IDrawable;

namespace DefconNull.LocalObjects;

public class Tracer : IDrawable
{
	static Effect tracerEffect = null!;
	public static void Init(ContentManager c)
	{
		Tracers.Clear();
		tracerEffect = c.Load<Effect>("CompressedContent/shaders/tracer");
	}
	public Tracer(Vector2 start, Vector2 end)
	{
		transform2 = new Transform2();
		transform2.Scale = new Vector2(10, 10);
		this._start = start;
		this._end = end;
		Tracers.Add(this);
	}

	public static readonly List<Tracer> Tracers = new List<Tracer>();
	private float _lifetime = TotalLife;
	private readonly Vector2 _start;
	private readonly Vector2 _end;
	const float TotalLife = 450f;
	
	private Transform2 transform2;

	public static void Update(float delta)
	{
		foreach (var tracer in new List<Tracer>(Tracers))
		{
			tracer._lifetime -= delta;
			if (tracer._lifetime <= 0)
			{
				Tracers.Remove(tracer);
			}
		}
	}
	public Transform2 GetDrawTransform()
	{
		//line from start to end
		//var height = _lifetime / TotalLife;
		var height =1;
		transform2.Rotation = (float)Math.Atan2(_end.Y - _start.Y, _end.X - _start.X);
		transform2.Position = _start;
		
		transform2.Scale = new Vector2(Vector2.Distance(_start, _end)/GetTexture().Width, height);
		return transform2;
	}

	public float GetDrawOrder()
	{
		return 10000;
	}

	public Texture2D GetTexture()
	{
		return TextureManager.GetTexture("tracer");
	}

	public Color GetColor()
	{
		return Color.Yellow;
	}

	public static void Render(SpriteBatch spriteBatch)
	{
		foreach (var tracer in new List<Tracer>(Tracers))
		{
			// Determine the tiles where the tracer spans over
			var tiles = GetTilesSpannedByTracer(tracer);

			foreach (var tile in tiles)
			{
				// Check if the tile is visible
				if (tile.IsVisible())
				{
					// Calculate the segment of the tracer corresponding to this tile
					var segment = GetTracerSegmentForTile(tracer, tile);

					tracerEffect.Parameters["lifeTime"]?.SetValue(tracer._lifetime/TotalLife);
					spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred, effect:tracerEffect);
					var texture = tracer.GetTexture();
					var transform = tracer.GetDrawTransform();
					var color = tracer.GetColor();
					spriteBatch.Draw(texture, new Vector2(segment.X,segment.Y), null, color, segment.Rotation, Vector2.Zero, segment.Scale, new SpriteEffects(), 0);
					spriteBatch.End();
				}
			}
		}
	}
	public List<WorldTile> GetTilesSpannedByTracer(Tracer tracer)
	{
		List<WorldTile> tiles = new List<WorldTile>();

		// Convert the tracer's start and end positions to grid coordinates
		WorldTile startTile = WorldManager.Instance.GetTileAtGrid(tracer._start);
		WorldTile endTile = WorldManager.Instance.GetTileAtGrid(tracer._end);

		// Determine the tiles that the tracer spans over
		// This could involve iterating over the grid coordinates from startGridPos to endGridPos
		// and adding each tile to the list
		// Assuming that the grid is a 2D array of tiles
		for (int i = startTile.Position.X; i <= endTile.Position.X; i++)
		{
			for (int j = startTile.Position.Y; j <= endTile.Position.Y; j++)
			{
				tiles.Add(WorldManager.Instance.GetTileAtGrid(new Vector2(i, j)));
			}
		}

		return tiles;
	}
	public Rectangle GetTracerSegmentForTile(Tracer tracer, WorldTile tile)
	{


		return segment;
	}
}