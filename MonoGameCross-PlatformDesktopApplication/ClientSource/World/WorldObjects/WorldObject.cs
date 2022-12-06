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
		private int spriteVariation = 0;
		public Transform2 GetDrawTransform()
		{
			DrawTransform.Position = Type.Transform.Position +Utility.GridToWorldPos(this.TileLocation.Position);
			return DrawTransform;
		}

		public Vector2Int GetWorldPos()
		{
			return this.TileLocation.Position;
		}

		public float GetDrawOrder()
		{
			float order = TileLocation.Position.X + TileLocation.Position.Y;
			if (Type.Surface)
			{
				order--;
				order--;
			}else if (!Type.Surface&&!Type.Edge)
			{
				order += 0.5f;
			}

			return order;
		}

		public Sprite GetSprite()
		{
			Sprite sprite;
			int spriteIndex;
			if (fliped)
			{
				spriteIndex = (int)Facing + 4;
			}
			else
			{
				spriteIndex = (int)Facing;
			}
			if (ControllableComponent != null&& ControllableComponent.Crouching)
			{//this is so fucking convoluted. i'll fix it whenever animations are in
				sprite = Type.Controllable.CrouchSpriteSheet[(int) Utility.NormaliseDir(spriteIndex)];
			
			}
			else
			{	
				sprite = Type.spriteSheet[spriteVariation][(int) Utility.NormaliseDir(spriteIndex)];
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

		public Visibility GetMinimumVisibility()
		{
			if (Type.Surface || Type.Edge)
			{
				return Visibility.None;
			}

			if (ControllableComponent != null && ControllableComponent.Crouching)
			{
				return Visibility.Full;
			}

			return Visibility.Partial;
		}

		public bool IsVisible()
		{
			return GetMinimumVisibility() <= TileLocation.Visible;
		}



		public bool IsTransparentUnderMouse()
		{
			return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
		}


	}


}