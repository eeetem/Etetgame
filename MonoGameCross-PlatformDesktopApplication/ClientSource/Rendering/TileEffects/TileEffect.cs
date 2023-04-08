using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno;

public class TileEffect : IDrawable
{
	private Transform2 _transform;
	private Texture2D sprite;
	public TileEffect(WorldTile parent, Texture2D sprite)
	{
		_transform = new Transform2();
		this.parent = parent;
		this.sprite = sprite;
	}
	private WorldTile parent;
	public Transform2 GetDrawTransform()
	{
		_transform.Position = Utility.GridToWorldPos(GetGridPos()+new Vector2(-1,0));
		return _transform;
	}

	public Vector2Int GetGridPos()
	{
		return parent.Position;
	}

	public float GetDrawOrder()
	{
		return GetGridPos().X + GetGridPos().Y;
	}

	public Texture2D GetTexture()
	{
		return sprite;
	}

	public Color GetColor()
	{
		return Color.White;
	}

	public bool IsVisible()
	{
		return true;
	}
}