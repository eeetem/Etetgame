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
	public bool IsAnimating => (CurrentAnimation != null && !CurrentAnimation.IsOver) || _animationQueue.Count > 0;


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
		if (Type.Edge && Type.Faceable && this.Facing == Direction.West)
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

		string state = "";
		if (UnitComponent != null)
		{
			state = "/Stand";
			if (UnitComponent!.Crouching)
			{
				state = "/Crouch";	
			}
		}
		if(destroyed && CurrentAnimation == null) return new Texture2D(Game1.instance.GraphicsDevice, 1, 1);//could cause garbage collector issues
		state+= CurrentAnimation?.GetState(Type.GetVariationName(spriteVariation)) ?? "";
		var baseSprite = Type.GetSprite(spriteVariation, spriteIndex,state);
		return baseSprite;

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

	private readonly Queue<Animation> _animationQueue = new Queue<Animation>();
	public Animation? CurrentAnimation = null;
	public Animation? Loop = null;
	public void AnimationUpdate(float msDelta)
	{
		CurrentAnimation?.Process(msDelta);
		if (CurrentAnimation == null || CurrentAnimation.IsOver)
		{
			CurrentAnimation = null;
			if (_animationQueue.Count > 0)
			{
				CurrentAnimation = _animationQueue.Dequeue();
			}
		}
		
	}

	public bool IsTransparentUnderMouse()
	{
		return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
	}


	public void StartAnimation(string name)
	{
		int count = Type.GetAnimationLenght(spriteVariation, name);
		_animationQueue.Enqueue(new Animation(name,count));
	}
	
}