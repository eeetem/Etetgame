using System.Collections.Generic;
using DefconNull.Rendering.UILayout.GameLayout;
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
	public bool IsAnimating => (CurrentAnimation != null && !CurrentAnimation.IsOver);


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
		if (Type.Edge && Type.Faceable && Facing == Direction.West)
		{
			DrawOrder += 0.1f;
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

		string state = GetExtraState();
		if(destroyed && CurrentAnimation == null) return new Texture2D(Game1.instance.GraphicsDevice, 1, 1);//could cause garbage collector issues
		if(CurrentAnimation != null)
		{
			state+= CurrentAnimation?.GetState(Type.GetVariationName(spriteVariation)) ?? "";
		}

		
		var baseSprite = Type.GetSprite(spriteVariation, spriteIndex,state);
		return baseSprite;

	}

	public string GetExtraState()
	{
		string state = "";
		if (UnitComponent != null)
		{
			;
			if (UnitComponent!.Crouching)
			{
				state = "/Crouch";	
			}
			else
			{
				state = "/Stand";
			}
		}
		state += _hiddenState;
		return state;
	}

	private string _hiddenState = "";
	public void SetHiddenState(string state)
	{
		_hiddenState = state;
	}

	public Color GetColor()
	{

		Color color = Color.White;

		if (UnitComponent != null)
		{
			//if (!UnitComponent.IsMyTeam())
			//{
			//	color = new Color(255, 180, 180);
			//}
			if(!IsVisible())
			{
				color = new Color(200, 100, 100) * 0.55f;
			}
			//else
			//{
			//	color = new Color(255, 200, 200);
			//}

		}
		else
		{
			color = ((WorldTile) TileLocation).GetTileColor();
		}



		if (IsTransparentUnderMouse())
		{
			color *= 0.3f;
		}

		return color;


	}

	public bool IsVisible()
	{
		return IsVisible(false);
	}

	public Animation? CurrentAnimation = null;

	public void AnimationUpdate(float msDelta)
	{
		CurrentAnimation?.Process(msDelta);
		if(CurrentAnimation?.ShouldStop == true)
		{
			CurrentAnimation = null;
		}
		if(CurrentAnimation == null)
		{
			StartAnimation("loop");
		}
		
	}

	public bool IsTransparentUnderMouse()
	{
		return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
	}


	public void StartAnimation(string name)
	{
		
		(int, int) anim = Type.GetAnimation(spriteVariation, name,GetExtraState());
		if (anim.Item1 != 0)
			CurrentAnimation = new Animation(name, anim.Item1, anim.Item2);
	}
	
}