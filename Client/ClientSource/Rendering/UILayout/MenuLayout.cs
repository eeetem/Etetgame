using DefconNull.Networking;
using DefconNull.Rendering.CustomUIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering.UILayout;

public abstract class MenuLayout : UiLayout
{
	private static Panel? optionsMenu;
	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		if (currentKeyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
		{
		
			if (optionsMenu != null)
			{
				UI.Desktop.Widgets.Remove(optionsMenu);
				optionsMenu = null;

			}
			else
			{

				optionsMenu = new Panel();
				optionsMenu.HorizontalAlignment = HorizontalAlignment.Center;
				optionsMenu.VerticalAlignment = VerticalAlignment.Center;
				optionsMenu.Background = new SolidBrush(Color.Black * 0.5f);
				optionsMenu.BorderThickness = new Thickness(1);
				var stack = new VerticalStackPanel();
				optionsMenu.Widgets.Add(stack);
				var btn1 = new SoundTextButton();
				btn1.Text = "Resume";
				btn1.Click += (sender, args) => { optionsMenu.RemoveFromDesktop(); };
				stack.Widgets.Add(btn1);

				var btn2 = new SoundTextButton();
				btn2.Text = "Settings";
				btn2.Click += (sender, args) => { UI.SetUI(new SettingsLayout()); };
				stack.Widgets.Add(btn2);

				var btn3 = new SoundTextButton();
				btn3.Text = "Quit";
				btn3.Click += (sender, args) => { HandleMenuQuit(); };

				stack.Widgets.Add(btn3);

				if (UI.Desktop != null) UI.Desktop.Widgets.Add(optionsMenu);
			}
			
		}
	}

	protected virtual void HandleMenuQuit()
	{
		GameLayout.GameLayout.tutorial = false;
		if (MasterServerNetworking.IsConnected)
		{
			UI.SetUI(new LobbyBrowserLayout());
		}
		else
		{
			UI.SetUI(new MainMenuLayout());
		}

		
	}
}