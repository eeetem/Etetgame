using System.Numerics;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class RulerTool : GameTool
{
    private Vector2Int? location1, location2 = null;
    private double distance = 0;
    
    public override void click(Vector2Int TileCoordinate){ //issue 2
        Log.Message("Test"," 4. ruler tool click called");
        //assigns 1st click to variable
        if (location1 == null)
        { 
            location1 = TileCoordinate;
            return;
        }

        //assigns 2nd click to variable
        if (location2 == null)
        { 
            location2 = TileCoordinate;
            return;
        }

        //caluculates distance between the 2 tiles
        
        //use location.value since location is nullable, vector2int? is a container that holds null or a vector2int
        //to access the vector2int value, do location.value but only if checked location isnt null
        distance = Vector2Int.Distance(location1.Value, location2.Value);
        Log.Message("Test"," 5. distance is"+distance);
        
        //deselects tool after 3rd click, as 3rd click calls method again which reaches the next line
        GameLayout.SelectGameTool(null);
        Log.Message("Test"," 6. tool deselected");
    }

    public override void render()
    {
        
        //draw lines from loc1 to loc2
        
        //display distance to screen
        
    }
    
}