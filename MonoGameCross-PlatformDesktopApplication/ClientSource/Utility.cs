using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno;

public static partial class Utility
{
	
	public static void DrawText(this SpriteBatch spriteBatch, string text, Vector2 position, float scale, int width, Color c)
	{
		int row = 0;
		int charsinRow = -1;
		text = text.ToLower(CultureInfo.InvariantCulture);
		for (int index = 0; index < text.Length; index++)
		{
			char car = text[index];
			charsinRow++;
			int nextSpaceCounter = 0;
			int nextSpace = 0;
			//look for next space
			for (int i = index; i < text.Length; i++)
			{
				
				nextSpaceCounter++;
				if (text[i] == ' ')
				{
					nextSpace = nextSpaceCounter;
					break;
				}
			}
			if (charsinRow+nextSpace > width)
			{
				row++;
				charsinRow = 0;
			}

			string texId;
			switch (car)
			{
				case ' ':
					continue;
				case '.':
					texId = "period";
					break;
				case ',':
					texId= "comma";
					break;
				case '+':
					texId= "plus";
					break;
				case '-':
					texId= "minus";
					break;				
				case '!':
					texId= "exclamationmark";
					break;
				case '?':
					texId= "questionmark";
					break;
				case'\'':
					texId = "apostrophe";
					break;
				default:
					texId = ""+car;
					break;
			}

			Texture2D t = TextureManager.GetTexture("UI/text/" + texId);
			spriteBatch.Draw(t, position + new Vector2(8 * charsinRow, 11 * row) * scale, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
	}
}