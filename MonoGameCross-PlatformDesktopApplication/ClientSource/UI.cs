using System.IO;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerXeno.Pathfinding;
using Myra;
using Myra.Graphics2D.UI;
using Network;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop;
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		private static Texture2D[] coverIndicator = new Texture2D[8];

		public static void Init(ContentManager content, GraphicsDevice graphicsdevice)
		{
			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			MyraEnvironment.Game = Game1.instance;

			
			Desktop = new Desktop();

			Texture2D indicatorSpriteSheet = content.Load<Texture2D>("coverIndicator");

			coverIndicator = Utility.SplitTexture(indicatorSpriteSheet, indicatorSpriteSheet.Width / 3, indicatorSpriteSheet.Width / 3);

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
				Text = "IP"
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
				WorldManager.SaveData("map.mapdata");
			};
			
			var load = new TextButton
			{
				GridColumn = ypos+1,
				GridRow = 1,
				Text = "load"
			};

			load.Click += (s, a) =>
			{ 
				WorldManager.LoadData(File.ReadAllBytes("map.mapdata"));
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

		public static void Render(float deltaTime)
		{
			
			var TileCoordinate = WorldManager.WorldPostoGrid(Camera.GetMouseWorldPos());
			

			Vector2Int? result = null;
			bool found = true;
			
				result = WorldManager.Raycast(new Vector2Int(5,5), TileCoordinate);

			
			if (result == null)
			{
				found = false;
				result = TileCoordinate;
			}
			
			
			var Mousepos = WorldManager.GridToWorldPos(((Vector2)result + new Vector2(-2f,-1f)));


			
			UI.Desktop.Render();
			spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			
			if(result.X < 0 || result.Y < 0) return;
			
			for (int i = 0; i < 8; i++)
			{
				var indicator = coverIndicator[i];
				Color c = Color.White;
				if (found)
				{
					c=Color.Red;
				}
			
					
				spriteBatch.Draw(indicator, Mousepos,c);
			}
			spriteBatch.End();
		
			
			return;
			
			if(WorldManager.PreviewPath == null) return;
			
			foreach (var path in WorldManager.PreviewPath)
			{
				spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
				if(path.X < 0 || path.Y < 0) return;
				Mousepos = WorldManager.GridToWorldPos((Vector2)path + new Vector2(-2f,-1f));
				for (int i = 0; i < 8; i++)
				{
					var indicator = coverIndicator[i];
					Color c = Color.White;
					switch ((Cover)WorldManager.GetTileAtGrid(path).GetCover((Direction)i))
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
					
					spriteBatch.Draw(indicator, Mousepos,c);
				}
				spriteBatch.End();
			}

			



		}
	}
}