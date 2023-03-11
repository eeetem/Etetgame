using System.Linq;
using Microsoft.Xna.Framework;
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
				Background = new TextureRegion(TextureManager.GetTexture("UI/background")),
				Padding =new Thickness(35),
				GridColumnSpan = 10,
				GridRowSpan = 10
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
				Margin = new Thickness(5)
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
				Margin = new Thickness(5)
			};
			grid.Widgets.Add(sfxlabel);
			var musiclabel = new Label()
			{
				GridRow = 2,
				GridColumn =1,
				Text = "Music Volume",
				Margin = new Thickness(5)
			};
			grid.Widgets.Add(musiclabel);
			var sfxVolume = new HorizontalSlider()
			{
				GridRow = 1,
				GridColumn = 1,
				Maximum = 1,
				Minimum = 0,
				Value = float.Parse(Game1.config.GetValue("settings", "sfxVol", "0.5"))
			};
			var musicVolume = new HorizontalSlider()
			{
				GridRow = 3,
				GridColumn = 1,
				Maximum = 1,
				Minimum = 0,
				Value = float.Parse(Game1.config.GetValue("settings", "musicVol", "0.5"))
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
			var ok = new SoundButton()
			{
				Text = "OK",	
				GridColumn = 6,
				GridRow = 5,
				Margin = new Thickness(1),

			};
			ok.Click+= (sender, args) =>
			{
				Game1.config.SetValue("settings", "Resolution", resoultion.SelectedItem.Text);
				Game1.config.SetValue("settings", "musicVol", musicVolume.Value);
				Game1.config.SetValue("settings", "sfxVol", sfxVolume.Value);
				Game1.instance.UpdateGameSettings();
				
				UI.SetUI(new MainMenuLayout());
			};
			grid.Widgets.Add(ok);

			return grid;
	}
}