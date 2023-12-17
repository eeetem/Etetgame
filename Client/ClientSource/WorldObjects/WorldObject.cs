using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using IDrawable = DefconNull.Rendering.IDrawable;

namespace DefconNull.WorldObjects;

public partial class WorldObject : IDrawable
{
	private Transform2 DrawTransform = null!;
	public int spriteVariation;
	public PreviewData PreviewData;
	

	public Transform2 GetDrawTransform()
	{
		DrawTransform.Position = Type.Transform.Position + Utility.GridToWorldPos(TileLocation.Position);
		return DrawTransform;
	}

	public Vector2Int GetGridPos()
	{
		return TileLocation.Position;
	}



	public float GetDrawOrder()
	{
		float DrawOrder = TileLocation.Position.X + TileLocation.Position.Y + 0.1f;
		if (Type.Surface)
		{
			DrawOrder--;
			DrawOrder--;
		}else if (!Type.Surface&&!Type.Edge)
		{
			DrawOrder += 0.5f;
		}

		if (Type.Edge)
		{
			switch (Type.SolidCover)
			{
				case Cover.Full:
					DrawOrder += 0.3f;
					break;
				case Cover.High:
					DrawOrder += 0.2f;
					break;
				case Cover.Low:
					DrawOrder += 0.1f;
					break;
			}
		}

		DrawOrder += Type.Zoffset;
		return DrawOrder;
	}


	public Texture2D GetTexture()
	{
		int spriteIndex;
		if (Fliped&& Type.Faceable)
		{
			spriteIndex = (int)Facing + 4;
		}
		else
		{
			spriteIndex = (int)Facing;
		}

		string state = "";
		if (UnitComponent != null)
		{
			state = "Stand";
			if (UnitComponent!.Crouching)
			{
				state = "Crouch";	
			}
		}
		state+= CurrentAnimation?.GetState() ?? "";
		var baseSprite = Type.GetSprite(spriteVariation, spriteIndex,state);
		return baseSprite;

	}

	public Color GetColor()
	{

		Color color = ((WorldTile) TileLocation).GetTileColor();

		if (UnitComponent != null)
		{
			if (UnitComponent.IsMyTeam())
			{
				color = new Color(200, 255, 200);
			}
			else if(!IsVisible())
			{
				color = new Color(255, 100, 100);
			}
			else
			{
				color = new Color(255, 200, 200);
			}

		}



		if (IsTransparentUnderMouse())
		{
			color *= 0.3f;
		}

		return color;


	}

	private Queue<Animation> animationQueue = new Queue<Animation>();
	public Animation? CurrentAnimation = null;
	public void AnimationUpdate(float gametime)
	{

		if (UnitComponent != null)
		{
			UnitComponent.Update(gametime);
		}
		CurrentAnimation?.Process(gametime);

		if (CurrentAnimation == null || CurrentAnimation.IsOver)
		{
			CurrentAnimation = null;
			if (animationQueue.Count > 0)
			{
				CurrentAnimation = animationQueue.Dequeue();
			}
		}
		
	}

	public bool IsTransparentUnderMouse()
	{
		return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
	}


}