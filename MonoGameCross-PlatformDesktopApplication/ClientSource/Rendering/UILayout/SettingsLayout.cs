using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using MultiplayerXeno;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class SettingsLayout : UiLayout
{
	public override Widget Generate(Desktop desktop)
	{
		var grid = new Grid()
			{
				Background = new TextureRegion(ResourceManager.GetTexture("UI/background")),
				Padding =new Thickness(35),
			};
			grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

			var reslabel = new Label()
			{
				GridRow = 0,
				GridColumn = 0,
				Text = "Resolution",
			};
				grid.Widgets.Add(reslabel);
			var resoultion = new ComboBox()
			{
				GridRow = 1,
				GridColumn = 0,
				
			};
		
			string currentRes = Game1.config.GetValue("settings", "Resolution", "800x600");
			foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes) {
				resoultion.Items.Add(new ListItem(mode.Width + "x" + mode.Height));
				if (mode.Width + "x" + mode.Height == currentRes)
				{
					resoultion.SelectedItem = resoultion.Items.Last();
				}
			}

			if (resoultion.SelectedItem == null)
			{
				resoultion.Items.Add(new ListItem(currentRes));
				resoultion.SelectedItem = resoultion.Items.Last();
			}
			grid.Widgets.Add(resoultion);

			var sfxlabel = new Label()
			{
				GridRow = 0,
				GridColumn = 1,
				Text = "Sound Volume",
			};
			grid.Widgets.Add(sfxlabel);
			var musiclabel = new Label()
			{
				GridRow = 2,
				GridColumn =1,
				Text = "Music Volume",
			};
			grid.Widgets.Add(musiclabel);
			var sfxVolume = new HorizontalSlider()
			{
				GridRow = 1,
				GridColumn = 1,
			};
			var musicVolume = new HorizontalSlider()
			{
				GridRow = 4,
				GridColumn = 1,
			};
			grid.Widgets.Add(sfxVolume);
			grid.Widgets.Add(musicVolume);
			var cancel = new SoundButton()
			{
				Text = "Cancel",
				Margin = new Thickness(1),
				GridColumn = 5,
				GridRow = 5,
			};
			cancel.Click+= (sender, args) =>
			{
				UI.SetUI(new MainMenuLayout());
			};
			grid.Widgets.Add(cancel);
			var ok = new TextButton()
			{
				Text = "OK",	
				GridColumn = 6,
				GridRow = 5,
				Margin = new Thickness(1),

			};
			ok.Click+= (sender, args) =>
			{
				Game1.config.SetValue("settings", "Resolution", resoultion.SelectedItem.Text);
				Game1.instance.UpdateGameSettings();
				
				UI.SetUI(new MainMenuLayout());
			};
			grid.Widgets.Add(ok);

			return grid;
	}
}