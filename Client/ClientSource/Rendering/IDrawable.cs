using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.Rendering;

public interface IDrawable
{
	public Transform2 GetDrawTransform();
	public float GetDrawOrder();
	public Texture2D GetTexture();

	public Color GetColor();
	
	bool IsVisible();
}