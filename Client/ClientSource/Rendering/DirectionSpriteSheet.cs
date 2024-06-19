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
		_variation = variation;
		_faceable = faceable;
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
		
		if (!_faceable)
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

	private readonly Dictionary<string,int> _animations = new Dictionary<string,int>();
	static readonly object AnimationLock = new object();
	public int GetAnimation(string extraState, string name)
	{
		var path = "Content/"+GetFulLName()+ extraState+ "/" + name+"/";
		lock (AnimationLock)
		{
			if(_animations.TryGetValue(path, out int lenght)) return lenght;
	
			if (!Directory.Exists(path))
			{
				_animations.Add(path, 0);
				return 0;
			}

			var res = Directory.EnumerateFiles(path);
			_animations.Add(path, res.Count());
			return _animations[path];
		}
	}

	public string GetVariationName()
	{
		return _variation.Name;
	}
}