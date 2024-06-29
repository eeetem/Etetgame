//#define SINGLEPLAYER_ONLY
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.Rendering.CustomUIElements;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using FontStashSharp.RichText;
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
	public static Vector2 GradientPos = new Vector2(-10000, 0);
	private static Vector2 targetGradientPos = new Vector2(0, 0);
	public override void Update(float deltatime)
	{
		var mouse = Camera.GetMouseWorldPos().X;
		var customScale =   3.2f/GlobalScale.X;
		var diff1 = Game1.instance.GraphicsDevice.Viewport.Width*customScale- mouse*customScale;

		_gruntOffestTarget = new Vector2(-275+diff1*0.1f,0);
		_gruntOffestTarget = Vector2.Clamp(_gruntOffestTarget,new Vector2(-275,0),new Vector2(25,0));
		_gruntOffestCurrent = Vector2.Lerp(_gruntOffestCurrent,_gruntOffestTarget,0.05f);
		
		
		
		GradientPos = Vector2.Lerp(GradientPos, targetGradientPos, 0.05f);
		_menuStack.Left = (int)((25+GradientPos.X) * GlobalScale.X);
		_socialStack.Left = (int)((-20-GradientPos.X) * GlobalScale.X);
		int i = 0;
		foreach (var wid in _menuStack.Widgets)
		{
			wid.Left = (int)(GradientPos.X * i*GlobalScale.X);
			
			i++;
		}
		base.Update(deltatime);
	}
	
	Vector2 _gruntOffestTarget = new Vector2(-10,0);
	Vector2 _gruntOffestCurrent = new Vector2(-10,0);
	private static readonly Vector2 Chatsize = new Vector2(150, 200);

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

		var gruntOffset = _gruntOffestCurrent;
		
		batch.Begin(transformMatrix: transform,sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(backdrop,blankOffset, drawscale,Color.White);
		batch.Draw(smoke3,blankOffset  -gruntOffset/10f,drawscale,Color.White);
		batch.Draw(smoke2,blankOffset + gruntOffset/5f,drawscale,Color.White);

		batch.Draw(back, blankOffset -gruntOffset/4f, drawscale,Color.White);
		
		
		batch.Draw(deb1,  blankOffset-gruntOffset*0.2f,drawscale,Color.White);
		batch.Draw(deb2,  blankOffset-gruntOffset*0.2f,drawscale,Color.White);
		batch.Draw(deb0,  blankOffset-gruntOffset*0.1f,drawscale,Color.White);
		
		
		
		
		batch.Draw(grunt,  blankOffset+gruntOffset*0.8f,drawscale*scale,Color.White);
		batch.Draw(smoke1,  blankOffset+-gruntOffset*0.5f,drawscale*scale,Color.White);
		batch.Draw(smoke0,  blankOffset+ gruntOffset*0.1f,drawscale*scale,Color.White);
		
		
		
		var chat = TextureManager.GetTexture("MainMenu/butts/chatMenuDisconnected");
		if (MasterServerNetworking.IsConnected)
		{
			chat = TextureManager.GetTexture("MainMenu/butts/chatMenuConnected");
		}
		batch.End();
		batch.Begin(samplerState: SamplerState.PointClamp);
		batch.Draw(border,blankOffset,drawscale,Color.White);
		batch.Draw(overlay,GradientPos+blankOffset,drawscale,Color.White);
		Vector2 chatpos = new Vector2(0, Game1.instance.GraphicsDevice.Viewport.Height- chat.Height*drawscale/3f);
		
		float chattextpos =  Game1.instance.GraphicsDevice.Viewport.Height - Chatsize.Y*drawscale;
		if (MasterServerNetworking.IsConnected)
		{
			batch.Draw(TextureManager.GetTexture(""), new Vector2(0, chattextpos), null, new Color(29f / 255f, 33f / 255f, 42f / 255f, 0.7f), 0, Vector2.Zero, Chatsize*drawscale, SpriteEffects.None, 0);
		}

		batch.Draw(chat,chatpos,drawscale/3f,Color.White);
		if (MasterServerNetworking.IsConnected)
		{
			string chatmsg = "";
			int extraLines = 0;
			int width = 35;
			foreach (var msg in Chat.Messages)
			{
				chatmsg += msg + "\n";
				if (msg.Length > width)
				{
					extraLines++;
				}

				extraLines++;
			}

			batch.DrawText(chatmsg, new Vector2(5, chattextpos), drawscale*0.6f, width, Color.White);
		}

		batch.End();

		
	}

	VerticalStackPanel _menuStack = null!;
	VerticalStackPanel _socialStack = null!;
	private static Panel _currentPanel = null!;
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		GradientPos = new Vector2(-1000, 0);
		NetworkingManager.Disconnect();
		var panel = new Panel()
		{
			//Background = new TextureRegion(TextureManager.GetTexture("background")),
		};
		//return panel;

		_menuStack = new VerticalStackPanel()
		{
			//	Background = new SolidBrush(Color.wh),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			Left = (int)(25 * GlobalScale.X),
			Top = (int) (200 * GlobalScale.Y),
			Height = (int) (800 * GlobalScale.Y),
			Width = (int) (150 * GlobalScale.X),
			Spacing = (int) (5 * GlobalScale.X),

		};
		panel.Widgets.Add(_menuStack);

		var tutorial = new SoundTextButton(Color.Gray)
		{
			Text = "Tutorial-Coming Soon",
			Height = (int)(9 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		//	Color = Color.Black
			
		};
		tutorial.Click += (a, b) =>
		{
			return;
			GameManager.StartLocalServer();

			NetworkingManager.StartTutorial();

			GameLayout.GameLayout.TutorialSequence();
			Audio.ForcePlayMusic("Warframes");
			//Audio.OnGameStateChange(GameState.Playing);

		};
		_menuStack.Widgets.Add(tutorial);
		
		


		var multi = new SoundTextButton
		{
			Text = "Multiplayer",
			Height = (int)(12 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center
		};
		multi.Click += (a, b) =>
		{
			_lastSelected?.ForceDeselect();
			_lastSelected = multi;
			multi.ForceSelect();
			Multiplayer();
		};
		
		_menuStack.Widgets.Add(multi);

		var singleplayer = new SoundTextButton
		{
			Text = "SinglePlayer",
			Height = (int)(12 * GlobalScale.X),
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
			//	NetworkingManager.SendStartGame();
			});

		};
		_menuStack.Widgets.Add(singleplayer);
		

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
			var label = new TextLabel()
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

			Text = "Direct Connect",

		btn.Click += (s, a) => { UI.SetUI(new ConnectionLayout()); };

*/
		var button2 = new SoundTextButton
		{
			Text = "Map Editor",
			Height = (int)(12 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		};

		button2.Click += (s, a) =>
		{
			UI.SetUI(new EditorUiLayout());

		};

		_menuStack.Widgets.Add(button2);
			
		var button3 = new SoundTextButton
		{
			Text = "Settings",
			Height = (int)(12 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		};

		button3.Click += (s, a) =>
		{
			_lastSelected?.ForceDeselect();
			_lastSelected = button3;
			button3.ForceSelect();
			
			Settings();
		};


		_menuStack.Widgets.Add(button3);
			
		var button4 = new SoundTextButton
		{
			Text = "QUIT",
			Height = (int)(12 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center,
		};

		button4.Click += (s, a) =>
		{
			Game1.instance.Exit();

		};
		_menuStack.Widgets.Add(button4);

		_socialStack = new VerticalStackPanel()
		{
			Background = new SolidBrush(Color.Black*0.45f),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Left = (int)(-20 * GlobalScale.X),
			Top = (int) (5 * GlobalScale.X),
			Height = (int) ((45*3 +15) * GlobalScale.X),
			Width = (int) (50 * GlobalScale.X),
			Spacing = (int) (5 * GlobalScale.X),

		};
		var discord = new ImageButton();
		discord.Image = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/discord"));
		discord.OverImage = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/discordClicked"));
		discord.Background = new SolidBrush(Color.Transparent);
		discord.OverBackground = new SolidBrush(Color.Transparent);
		discord.PressedBackground = new SolidBrush(Color.Transparent);
		discord.Width =(int)(45*GlobalScale.X);
		discord.Height =(int)(45*GlobalScale.X);
		discord.ImageHeight =(int)(45*GlobalScale.X);
		discord.ImageWidth =(int)(45*GlobalScale.X);
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
		twitter.Width =(int)(45*GlobalScale.X);
		twitter.Height =(int)(45*GlobalScale.X);
		twitter.ImageHeight =(int)(45*GlobalScale.X);
		twitter.ImageWidth =(int)(45*GlobalScale.X);
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
		itch.Width =(int)(45*GlobalScale.X);
		itch.Height =(int)(45*GlobalScale.X);
		itch.ImageHeight =(int)(45*GlobalScale.X);
		itch.ImageWidth =(int)(45*GlobalScale.X);
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
		_socialStack.Widgets.Add(discord);
		_socialStack.Widgets.Add(itch);
		_socialStack.Widgets.Add(twitter);
		panel.Widgets.Add(_socialStack);


	
		var chat = new TextBox()
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Bottom,
			Width = (int)((Chatsize.X-20)*GlobalScale.X),
			Height = (int)(10*GlobalScale.X),
			Left = (int)(13*GlobalScale.X),
			Font = DefaultFont.GetFont(5*GlobalScale.X)
		};
		chat.KeyDown += (s, a) =>
		{
			if (a.Data == Keys.Enter)
			{
				Chat.SendMessage(chat.Text);
				chat.Text = "";
			}
		};
		panel.Widgets.Add(chat);
	


		_currentPanel = panel;
		return panel;


	}

	
	static Panel _menuBox;
	static SoundTextButton? _lastSelected;

	private static void MakeMenuBox()
	{
		_currentPanel.Widgets.Remove(_menuBox);
        
		_menuBox = new Panel();
		_menuBox.Background = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/menubox"));
		float scale = 0.8f;
		_menuBox.Height = (int) (438*scale*GlobalScale.X);
		_menuBox.Width = (int) (565*scale*GlobalScale.X);
		_menuBox.Left = (int) (55 * GlobalScale.X);
		_menuBox.Top = (int) (23 * GlobalScale.X);
		_menuBox.HorizontalAlignment = HorizontalAlignment.Center;
		_menuBox.VerticalAlignment = VerticalAlignment.Center;
	
		_currentPanel.Widgets.Add(_menuBox);
	}
	public static void Settings()
	{
		MakeMenuBox();
		var settings = SettingsMenu(() => { _lastSelected?.ForceDeselect(); _lastSelected = null; _menuBox.RemoveFromParent();});
		_menuBox.Widgets.Add(settings);
	

	}

	private static Panel _subPanel = null!;
	private void Multiplayer()
	{
		MakeMenuBox();
		var tabs = new HorizontalStackPanel();
		tabs.HorizontalAlignment = HorizontalAlignment.Stretch;
		tabs.VerticalAlignment = VerticalAlignment.Top;
		tabs.Spacing = (int)(5*GlobalScale.X);
		tabs.Left = (int)(10*GlobalScale.X);
		tabs.Top = (int)(10*GlobalScale.X);
		var btn = new SoundTextButton
		{
			Text = "Server Browser",
			Height = (int)(12 * GlobalScale.X),
			Width =	(int)(190 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center
		};

		var btn2 = new SoundTextButton
		{
			Text = "Direct Connect",
			Height = (int)(12 * GlobalScale.X),
			Width =	(int)(190 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Center
		};
		btn.Click += (s, a) =>
		{
			btn.ForceSelect();
			btn2.ForceDeselect();
			ServerBrowser();
		};
		btn2.Click += (s, a) =>
		{
			btn2.ForceSelect();
			btn.ForceDeselect();
			DirectConnect();
		};
		tabs.Widgets.Add(btn);
		tabs.Widgets.Add(btn2);
		
		_subPanel = new Panel();
		_subPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
		_subPanel.VerticalAlignment = VerticalAlignment.Stretch;
		_subPanel.Top = (int)(28*GlobalScale.X);
		_subPanel.Width = _menuBox.Width;
		_subPanel.Height = _menuBox.Height - (int) (28 * GlobalScale.X);
		_menuBox.Widgets.Add(_subPanel);
		_menuBox.Widgets.Add(tabs);
		btn.ForceSelect();
		btn2.ForceDeselect();
		ServerBrowser();
		
	}

	private static void ServerBrowser()
	{
		_subPanel.Widgets.Clear();
		if (!MasterServerNetworking.IsConnected)
		{
			var name= new TextBox()
			{
				Text = Game1.config.GetValue("config","Name","Operative#"+Random.Shared.Next(1000)),
				Height = (int) (25 * GlobalScale.X),
				
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Background = new SolidBrush(Color.Transparent),
			};
			_subPanel.Widgets.Add(new TextLabel()
			{
				Text = "Connect to Master Server",
				Height = (int) (15 * GlobalScale.X),
				Top = (int) (-50 * GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			});
			_subPanel.Widgets.Add(new TextLabel()
			{
				Text = "Enter Name",
				Height = (int) (15 * GlobalScale.X),
				Top = (int) (-25 * GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			});
			_subPanel.Widgets.Add(name);
			var connect = new SoundTextButton()
			{
				Text = "Connect",
				Height = (int) (18 * GlobalScale.X),
				Top = (int) (35 * GlobalScale.X),
				Width = (int)(120 * GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			};
			connect .Click += (s, a) =>
			{
				Game1.config.SetValue("config","Name",name.Text);
				Game1.config.Save();
				if (MasterServerNetworking.Connect(Game1.config.GetValue("config", "MasterServerIP", ""), name.Text))
				{
					UI.ShowMessage("", "Connecting to Master Server....");
				}
			
				
			};
			_subPanel.Widgets.Add(connect);
			
		}
		else
		{
			
			var lobbyViewer = new ScrollViewer()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = new SolidBrush(Color.Transparent)
			};
			_subPanel.Widgets.Add(lobbyViewer);

			var lobbies = new Grid()
			{
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				ColumnSpacing = 10,
				ShowGridLines = true,
				GridLinesColor = new Color(255,255,255,100), 
					

			};
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Pixels,(int)(150*GlobalScale.X)));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Pixels,(int)(60*GlobalScale.X)));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.DefaultRowProportion = new Proportion(ProportionType.Auto);



			lobbies.Widgets.Add(new TextLabel()
			{
				Text = "Name",
				GridRow = 0,
				GridColumn = 1,
				Height = (int)(12*GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Left,
								
			});
			lobbies.Widgets.Add(new TextLabel()
			{
				Text = "Map",
				GridRow = 0,
				GridColumn = 2,
				Height = (int)(12*GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Left,
								
			});
			lobbies.Widgets.Add(new TextLabel()
			{
				Text = "Players",
				GridRow = 0,
				GridColumn = 3,
				Height = (int)(12*GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Left,
								
			});
			lobbies.Widgets.Add(new TextLabel()
			{
				Text = "State",
				GridRow = 0,
				GridColumn = 4,
				Height = (int)(12*GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Left,
								
			});
			
			
			int row = 1;
			foreach (var lobby in MasterServerNetworking.Lobbies)
			{
								
				var lobbybtn = new SoundTextButton()
				{
					Text = "Join",
					Height = (int) (10*GlobalScale.X), 
					Width = (int)(50*GlobalScale.X),
					GridRow = row,
					GridColumn =0,
					
				};

				lobbybtn.Click += (sender, args) =>
				{
					var ip = Game1.config.GetValue("config", "MasterServerIP", "");
					ip = ip.Substring(0, ip.LastIndexOf(':'));
					ip += ":" + lobby.Port;
					var result = NetworkingManager.Connect(ip, Game1.config.GetValue("config", "Name", "Operative#"+Random.Shared.Next(1000)));
					if (result)
					{
						UI.ShowMessage("Connection Notice", "Connecting to the server.....");
					}
					else
					{
						UI.ShowMessage("Connection Notice", "Failed to connect: " + result);

					}
				};
				lobbies.Widgets.Add(lobbybtn);
				lobbies.Widgets.Add(new TextLabel()
				{
					Text = lobby.Name,
					Height = (int) (10*GlobalScale.X), 
					GridRow = row,
					GridColumn = 1,
					
					HorizontalAlignment = HorizontalAlignment.Left,
									
					Margin = new Thickness(0,5,0,5)
				});
				lobbies.Widgets.Add(new TextLabel()
				{
					Text = lobby.MapName,
					Height = (int) (7*GlobalScale.X), 
					GridRow = row,
					GridColumn = 2,
					
					HorizontalAlignment = HorizontalAlignment.Left,
									
					Margin = new Thickness(0,5,0,5)
				});
				lobbies.Widgets.Add(new TextLabel()
				{
					Text = lobby.PlayerCount+"/2",
					Height = (int) (12*GlobalScale.X), 
					GridRow = row,
					GridColumn = 3,
					
					HorizontalAlignment = HorizontalAlignment.Left,
								
					Margin = new Thickness(0,5,0,5)
				});
				lobbies.Widgets.Add(new TextLabel()
				{
					Text = lobby.GameState,
					Height = (int) (8*GlobalScale.X), 
					GridRow = row,
					GridColumn = 4,
					
					HorizontalAlignment = HorizontalAlignment.Left,
								
					Margin = new Thickness(0,5,0,5)
				});

				row++;
			}
			lobbyViewer.Content = lobbies;

			var btnstack = new HorizontalStackPanel();
			btnstack.VerticalAlignment = VerticalAlignment.Bottom;
			btnstack.HorizontalAlignment = HorizontalAlignment.Center;
			btnstack.Spacing = 20;
			_subPanel.Widgets.Add(btnstack);
			var btn = new SoundTextButton()
			{
				Text = "Create Lobby",
				Height = (int)(14*GlobalScale.X),
				Width = (int)(150*GlobalScale.X),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			};
			btn.Click += (sender, args) =>
			{
				//lobby creation popup
				var popup = new Panel();
				var lbl = new TextLabel()
				{
					Top = -10,
					Height = (int)(10*GlobalScale.X),
					Width = (int)(150*GlobalScale.X),
					Text = "Enter Lobby Name"
				};
				popup.Widgets.Add(lbl);
				var txt = new TextBox()
				{
					Top = 25,
					Height = (int)(25*GlobalScale.X),
					Width = (int)(150*GlobalScale.X),
					Text = "Lobby#"+Random.Shared.Next(1000)
				};
				popup.Widgets.Add(txt);
				//	var password = new TextBox()
				//		{
				//		Top = 50,
				//		Text = "Enter Password"
				//	};
				//		popup.Widgets.Add(password);
				var dialog = Dialog.CreateMessageBox("Creating Server...", popup);
				dialog.ButtonOk.Click += (sender, args) =>
				{
					MasterServerNetworking.CreateLobby(txt.Text);
					var dialog = Dialog.CreateMessageBox("Creating Server...", new TextLabel()
					{
						Text = "Please Wait...",
						Height = (int)(10*GlobalScale.X),
					});

					dialog.ShowModal(UI.Desktop);
				};
				dialog.Width = (int)(200*GlobalScale.X);
				dialog.Height = (int)(80*GlobalScale.X);
				
				dialog.ShowModal(UI.Desktop);
			};
		
			btnstack.Widgets.Add(btn);
			
			var btn2 = new SoundTextButton()
			{
				Text = "Refresh",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Height = (int)(14*GlobalScale.X),
				Width = (int)(150*GlobalScale.X)
			};
			btn2.Click += (sender, args) =>
			{
				MasterServerNetworking.RefreshServers();
			};
			btnstack.Widgets.Add(btn2);
			lobbyViewer.Content = lobbies;

		}
	}
	private void DirectConnect()
	{
		_subPanel.Widgets.Clear();
		
		_subPanel.Widgets.Add(new TextLabel()
		{
			Text = "Direct Connection",
			Height = (int) (15 * GlobalScale.X),
			Top = (int) (-80 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		});
		
		_subPanel.Widgets.Add(new TextLabel()
		{
			Text = "Enter Name",
			Height = (int) (15 * GlobalScale.X),
			Top = (int) (-50 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		});
		var name= new TextBox()
		{
			Text = Game1.config.GetValue("config","Name","Operative#"+Random.Shared.Next(1000)),
			Height = (int) (25 * GlobalScale.X),
			Top = (int) (-25*GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Background = new SolidBrush(Color.Transparent),
		};
		_subPanel.Widgets.Add(name);
		_subPanel.Widgets.Add(new TextLabel()
		{
			Text = "Enter IP",
			Height = (int) (15 * GlobalScale.X),
			Top = (int) (5 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		});
		var ip= new TextBox()
		{
			Text = Game1.config.GetValue("config","LastServer","localhost:52233"),
			Height = (int) (25 * GlobalScale.X),
			Top = (int) (30*GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Background = new SolidBrush(Color.Transparent),
		};
		_subPanel.Widgets.Add(ip);
		var connect = new SoundTextButton()
		{
			Text = "Connect",
			Height = (int) (18 * GlobalScale.X),
			Top = (int) (65 * GlobalScale.X),
			Width = (int)(120 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};
		connect .Click += (s, a) =>
		{
			Game1.config.SetValue("config","Name",name.Text);
			Game1.config.Save();
			if (NetworkingManager.Connect(ip.Text, name.Text))
			{
				UI.ShowMessage("", "Connecting to Server....");
			}
			
				
		};
		_subPanel.Widgets.Add(connect);
	}

	public static void RefreshLobbies()
	{
		ServerBrowser();
		
	}
}