using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.Rendering;

public static class TextureManager
{
			
    private static ConcurrentDictionary<string, Texture2D> Textures = new ConcurrentDictionary<string, Texture2D>();
    private static ConcurrentDictionary<string, Texture2D> PngTextures = new ConcurrentDictionary<string, Texture2D>();
    private static List<string> MissingTextures = new List<string>();
    private static ConcurrentDictionary<string, Texture2D[]> Sheets = new ConcurrentDictionary<string, Texture2D[]>();

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

    public static Texture2D GetTextureFromPNG(string name)
    {
        if (PngTextures.TryGetValue(name, out var png))
        {
            return png;
        }


        FileStream fileStream = new FileStream("Content/" + name + ".png", FileMode.Open,FileAccess.Read);


        Texture2D tex = Texture2D.FromStream(Game1.instance.GraphicsDevice, fileStream);
        fileStream.Dispose();
        PngTextures.TryAdd(name, tex);

        return PngTextures[name];
    }
    public static Texture2D GetTexture(string name)
    {
 
	
        if (Textures.TryGetValue(name, out var texture))
        {
            return texture;
        }
        if (name != "")
        {
			
            Textures.TryAdd(name, Content.Load<Texture2D>("CompressedContent/textures/" + name));
			
        }
        else
        {
            var tex = new Texture2D(Game1.instance.GraphicsDevice, 10, 10);
            //make it white
            var data = new Color[100];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
            tex.SetData(data);
		
            Textures.TryAdd(name, tex);
			
        }
		

        return Textures[name];
		
    }
    public static Texture2D[] GetSpriteSheet(string name, int x, int y)
    {
        string hash = name + x + y;
        if (Sheets.TryGetValue(hash, out var spriteSheet))
        {
            return spriteSheet;
        }

        var texutre = Content.Load<Texture2D>("CompressedContent/textures/" + name);
        var sheet = Utility.MakeSpriteSheet(texutre, x, y);
        Sheets.TryAdd(hash, sheet);
		
		

        return Sheets[hash];
    }

}