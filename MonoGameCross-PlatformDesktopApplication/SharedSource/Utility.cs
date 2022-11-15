using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MultiplayerXeno;

namespace MultiplayerXeno
{
	public static class Utility
	{ 
        
        public static Direction ToClampedDirection(Vector2 vector2)
        {
            vector2.Normalize();

            float angle = vector2.ToAngle() * (float) (180 / Math.PI);

            angle = (float) Math.Round(angle / 45) * 45;

            Vector2 clamped = new Vector2(-(float) Math.Sin(angle * (Math.PI / 180)), (float) Math.Cos(angle * (Math.PI / 180)));

            clamped.Normalize();

            clamped.Round();

            return Vec2ToDir(clamped);
        }
        public static Vector2Int DirToVec2(Direction dir)
        {
            dir = Utility.NormaliseDir(dir);
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
        
        
        private const int SIZE = 180;

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

        public static Direction Vec2ToDir(Vector2Int vec2)
        {
            
            switch (vec2)
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
        original.GetData<Color>(originalData);

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
                            partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                    }

                //Fill the part with the extracted data
                part.SetData<Color>(partData);
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
    }
    
    
}
