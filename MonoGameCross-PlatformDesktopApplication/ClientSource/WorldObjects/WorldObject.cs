using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;


namespace MultiplayerXeno
{
	public partial class WorldObject
	{

		public Sprite GetSprite()
		{
			if (fliped)
			{
				return Type.spriteSheet[(int)Utility.NormaliseDir((int)Facing+4)];
			}
			return Type.spriteSheet[(int)Facing];
		}
		public int GetDrawLayer()
		{
			//if (fliped)
			//{
		//		return Type.DrawLayer + 1;
		//	}

			return Type.DrawLayer;
		}
	}


}