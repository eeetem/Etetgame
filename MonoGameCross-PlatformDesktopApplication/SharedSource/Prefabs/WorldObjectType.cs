using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno.Prefabs
{
	public class WorldObjectType
	{
		
#if CLIENT
			public Vector2 Offset =Vector2.Zero;
			public int DrawLayer = 0;
#endif

		public readonly string TypeName;
			public bool Faceable = false;

			
			
			public Dictionary<WorldObject.Direction, WorldObject.Cover> Covers  = new Dictionary<WorldObject.Direction, WorldObject.Cover>();


			public virtual WorldObject InitialisePrefab(int ID,Vector2Int position,WorldObject.Direction facing)
			{
				
				WorldObject WO = new WorldObject(position,this,ID);
				WO.Face(facing);
			


				return WO;
				
			}
			public void GenerateSpriteSheet(Texture2D texture)
			{

				if (!Faceable)
				{
					spriteSheet = new[] {new Sprite(texture)};
					return;
				}

				Texture2D[] texture2Ds = Utility.SplitTexture(texture, texture.Width/3, texture.Height/3, out int _, out int _);


				int dir = 0;
				foreach (var splitTexture in texture2Ds)
				{
					if (dir > 7) break;
					spriteSheet[dir] = new Sprite(splitTexture);



					dir++;
				}
			
			}

			public Sprite GetSprite(int index)
			{
				return spriteSheet[index];
			}

			private Sprite[] spriteSheet = new Sprite[8];

	}
}