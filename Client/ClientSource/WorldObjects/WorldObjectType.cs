﻿using System;
using System.Collections.Generic;
using DefconNull.Rendering;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.WorldObjects;

public partial class WorldObjectType
{

	public Transform2 Transform = new Transform2();
	private DirectionSpriteSheet[] _variationSheets = null!;
	public int TotalVariationsWeight = 1;
	public int TotalVarationCount = 1;
	public float Zoffset { get; set; }

	public string GetVariationName(int variation)
	{
		return _variationSheets[variation].GetVariationName();
	}
	public string GetVariationPath(int variation)
	{
		return _variationSheets[variation].GetFulLName();
	}
	public int GetRandomVariationIndex(int seed)
	{
		var r = new Random(seed);
		int roll = r.Next(1000) % TotalVariationsWeight;
		for (int i = 0; i < TotalVarationCount; i++)
		{
			if (roll < _variationSheets[i].GetWeight())
			{
				return i;
			}
			roll -= _variationSheets[i].GetWeight();
		
		}
		throw new Exception("failed to generate sprite variation");
		
	}
	Dictionary<string,int> animationFps = new Dictionary<string, int>();
	public virtual void GenerateSpriteSheet(string name,List<SpriteVariation> variations, Dictionary<string,int> animations)
	{
		animationFps = animations;
		if(variations.Count==0){
			variations.Add(new SpriteVariation("", 1));
		}
		TotalVariationsWeight = 0;
		TotalVarationCount = variations.Count;
		foreach (var va in variations)
		{
			TotalVariationsWeight += va.Weight;
		}
		_variationSheets = new DirectionSpriteSheet[variations.Count];
		for (int i = 0; i < variations.Count; i++)
		{
			_variationSheets[i] = new DirectionSpriteSheet(name, variations[i], Faceable);
		}
	}

	public Texture2D GetSprite(int spriteVariation, int spriteIndex, string extraState = "")
	{
		return _variationSheets[spriteVariation].GetSprite(spriteIndex, new List<string>(){extraState});
	}

	public Texture2D GetSprite(int spriteVariation, int spriteIndex, List<string> extraState )
	{
		return _variationSheets[spriteVariation].GetSprite(spriteIndex, extraState);
	}

    
	public (int,int,List<string>) GetAnimation(int spriteVariation, string name, List<string> extraState)
	{
		int fps = 5;
		if(animationFps.ContainsKey(name)){
			fps = animationFps[name];
		}
		(int, List<string>) lenght = _variationSheets[spriteVariation].GetAnimation(extraState, name);
		return (lenght.Item1, fps,lenght.Item2);
	}
}