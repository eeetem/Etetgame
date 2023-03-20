using System.IO;
using System.Linq;
using CommonData;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class EditorUiLayout : UiLayout
{
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{

			var panel = new Panel();

			var stack = new VerticalStackPanel();
			stack.HorizontalAlignment = HorizontalAlignment.Left;
			panel.Widgets.Add(stack);
			foreach (var prefabDictElement in PrefabManager.Prefabs)
			{
				var button = new TextButton
				{
					Text = prefabDictElement.Key
				};

				button.Click += (s, a) =>
				{ 
					WorldEditSystem.ActivePrefab = prefabDictElement.Key;
				};
				stack.Widgets.Add(button);
				
				

			}

			var save = new TextButton();
			save.Text = "save";
			save.HorizontalAlignment = HorizontalAlignment.Right;
			save.VerticalAlignment = VerticalAlignment.Top;
			save.Click += (s, a) =>
			{ 			
				var panel = new Panel();
				var stack = new VerticalStackPanel();
				panel.Widgets.Add(stack);
				stack.Spacing = 25;
				var label = new Label()
				{
					Text = "Enter Map Name",
					Top = 0,
				};
				stack.Widgets.Add(label);
				
				var mapname = new TextBox()
				{
					Top = 25,
				};
				mapname.Text = WorldManager.Instance.CurrentMap.Name;
				stack.Widgets.Add(mapname);
				label = new Label()
				{
					Text = "Author Name",
					Top = 50
				};
				stack.Widgets.Add(label);
				var authorname = new TextBox()
				{
					Top = 75,
					Text = WorldManager.Instance.CurrentMap.Author,
				};
				stack.Widgets.Add(authorname);
				label = new Label()
				{
					Text = "Unit Count",
					Top = 100,
				};
				stack.Widgets.Add(label);
				var unitCount = new TextBox()
				{
					Top = 125,
					Text = WorldManager.Instance.CurrentMap.unitCount.ToString(),
				};
				stack.Widgets.Add(unitCount);
				
				
				
				var dialog = Dialog.CreateMessageBox("Save Map", panel);
				dialog.Width = (int)(450f*globalScale.X);
				dialog.Height = (int) (500f * globalScale.Y);
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

			var load = new TextButton();
			load.Text = "load";
			load.HorizontalAlignment = HorizontalAlignment.Right;
			load.VerticalAlignment = VerticalAlignment.Top;
			load.Left = (int)(-100*globalScale.X);

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
				selection.Top = (int)(30*globalScale.X);
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
				dialog.Width = (int)(450f*globalScale.X);
				dialog.Height = (int) (500f * globalScale.Y);
				dialog.ButtonOk.Click += (sender, args) =>
				{
					WorldManager.Instance.LoadMap("./Maps/"+selection.SelectedItem.Text+".mapdata");
				};
				dialog.ShowModal(desktop);
				
			};
			panel.Widgets.Add(save);
			panel.Widgets.Add(load);

			var optionstack = new HorizontalStackPanel();
			optionstack.HorizontalAlignment = HorizontalAlignment.Center;
			optionstack.VerticalAlignment = VerticalAlignment.Bottom;
			optionstack.Spacing = 10;
			panel.Widgets.Add(optionstack);
			var rotateq = new TextButton
			{
				Text = "<- Rotate(Q)"
			};
			rotateq.Click += (s, a) =>
			{
				WorldEditSystem.ActiveDir--;
			};
			optionstack.Widgets.Add(rotateq);
			var point = new TextButton();
			point.Text = "Point(1)";
			point.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Point;
			};
			optionstack.Widgets.Add(point);
			var selection = new TextButton
			{
				Text = "Selection(2)"
			};
			selection.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Selection;
			};
			optionstack.Widgets.Add(selection);
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

			var rotatee = new TextButton
			{
				Text = "Rotate(E) -->"
			};
			rotatee.Click += (s, a) =>
			{
				WorldEditSystem.ActiveDir++;
			};
			optionstack.Widgets.Add(rotatee);



			return panel;
	}
}