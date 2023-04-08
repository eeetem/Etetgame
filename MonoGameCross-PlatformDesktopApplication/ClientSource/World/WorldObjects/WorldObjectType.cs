using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace MultiplayerXeno
{
	public partial class WorldObjectType
	{

		public Transform2 Transform;
		public Texture2D[][] spriteSheet;
		public int variations;
		
	
		public void GenerateSpriteSheet(string name,int variations)
		{
			this.variations = variations;
			spriteSheet = new Texture2D[variations][];
			for (int i = 0; i < variations; i++)
			{
				string spriteName = name;
				if (i > 0)
				{
					spriteName = name + i;
				}

				if (!Faceable)
				{
					spriteSheet[i] = new[] {TextureManager.GetTexture(spriteName)};
					continue;
				}

				spriteSheet[i] = Utility.MakeSpriteSheet(TextureManager.GetTexture(spriteName), 3, 3);
			}
		}
		
	}
}