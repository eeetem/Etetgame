using CommonData;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno;

public interface IDrawable
{
	public Transform2 GetDrawTransform();
	public Vector2Int GetWorldPos();
	public int GetDrawOrder();
	public Sprite GetSprite();

	public Visibility GetMinimumVisibility();

	public bool IsVisible();

	public bool IsTransparentUnderMouse();
}