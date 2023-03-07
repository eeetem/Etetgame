using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonData;
using MonoGame.Extended;
using MultiplayerXeno.UILayouts;

namespace MultiplayerXeno
{
	public static class WorldEditSystem 
	{

		
		public static bool enabled = false;
		public static void Init()
		{
			enabled = true;
			DiscordManager.client.UpdateState("In Level Editor");
			WorldManager.Instance.CurrentMap = new MapData();
		}

		public enum Brush 
		{
			Point,
			Line,
			Selection
			
		}


		public static Brush ActiveBrush = Brush.Point;
		public static string ActivePrefab = "basicFloor";
		public static Direction ActiveDir = Direction.North;

		private static bool rightMouseDown;
		private static bool leftMouseDown;

		private static bool MouseDown => rightMouseDown || leftMouseDown;

		public static void GenerateUI()
		{
			if(!enabled) return;
			UI.SetUI(new EditorUiLayout());
			UI.LeftClick += (s) =>
			{
				leftMouseDown = true;
			};
			UI.LeftClickUp += (s) => { leftMouseDown = false; };
			UI.RightClick += (s) => { rightMouseDown = true; };
			UI.RightClickUp += (s) => { rightMouseDown = false; };

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					WorldManager.Instance.MakeWorldObject(ActivePrefab,new Vector2Int(x,y),ActiveDir);
				}	
			}



		}

		private static void DeletePrefab(Vector2Int Pos)
		{
			if (!WorldManager.IsPositionValid(Pos))
			{
				return;
			}

			WorldTile tile = WorldManager.Instance.GetTileAtGrid(Pos);

			if (tile.ObjectAtLocation != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.ObjectAtLocation);
				return;
			}
			if (tile.NorthEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.NorthEdge);
				return;
			}
			if (tile.WestEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.WestEdge);
				return;
			}
			if(Pos.Y != 100){
				WorldTile southTile = WorldManager.Instance.GetTileAtGrid(Pos+Utility.DirToVec2(Direction.South));
				if (southTile.NorthEdge != null)
				{
					WorldManager.Instance.DeleteWorldObject(southTile.NorthEdge);
					return;
				}
			}

			if (Pos.X != 100)
			{
				WorldTile eastTile = WorldManager.Instance.GetTileAtGrid(Pos+Utility.DirToVec2(Direction.East));
				if (eastTile.WestEdge != null)
				{
					WorldManager.Instance.DeleteWorldObject(eastTile.WestEdge);
					return;
				}
			}

		


			if (tile.Surface != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.Surface);
				return;
			}




		}

		private static bool IsValidPlacement(Vector2Int pos)
		{
			if (!WorldManager.IsPositionValid(pos))
			{
				return false;
			}

			var tile = WorldManager.Instance.GetTileAtGrid(pos);
			WorldTile tile2;
			WorldObjectType type = PrefabManager.Prefabs[ActivePrefab];
			if (type.Surface)
			{
				return (tile.Surface == null);
			}
			else if (type.Edge)
			{
				switch (ActiveDir)
				{
					case Direction.North:
						return (tile.NorthEdge == null);
						break;
					case Direction.West:
						return (tile.WestEdge == null);
						break;
					case Direction.East:
						tile2 = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.East));
						return (tile2.WestEdge == null);
						break;
					case Direction.South:
						tile2 = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.South));
						return (tile2.NorthEdge == null);
						break;
					default:
						return false;
				}
			}
			else
			{
				return (tile.ObjectAtLocation == null);
			}
		}



		private static KeyboardState lastState;
		private static bool lastMouseDown;
		private static bool lastLeftMouseDown;
		private static bool lastRghtMouseDown;
		private static Vector2Int lastMousePos;

		private static Vector2Int startPoint = new Vector2Int(0,0);
		private static Vector2Int currentPoint = new Vector2Int(0,0);
		private static Vector2Int topLeftSelection = new Vector2Int(0,0);
		private static Vector2Int bottomRightSelection = new Vector2Int(0,0);
		
		public static void Update(GameTime gameTime)
		{
			if(!enabled) return;
			
			var keyboardState = Keyboard.GetState();
			if (keyboardState.IsKeyDown(Keys.E) && lastState.IsKeyUp(Keys.E))
			{
				ActiveDir += 1;
			} else if (keyboardState.IsKeyDown(Keys.Q)&& lastState.IsKeyUp(Keys.Q))
			{
				ActiveDir -= 1;
			}
			if(keyboardState.IsKeyDown(Keys.D1))
			{
				ActiveBrush = Brush.Point;
			}
			if(keyboardState.IsKeyDown(Keys.D2))
			{
				ActiveBrush = Brush.Selection;
			}
			if(keyboardState.IsKeyDown(Keys.D3))
			{
				ActiveBrush = Brush.Line;
			}

			ActiveDir = Utility.NormaliseDir(ActiveDir);

			lastState = keyboardState;

			Vector2Int mousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			switch (ActiveBrush)
			{
				case Brush.Point:
					if (leftMouseDown && IsValidPlacement(mousePos))
					{
						WorldManager.Instance.MakeWorldObject(ActivePrefab,mousePos,ActiveDir);
					}else if ((rightMouseDown && !lastRghtMouseDown)||(rightMouseDown && lastMousePos!=mousePos))
					{
						DeletePrefab(mousePos);

					}

					break;
				case Brush.Selection:
					if (MouseDown && !lastMouseDown)
					{
						startPoint = mousePos;
						break;
					}

					currentPoint = mousePos;
					if (startPoint.X > currentPoint.X)
					{
						topLeftSelection.X = currentPoint.X;
						bottomRightSelection.X = startPoint.X;

					}
					else
					{
						topLeftSelection.X = startPoint.X;
						bottomRightSelection.X = currentPoint.X;
					}
					if (startPoint.Y > currentPoint.Y)
					{
						topLeftSelection.Y = currentPoint.Y;
						bottomRightSelection.Y = startPoint.Y;

					}
					else
					{
						topLeftSelection.Y = startPoint.Y;
						bottomRightSelection.Y = currentPoint.Y;
					}
					if (lastRghtMouseDown && !rightMouseDown)
					{
				
						
						for (int x = topLeftSelection.X; x <= bottomRightSelection.X; x++)
						{
							for (int y = topLeftSelection.Y; y <= bottomRightSelection.Y; y++)
							{
								DeletePrefab(new Vector2Int(x,y));
								
							}
						}
					}else if (lastLeftMouseDown && !leftMouseDown)
					{
						for (int x = topLeftSelection.X; x <= bottomRightSelection.X; x++)
						{
							for (int y = topLeftSelection.Y; y <= bottomRightSelection.Y; y++)
							{
								if(IsValidPlacement(new Vector2Int(x,y)))
								{
									WorldManager.Instance.MakeWorldObject(ActivePrefab,new Vector2Int(x,y),ActiveDir);	
								}
								
								
							}
						}
					}


					break;
			}
			//Console.WriteLine(MouseDown+" "+lastMouseDown);
			lastLeftMouseDown = leftMouseDown;
			lastMouseDown = MouseDown;
			lastRghtMouseDown = rightMouseDown;
			lastMousePos = mousePos;
		}


		public static void Draw(SpriteBatch batch)
		{
			if(!enabled) return;
			var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			batch.DrawString(Game1.SpriteFont,"X:"+MousePos.X+" Y:"+MousePos.Y +" Mode: "+ActiveBrush,  Camera.GetMouseWorldPos(),Color.Wheat, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			batch.DrawLine(Utility.GridToWorldPos(new Vector2(MousePos.X, 0)), Utility.GridToWorldPos(new Vector2(MousePos.X, 100)), Color.White, 5);
			batch.DrawLine(Utility.GridToWorldPos(new Vector2(MousePos.X+1, 0)), Utility.GridToWorldPos(new Vector2(MousePos.X+1, 100)), Color.White, 5);
			batch.DrawLine(Utility.GridToWorldPos(new Vector2(0,  MousePos.Y)), Utility.GridToWorldPos(new Vector2(100, MousePos.Y)), Color.White, 5);
			batch.DrawLine(Utility.GridToWorldPos(new Vector2(0,  MousePos.Y+1)), Utility.GridToWorldPos(new Vector2(100, MousePos.Y+1)), Color.White, 5);
			switch (ActiveBrush)
			{
				case Brush.Selection:
					if (MouseDown)
					{
						batch.DrawLine(Utility.GridToWorldPos(new Vector2(topLeftSelection.X, topLeftSelection.Y)), Utility.GridToWorldPos(new Vector2(topLeftSelection.X, bottomRightSelection.Y + 1)), Color.Peru, 5);
						batch.DrawLine(Utility.GridToWorldPos(new Vector2(topLeftSelection.X, topLeftSelection.Y)), Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, topLeftSelection.Y)), Color.Peru, 5);
						batch.DrawLine(Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, bottomRightSelection.Y + 1)), Utility.GridToWorldPos(new Vector2(topLeftSelection.X, bottomRightSelection.Y + 1)), Color.Peru, 5);
						batch.DrawLine(Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, bottomRightSelection.Y + 1)), Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, topLeftSelection.Y)), Color.Peru, 5);
					}

					break;
			}

			Texture2D previewSprite;
			if (PrefabManager.Prefabs[ActivePrefab].Faceable)
			{
				previewSprite= PrefabManager.Prefabs[ActivePrefab].spriteSheet[0][(int) ActiveDir];
				if ((int)ActiveDir == 2)
				{
					MousePos += new Vector2(1, 0);
				}else if ((int)ActiveDir == 4)
				{
					MousePos += new Vector2(0, 1);
				}
			}else
			{
				 previewSprite = PrefabManager.Prefabs[ActivePrefab].spriteSheet[0][0];
			}



			batch.Draw(previewSprite, Utility.GridToWorldPos(MousePos+new Vector2(-1.5f,-0.5f)), Color.White*0.5f);
		}
	}
}