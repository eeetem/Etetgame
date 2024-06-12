using System.Data;
using System.Numerics;

namespace DefconNull.Rendering.UILayout.GameLayout;

public abstract class GameTool
{
    public abstract void click(Vector2Int clickPosition); //processes/registers mouse clicks

    public abstract void render(); //draws sprites

}
