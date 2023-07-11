using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno;

public interface IDrawable
{
	public Transform2 GetDrawTransform();
	public Vector2Int GetGridPos();
	public float GetDrawOrder();
	public Texture2D GetTexture();

	public Color GetColor();

	public bool IsVisible();


}