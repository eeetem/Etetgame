﻿using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.Rendering;

public static class TextureManager
{
			
	private static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
	private static Dictionary<string, Texture2D> PngTextures = new Dictionary<string, Texture2D>();
	private static List<string> MissingTextures = new List<string>();
	private static Dictionary<string, Texture2D[]> Sheets = new Dictionary<string, Texture2D[]>();

	private static ContentManager Content = null!;

	private static readonly object SyncObj = new object();
	public static void Init(ContentManager contentManager)
	{
		Content = contentManager;
	}

	public static bool HasTexture(string name)
	{
		if (Textures.ContainsKey(name))
		{
			return true;
		}
		if(MissingTextures.Contains(name))
		{
			return false;
		}


		try
		{
			GetTexture(name);
			return true;
		}
		catch (ContentLoadException)
		{
			MissingTextures.Add(name);
			return false;
		}

		

	}
	private static readonly object textureLock1 = new object();
	private static readonly object textureLock2 = new object();

	public static Texture2D GetTextureFromPNG(string name)
	{
		
		lock (SyncObj)
		{
			if (PngTextures.ContainsKey(name))
			{
				return PngTextures[name];
			}

	
			FileStream fileStream = new FileStream("Content/" + name + ".png", FileMode.Open);
			Texture2D tex = Texture2D.FromStream(Game1.instance.GraphicsDevice, fileStream);
			fileStream.Dispose();
			PngTextures.Add(name, tex);
				
			
		}


		return PngTextures[name];
	}
	public static Texture2D GetTexture(string name)
	{

		lock (SyncObj)
		{
			if (Textures.ContainsKey(name))
			{
				return Textures[name];
			}
			if (name != "")
			{
			
				Textures.Add(name, Content.Load<Texture2D>("CompressedContent/textures/" + name));
			
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

		var texutre = Content.Load<Texture2D>("CompressedContent/textures/" + name);
		var sheet = Utility.MakeSpriteSheet(texutre, x, y);
		Sheets.Add(hash, sheet);
		
		

		return Sheets[hash];
	}

}