using System;
using System.Collections.Generic;
using DefconNull.Rendering;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;

namespace DefconNull.World.WorldObjects;

public partial class WorldObjectType
{

	public Transform2 Transform = new Transform2();
	public Texture2D[][] spriteSheet;
	public List<Tuple<string, int>> Variations;
	public int TotalVariationsWeight;
	public float Zoffset { get; set; }
	public void GenerateSpriteSheet(string name,List<Tuple<string, int>>? variations = null)
	{
		if(variations == null){
			variations = new List<Tuple<string, int>>();
			variations.Add(new Tuple<string, int>("", 1));
		}
		foreach (var va in variations)
		{
			TotalVariationsWeight += va.Item2;
		}
		Variations = variations;
		spriteSheet = new Texture2D[variations.Count][];
		for (int i = 0; i < variations.Count; i++)
		{
			string spriteName = name;
			spriteName += variations[i].Item1;
			Texture2D tex = TextureManager.GetTextureFromPNG(spriteName);


			if (!Faceable)
			{
					
				spriteSheet[i] = new[] {tex};
				continue;
			}

			spriteSheet[i] = Utility.MakeSpriteSheet(tex, 3, 3);
		}
	}

	public virtual Texture2D GetSprite(int spriteVariation, int spriteIndex, WorldObject worldObject)
	{
		return spriteSheet[spriteVariation][(int) Utility.NormaliseDir(spriteIndex)];
	}
}