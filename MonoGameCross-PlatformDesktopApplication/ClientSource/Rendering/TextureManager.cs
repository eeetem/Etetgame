using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public static class TextureManager
{
			
	public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
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

		Textures.Add(name,Content.Load<Texture2D>("textures/"+name));
		
		return Textures[name];
	}
}