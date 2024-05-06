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
				GenerateMenu();
			}
		}
	}

	private void GenerateMenu()
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
		btn1.Click += (sender, args) => { optionsMenu.RemoveFromDesktop(); optionsMenu = null;};
		btn1.Height = (int) (20 * GlobalScale.X);
		btn1.HorizontalAlignment = HorizontalAlignment.Center;
		btn1.Width = (int) (200 * GlobalScale.X);
        
		stack.Widgets.Add(btn1);
        
		var btn2 = new SoundTextButton();
		btn2.Text = "Settings";
		btn2.Click += (sender, args) =>
		{
			var settings = SettingsMenu((() =>
			{
				optionsMenu.Widgets.Clear();
				GenerateMenu();
			}));
			optionsMenu.Widgets.Clear();
			
			
			optionsMenu.Widgets.Add(settings);
			optionsMenu.Height = (int) (250 * GlobalScale.X);
		};
		btn2.Height = (int) (20 * GlobalScale.X);
		btn2.HorizontalAlignment = HorizontalAlignment.Center;
		btn2.Width = (int) (200 * GlobalScale.X);
        
		stack.Widgets.Add(btn2);
        
		var btn3 = new SoundTextButton();
		btn3.Text = "Quit";
		btn3.Click += (sender, args) => { HandleMenuQuit(); };
		btn3.Height = (int) (20 * GlobalScale.X);
		btn3.HorizontalAlignment = HorizontalAlignment.Center;
		btn3.Width = (int) (200 * GlobalScale.X);
        
		stack.Widgets.Add(btn3);
        
		if (UI.Desktop != null) UI.Desktop.Widgets.Add(optionsMenu);
        			
	}

	protected virtual void HandleMenuQuit()
	{
		GameLayout.GameLayout.tutorial = false;
		UI.SetUI(new MainMenuLayout());
		
	}
}