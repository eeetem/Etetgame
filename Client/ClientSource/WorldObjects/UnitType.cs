using System;
using System.Collections.Generic;
using DefconNull.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.WorldObjects;

public partial class UnitType : WorldObjectType
{
	///private DirectionSpriteSheet[] unitVariationSheets = null!;
	///public override void GenerateSpriteSheet(string name, List<Tuple<string, int>> variations)
	///{
	///	base.GenerateSpriteSheet(name+"/Bottom/", variations);//will make sure variations is not null
	///	
	///	unitVariationSheets = new DirectionSpriteSheet[variations.Count];
	///	for (int i = 0; i < variations.Count; i++)
	///	{
	///		string spriteName = name;
	///		spriteName += variations[i].Item1;
	///		unitVariationSheets[i] = new DirectionSpriteSheet(spriteName+"/Top/", Faceable);
	///	}
	///
	///}
	///
	///
	///public Texture2D GetUnitTopSprite(int spriteVariation, int spriteIndex, string extraState = "")
	///{
	///	return unitVariationSheets[spriteVariation].GetSprite(spriteIndex, extraState);
	///}

}