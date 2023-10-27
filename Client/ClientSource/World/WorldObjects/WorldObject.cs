using System.Collections.Generic;
using DefconNull.World.WorldObjects.Units;
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

		string state = "";
		if (UnitComponent != null)
		{
			state = "/Stand";
			if (UnitComponent!.Crouching)
			{	
				state = "/Crouch";	
			}
		}
		


		return Type.GetSprite(spriteVariation, spriteIndex,state);

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



		if (IsTransparentUnderMouse())
		{
			color *= 0.3f;
		}

		return color;


	}

	private Queue<Animation> animationQueue = new Queue<Animation>();
	private Animation? _currentAnimation = null;
	public void AnimationUpdate(float gametime)
	{

		if (UnitComponent != null)
		{
			UnitComponent.Update(gametime);
		}
		_currentAnimation?.Process(gametime);

		if (_currentAnimation == null || _currentAnimation.IsOver)
		{
			_currentAnimation = null;
			if (animationQueue.Count > 0)
			{
				_currentAnimation = animationQueue.Dequeue();
			}
		}
		
	}

	public bool IsTransparentUnderMouse()
	{
		return Type.Edge && Utility.DoesEdgeBorderTile(this, Utility.WorldPostoGrid(Camera.GetMouseWorldPos()));
	}


}