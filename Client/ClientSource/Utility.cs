﻿using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.Rendering;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull;

public static partial class Utility
{

	public static void DrawOutline(this SpriteBatch spriteBatch, IEnumerable<Vector2Int> area, Color c, float thickness)
	{
		HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

		foreach (var tile in area)
		{
			tiles.Add(tile);
		}

		DrawOutline(spriteBatch, tiles, c, thickness);
	}

	public static void DrawOutline(this SpriteBatch spriteBatch, IEnumerable<IWorldTile> area, Color c, float thickness)
	{
		HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

		foreach (var tile in area)
		{
			tiles.Add(tile.Position);
		}

		DrawOutline(spriteBatch, tiles, c, thickness);
	}

	public static void DrawOutline(this SpriteBatch spriteBatch, HashSet<Vector2Int> area, Color c, float thickness)
	{
		
		foreach (var t in area)
		{
			if (!area.Contains(t+new Vector2Int(0,1)))
			{
				spriteBatch.DrawLine(GridToWorldPos(t+new Vector2Int(0,1)), GridToWorldPos(t+new Vector2Int(1,1)), c, thickness);
			}
			if (!area.Contains(t+new Vector2Int(1,0)))
			{
				spriteBatch.DrawLine(GridToWorldPos(t+new Vector2Int(1,0)), GridToWorldPos(t+new Vector2Int(1,1)), c, thickness);
			}
			if (!area.Contains(t+new Vector2Int(0,-1)))
			{
				spriteBatch.DrawLine(GridToWorldPos(t+new Vector2Int(0,0)), GridToWorldPos(t+new Vector2Int(1,0)), c, thickness);
			}
			if (!area.Contains(t+new Vector2Int(-1,0)))
			{
				spriteBatch.DrawLine(GridToWorldPos(t+new Vector2Int(0,0)), GridToWorldPos(t+new Vector2Int(0,1)), c, thickness);
			}
		}

		// Draw the outline of the bigger rectangle
		//spriteBatch.DrawRectangle(GridToWorldPos(new Vector2(x,y)),GridToWorldPos(new Vector2(width,height)), c, thickness);



	}
	public static void DrawText(this SpriteBatch spriteBatch, string text, Vector2 position, Color c)
	{
		DrawText(spriteBatch,  text,  position, 1,100,  c);
	}
	public static void DrawText(this SpriteBatch spriteBatch, string text, Vector2 position,float scale, Color c)
	{
		DrawText(spriteBatch,  text,  position, scale,100,  c);
	}
	public static void DrawPrefab(this SpriteBatch spriteBatch, Vector2 Pos, string prefab, Direction dir, bool fliped = false)
	{
		Texture2D previewSprite;
		Color c = Color.White;
		if (PrefabManager.WorldObjectPrefabs[prefab].Faceable)
		{
			if (PrefabManager.WorldObjectPrefabs[prefab].Edge)
			{
				if ((int) dir == 2)
				{
					Pos += GridToWorldPos(new Vector2(1, 0));
				}
				else if ((int) dir == 4)
				{
					Pos += GridToWorldPos(new Vector2(0, 1));
				}
			}

			if (fliped)
			{
				dir+= 4;
			}
			dir = ClampFacing(dir);

			previewSprite = PrefabManager.WorldObjectPrefabs[prefab].GetSprite(0, (int) dir);
		}
		else
		{
			previewSprite = PrefabManager.WorldObjectPrefabs[prefab].GetSprite(0,0);
		}
		spriteBatch.Draw(previewSprite, Pos + PrefabManager.WorldObjectPrefabs[prefab].Transform.Position, c * 0.5f);
	}



	public static void DrawText(this SpriteBatch spriteBatch, string text, Vector2 position, float scale, int width, Color c)
	{
		int row = 0;
		int charsinRow = -1;
		Color originalColor = c;

		for (int index = 0; index < text.Length; index++)
		{
			char car = text[index];
			car = Char.ToLower(car);
			

			if (car == '\n')
			{
				row++;
				charsinRow = -1;
				continue;
			}

			if (car == '[')
			{
				//extrat color
				string color = "";
				for (int i = index + 1; i < text.Length; i++)
				{
					if (text[i] == ']')
					{
						index = i;
						break;
					}
					color += text[i];
					index = i+1;
				}

				if (color == "-")
				{
					c = originalColor;
				}
				else
				{
					var prop = typeof(Color).GetProperty(color);
					if (prop != null)
						c = (Color)(prop.GetValue(null, null) ?? Color.White);
					
				}
				continue;


			}

			if (car == ' ')
			{
				int nextSpaceCounter = 0;
				int nextSpace = 0;
				//look for next space
				for (int i = index+1; i < text.Length; i++)
				{
					nextSpaceCounter++;
					if (text[i] == ' ' || text[i] == '\n')
					{
						nextSpace = nextSpaceCounter;
						break;
					}
				}

				if (charsinRow + nextSpace > width)
				{
					row++;
					charsinRow = -1;
					continue;
				}
			}
			if (charsinRow > width)
			{
				row++;
				charsinRow = -1;
			}

			string texId;
			switch (car)
			{
				case ' ':
					charsinRow++;
					continue;
				case '.':
					texId = "period";
					break;
				case ',':
					texId = "comma";
					break;
				case '+':
					texId = "plus";
					break;
				case '-':
					texId = "minus";
					break;
				case '!':
					texId = "exclamationmark";
					break;
				case '?':
					texId = "questionmark";
					break;
				case ':':
					texId = "colon";
					break;
				case ';':
					texId = "semicolon";
					break;
				case '\'':
					texId = "apostrophe";
					break;
				case '(':
					texId = "leftParentheses";
					break;
				case ')':
					texId = "rightParentheses";
					break;
				case '#':
					texId = "hash";
					break;
				default:
					texId = "" + car;
					break;
			}

			Texture2D t;
			if(TextureManager.HasTexture("UI/text/" + texId)){
				t= TextureManager.GetTexture("UI/text/" + texId);
			}else{
				t = TextureManager.GetTexture("UI/text/broken");
			
			}
		

			spriteBatch.Draw(t, position + new Vector2(8 * charsinRow, 11 * row) * scale, null, c, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
			charsinRow++;
		}
	}
}