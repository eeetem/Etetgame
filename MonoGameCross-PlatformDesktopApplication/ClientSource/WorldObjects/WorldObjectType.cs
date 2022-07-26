using System;
using System.Security;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;


namespace MultiplayerXeno
{
	public partial class WorldObjectType
	{

		public Vector2 Offset =Vector2.Zero;
		public int DrawLayer = 0;
		public Sprite[] spriteSheet;
	
		public void GenerateSpriteSheet(Texture2D texture)
		{

			if (!Faceable)
			{
				spriteSheet = new[] {new Sprite(texture)};
			}

			spriteSheet = new Sprite[8];
			Texture2D[] texture2Ds = Utility.SplitTexture(texture, texture.Width/3, texture.Height/3, out int _, out int _);


			int dir = 0;
			foreach (var splitTexture in texture2Ds)
			{
				if (dir > 7) break;
				spriteSheet[dir] = new Sprite(splitTexture);



				dir++;
			}

		
		}
		
	}
}