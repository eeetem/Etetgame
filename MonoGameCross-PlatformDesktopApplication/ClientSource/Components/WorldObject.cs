using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public partial class WorldObject
	{
		

		public static readonly Dictionary<string, Sprite[]> spriteSheets = new Dictionary<string, Sprite[]>();
		public Transform2 Transform;
		public int DrawLayer;
		public Sprite GetSprite()
		{
			return spriteSheets[PrefabName][(int)Facing];
		}
	}


}