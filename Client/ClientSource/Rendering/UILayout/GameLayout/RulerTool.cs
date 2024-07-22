using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Vector2 = System.Numerics.Vector2;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class RulerTool : GameTool
{
    private Vector2Int? location1, location2, midpoint = null;
    private double distance = 0;
    
    public override void click(Vector2Int TileCoordinate, bool rightclick){
        Log.Message("Test"," 4. ruler tool click called");
        //assigns 1st click to variable
        if (location1 == null && rightclick)
        { 
            location1 = TileCoordinate;
            return;
        }

        //assigns 2nd click to variable
        if (location2 == null && rightclick)
        { 
            location2 = TileCoordinate;
            
            //caluculates distance between the 2 tiles
        
            //use location.value since location is nullable, vector2int? is a container that holds null or a vector2int
            //to access the vector2int value, do location.value but only if checked location isnt null
            distance = Vector2Int.Distance(location1.Value, location2.Value);
            //Log.Message("Test"," 5. distance is "+distance);
        
            //get midpoint of 2 points
            midpoint = Vector2Int.Midpoint(location1.Value, location2.Value);
            
            return;
        }

        //prevents user from moving until they finish using ruler tool
        if (!rightclick)
        {
            return;
        }
        
        //deselects tool after 3rd click, as 3rd click calls method again which reaches the next line
        GameLayout.SelectGameTool(null);
    }

    public override void render(SpriteBatch spriteBatch)
    {
        //case switch draws text giving instructions for tool
        var mousePos = Camera.GetMouseWorldPos();
        
        switch (location1)
        {
            case null:
                spriteBatch.DrawText("Ruler Selected: right click first position",(new Vector2(mousePos.X + 2/Camera.GetZoom(), mousePos.Y + 45/Camera.GetZoom())),2/Camera.GetZoom(),Color.Wheat);
                break;
            default:
                if (location2 == null)
                {
                    spriteBatch.DrawText("Ruler Selected: right click second position", (new Vector2(mousePos.X + 2/Camera.GetZoom(), mousePos.Y + 45/Camera.GetZoom())), 2/Camera.GetZoom(),Color.Wheat);
                }
                else
                {
                    spriteBatch.DrawText("Ruler Selected: right click to close",(new Vector2(mousePos.X + 2/Camera.GetZoom(), mousePos.Y + 45/Camera.GetZoom())) ,2/Camera.GetZoom(),Color.Wheat);
                }
                break;
        }
        
        if (location1 != null && location2 != null)
        {
            //draw lines from loc1 to loc2 using midpoint
            spriteBatch.DrawLine(Utility.GridToWorldPos(location1.Value+ new Microsoft.Xna.Framework.Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(location2.Value+ new Microsoft.Xna.Framework.Vector2(0.5f, 0.5f)), Color.Green, 3F, 0F);
            //display distance to screen next to lines
            if (midpoint != null)
            {
                spriteBatch.DrawText(""+Math.Ceiling(distance),Utility.GridToWorldPos(midpoint.Value),3F, Color.White);
            }
        }
    }
    
}