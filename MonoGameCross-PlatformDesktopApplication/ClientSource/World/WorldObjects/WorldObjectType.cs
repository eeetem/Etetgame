using System;
using System.Security;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;


namespace MultiplayerXeno
{
	public partial class WorldObjectType
	{

		public Transform2 Transform;
		public int DrawLayer = 0;
		public Sprite[] spriteSheet;
	
		public void GenerateSpriteSheet(Texture2D texture)
		{
			
			if (!Faceable)
			{
				spriteSheet = new[] {new Sprite(texture)};
				return;
			}

			spriteSheet = Utility.MakeSpriteSheet(texture, 3, 3);
			
		}
		
	}
}