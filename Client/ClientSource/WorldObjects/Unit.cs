using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using IDrawable = DefconNull.Rendering.IDrawable;

namespace DefconNull.WorldObjects;

public partial class Unit :IDrawable
{
	
	public bool IsMyTeam()
	{
		return GameManager.IsPlayer1 == IsPlayer1Team;
	}
	public void Spoted()
	{
		foreach (var tile in overWatchedTiles)
		{
			WorldManager.Instance.GetTileAtGrid(tile).CalcWatchLevel();
		}
			
	}


	public Transform2 GetDrawTransform()
	{
		var t = WorldObject.GetDrawTransform();
		if (Crouching)
		{
			t = new Transform2(t.Position+ new Vector2(0,15), t.Rotation, t.Scale);
		}
		return t;
	}

	public Vector2Int GetGridPos()
	{
		return WorldObject.GetGridPos();
	}

	public float GetDrawOrder()
	{
		var order = WorldObject.GetDrawOrder() + 0.6f;
		if(!Crouching) order += 0.1f;
		return order;
	}

	public Texture2D GetTexture()
	{
		int spriteIndex;
		if (WorldObject.Fliped&& Type.Faceable)
		{
			spriteIndex = (int) WorldObject.Facing + 4;
		}
		else
		{
			spriteIndex = (int)WorldObject.Facing;
		}

		string state = "";
		state += WorldObject.CurrentAnimation?.GetState() ?? "";
		if(state=="") state = "Base";
		var baseSprite = Type.GetUnitTopSprite(WorldObject.spriteVariation, spriteIndex,state);
		return baseSprite;
	}

	public Color GetColor()
	{
		return WorldObject.GetColor();
	}

	public bool IsVisible()
	{
		return WorldObject.IsVisible();
	}
}