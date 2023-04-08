using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Smoke : TileEffect
{
	public Smoke(WorldTile parent) : base(parent, TextureManager.GetTexture("Environment/smoke"))
	{
	}
}