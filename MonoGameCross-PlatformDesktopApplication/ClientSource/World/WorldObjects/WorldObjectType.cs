using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace MultiplayerXeno;

public partial class WorldObjectType
{

	public Transform2 Transform;
	public Texture2D[][] spriteSheet;
	public List<Tuple<string, int>> Variations;
	public int TotalVariationsWeight;
	public float Zoffset { get; set; }
	public void GenerateSpriteSheet(string name,List<Tuple<string, int>> variations, bool png = false)
	{
		foreach (var va in variations)
		{
			TotalVariationsWeight += va.Item2;
		}
		this.Variations = variations;
		spriteSheet = new Texture2D[variations.Count][];
		for (int i = 0; i < variations.Count; i++)
		{
			string spriteName = name;
			spriteName += variations[i].Item1;
			Texture2D tex;
			if (png)
			{
				tex = TextureManager.GetTextureFromPNG(spriteName);
			}
			else
			{
				tex = TextureManager.GetTexture(spriteName);
			}

			if (!Faceable)
			{
					
				spriteSheet[i] = new[] {tex};
				continue;
			}

			spriteSheet[i] = Utility.MakeSpriteSheet(tex, 3, 3);
		}
	}
		
}