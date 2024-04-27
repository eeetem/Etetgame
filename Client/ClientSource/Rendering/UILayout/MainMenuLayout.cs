//#define SINGLEPLAYER_ONLY
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.Rendering.CustomUIElements;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Unit = DefconNull.WorldObjects.Unit;

namespace DefconNull.Rendering.UILayout;



public class MainMenuLayout : UiLayout
{
	public static Vector2 gradientPos = new Vector2(-10000, 0);
	private static Vector2 targetGradientPos = new Vector2(0, 0);
	public override void Update(float deltatime)
	{
		
		gruntOffestTarget = new Vector2((-275+(Game1.instance.GraphicsDevice.Viewport.Width- Camera.GetMouseWorldPos().X)*0.1f),0);
		gruntOffestTarget = Vector2.Clamp(gruntOffestTarget,new Vector2(-275,0),new Vector2(25,0));
		gruntOffestCurrent = Vector2.Lerp(gruntOffestCurrent,gruntOffestTarget,0.05f);

		gradientPos = Vector2.Lerp(gradientPos, targetGradientPos, 0.05f);
		menuStack.Left = (int)((25+gradientPos.X) * globalScale.X);
		socialStack.Left = (int)((-20-gradientPos.X) * globalScale.X);
		int i = 0;
		foreach (var wid in menuStack.Widgets)
		{
			wid.Left = (int)(gradientPos.X * i*globalScale.X);
			
			i++;
		}
		base.Update(deltatime);
	}
	
	Vector2 gruntOffestTarget = new Vector2(-10,0);
	Vector2 gruntOffestCurrent = new Vector2(-10,0);
	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		var back = TextureManager.GetTexture("MainMenu/staticLayer");
		var backdrop = TextureManager.GetTexture("MainMenu/backdrop");
	
		var border = TextureManager.GetTexture("MainMenu/borders");
		var grunt = TextureManager.GetTexture("MainMenu/grunt");
		var smoke0 = TextureManager.GetTexture("MainMenu/smokeLayer0");
		var smoke1 = TextureManager.GetTexture("MainMenu/smokeLayer1");
		var smoke2 = TextureManager.GetTexture("MainMenu/smokeLayer2");
		var smoke3 = TextureManager.GetTexture("MainMenu/smokeLayer3");
		
		
		var deb0 = TextureManager.GetTexture("MainMenu/debrislayer0");
		var deb1 = TextureManager.GetTexture("MainMenu/debrislayer1");
		var deb2 = TextureManager.GetTexture("MainMenu/debrislayer2");
		
		var overlay = TextureManager.GetTexture("MainMenu/overlaygradients");
		
		
		
		Vector2 center = new Vector2(back.Width / 2f, back.Height / 2f);

		var mousePos =  Camera.GetMouseWorldPos();
		float scale = 1f + 0.00001f *  (Game1.instance.GraphicsDevice.Viewport.Height- mousePos.Y); // Adjust the multiplier for the desired zoom speed and amount
		scale = Math.Clamp(scale,1,1.1f);
		Matrix transform = Matrix.CreateTranslation(-center.X, -center.Y, 0) *
		                   Matrix.CreateScale(scale) *
		                   Matrix.CreateTranslation(center.X, center.Y, 0);
		float drawscale =  (float)Game1.instance.GraphicsDevice.Viewport.Height/back.Height;
		
		Vector2 blankOffset = new Vector2(-200*drawscale, 0);
		
		batch.Begin(transformMatrix: transform,sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(backdrop,blankOffset, drawscale,Color.White);
		batch.Draw(smoke3,blankOffset  -gruntOffestCurrent/10f,drawscale,Color.White);
		batch.Draw(smoke2,blankOffset + gruntOffestCurrent/5f,drawscale,Color.White);

		batch.Draw(back, blankOffset -gruntOffestCurrent/4f, drawscale,Color.White);
		
		
		batch.Draw(deb1,  blankOffset-gruntOffestCurrent*0.2f,drawscale,Color.White);
		batch.Draw(deb2,  blankOffset-gruntOffestCurrent*0.2f,drawscale,Color.White);
		batch.Draw(deb0,  blankOffset-gruntOffestCurrent*0.1f,drawscale,Color.White);
		
		
		
		
		batch.Draw(grunt,  blankOffset+gruntOffestCurrent*0.8f,drawscale*scale,Color.White);
		batch.Draw(smoke1,  blankOffset+-gruntOffestCurrent*0.5f,drawscale*scale,Color.White);
		batch.Draw(smoke0,  blankOffset+ gruntOffestCurrent*0.1f,drawscale*scale,Color.White);
		
		
		
		var chat = TextureManager.GetTexture("MainMenu/butts/chatmenuDisconnected");
		batch.End();
		batch.Begin(samplerState: SamplerState.PointClamp);
		batch.Draw(border,blankOffset,drawscale,Color.White);
		batch.Draw(overlay,gradientPos+blankOffset,drawscale,Color.White);
		Vector2 chatpos = new Vector2(0,230)*globalScale.X;
		batch.Draw(chat,chatpos,drawscale,Color.White);
		batch.End();

		
	}

	VerticalStackPanel menuStack;
	VerticalStackPanel socialStack;
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		gradientPos = new Vector2(-1000, 0);
		NetworkingManager.Disconnect();
		MasterServerNetworking.Disconnect();
		var panel = new Panel()
		{
			//Background = new TextureRegion(TextureManager.GetTexture("background")),
		};
		//return panel;

		menuStack = new VerticalStackPanel()
		{
			//	Background = new SolidBrush(Color.wh),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			Left = (int)(25 * globalScale.X),
			Top = (int) (200 * globalScale.Y),
			Height = (int) (800 * globalScale.Y),
			Width = (int) (150 * globalScale.X),
			Spacing = (int) (5 * globalScale.X),

		};
		panel.Widgets.Add(menuStack);

		var tutorial = new SoundTextButton
		{
			Text = "Tutorial",
			Height = (int)(12 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center
		};
		tutorial.Click += (a, b) =>
		{
	
			GameManager.StartLocalServer();

			NetworkingManager.StartTutorial();

			GameLayout.GameLayout.TutorialSequence();
			Audio.ForcePlayMusic("Warframes");
			//Audio.OnGameStateChange(GameState.Playing);

		};
		menuStack.Widgets.Add(tutorial);
		
		

///		reconnect.Click += (a, b) =>
///		{
///			NetworkingManager.Connect(Game1.config.GetValue("config","LastServer","localhost:52233"),Game1.config.GetValue("config","Name","Operative#"+Random.Shared.Next(1000)));		
///		};


		var multi = new SoundTextButton
		{
			Text = "Multiplayer",
			Height = (int)(12 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center
		};
		
		menuStack.Widgets.Add(multi);

		var singleplayer = new SoundTextButton
		{
			Text = "SinglePlayer",
			Height = (int)(12 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center
		};
		singleplayer.Click += (a, b) =>
		{
			GameManager.StartLocalServer();
			NetworkingManager.AddAI();
			Task.Run(() =>
			{
				Thread.Sleep(2500);
				NetworkingManager.SwapMap("/Maps/Ground Zero.mapdata");
				Thread.Sleep(2500);
				NetworkingManager.SendStartGame();
			});

		};
		menuStack.Widgets.Add(singleplayer);
		

/*
		
		var lobybtn = new SoundButton
		{
			GridColumn = 0,
			GridRow = 2,
			Text = "Server Browser",
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};
#if !SINGLEPLAYER_ONLY
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
#else
		lobybtn.TextColor = Color.Gray;
#endif
		MPStack.Widgets.Add(lobybtn);

		var btn = new SoundButton
		{
			Text = "Direct Connect",
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};
#if !SINGLEPLAYER_ONLY
		btn.Click += (s, a) => { UI.SetUI(new ConnectionLayout()); };
#else
	btn.TextColor = Color.Gray;		
#endif
		MPStack.Widgets.Add(btn);
*/
		var button2 = new SoundTextButton
		{
			Text = "Map Editor",
			Height = (int)(12 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		};

		button2.Click += (s, a) =>
		{
			UI.SetUI(new EditorUiLayout());

		};

		menuStack.Widgets.Add(button2);
			
		var button3 = new SoundTextButton
		{
			Text = "Settings",
			Height = (int)(12 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		};

		button3.Click += (s, a) =>
		{
			UI.SetUI(new SettingsLayout());
		};

		menuStack.Widgets.Add(button3);
			
		var button4 = new SoundTextButton
		{
			Text = "QUIT",
			Height = (int)(12 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		};

		button4.Click += (s, a) =>
		{
			Game1.instance.Exit();

		};
		menuStack.Widgets.Add(button4);

		socialStack = new VerticalStackPanel()
		{
			Background = new SolidBrush(Color.Black*0.45f),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Left = (int)(-20 * globalScale.X),
			Top = (int) (5 * globalScale.X),
			Height = (int) ((45*3 +15) * globalScale.X),
			Width = (int) (50 * globalScale.X),
			Spacing = (int) (5 * globalScale.X),

		};
		var discord = new ImageButton();
		discord.Image = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/discord"));
		discord.OverImage = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/discordClicked"));
		discord.Background = new SolidBrush(Color.Transparent);
		discord.OverBackground = new SolidBrush(Color.Transparent);
		discord.PressedBackground = new SolidBrush(Color.Transparent);
		discord.Width =(int)(45*globalScale.X);
		discord.Height =(int)(45*globalScale.X);
		discord.ImageHeight =(int)(45*globalScale.X);
		discord.ImageWidth =(int)(45*globalScale.X);
		discord.HorizontalAlignment = HorizontalAlignment.Center;
		discord.Click += (s, a) =>
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start("explorer", "https://discord.gg/TrmAJbMaQ3");
			}
			else
			{
				Process.Start("xdg-open", "https://discord.gg/TrmAJbMaQ3");
			}
		};
		var twitter = new ImageButton();
		twitter.Image = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/xitter"));
		twitter.OverImage = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/xitterClicked"));
		twitter.Background = new SolidBrush(Color.Transparent);
		twitter.OverBackground = new SolidBrush(Color.Transparent);
		twitter.PressedBackground = new SolidBrush(Color.Transparent);
		twitter.Width =(int)(45*globalScale.X);
		twitter.Height =(int)(45*globalScale.X);
		twitter.ImageHeight =(int)(45*globalScale.X);
		twitter.ImageWidth =(int)(45*globalScale.X);
		twitter.HorizontalAlignment = HorizontalAlignment.Center;
		twitter.Click += (s, a) =>
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start("explorer", "https://twitter.com/EtetStudios");
			}
			else
			{
				Process.Start("xdg-open", "https://twitter.com/EtetStudios");
			}
		};
		var itch = new ImageButton();
		itch.Image = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/itch"));
		itch.OverImage = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/itchClicked"));
		itch.Background = new SolidBrush(Color.Transparent);
		itch.OverBackground = new SolidBrush(Color.Transparent);
		itch.PressedBackground = new SolidBrush(Color.Transparent);
		itch.Width =(int)(45*globalScale.X);
		itch.Height =(int)(45*globalScale.X);
		itch.ImageHeight =(int)(45*globalScale.X);
		itch.ImageWidth =(int)(45*globalScale.X);
		itch.HorizontalAlignment = HorizontalAlignment.Center;
		itch.Click += (s, a) =>
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start("explorer", "https://etetstudios.itch.io/etet");
			}
			else
			{
				Process.Start("xdg-open", "https://etetstudios.itch.io/etet");
			}
		};
		socialStack.Widgets.Add(discord);
		socialStack.Widgets.Add(itch);
		socialStack.Widgets.Add(twitter);
		panel.Widgets.Add(socialStack);


		var version = new Label();
		version.Text = "Alpha 3.1";
		version.VerticalAlignment = VerticalAlignment.Top;
		version.HorizontalAlignment = HorizontalAlignment.Left;
		version.Background = new SolidBrush(Color.Transparent);
		panel.Widgets.Add(version);


		
		return panel;


	}


}