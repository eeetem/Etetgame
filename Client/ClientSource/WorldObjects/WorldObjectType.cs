using System;
using System.Collections.Generic;
using DefconNull.Rendering;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.WorldObjects;

public partial class WorldObjectType
{

	public Transform2 Transform = new Transform2();
	private DirectionSpriteSheet[] variationSheets = null!;
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
		variationSheets = new DirectionSpriteSheet[variations.Count];
		for (int i = 0; i < variations.Count; i++)
		{
			string spriteName = name;
			spriteName += variations[i].Item1;
			variationSheets[i] = new DirectionSpriteSheet(spriteName, Faceable);
		}
	}

	public virtual Texture2D GetSprite(int spriteVariation, int spriteIndex, string extraState = "")
	{
		return variationSheets[spriteVariation].GetSprite(spriteIndex, extraState);
	}
}