using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class FOVTool : GameTool
{
    private Vector2Int? location1, location2 = null;
    private Direction direction;
    private double distance = 0;
    private IDictionary<Vector2Int,Visibility> previewTiles = new Dictionary<Vector2Int, Visibility>();
    private bool toggleCrouch = false;
    public override void click(Vector2Int clickPosition, bool rightclick)
    {
        if (location1 == null && !rightclick)
        {
            location1 = clickPosition;
            return;
        }

        if (location2 == null && !rightclick)
        {
            location2 = clickPosition;
            
            distance = Vector2Int.Distance(location1.Value, location2.Value);
            direction = Utility.Vec2ToDir(location2.Value - location1.Value);
            
            previewTiles = WorldManager.Instance.GetVisibleTiles(location1.Value, direction, (int)distance, toggleCrouch);
            Log.Message("Test", "direction is: "+direction + ", distance is: "+distance);
            return;
        }

        //lets user rightclick to crouch or not
        if (location2 != null && rightclick)
        {
            toggleCrouch = !toggleCrouch;
            previewTiles = WorldManager.Instance.GetVisibleTiles(location1.Value, direction, (int)distance, toggleCrouch);
            return;
        }

        //lets user to finish using tool with 3rd left click
        if (!rightclick && location2 != null)
        {
            GameLayout.SelectGameTool(null);
        }
    }

    public override void render(SpriteBatch spriteBatch)
    {
        foreach (var visTuple in previewTiles)
        {
            WorldTile tile = WorldManager.Instance.GetTileAtGrid(visTuple.Key);
            if (tile.Surface == null) continue;

            Texture2D sprite = tile.Surface.GetTexture();
            Color c = Color.DarkCyan;
            if (visTuple.Value == Visibility.Full)
            {
                c = Color.Purple * 0.45f;
            }else if (visTuple.Value == Visibility.Partial)
            {
                c = Color.Blue * 0.45f;
            }

            spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, c);
			
        }
        spriteBatch.DrawText("\nRange:"+(int)distance+ " Crouched:"+toggleCrouch,  (Camera.GetMouseWorldPos()+new Microsoft.Xna.Framework.Vector2(35, 35)),  2/Camera.GetZoom(),Color.Wheat);
    }
}