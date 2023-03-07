using System.IO;
using System.Linq;
using CommonData;
using MultiplayerXeno;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;

namespace MultiplayerXeno.UILayouts;

public class EditorUiLayout : UiLayout
{
	public override Widget Generate(Desktop desktop)
	{
		
			var grid = new Grid
			{
				RowSpacing = 0,
				ColumnSpacing = 0
			};

			int ypos = 0;
			foreach (var prefabDictElement in PrefabManager.Prefabs)
			{
				
				
				var button = new TextButton
				{
					GridColumn = 0,
					GridRow = ypos,
					Text = prefabDictElement.Key
				};

				button.Click += (s, a) =>
				{ 
					WorldEditSystem.ActivePrefab = prefabDictElement.Key;
				};
				grid.Widgets.Add(button);
				
				
			
				ypos += 1;
			
			}
			var save = new TextButton
			{
				GridColumn = 5,
				GridRow = 0,
				Text = "save"
			};

			save.Click += (s, a) =>
			{ 			
				var panel = new Panel();
				var label = new Label()
				{
					Text = "Enter Map Name",
					Top = 0,
				};
				panel.Widgets.Add(label);
				
				var mapname = new TextBox()
				{
					Top = 25,
				};
				mapname.Text = WorldManager.Instance.CurrentMap.Name;
				panel.Widgets.Add(mapname);
				label = new Label()
				{
					Text = "Author Name",
					Top = 50
				};
				panel.Widgets.Add(label);
				var authorname = new TextBox()
				{
					Top = 75,
					Text = WorldManager.Instance.CurrentMap.Author,
				};
				panel.Widgets.Add(authorname);
				label = new Label()
				{
					Text = "Unit Count",
					Top = 100,
				};
				panel.Widgets.Add(label);
				var unitCount = new TextBox()
				{
					Top = 125,
					Text = WorldManager.Instance.CurrentMap.unitCount.ToString(),
				};
				panel.Widgets.Add(unitCount);
				
				
				
				var dialog = Dialog.CreateMessageBox("Save Map", panel);
				dialog.Height = 250;
				dialog.ButtonOk.Click += (sender, args) =>
				{
					
					WorldManager.Instance.CurrentMap = new MapData();
					WorldManager.Instance.CurrentMap.Name = mapname.Text;
					WorldManager.Instance.CurrentMap.Author = authorname.Text;
					int units = 6;
					bool result  = int.TryParse(unitCount.Text, out units);
					if (result)
					{
						WorldManager.Instance.CurrentMap.unitCount = units;
					}
					else
					{
						WorldManager.Instance.CurrentMap.unitCount = 6;
					}
					WorldManager.Instance.SaveCurrentMapTo("./Maps/"+mapname.Text+".mapdata");
				};
				dialog.ShowModal(desktop);
				
			};
			
			var load = new TextButton
			{
				GridColumn = 5,
				GridRow = 1,
				Text = "load"
			};

			load.Click += (s, a) =>
			{ 
				//dropdown with all maps
				string[] filePaths = Directory.GetFiles("./Maps/", "*.mapdata");
				var panel = new Panel();
				var label = new Label()
				{
					Text = "Select a map to load"
				};
				panel.Widgets.Add(label);
				
				var selection = new ListBox();
				panel.Widgets.Add(selection);
				
				foreach (var path in filePaths)
				{
					var item = new ListItem()
					{
						Text = path.Split("/").Last().Split(".").First(),
					};
					selection.Items.Add(item);
				}
				var dialog = Dialog.CreateMessageBox("Load Map", panel);
				dialog.Width = 100;
				dialog.Height = (int) (1000f * globalScale.Y);
				dialog.ButtonOk.Click += (sender, args) =>
				{
					WorldManager.Instance.LoadMap("./Maps/"+selection.SelectedItem.Text+".mapdata");
				};
				dialog.ShowModal(desktop);
				
			};
			grid.Widgets.Add(save);
			grid.Widgets.Add(load);
			
			var point = new TextButton
			{
				GridColumn = 1,
				GridRow = ypos,
				Text = "Point(1)"
			};
			point.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Point;
			};
			grid.Widgets.Add(point);
			var selection = new TextButton
			{
				GridColumn = 2,
				GridRow = ypos,
				Text = "Selection(2)"
			};
			selection.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Selection;
			};
			grid.Widgets.Add(selection);
			/*var line = new TextButton
			{
				GridColumn = 3,
				GridRow = ypos,
				Text = "Line(3)"
			};
			line.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Line;
			};
			grid.Widgets.Add(line);*/
			
			
			var rotateq = new TextButton
			{
				GridColumn = 2,
				GridRow = ypos-1,
				Text = "<- Rotate(Q)"
			};
			rotateq.Click += (s, a) =>
			{
				WorldEditSystem.ActiveDir--;
			};
			grid.Widgets.Add(rotateq);
			
			var rotatee = new TextButton
			{
				GridColumn = 3,
				GridRow = ypos-1,
				Text = "Rotate(E) -->"
			};
			rotatee.Click += (s, a) =>
			{
				WorldEditSystem.ActiveDir++;
			};
			grid.Widgets.Add(rotatee);



			return grid;
	}
}