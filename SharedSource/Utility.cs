using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;

namespace MultiplayerXeno;

public  static partial class Utility
{ 
        
    public static void AddNullableString(this Message msg, string? s)
    {
        if (s is null)
        {
            msg.Add(false);
        }
        else
        {
            msg.Add(true);
            msg.Add(s);
        }
    }
    public static string? GetNullableString(this Message msg)
    {
        if (msg.GetBool())
        {
            return msg.GetString();
        }
        else
        {
            return null;
        }
    }

    public static double Distance(Vector2Int from, Vector2Int to)
    {
        return Math.Sqrt(Vector2Int.SqrDistance(from, to));
    }
    public static Vector2Int DirToVec2(Direction dir)
    {
        dir = NormaliseDir(dir);
        switch (dir)
        {
            case Direction.East:
                return new Vector2Int(1, 0);
            case Direction.North:
                return new Vector2Int(0, -1);
            case Direction.NorthEast:
                return new Vector2Int(1, -1);
            case Direction.West:
                return new Vector2Int(-1, 0);
            case Direction.South:
                return new Vector2Int(0, 1);
            case Direction.SouthWest:
                return new Vector2Int(-1, 1);
            case Direction.SouthEast:
                return new Vector2Int(1, 1);
            case Direction.NorthWest:
                return new Vector2Int(-1, -1);
        }

        throw new Exception("impossible direction");
    }
    public static float Lerp(float firstFloat, float secondFloat, float by)
    {
        by = Math.Clamp(by, 0, 1);
        return firstFloat * (1 - by) + secondFloat * by;
    }
        
    public static int SIZE = 180;

    public static Vector2 GridToWorldPos(Vector2 gridpos)
    {
        Matrix2 isometricTransform = Matrix2.Multiply(Matrix2.CreateRotationZ((float) (Math.PI / 4)), Matrix2.CreateScale(1, 0.5f));

        Vector2 transformVector = Vector2.Transform(new Vector2(gridpos.X * SIZE, gridpos.Y * SIZE), isometricTransform);

        return transformVector;
    }

    public static Vector2 WorldPostoGrid(Vector2 worldPos, bool clamp = true)
    {
        Matrix2 isometricTransform = Matrix2.Multiply(Matrix2.CreateRotationZ((float) (Math.PI / 4)), Matrix2.CreateScale(1, 0.5f));
        isometricTransform = Matrix2.Invert(isometricTransform);

        Vector2 transformVector = Vector2.Transform(worldPos, isometricTransform);

        Vector2 gridPos;

        if (clamp)
        {
            gridPos = new Vector2((int) Math.Floor(transformVector.X / SIZE), (int) Math.Floor(transformVector.Y / SIZE));
        }
        else
        {
            gridPos = new Vector2(transformVector.X / SIZE, transformVector.Y / SIZE);
        }

        if (gridPos.X < 0)
        {
            gridPos.X = 0;
        }

        if (gridPos.Y < 0)
        {
            gridPos.Y = 0;
        }

        return gridPos;
    }

    public static Direction Vec2ToDir(Vector2 vec2)
    {
        vec2.Normalize();

        float angle = vec2.ToAngle() * (float) (180 / Math.PI);

        angle = (float) Math.Round(angle / 45) * 45;

        Vector2 clamped = new Vector2((float) Math.Sin(angle * (Math.PI / 180)), -(float) Math.Cos(angle * (Math.PI / 180)));

        clamped.Normalize();

        clamped.Round();
            
        switch (clamped)
        {
            case (1, 0):
                return Direction.East;
            case (0, -1):
                return Direction.North;
            case (1, -1):
                return Direction.NorthEast;
            case (-1, 0):
                return Direction.West;
            case (0, 1):
                return Direction.South;
            case (-1, 1):
                return Direction.SouthWest;
            case (1, 1):
                return Direction.SouthEast;
            case (-1, -1):
                return Direction.NorthWest;
        }

        throw new Exception("incorrect vector");
    }

    public static Texture2D[] SplitTexture(Texture2D original, int partWidth, int partHeight, out int xCount, out int yCount){
        xCount = original.Width / partWidth + (original.Width % partWidth == 0 ? 0 : 1);//The number of textures in each horizontal row
        yCount = original.Height / partHeight + (original.Height % partHeight == 0 ? 0 : 1);//The number of textures in each vertical column
        Texture2D[] r = new Texture2D[xCount * yCount];//Number of parts = (area of original) / (area of each part).
        int dataPerPart = partWidth * partHeight;//Number of pixels in each of the split parts

        //Get the pixel data from the original texture:
        Color[] originalData = new Color[original.Width * original.Height];
        original.GetData(originalData);

        int index = 0;
        for (int y = 0; y < yCount * partHeight; y += partHeight)
        for (int x = 0; x < xCount * partWidth; x += partWidth)
        {
            //The texture at coordinate {x, y} from the top-left of the original texture
            Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
            //The data for part
            Color[] partData = new Color[dataPerPart];

            //Fill the part data with colors from the original texture
            for (int py = 0; py < partHeight; py++)
            for (int px = 0; px < partWidth; px++)
            {
                int partIndex = px + py * partWidth;
                //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                if (y + py >= original.Height || x + px >= original.Width)
                    partData[partIndex] = Color.Transparent;
                else
                    partData[partIndex] = originalData[x + px + (y + py) * original.Width];
            }

            //Fill the part with the extracted data
            part.SetData(partData);
            //Stick the part in the return array:                    
            r[index++] = part;
        }
        //Return the array of parts.
        return r;
    }

    private static Dictionary<Texture2D, Rectangle> cachedRectangles = new Dictionary<Texture2D, Rectangle>();
    public static Rectangle GetSmallestRectangleFromTexture(Texture2D Texture)
    {
        if (cachedRectangles.ContainsKey(Texture))
        {
            return cachedRectangles[Texture];
        }

        //Create our index of sprite frames
        Color[,] Colors = TextureTo2DArray(Texture);
 
        //determine the min/max bounds
        int x1 = 9999999, y1 = 9999999;
        int x2 = -999999, y2 = -999999;
 
        for (int a = 0; a < Texture.Width; a++)
        {
            for (int b = 0; b < Texture.Height; b++)
            {
                //If we find a non transparent pixel, update bounds if required
                if (Colors[a, b].A != 0)
                {
                    if (x1 > a) x1 = a;
                    if (x2 < a) x2 = a;
 
                    if (y1 > b) y1 = b;
                    if (y2 < b) y2 = b;
                }
            }
        }

        Rectangle rect = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
        cachedRectangles.Add(Texture,rect);
        //We now have our smallest possible rectangle for this texture
        return rect;
    }

    public static Texture2D[] MakeSpriteSheet(Texture2D texture, int xsplits, int ysplits)
    {
           
        Texture2D[] texture2Ds = SplitTexture(texture, texture.Width/xsplits, texture.Height/ysplits, out int _, out int _);

        return texture2Ds;
    }

    //convert texture to 2d array
    private static Color[,] TextureTo2DArray(Texture2D texture)
    {
        //Texture.GetData returns a 1D array
        Color[] colors1D = new Color[texture.Width * texture.Height];
        texture.GetData(colors1D);
 
        //convert the 1D array to 2D for easier processing
        Color[,] colors2D = new Color[texture.Width, texture.Height];
        for (int x = 0; x < texture.Width; x++)
        for (int y = 0; y < texture.Height; y++)
            colors2D[x, y] = colors1D[x + y * texture.Width];
 
        return colors2D;
    }


    public static Texture2D[] SplitTexture(Texture2D texture, int partWidth, int partHeight)
    {
        int xout = 0;
        int yout = 0;
        return  SplitTexture(texture, partWidth, partHeight, out xout, out yout);
    }

    public static Direction NormaliseDir(Direction dir)
    {
        return NormaliseDir((int) dir);
    }

    public static Direction NormaliseDir(int dir)
    {
        while (dir < 0)
        {
            dir += 8;
        }

        while (dir > 7)
        {
            dir -= 8;
        }

        return (Direction)dir;
    }
    
        
    public static Direction GetDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int dir = to - from;
        return Vec2ToDir(dir);
    }
    public static Direction GetDirectionToSideWithPoint(Vector2Int tile, Vector2 point)
    {
        Vector2 dir = point - (Vector2)tile;
        if (dir.X == dir.Y || dir.X == 1-dir.Y || 1-dir.X == dir.Y)
        {
            return Vec2ToDir(dir);
        }


        if (dir.X < 0.1 && dir.X > -0.1)
        {
            return Direction.West;
        }
        if (dir.X < 1.1 && dir.X > 0.9)
        {
            return Direction.East;
        }
        if (dir.Y < 0.1 && dir.Y > -0.1)
        {
            return Direction.North;
        }
        if (dir.Y < 1.1 && dir.Y > 0.9)
        {
            return Direction.South;
        }
        throw new Exception("not an side");

    }

    public static bool IsClose(WorldObject edge, Vector2Int pos)
    {
        if(DoesEdgeBorderTile(edge, pos+new Vector2(1,0)))return true;
        if(DoesEdgeBorderTile(edge, pos+new Vector2(0,1)))return true;
        if(DoesEdgeBorderTile(edge, pos+new Vector2(-1,0)))return true;
        if(DoesEdgeBorderTile(edge, pos+new Vector2(0,-1)))return true;
        return false;
    }


    public static bool DoesEdgeBorderTile(WorldObject edge, Vector2Int pos)
    {
        if(!WorldManager.IsPositionValid(pos))return false;
                
        WorldTile tile = WorldManager.Instance.GetTileAtGrid(pos);
        if (tile.NorthEdge == edge || tile.WestEdge == edge)
        {
            return true;
        }
           
        if (WorldManager.IsPositionValid(pos + new Vector2Int(0, 1)))
        {
            tile = WorldManager.Instance.GetTileAtGrid(pos + new Vector2Int(0, 1));
            if (tile.NorthEdge == edge)
            {
                return true;
            }
        }

        if (WorldManager.IsPositionValid(pos + new Vector2Int(1, 0)))
        {
            tile = WorldManager.Instance.GetTileAtGrid(pos + new Vector2Int(1, 0));
            if (tile.WestEdge == edge)
            {
                return true;
            }
        }

        return false;

    }
        
    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2((float) Math.Cos(radian), (float) Math.Sin(radian));
    }
    public static Vector2 RadianToVector2(float radian, float length)
    {
        return RadianToVector2(radian) * length;
    }
    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * (MathF.PI/180));
    }
    public static Vector2 DegreeToVector2(float degree, float length)
    {
        return RadianToVector2(degree * (MathF.PI/180)) * length;
    }
    public static float RadToDeg(double rad) { return (float) (rad*(180/Math.PI)); }
        
    public static float Vector2ToDegree(Vector2 vec) {
        if (vec.X == 0) // special cases
            return vec.Y > 0? 90
                : vec.Y == 0? 0
                : 270;
        else if (vec.Y == 0) // special cases
            return vec.X >= 0? 0
                : 180;
        float ret = RadToDeg(Math.Atan(vec.Y/vec.X));
        if (vec.X < 0 && vec.Y < 0) // quadrant Ⅲ
            ret = 180 + ret;
        else if (vec.X < 0) // quadrant Ⅱ
            ret = 180 + ret; // it actually substracts
        else if (vec.X < 0) // quadrant Ⅳ
            ret = 270 + (90 + ret); // it actually substracts
        return ret;
    }


    public static Direction ClampFacing(Direction dir)
    {
        while (dir < 0)
        {
            dir += 8;
        }

        while (dir > (Direction) 7)
        {
            dir -= 8;
        }
        return dir;
    }
    public static int ClampFacing(int dir)
    {
        while (dir < 0)
        {
            dir += 8;
        }

        while (dir > 7)
        {
            dir -= 8;
        }
        return dir;
    }
}