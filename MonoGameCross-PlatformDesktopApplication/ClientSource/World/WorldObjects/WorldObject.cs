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
		private float DrawOrder;
		public Transform2 GetDrawTransform()
		{
			DrawTransform.Position = Type.Transform.Position +Utility.GridToWorldPos(this.TileLocation.Position);
			return DrawTransform;
		}

		public Vector2Int GetWorldPos()
		{
			return this.TileLocation.Position;
		}

		private void GenerateDrawOrder()
		{
			
			DrawOrder = TileLocation.Position.X + TileLocation.Position.Y;
			if (Type.Surface)
			{
				DrawOrder--;
				DrawOrder--;
			}else if (!Type.Surface&&!Type.Edge)
			{
				DrawOrder += 0.5f;
			}
		}

		public float GetDrawOrder()
		{
			return DrawOrder;
		}

		public Texture2D GetTexture()
		{
			Texture2D sprite;
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


			return sprite;
		}

		public Color GetColor()
		{
			
			Color color = Color.White;
				
			if (ControllableComponent != null)
			{
				if (ControllableComponent.IsMyTeam())
				{
					color = new Color(200,255,200);
				}
				else
				{
					color = new Color(255,200,200);
				}
				
			}


			if (TileLocation.Visible == Visibility.None)
			{
				color = Color.DimGray;
					
			}else if (TileLocation.Visible == Visibility.Partial)
			{
				color = Color.LightPink;
			}


			if (IsTransparentUnderMouse())
			{
				color *= 0.3f;
			}

			return color;


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