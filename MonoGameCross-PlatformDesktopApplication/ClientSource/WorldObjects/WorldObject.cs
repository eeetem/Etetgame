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
			return Type.spriteSheet[(int)Facing];
		}
	}


}