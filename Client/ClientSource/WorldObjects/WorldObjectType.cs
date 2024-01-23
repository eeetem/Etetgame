using System;
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
    public Dictionary<string,(int,int)> Animations = new Dictionary<string, (int, int)>();
    public float Zoffset { get; set; }

    public int GetRandomVariationIndex(int seed)
    {
        var r = new Random(seed);
        int roll = (r.Next(1000)) % TotalVariationsWeight;
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
    public virtual void GenerateSpriteSheet(string name,List<SpriteVariation> variations)
    {
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
        return _variationSheets[spriteVariation].GetSprite(spriteIndex, extraState);
    }
}