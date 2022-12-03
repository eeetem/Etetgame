using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;


namespace MultiplayerXeno
{
	public partial class WorldObject : IDrawable
	{
		private Transform2 DrawTransform;
		public Transform2 GetDrawTransform()
		{
			DrawTransform.Position = Type.Transform.Position +Utility.GridToWorldPos(this.TileLocation.Position);
			return DrawTransform;
		}

		public Vector2Int GetWorldPos()
		{
			return this.TileLocation.Position;
		}

		public int GetDrawOrder()
		{
			int order = TileLocation.Position.X + TileLocation.Position.Y;
			if (Type.Surface)
			{
				order--;
			}else if (!Type.Surface && !Type.Edge)
			{
				order++;
			}

			return order;
		}

		public Sprite GetSprite()
		{
			Sprite sprite;
			if (fliped)
			{
				sprite =  Type.spriteSheet[(int)Utility.NormaliseDir((int)Facing+4)];
			}
			else
			{
				sprite = Type.spriteSheet[(int) Facing];
			}

			if (ControllableComponent != null)
			{
				if (ControllableComponent.IsMyTeam())
				{
					sprite.Color = new Color(200,255,200);
				}
				else
				{
					sprite.Color = new Color(255,200,200);
				}
			}
			else
			{
				sprite.Color = Color.White;
			}

			return sprite;
		}

		public bool IsAlwaysVisible()
		{
			return Type.Surface || Type.Edge;
		}

		public bool IsTransparentUnderMouse()
		{
			return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
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