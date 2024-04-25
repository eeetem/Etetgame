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
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Unit = DefconNull.WorldObjects.Unit;

namespace DefconNull.Rendering.UILayout;



public class MainMenuLayout : UiLayout
{

	public override void Update(float deltatime)
	{
		
		gruntOffestTarget = new Vector2((-275+(Game1.instance.GraphicsDevice.Viewport.Width- Camera.GetMouseWorldPos().X)*0.1f),0);
		gruntOffestTarget = Vector2.Clamp(gruntOffestTarget,new Vector2(-275,0),new Vector2(25,0));
		gruntOffestCurrent = Vector2.Lerp(gruntOffestCurrent,gruntOffestTarget,0.05f);

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
		
		
		
		Vector2 center = new Vector2(back.Width / 2f, back.Height / 2f);

		var mousePos =  Camera.GetMouseWorldPos();
		float scale = 1f + 0.00001f *  (Game1.instance.GraphicsDevice.Viewport.Height- mousePos.Y); // Adjust the multiplier for the desired zoom speed and amount
		scale = Math.Clamp(scale,1,1.1f);
		Matrix transform = Matrix.CreateTranslation(-center.X, -center.Y, 0) *
		                   Matrix.CreateScale(scale) *
		                   Matrix.CreateTranslation(center.X, center.Y, 0);
		var drawscale =  Game1.instance.GraphicsDevice.Viewport.Height/back.Height;
		
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
		batch.End();
		batch.Begin(samplerState: SamplerState.PointClamp);
		batch.Draw(border,blankOffset,drawscale,Color.White);
		batch.End();

		
	}

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		NetworkingManager.Disconnect();
		MasterServerNetworking.Disconnect();
		var panel = new Panel()
		{
			//Background = new TextureRegion(TextureManager.GetTexture("background")),
		};
		//return panel;

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
			Top = (int) (-45 * globalScale.Y)
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
		
		var reconnect = new SoundButton
		{
			GridColumn = 0,
			GridRow = 2,
			Text = "Reconnect to last",
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};
#if !SINGLEPLAYER_ONLY
		reconnect.Click += (a, b) =>
		{
			NetworkingManager.Connect(Game1.config.GetValue("config","LastServer","localhost:52233"),Game1.config.GetValue("config","Name","Operative#"+Random.Shared.Next(1000)));		
		};
#else
reconnect.TextColor = Color.Gray;	
#endif
		MPStack.Widgets.Add(reconnect);
		
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
			NetworkingManager.AddAI();
			Task.Run(() =>
			{
				Thread.Sleep(2500);
				NetworkingManager.SwapMap("/Maps/Ground Zero.mapdata");
				Thread.Sleep(2500);
				NetworkingManager.SendStartGame();
			});

		};
		MPStack.Widgets.Add(singleplayer);
		
		var tutorial = new SoundButton
		{
			GridColumn = 0,
			GridRow = 2,
			Text = "Tutorial",
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch
		};
		tutorial.Click += (a, b) =>
		{
	
			GameManager.StartLocalServer();

			NetworkingManager.StartTutorial();

			GameLayout.GameLayout.TutorialSequence();

		};
		MPStack.Widgets.Add(tutorial);

		
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
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start("explorer", "https://discord.gg/TrmAJbMaQ3");
			}
			else
			{
				Process.Start("xdg-open", "https://discord.gg/TrmAJbMaQ3");
			}
		};
		panel.Widgets.Add(discord);


		var version = new Label();
		version.Text = "Alpha 3.1";
		version.VerticalAlignment = VerticalAlignment.Top;
		version.HorizontalAlignment = HorizontalAlignment.Left;
		version.Background = new SolidBrush(Color.Transparent);
		panel.Widgets.Add(version);
			
		menustack.Widgets.Add(button4);

		
		return panel;


	}


}