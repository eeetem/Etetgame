﻿
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.World.WorldObjects;

public partial class WorldObject : Rendering.IDrawable
{
	private Transform2 DrawTransform = null!;
	private int spriteVariation;
	private float DrawOrder;
	public PreviewData PreviewData;
	public Color? OverRideColor { get; set; }

	public Transform2 GetDrawTransform()
	{
		DrawTransform.Position = Type.Transform.Position + Utility.GridToWorldPos(TileLocation.Position);
		return DrawTransform;
	}

	public Vector2Int GetGridPos()
	{
		return TileLocation.Position;
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
		
		DrawOrder += Type.Zoffset;
	}

	public float GetDrawOrder()
	{
		return DrawOrder;
	}

	public Texture2D GetTexture()
	{
		Texture2D sprite;
		int spriteIndex;
		if (fliped&& Type.Faceable)
		{
			spriteIndex = (int)Facing + 4;
		}
		else
		{
			spriteIndex = (int)Facing;
		}
		return Type.GetSprite(spriteVariation, spriteIndex, this);

	}

	public Color GetColor()
	{

		Color color = ((WorldTile)TileLocation).GetTileColor();
				
		if (UnitComponent != null)
		{
			if (UnitComponent.IsMyTeam())
			{
				color = new Color(200,255,200);
			}
			else
			{
				color = new Color(255,200,200);
			}
				
		}

		if (OverRideColor != null)
		{
			Color newcolor = OverRideColor.Value;
			//	newcolor.R += color.R;
			//	newcolor.G += color.G;
			//	newcolor.B += color.B;
			//		newcolor.A += color.A;
			color = newcolor;
		}


		if (IsTransparentUnderMouse())
		{
			color *= 0.3f;
		}

		return color;


	}



	public bool IsTransparentUnderMouse()
	{
		return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
	}


}