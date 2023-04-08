﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts.LayoutWithMenu;

public abstract class MenuLayout : UiLayout
{
	private static Panel optionsMenu;
	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		if (currentKeyboardState.IsKeyDown(Keys.Escape) && lastKeyboardState.IsKeyUp(Keys.Escape))
		{
		
			if (optionsMenu != null && UI.Desktop != null)
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
				var btn1 = new SoundButton();
				btn1.Text = "Resume";
				btn1.Click += (sender, args) => { optionsMenu.RemoveFromDesktop(); };
				stack.Widgets.Add(btn1);

				var btn2 = new SoundButton();
				btn2.Text = "Settings";
				btn2.Click += (sender, args) => { UI.SetUI(new SettingsLayout()); };
				stack.Widgets.Add(btn2);

				var btn3 = new SoundButton();
				btn3.Text = "Quit";
				btn3.Click += (sender, args) => { HandleMenuQuit(); };

				stack.Widgets.Add(btn3);

				if (UI.Desktop != null) UI.Desktop.Widgets.Add(optionsMenu);
			}
			
		}
	}

	protected virtual void HandleMenuQuit()
	{
		Networking.Disconnect(); 
	}
}