using System.Data;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.Rendering.UILayout.GameLayout;

public abstract class GameTool
{
    public abstract void click(Vector2Int clickPosition, bool rightclick); //processes/registers mouse clicks

    public abstract void render(SpriteBatch spriteBatch); //draws sprites

}
