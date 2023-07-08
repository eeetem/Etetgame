using System;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class ConnectionLayout : UiLayout
{
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		var grid = new Grid
		{
			RowSpacing = 0,
			ColumnSpacing = 0,
			Background = new TextureRegion(TextureManager.GetTexture("UI/background")),
		};
		grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels,300));
		grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
		grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        
		var textBox = new TextBox()
		{
			GridColumn = 1,
			GridRow = 1,
			Text = "46.7.175.47:52233",
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		grid.Widgets.Add(textBox);
		var textBox2 = new TextBox()
		{
			GridColumn = 1,
			GridRow = 2,
			Text =  Game1.config.GetValue("config","Name","Operative#"+Random.Shared.Next(1000)),
			HorizontalAlignment = HorizontalAlignment.Stretch
		};
		grid.Widgets.Add(textBox2);
        
		var button = new SoundButton
		{
			GridColumn = 2,
			GridRow = 1,
			HorizontalAlignment = HorizontalAlignment.Right,
			Text = "Connect"
		};
		var exit = new SoundButton
		{
			GridColumn = 2,
			GridRow = 2,
			HorizontalAlignment = HorizontalAlignment.Right,
			Text = "Main Menu"
		};
		exit.Click += (s, a) => { UI.SetUI(new MainMenuLayout()); };
		grid.Widgets.Add(exit);
		button.Click += (s, a) =>
		{
			var result = Networking.Connect(textBox.Text.Trim(), textBox2.Text.Trim());
			if (result)
			{
				UI.ShowMessage("Connection Notice", "Connecting to server....");
        
			}
			else
			{
				UI.ShowMessage("Connection Notice", "Could not find server!");

			}
        
		};
        
		grid.Widgets.Add(button);


		return grid;
	}
}