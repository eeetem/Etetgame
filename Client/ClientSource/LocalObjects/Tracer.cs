using System;
using System.Collections.Generic;
using DefconNull.Rendering;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using IDrawable = DefconNull.Rendering.IDrawable;

namespace DefconNull.LocalObjects;

public class Tracer : IDrawable
{
	
	public Tracer(Vector2 start, Vector2 end)
	{
		transform2 = new Transform2();
		transform2.Scale = new Vector2(10, 10);
		this._start = start;
		this._end = end;
		Tracers.Add(this);
	}

	public static readonly List<Tracer> Tracers = new List<Tracer>();
	public float Lifetime = TotalLife;
	private readonly Vector2 _start;
	private readonly Vector2 _end;
	public const float TotalLife = 450f;
	
	private Transform2 transform2;

	public static void Update(float delta)
	{
		foreach (var tracer in new List<Tracer>(Tracers))
		{
			tracer.Lifetime -= delta;
			if (tracer.Lifetime <= 0)
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

	public bool IsVisible()
	{
		return true;
	}


	//public static void Render(SpriteBatch spriteBatch)
	//{
	//	
	//	foreach (var tracer in new List<Tracer>(Tracers))
	//	{
	//		tracerEffect.Parameters["lifeTime"]?.SetValue(tracer._lifetime/TotalLife);
	//		spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred, effect:tracerEffect);
	//		var texture = tracer.GetTexture();
	//		var transform = tracer.GetDrawTransform();
	//		var color = tracer.GetColor();
	//		spriteBatch.Draw(texture, transform.Position,null,color, transform.Rotation,Vector2.Zero, transform.Scale, new SpriteEffects(), 0);
	//		spriteBatch.End();
	//	}
	//
	//}



}