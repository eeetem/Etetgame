using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.Rendering;

public class DirectionSpriteSheet
{
	private readonly Dictionary<string,Texture2D[]> _spriteSheets = new Dictionary<string, Texture2D[]>();
	private string texturePath;
	private bool faceable;
	public DirectionSpriteSheet(string texturePath, bool faceable)
	{
		this.texturePath = texturePath;
		this.faceable = faceable;
	}

	private void GenerateSheetForState(string state)
	{
		Texture2D tex = TextureManager.GetTextureFromPNG(texturePath+state);
		
		if (!this.faceable)
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
		if(!faceable) return sheet[0];
		return sheet[(int) dir];
	}
	public Texture2D GetSprite(int dir, string extraState="")
	{
		return GetSprite(Utility.NormaliseDir(dir),extraState);
	}
}