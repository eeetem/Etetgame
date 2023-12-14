using System;
using System.Diagnostics;
using DefconNull.Networking;
using DefconNull.Rendering.CustomUIElements;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Unit = DefconNull.WorldObjects.Unit;

namespace DefconNull.Rendering.UILayout;

public class MainMenuLayout : UiLayout
{
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		NetworkingManager.Disconnect();
		MasterServerNetworking.Disconnect();
		var panel = new Panel()
		{
			Background = new TextureRegion(TextureManager.GetTexture("background")),
		};

		var menustack = new HorizontalStackPanel()
		{
			//	Background = new SolidBrush(Color.wh),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Bottom,
			Top = (int) (200 * globalScale.Y),
			Height = (int) (300 * globalScale.Y),
			Width = (int) (500 * globalScale.X),
			Spacing = 25,

		};
		panel.Widgets.Add(menustack);





		var MPStack = new VerticalStackPanel()
		{
			//	Background = new SolidBrush(new Color(10,10,10)),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Bottom,
			Height = (int) (300 * globalScale.Y),
			Top = (int) (-15 * globalScale.Y)
		};
		menustack.Widgets.Add(MPStack);

		var startlbl = new Label()
		{
			TextColor = Color.Red,
			GridColumn = 0,
			GridRow = 1,
			Text = "Start Game",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		MPStack.Widgets.Add(startlbl);
		var singleplayer = new SoundButton
		{
			GridColumn = 0,
			GridRow = 2,
			Text = "SinglePlayer",
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};
		singleplayer.Click += (a, b) =>
		{
			GameManager.StartLocalServer();
		};


		MPStack.Widgets.Add(singleplayer);
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
				MasterServerNetworking.Connect(Game1.config.GetValue("config","MasterServerIP",""),input.Text);
	
			};
			dialog.ShowModal(desktop);
				
				
		};
		MPStack.Widgets.Add(lobybtn);

		var btn = new SoundButton
		{
			Text = "Direct Connect",
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};

		btn.Click += (s, a) => { UI.SetUI(new ConnectionLayout()); };

		MPStack.Widgets.Add(btn);

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
		discord.Image = new TextureRegion(TextureManager.GetTexture("discord"));
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
		version.Text = "Alpha 2.1.3";
		version.VerticalAlignment = VerticalAlignment.Top;
		version.HorizontalAlignment = HorizontalAlignment.Left;
		version.Background = new SolidBrush(Color.Transparent);
		panel.Widgets.Add(version);
			
		menustack.Widgets.Add(button4);

		
		return panel;


	}


	private int switchTicker = -1;
	public override void Update(float deltatime)
	{
		if (JustPressed(Keys.OemTilde))
		{
			var preGameDataStruct = GameManager.PreGameData;
			preGameDataStruct.SinglePLayerFeatures = true;
GameManager.PreGameData = preGameDataStruct;			
			
			for (int i = 0; i < 8; i++)
			{
				var cdata = new Unit.UnitData(false);
				int r = Random.Shared.Next(1);
				string unit;
				switch (r)
				{
					case 0:
						unit = "Scout";
						break;
					case 1:
						unit = "Grunt";
						break;
					case 2:
						unit = "Heavy";
						break;
					case 3:
						unit = "Officer";
						break;
					case 4:
						unit = "Specialist";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				SequenceManager.AddSequence(WorldObjectManager.MakeWorldObject.Make(unit,new Vector2Int(i,0),(Direction)Random.Shared.Next(8),cdata));
			}
			for (int i = 0; i < 8; i++)
			{
				var cdata = new Unit.UnitData(true);
				int r = Random.Shared.Next(1);
				string unit;
				switch (r)
				{
					case 0:
						unit = "Scout";
						break;
					case 1:
						unit = "Grunt";
						break;
					case 2:
						unit = "Heavy";
						break;
					case 3:
						unit = "Officer";
						break;
					case 4:
						unit = "Specialist";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				SequenceManager.AddSequence(WorldObjectManager.MakeWorldObject.Make(unit,new Vector2Int(i,5),(Direction)Random.Shared.Next(8),cdata));
			}
			
			switchTicker = 0;
		}
		if(switchTicker >= 0)
		{
			switchTicker++;
			if (switchTicker > 10)
			{
				switchTicker = -1;
				UI.SetUI(new GameLayout.GameLayout());
			}
		}

		base.Update(deltatime);
	}
}