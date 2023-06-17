using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace MultiplayerXeno
{
	public partial class WorldObjectType
	{

		public Transform2 Transform;
		public Texture2D[][] spriteSheet;
		public int variations;
		
	
		public void GenerateSpriteSheet(string? name,int variations, bool png = false)
		{
			this.variations = variations;
			spriteSheet = new Texture2D[variations][];
			for (int i = 0; i < variations; i++)
			{
				string? spriteName = name;
				if (this.variations>1)
				{
					spriteName = name + i;
				}
				Texture2D tex;
				if (png)
				{
					tex = TextureManager.GetTextureFromPNG(spriteName);
				}
				else
				{
					tex = TextureManager.GetTexture(spriteName);
				}

				if (!Faceable)
				{
					
					spriteSheet[i] = new[] {tex};
					continue;
				}

				spriteSheet[i] = Utility.MakeSpriteSheet(tex, 3, 3);
			}
		}
		
	}
}