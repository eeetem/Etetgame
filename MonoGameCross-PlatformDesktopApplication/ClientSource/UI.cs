using System.IO;
using Myra;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop;
		public static void Init()
		{
				
			MyraEnvironment.Game = Game1.instance;

			
			Desktop = new Desktop();
			
		}

		public static void PopUp(string title, string content)
		{
			
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ShowModal(Desktop);
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
				bool result = Networking.Connect(textBox.Text,textBox2.Text);
				if (result)
				{
					var messageBox = Dialog.CreateMessageBox("Connection Notice","Connected to server!");
					messageBox.ShowModal(Desktop);
					grid.Widgets.Remove(button);
					grid.Widgets.Remove(textBox);
					GameUi();
				}
				else
				{
					var messageBox = Dialog.CreateMessageBox("Connection Notice","Failed to connect");
					messageBox.ShowModal(Desktop);
					
				}
	
			};

			grid.Widgets.Add(button);



			Desktop.Root = grid;



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
				WorldObjectManager.SaveData();
			};
			
			var load = new TextButton
			{
				GridColumn = ypos+1,
				GridRow = 1,
				Text = "load"
			};

			load.Click += (s, a) =>
			{ 
				WorldObjectManager.LoadData(File.ReadAllBytes("map.mapdata"));
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
	}
}