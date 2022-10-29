using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MultiplayerXeno.Pathfinding;
using Myra;
using Myra.Graphics2D.UI;
using Network;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop { get; private set; }
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		private static Texture2D[] coverIndicator = new Texture2D[8];
		private static Texture2D[] infoIndicator = new Texture2D[6];

		public static void Init(ContentManager content, GraphicsDevice graphicsdevice)
		{
			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			MyraEnvironment.Game = Game1.instance;

			
			Desktop = new Desktop();
			Desktop.TouchDown += MouseClicked;
		//	Desktop.TouchUp += MouseManager.UiUnlock;
			

			Texture2D coverIndicatorSpriteSheet = content.Load<Texture2D>("coverIndicator");
			

			coverIndicator = Utility.SplitTexture(coverIndicatorSpriteSheet, coverIndicatorSpriteSheet.Width / 3, coverIndicatorSpriteSheet.Width / 3);

			Texture2D indicatorSpriteSheet = content.Load<Texture2D>("indicators");
			

			infoIndicator = Utility.SplitTexture(indicatorSpriteSheet, indicatorSpriteSheet.Width / 6, indicatorSpriteSheet.Height);

			previewMoves[0] = new List<Vector2Int>();
			previewMoves[1] = new List<Vector2Int>();
		}

		public delegate void MouseClick(Vector2Int gridPos);

		public static event MouseClick RightClick;
		public static event MouseClick LeftClick;


		public static void MouseClicked(object? sender, EventArgs e)
		{
			if (UI.Desktop.IsMouseOverGUI)
			{
				return;//let myra do it's thing
			}
			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				LeftClick?.Invoke(gridClick);
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				RightClick?.Invoke(gridClick);
			}


	
		}
	




		public static void MainMenu()
		{
			var grid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8
			};
		
	
			var button = new TextButton
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "Join Server"
			};

			button.Click += (s, a) =>
			{
				ConnectionMenu();
			};

			grid.Widgets.Add(button);
			
			var button2 = new TextButton
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "Map Editor"
			};

			button2.Click += (s, a) =>
			{
				WorldEditSystem.Init();
				WorldEditSystem.GenerateUI();
			};

			grid.Widgets.Add(button2);



			Desktop.Root = grid;

			
		}

		public static void ConnectionMenu()
		{
			
			
			
			var grid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8
			};
		
			var textBox = new TextBox()
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "localhost"
			};
			grid.Widgets.Add(textBox);
			var textBox2 = new TextBox()
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "name"
			};
			grid.Widgets.Add(textBox2);
		
			var button = new TextButton
			{
				GridColumn = 2,
				GridRow = 1,
				Text = "Connect"
			};

			button.Click += (s, a) =>
			{
				ConnectionResult result = Networking.Connect(textBox.Text.Trim(),textBox2.Text.Trim());
				if (result == ConnectionResult.Connected)
				{
					ShowMessage("Connection Notice","Connected to server!");
					
					grid.Widgets.Remove(button);
					grid.Widgets.Remove(textBox);
					GameUi();
				}
				else
				{
					ShowMessage("Connection Notice","Failed to connect: "+result);

				}
	
			};

			grid.Widgets.Add(button);



			Desktop.Root = grid;



		}

		public static void ShowMessage(string title, string content)
		{
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ShowModal(Desktop);

		}

		public static void EditorMenu()
		{
			
			
			
			var grid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8
			};

			int xpos = 0;
			int ypos = 0;
			foreach (var prefabDictElement in PrefabManager.Prefabs)
			{
				
				
				var button = new TextButton
				{
					GridColumn = ypos,
					GridRow = xpos,
					Text = prefabDictElement.Key
				};

				button.Click += (s, a) =>
				{ 
					WorldEditSystem.ActivePrefab = prefabDictElement.Key;
				};
				grid.Widgets.Add(button);
			
				xpos += 1;
			
			}
			var save = new TextButton
			{
				GridColumn = ypos+1,
				GridRow = 0,
				Text = "save"
			};

			save.Click += (s, a) =>
			{ 
				WorldManager.Instance.SaveData("map.mapdata");
			};
			
			var load = new TextButton
			{
				GridColumn = ypos+1,
				GridRow = 1,
				Text = "load"
			};

			load.Click += (s, a) =>
			{ 
				WorldManager.Instance.LoadData(File.ReadAllBytes("map.mapdata"));
			};
			grid.Widgets.Add(save);
			grid.Widgets.Add(load);
			
			Desktop.Root = grid;
			
			
		}

		public static void GameUi()
		{
			
			
			var grid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8
			};

			var end = new TextButton
			{
				GridColumn = 8,
				GridRow = 0,
				Text = "End Turn"
			};
			end.Click += (o,a) => GameManager.EndTurn();		



	
			grid.Widgets.Add(end);
	
			
			Desktop.Root = grid;





		}

		private static List<Vector2Int>[] previewMoves = new List<Vector2Int>[2];
		public static void FullUnitUI(WorldObject worldObject)
		{
			previewMoves = worldObject.ControllableComponent.GetPossibleMoveLocations();
			GameUi();
			var root = (Grid)Desktop.Root;
			
			
			var fire = new TextButton
			{
				GridColumn = 2,
				GridRow = 8,
				Text = "Fire"
			};
			fire.Click += (o, a) => Controllable.ToggleTarget();
			root.Widgets.Add(fire);
		}

		public static void DrawControllableHoverHud(SpriteBatch batch, WorldObject worldObject)
		{
			Controllable controllable = worldObject.ControllableComponent;
			Debug.Assert(controllable != null, nameof(controllable) + " != null");


			Queue<Texture2D> indicators = new Queue<Texture2D>();
			for (int i = 1; i <= 2; i++)
			{
				
				if (controllable.movePoints < i)
				{
					indicators.Enqueue(infoIndicator[0]);
				}
				else
				{
					indicators.Enqueue(infoIndicator[1]);
				}

			}
			for (int i = 1; i <= 2; i++)
			{
				
				if (controllable.turnPoints < i)
				{
					indicators.Enqueue(infoIndicator[2]);
				}
				else
				{
					indicators.Enqueue(infoIndicator[3]);
				}

		
			}
			for (int i = 1; i <= 1; i++)
			{
				
				if (controllable.actionPoints < i)
				{
					indicators.Enqueue(infoIndicator[4]);
				}
				else
				{
					indicators.Enqueue(infoIndicator[5]);
				}

			}

			int offset = 0;
			foreach (var indicator in indicators)
			{

				batch.Draw(indicator,Utility.GridToWorldPos((Vector2)worldObject.TileLocation.Position+new Vector2(-1.0f,-0.6f))+new Vector2(60*offset,0),Color.White);
				offset++;
			}
		}
		
		 static Vector2Int lastMousePos;
		static List<Vector2Int> previewPath = new List<Vector2Int>();
		private static WorldManager.RayCastOutcome previewShot;

		public static void Update(float deltatime)
		{
			if (Controllable.Selected != null)
			{
				Vector2Int currentPos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
				if (lastMousePos !=currentPos)
				{

					if (Controllable.Targeting)
					{
						previewShot = WorldManager.Instance.Raycast(Controllable.Selected.worldObject.TileLocation.Position, currentPos);
					}
					else
					{
						previewPath = PathFinding.GetPath(Controllable.Selected.worldObject.TileLocation.Position, currentPos).Path;
						if (previewPath == null)
						{
							previewPath = new List<Vector2Int>();
						}
					}

					
					lastMousePos = currentPos;
				}
			}
		}

		public static void Render(float deltaTime)
		{
		
			
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			
			

			bool found = true;
			
			
			
			var Mousepos = Utility.GridToWorldPos(((Vector2)TileCoordinate+new Vector2(-1.5f,-0.5f)));//idk why i need to add a vector but i do and im not bothered figuring out why


			
			UI.Desktop.Render();
			spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			
			if(TileCoordinate.X < 0 || TileCoordinate.Y < 0) return;
			
			for (int i = 0; i < 8; i++)
			{
				var indicator = coverIndicator[i];
				Color c = Color.White;
				switch ((Cover)WorldManager.Instance.GetTileAtGrid(TileCoordinate).GetCover((Direction)i))
				{
					case Cover.Full:
						c = Color.Red;
						break;
					case Cover.High:
						c = Color.Yellow;
						break;
					case Cover.Low:
						c = Color.Green;
						break;
				}
			
				//spriteBatch.DrawCircle(Mousepos, 5, 10, Color.Red, 200f);
				spriteBatch.Draw(indicator, Mousepos,c);
			}
			spriteBatch.End();

		/*raycastdebug
			spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			var templist = new List<WorldManager.Instance.RayCastOutcome>(WorldManager.Instance.RecentFOVRaycasts);
			templist.Add(WorldManager.Instance.Raycast(new Vector2Int(5,5), TileCoordinate));
			
			foreach (var cast in templist)
			{

				spriteBatch.DrawLine(Utility.GridToWorldPos(cast.StartPoint),Utility.GridToWorldPos(cast.EndPoint),Color.Green,5);
				if(cast.CollisionPoint.Count == 0) break;
				spriteBatch.DrawCircle(Utility.GridToWorldPos(cast.CollisionPoint.Last()), 5, 10, Color.Red, 5f);
				spriteBatch.DrawLine(Utility.GridToWorldPos(cast.CollisionPoint.Last()), Utility.GridToWorldPos(cast.CollisionPoint.Last())+(Utility.GridToWorldPos(cast.VectorToCenter)/2f),Color.Red,5);

				foreach (var point in cast.CollisionPoint)
				{
					
					spriteBatch.DrawCircle(Utility.GridToWorldPos(point), 5, 10, Color.Green, 5f);
				}
				
			}
			
			spriteBatch.End();
		
			
			
			
			*/

/* griddebug
			spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
				
					spriteBatch.DrawCircle(Utility.GridToWorldPos(new Vector2(x,y)), 5, 10, Color.Black, 5f);
				}
			}
			spriteBatch.End();
			
			*/

			int count = 0;
			foreach (var moves in previewMoves.Reverse())
			{
				foreach (var path in moves)
				{
				
					spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
					if(path.X < 0 || path.Y < 0) return;
					Mousepos = Utility.GridToWorldPos((Vector2)path + new Vector2(0.5f,0.5f));

					Color c = Color.White;
					switch (count)
					{
						case 0:
							c = Color.Yellow;
							break;
						case 1:
							c= Color.Green;
							break;
						default:
							c=Color.Red;
							break;

					}
					
					spriteBatch.DrawRectangle(Mousepos, new Size2(20, 20), c, 5);
				
					spriteBatch.End();
				
				}

				count++;
			}


			if (Controllable.Targeting)
			{
				

				spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);

				var startPoint = Utility.GridToWorldPos(previewShot.StartPoint);
				var endPoint = Utility.GridToWorldPos(previewShot.EndPoint);
				spriteBatch.DrawLine(startPoint.X,startPoint.Y,endPoint.X,endPoint.Y,Color.Red,10);
				if (previewShot.hit)
				{
					spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.CollisionPoint.Last()),15,10,Color.Yellow,50f);
				}
				spriteBatch.End();
				
			}
			else
			{
				
				foreach (var path in previewPath)
				{
					spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
					if(path.X < 0 || path.Y < 0) return;
					Mousepos = Utility.GridToWorldPos((Vector2)path + new Vector2(0.5f,0.5f));
				
					spriteBatch.DrawCircle(Mousepos,20,10,Color.Green,20f);
				
					spriteBatch.End();
				}
			}

		

			



		}
	}
}