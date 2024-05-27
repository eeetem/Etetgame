﻿using System;
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
			tracerEffect.Parameters["lifeTime"]?.SetValue(tracer._lifetime/TotalLife);
			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred, effect:tracerEffect);
			var texture = tracer.GetTexture();
			var transform = tracer.GetDrawTransform();
			var color = tracer.GetColor();
			spriteBatch.Draw(texture, transform.Position,null,color, transform.Rotation,Vector2.Zero, transform.Scale, new SpriteEffects(), 0);
			spriteBatch.End();
		}
	
	}
	
	
//	public static void Render(SpriteBatch spriteBatch)
//	{
//		// Get the list of all tiles
//		var tiles = WorldManager.Instance.get
//			
//			
//		HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
//
//		WorldManager.RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(WorldObject.TileLocation.Position,endTile.Position,Cover.Full,makePath: true);
//
//		foreach (var p in outcome.Path)
//			{
//				positions.Add(p);
//			}
//	
//
//		
//
//			
//			
//		HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
//		foreach (var position in positions)
//		{	
//			var tile = WorldManager.Instance.GetTileAtGrid(position);
//			if(tile.Surface==null) continue;
//			if (action.IsPlausibleToPerform(this, tile.Surface, -1).Item1)
//			{
//				result.Add(position);
//			}
//		}
//
//
//		foreach (var tracer in new List<Tracer>(Tracers))
//		{
//			tracerEffect.Parameters["lifeTime"]?.SetValue(tracer._lifetime/TotalLife);
//			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred, effect:tracerEffect);
//			var texture = tracer.GetTexture();
//			var transform = tracer.GetDrawTransform();
//			var color = tracer.GetColor();
//
//			// Check if the tracer's position intersects with any of the non-visible tiles
//			foreach (var tile in tiles)
//			{
//				
//				if (!tile.IsVisible() && tile.GetDrawBounds().Intersects(new Rectangle((int)transform.Position.X, (int)transform.Position.Y, texture.Width, texture.Height)))
//				{
//					// Calculate the intersection area and mask out that part of the tracer
//					var intersection = Rectangle.Intersect(tile.GetDrawBounds(), new Rectangle((int)transform.Position.X, (int)transform.Position.Y, texture.Width, texture.Height));
//					spriteBatch.Draw(texture, transform.Position, intersection, color, transform.Rotation, Vector2.Zero, transform.Scale, new SpriteEffects(), 0);
//				}
//				else
//				{
//					spriteBatch.Draw(texture, transform.Position, null, color, transform.Rotation, Vector2.Zero, transform.Scale, new SpriteEffects(), 0);
//				}
//			}
//
//			spriteBatch.End();
//		}
//	}

}