using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using MultiplayerXeno;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Network;

namespace MultiplayerXeno.UILayouts;

public class MainMenuLayout : UiLayout
{
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
			var panel = new Panel()
    			{
				Background = new TextureRegion(TextureManager.GetTexture("UI/background")),
			};
			
			

			var menustack = new HorizontalStackPanel()
			{
			//	Background = new SolidBrush(Color.wh),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Bottom,
				Top = (int)(200*globalScale.Y),
				Height = (int)(300*globalScale.Y),
				Width = (int)(500*globalScale.X),
				Spacing = 25,
				
			};
			panel.Widgets.Add(menustack);
			var stack = new VerticalStackPanel()
			{
			//	Background = new SolidBrush(new Color(10,10,10)),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Bottom,
				Height = (int)(300*globalScale.Y),
				Top = (int)(15*globalScale.Y)
			};
			menustack.Widgets.Add(stack);
			
			var startlbl = new Label()
			{
				TextColor = Color.Red,
				GridColumn = 0,
				GridRow = 1,
				Text = "START",
				HorizontalAlignment = HorizontalAlignment.Center
			};
			stack.Widgets.Add(startlbl);
			
			var lobybtn = new SoundButton
			{
				GridColumn = 0,
				GridRow = 2,
				Text = "Server Browser",
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};

			lobybtn.Click += (s, a) =>
			{
				var panel = new Panel();
				var label = new Label()
				{
					Text = "Enter Name"
				};
				panel.Widgets.Add(label);
				var input = new TextBox();
				input.Text = Game1.config.GetValue("config","Name","Operative#"+Random.Shared.Next(1000));
				panel.Widgets.Add(input);
				var dialog = Dialog.CreateMessageBox("Connecting to Master Server...", panel);
				dialog.ButtonOk.Click += (sender, args) =>
				{
					Game1.config.SetValue("config","Name",input.Text);
					Game1.config.Save();
					var result = MasterServerNetworking.Connect(Game1.config.GetValue("config","MasterServerIP",""),input.Text);
					if (result == ConnectionResult.Connected)
					{
						UI.SetUI(new LobbyBrowserLayout());
					}
					else
					{
						UI.ShowMessage("Error", "Could not connect to master server");
					}
				};
				dialog.ShowModal(desktop);
				
				
			};
			stack.Widgets.Add(lobybtn);

			var btn = new SoundButton
			{
				Text = "Direct Connect",
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};

			btn.Click += (s, a) => { UI.SetUI(new ConnectionLayout()); };

			stack.Widgets.Add(btn);

			var button2 = new SoundButton
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "Map Editor",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				Top = (int)(-80*globalScale.Y),
			};

			button2.Click += (s, a) =>
			{
				UI.SetUI(new EditorUiLayout());

			};

			menustack.Widgets.Add(button2);
			
			var button3 = new SoundButton
			{
				GridColumn = 2,
				GridRow = 1,
				Text = "Settings",
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Top,
				Top = (int)(-80*globalScale.Y),
			};

			button3.Click += (s, a) =>
			{
				UI.SetUI(new SettingsLayout());
			};

			menustack.Widgets.Add(button3);
			
			var button4 = new SoundButton
			{
				GridColumn = 3,
				GridRow = 1,
				Text = "QUIT",
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Top,
				Top = (int)(-80*globalScale.Y),
			};

			button4.Click += (s, a) =>
			{
				Game1.instance.Exit();

			};


			var discord = new ImageButton();
			discord.Image = new TextureRegion(TextureManager.GetTexture("UI/discord"));
			discord.Background = new SolidBrush(Color.Transparent);
			discord.OverBackground = new SolidBrush(Color.Transparent);
			discord.VerticalAlignment = VerticalAlignment.Top;
			discord.Width = 100;
			discord.Height = 100;
			discord.HorizontalAlignment = HorizontalAlignment.Right;
			discord.Click += (s, a) =>
			{
				Process.Start("explorer", "https://discord.gg/TrmAJbMaQ3");
			};
			panel.Widgets.Add(discord);


			var version = new Label();
			version.Text = "Alpha 1";
			version.VerticalAlignment = VerticalAlignment.Top;
			version.HorizontalAlignment = HorizontalAlignment.Left;
			version.Background = new SolidBrush(Color.Transparent);
			panel.Widgets.Add(version);
			
			menustack.Widgets.Add(button4);

		
			return panel;


	}
}