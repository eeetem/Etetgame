using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;


namespace MultiplayerXeno
{
	public partial class WorldObject
	{
		public Transform2 Transform;
		public Sprite GetSprite()
		{
			return Type.GetSprite((int)Facing);
		}
	}


}