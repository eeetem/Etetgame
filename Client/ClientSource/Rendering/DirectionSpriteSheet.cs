using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.Rendering;

public class DirectionSpriteSheet//this operates on the whole folder looking for variants
{
	private readonly Dictionary<string,Texture2D[]> _spriteSheets = new Dictionary<string, Texture2D[]>();
	private readonly bool _faceable;
	private readonly string _baseName;
	private SpriteVariation _variation;
	public DirectionSpriteSheet(string baseName, SpriteVariation variation, bool faceable)
	{
		_baseName = baseName;
		this._variation = variation;
		this._faceable = faceable;
	}

	public string GetFulLName()
	{
		return _baseName+_variation.Name;
	}
	public int GetWeight()
	{
		return _variation.Weight;
	}
	private void GenerateSheetForState(string state)
	{
		Texture2D tex = TextureManager.GetTextureFromPNG(GetFulLName()+state);
		
		if (!this._faceable)
		{
			_spriteSheets.Add(state,new Texture2D[1]{tex});
			return;
		}

		_spriteSheets.Add(state,Utility.MakeSpriteSheet(tex, 3, 3));
	}

	public Texture2D GetSprite(Direction dir, string extraState="")
	{
		if(!_spriteSheets.ContainsKey(extraState)) GenerateSheetForState(extraState);
		var sheet = _spriteSheets[extraState];
		if(!_faceable) return sheet[0];
		return sheet[(int) dir];
	}
	public Texture2D GetSprite(int dir, string extraState="")
	{
		return GetSprite(Utility.NormaliseDir(dir),extraState);
	}

	public int GetAnimationLenght(string name)
	{
		var path = "Content/"+_baseName + "/" + name+_variation.Name+"/";
		if (!Directory.Exists(path)) return 0;
		var res = Directory.EnumerateFiles(path);
		return res.Count();
	}

	public string GetVariationName()
	{
		return _variation.Name;
	}
}