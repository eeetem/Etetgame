using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework.Content;
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
	private void GenerateSheetForState( List<string> extraState)
	{
		string state = string.Join("", extraState);
		Texture2D tex;
		while (true)
		{
			try
			{
				string realState = string.Join("", extraState);
				tex = TextureManager.GetTextureFromPNG(GetFulLName() + realState);
			} catch (Exception)
			{
				extraState.RemoveAt(extraState.Count - 1);
				continue;
			}
			break;
		}


		if (!_faceable)
		{
			_spriteSheets.Add(state,new Texture2D[1]{tex});
			return;
		}

		_spriteSheets.Add(state,Utility.MakeSpriteSheet(tex, 3, 3));
	}

	public Texture2D GetSprite(Direction dir,  List<string> extraState)
	{
		string joined = string.Join("", extraState);
		if(!_spriteSheets.ContainsKey(joined)) GenerateSheetForState(extraState);
		var sheet = _spriteSheets[joined];
		if(!_faceable) return sheet[0];
		return sheet[(int) dir];
	}
	public Texture2D GetSprite(int dir,  List<string> extraState)
	{
		return GetSprite(Utility.NormaliseDir(dir),extraState);
	}

	private readonly Dictionary<string,(int,List<string>)> _animations = new Dictionary<string, (int, List<string>)>();
	static readonly object AnimationLock = new object();
	public (int,List<string>) GetAnimation(List<string> extraState, string name)
	{

		lock (AnimationLock)
		{
			var state = string.Join("", extraState);
			var originalpath = "Content/"+GetFulLName()+state + "/" + name+"/";
			if(_animations.TryGetValue(originalpath, out (int, List<string>) lenght)) return lenght;

			var realpath = originalpath;
			while (extraState.Count>0)
			{
				var realState = string.Join("", extraState);
				realpath = "Content/"+GetFulLName()+realState + "/" + name+"/";
				if (!Directory.Exists(realpath))
				{
					extraState.RemoveAt(extraState.Count - 1);
					continue;
				}
				break;
			}
			if (!Directory.Exists(realpath))
			{
				_animations.Add(originalpath, (0,extraState));
				return _animations[originalpath];
			}
			

			var res = Directory.EnumerateFiles(realpath);
			_animations.Add(originalpath, (res.Count(),extraState));
			return _animations[originalpath];
		}
	}

	public string GetVariationName()
	{
		return _variation.Name;
	}
}