﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public static class TextureManager
{
			
	public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
	public static Dictionary<string, Texture2D[]> Sheets = new Dictionary<string, Texture2D[]>();

	private static ContentManager Content;

	public static void Init(ContentManager contentManager)
	{
		Content = contentManager;
	}

	public static Texture2D GetTexture(string name)
	{
		if (Textures.ContainsKey(name))
		{
			return Textures[name];
		}

		if (name != "")
		{
			Textures.Add(name, Content.Load<Texture2D>("textures/" + name));
		}
		else
		{
			var tex = new Texture2D(Game1.instance.GraphicsDevice, 10, 10);
			//make it white
			var data = new Color[100];
			for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
			tex.SetData(data);
			Textures.Add(name, tex);
		}


		return Textures[name];
	}
	public static Texture2D[] GetSpriteSheet(string name, int x, int y)
	{
		string hash = name + x + y;
		if (Sheets.ContainsKey(hash))
		{
			return Sheets[hash];
		}

		var texutre = Content.Load<Texture2D>("textures/" + name);
		var sheet = Utility.MakeSpriteSheet(texutre, x, y);
		Sheets.Add(hash, sheet);
		
		

		return Sheets[hash];
	}

}