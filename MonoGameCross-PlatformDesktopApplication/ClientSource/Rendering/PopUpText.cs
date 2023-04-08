﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno;

public class PopUpText
{
	private Transform2 Transform;
	private float aliveTime;
	private string text;
	
	public static readonly object syncobj = new object();
	

	public static List<PopUpText> Objects = new List<PopUpText>();

	public PopUpText(string text,Vector2 position)
	{
		Transform = new Transform2();
		Transform.Position = Utility.GridToWorldPos(position);
		this.text = text;
		lock (syncobj)
		{
			Objects.Add(this);
		}
	}
	

	private static GraphicsDevice graphicsDevice;
	public static void Init(GraphicsDevice graphicsdevice)
	{
		graphicsDevice = graphicsdevice;
	}



	public static void Update(float deltaTime)
	{
		lock (syncobj)
		{

			foreach (var obj in new List<PopUpText>(Objects))
			{

				obj.Transform.Position += new Vector2(0,-0.2f) * deltaTime;
				obj.aliveTime += deltaTime;

				if (obj.aliveTime > 1000)
				{
					Objects.Remove(obj);
					Console.WriteLine("deleted obj");
				}

			}
		}
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		lock (syncobj)
		{


			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
			foreach (var obj in new List<PopUpText>(Objects))
			{


				spriteBatch.DrawString(Game1.SpriteFont, obj.text, obj.Transform.Position, Color.Black * ((1000-obj.aliveTime) / 1000), 0, Vector2.Zero, 3, new SpriteEffects(), 0);



			}

			spriteBatch.End();
		}
	}

}