using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno
{
	public static class Utility
	{ 
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

  
    }
    
    
}
