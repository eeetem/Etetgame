using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommonData;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Myra.Utility;
using Network;
using Network.Converter;
using Salaros.Configuration;
using Thickness = Myra.Graphics2D.Thickness;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop { get; private set; }
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		private static Texture2D[] coverIndicator = new Texture2D[8];
		private static Texture2D[] infoIndicator = new Texture2D[6];
		private static Texture2D[] healthIndicator = new Texture2D[3];
		private static Texture2D[] vissionIndicator = new Texture2D[2];
		private static Texture2D targetingCUrsor;
		
		public static readonly List<Controllable> Controllables = new List<Controllable>();


		public static void Init(ContentManager content, GraphicsDevice graphicsdevice)
		{
			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			MyraEnvironment.Game = Game1.instance;
			


			Desktop = new Desktop();
			Desktop.TouchDown += MouseDown;
			Desktop.TouchUp += MouseUp;


			Texture2D coverIndicatorSpriteSheet = content.Load<Texture2D>("textures/UI/coverIndicator");
			coverIndicator = Utility.SplitTexture(coverIndicatorSpriteSheet, coverIndicatorSpriteSheet.Width / 3, coverIndicatorSpriteSheet.Width / 3);

			Texture2D indicatorSpriteSheet = content.Load<Texture2D>("textures/UI/indicators");
			infoIndicator = Utility.SplitTexture(indicatorSpriteSheet, indicatorSpriteSheet.Width / 6, indicatorSpriteSheet.Height);

			Texture2D healthIndicatorSpriteSheet = content.Load<Texture2D>("textures/UI/healthbar");
			healthIndicator = Utility.SplitTexture(healthIndicatorSpriteSheet, healthIndicatorSpriteSheet.Width / 3, healthIndicatorSpriteSheet.Height);

			Texture2D vissionIndicatorSpriteSheet = TextureManager.GetTexture("UI/VissionIndicator");
			vissionIndicator = Utility.SplitTexture(vissionIndicatorSpriteSheet, vissionIndicatorSpriteSheet.Width / 2, vissionIndicatorSpriteSheet.Height);
			
			targetingCUrsor = TextureManager.GetTexture("UI/targetingCursor");
			
			LeftClick += LeftClickAtPosition;
			RightClick += RightClickAtPosition;

			previewMoves[0] = new List<Vector2Int>();
			previewMoves[1] = new List<Vector2Int>();
		}


		public delegate void UIGen();

		private static UIGen currentUI;
		private static Vector2 globalScale = new Vector2(1, 1);

		private static Widget root;
		public static void SetUI(UIGen? uiMethod) {
			
				globalScale = new Vector2((Game1.instance.Window.ClientBounds.Width / 1000f) * 1f, (Game1.instance.Window.ClientBounds.Width / 1000f) * 1f);
				if (uiMethod != null)
				{
					currentUI = uiMethod;
				}
				Console.WriteLine("Changing UI to: "+currentUI.Method.Name);
				currentUI.Invoke();
			
		}

		public delegate void MouseClick(Vector2Int gridPos);

		public static event MouseClick RightClick;
		public static event MouseClick LeftClick;
		public static event MouseClick RightClickUp;
		public static event MouseClick LeftClickUp;

		private static MouseState lastMouseState;

		public static void MouseDown(object? sender, EventArgs e)
		{
			if (UI.Desktop.IsMouseOverGUI)
			{
				return; //let myra do it's thing
			}

			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				LeftClick?.Invoke(gridClick);
			}

			if (mouseState.RightButton == ButtonState.Pressed)
			{
				RightClick?.Invoke(gridClick);
			}

			lastMouseState = mouseState;
		}

		public static void MouseUp(object? sender, EventArgs e)
		{
			if (UI.Desktop.IsMouseOverGUI)
			{
				return; //let myra do it's thing
			}

			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (lastMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
			{
				LeftClickUp?.Invoke(gridClick);
			}

			if (lastMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
			{
				RightClickUp?.Invoke(gridClick);
			}

		}


		public static Controllable SelectedControllable { get; private set;}

		public static void SelectControllable(Controllable controllable)
		{
			
			if (controllable!= null&&!controllable.IsMyTeam())
			{
				return;
			}

			SelectedControllable = controllable;
			if(controllable==null) return;
			SetUI(UnitUi);
			Camera.SetPos(controllable.worldObject.TileLocation.Position);
			
		}
		
	
	private static void LeftClickAtPosition(Vector2Int position)
	{
		ClickAtPosition(position,false);
	}
	private static void RightClickAtPosition(Vector2Int position)
	{
		ClickAtPosition(position,true);
	}

	private static void ClickAtPosition(Vector2Int position,bool righclick)
	{
		if(!GameManager.IsMyTurn()) return;
		if(!WorldManager.IsPositionValid(position)) return;
		var Tile = WorldManager.Instance.GetTileAtGrid(position);

		WorldObject obj = Tile.ObjectAtLocation;
		if (obj!=null&&obj.ControllableComponent != null&& obj.GetMinimumVisibility() <= obj.TileLocation.Visible && Action.GetActiveActionType() == null) { 
			SelectControllable(obj.ControllableComponent);
			return;
		}
			
		
		if (!GameManager.IsMyTurn()) return;



		if (righclick)
		{
			switch (Action.GetActiveActionType())
			{

				case null:
					Action.SetActiveAction(ActionType.Face);
					break;
				case ActionType.Face:
					SelectedControllable?.DoAction(Action.ActiveAction,position);
					break;
				default:
					Action.SetActiveAction(null);
					break;
					

			}
		}
		else
		{
			switch (Action.GetActiveActionType())
			{
			
				case null:
					if (SelectedControllable != null)
					{
						Action.SetActiveAction(ActionType.Move);
					}
					break;
				default:
					SelectedControllable?.DoAction(Action.ActiveAction,position);
					break;
					

			}
			
		}
		
		


		}

		public static void LobbyBrowser()
		{
			var Grid = new Grid();
			Grid.ColumnsProportions.Add(Proportion.Auto);
			Grid.ColumnsProportions.Add(Proportion.Fill);
			Grid.ColumnsProportions.Add(Proportion.Auto);
			var chatPanel = new Panel()
			{
				GridColumn = 0,
				GridRow = 0,
			};
Grid.Widgets.Add(chatPanel);
			AttachChatBox(chatPanel);
			var lobbyViewer = new ScrollViewer()
			{
					Width = (int)(500f*globalScale.X),
					Height = (int)(1000f*globalScale.Y),
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Top,
					GridColumn = 1,
					GridRow = 0,
			
			};
			Grid.Widgets.Add(lobbyViewer);

			var lobbies = new Grid()
			{
				VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Left,
					ColumnSpacing = 10,

			};
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));


			lobbies.Widgets.Add(new Label()
			{
				Text = "Name",
				GridRow = 0,
				GridColumn = 1,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "Map",
				GridRow = 0,
				GridColumn = 2,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "Players",
				GridRow = 0,
				GridColumn = 3,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "Spectators",
				GridRow = 0,
				GridColumn = 4,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "State",
				GridRow = 0,
				GridColumn = 5,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left
			});
			
			
			int row = 1;
			foreach (var lobby in MasterServerNetworking.Lobbies)
			{
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.Name,
					GridRow = row,
					GridColumn = 1,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.MapName,
					GridRow = row,
					GridColumn = 2,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.PlayerCount+"/2",
					GridRow = row,
					GridColumn = 3,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.Spectators.ToString(),
					GridRow = row,
					GridColumn = 4,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.GameState,
					GridRow = row,
					GridColumn = 5,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left
				});
				
				var lobbybtn = new TextButton()
				{
					Text = "Join",
					HorizontalAlignment = HorizontalAlignment.Center,
					GridRow = row,
					GridColumn =0,
					
				};
				lobbybtn.Click += (sender, args) =>
				{
					var ip = Game1.config.GetValue("config", "MasterServerIP", "");
					ip = ip.Substring(0, ip.LastIndexOf(':'));
					ip += ":" + lobby.Port;
					var result = Networking.Connect(ip, Game1.config.GetValue("config", "Name", "john"));
					if (result == ConnectionResult.Connected)
					{
						ShowMessage("Connection Notice", "Connected to server!");
						SetUI(PreGameLobby);
						DiscordManager.client.UpdateState("In Battle");
					}
					else
					{
						ShowMessage("Connection Notice", "Failed to connect: " + result);

					}
				};
				lobbies.Widgets.Add(lobbybtn);
				row++;
			}
			lobbyViewer.Content = lobbies;

			var btn = new TextButton()
			{
				Text = "Create Lobby",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				GridColumn = 1,
				GridRow = 1,
			};
			btn.Click += (sender, args) =>
			{
				//lobby creation popup
				var popup = new Panel();
				var txt = new TextBox()
				{
					Top = -10,
					Text = "Enter Lobby Name"
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
					var packet = new LobbyStartPacket();
					packet.LobbyName = txt.Text;
				//	if (password.Text == "Enter Password")
				//	{
						packet.Password = "";
				//	}
				//	packet.Password = password.Text;
					MasterServerNetworking.CreateLobby(packet);
				};
				
				dialog.ShowModal(Desktop);
			};
		
			Grid.Widgets.Add(btn);
			
			var btn2 = new TextButton()
			{
				Text = "Refresh",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				GridColumn = 1,
				GridRow = 1,
				Top = 50,
			};
			btn2.Click += (sender, args) =>
			{
				MasterServerNetworking.RefreshServers();
			};
			Grid.Widgets.Add(btn2);
			
			var btn3 = new TextButton()
			{
				Text = "Main Menu",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				GridColumn = 1,
				GridRow = 1,
				Top = 80,
			};
			btn3.Click += (sender, args) =>
			{
				MasterServerNetworking.Disconnect();
				SetUI(MainMenu);
			};
			Grid.Widgets.Add(btn3);

			var players = new VerticalStackPanel()
			{
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				GridColumn = 2,
				GridRow = 0,
			};
			var playerlbl2 = new Label()
			{
				Text = "Players Online",
				HorizontalAlignment = HorizontalAlignment.Center,
		
			};
			players.Widgets.Add(playerlbl2);	
			foreach (var player in MasterServerNetworking.Players)
			{
				var playerlbl = new Label()
				{
					Text = player,
					HorizontalAlignment = HorizontalAlignment.Center,
					
				};
				players.Widgets.Add(playerlbl);			
			}
			Grid.Widgets.Add(players);
			lobbyViewer.Content = lobbies;

			root = Grid;
		}

		public static void MainMenu()
		{
			var grid = new Grid
			{
				RowSpacing = 0,
				ColumnSpacing = 0
			};

			var lobybtn = new TextButton
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "Server Browser",
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
				input.Text = Game1.config.GetValue("config","Name","john doe");
				panel.Widgets.Add(input);
				var dialog = Dialog.CreateMessageBox("Connecting to Master Server...", panel);
				dialog.ButtonOk.Click += (sender, args) =>
				{
					Game1.config.SetValue("config","Name",input.Text);
					Game1.config.Save();
					var result = MasterServerNetworking.Connect(Game1.config.GetValue("config","MasterServerIP",""),input.Text);
					if (result == ConnectionResult.Connected)
					{
						SetUI(LobbyBrowser);
					}
					else
					{
						ShowMessage("Error", "Could not connect to master server");
					}
				};
				dialog.ShowModal(Desktop);
				
				
			};
			grid.Widgets.Add(lobybtn);

			var button = new TextButton
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "Direct Connect"
			};

			button.Click += (s, a) => { SetUI(ConnectionMenu); };

			grid.Widgets.Add(button);

			var button2 = new TextButton
			{
				GridColumn = 1,
				GridRow = 3,
				Text = "Map Editor"
			};

			button2.Click += (s, a) =>
			{
				WorldEditSystem.Init();
				WorldEditSystem.GenerateUI();

			};

			grid.Widgets.Add(button2);



			root = grid;


		}

		public static void ConnectionMenu()
		{



			var grid = new Grid
			{
				RowSpacing = 0,
				ColumnSpacing = 0
			};

			var textBox = new TextBox()
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "46.7.175.47:52233"
			};
			grid.Widgets.Add(textBox);
			var textBox2 = new TextBox()
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "name"
			};
			grid.Widgets.Add(textBox2);

			var button = new TextButton
			{
				GridColumn = 2,
				GridRow = 1,
				Text = "Connect"
			};
			var exit = new TextButton
			{
				GridColumn = 2,
				GridRow = 2,
				Text = "Main Menu"
			};
			exit.Click += (s, a) => { SetUI(MainMenu); };
			grid.Widgets.Add(exit);
			button.Click += (s, a) =>
			{
				ConnectionResult result = Networking.Connect(textBox.Text.Trim(), textBox2.Text.Trim());
				if (result == ConnectionResult.Connected)
				{
					ShowMessage("Connection Notice", "Connected to server!");

					grid.Widgets.Remove(button);
					grid.Widgets.Remove(textBox);
					SetUI(PreGameLobby);
					DiscordManager.client.UpdateState("In Battle");
				}
				else
				{
					ShowMessage("Connection Notice", "Failed to connect: " + result);

				}

			};

			grid.Widgets.Add(button);



			root = grid;



		}
		public static void PreGameLobby()
		{
		GameManager.GameState = GameState.Lobby;
			
			var panel = new Panel
			{
				
			};
			AttachChatBox(panel);
			
			if(GameManager.PreGameData ==null) return;

			if (GameManager.IsPlayer1)
			{
				
				var btn = new TextButton()
				{
					Text = "Start Game",
					GridColumn = 1,
					GridRow = 2,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Top = 60,
				};
				btn.Click += (s, a) =>
				{
					Networking.SendStartGame();
				};
				panel.Widgets.Add(btn);
				var kick = new TextButton()
				{
					Text = "Kick",
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					Top = 25,
					Left = 100,
				};
				kick.Click += (s, a) =>
				{
					Networking.serverConnection.SendRawData(RawDataConverter.FromBoolean("kick",true));
				};
				panel.Widgets.Add(kick);
				var mapGrid = new Grid()
				{
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Top,
					MaxWidth = (int)(400f*globalScale.X),
					
				};
				mapGrid.RowSpacing = 0;
				mapGrid.RowsProportions.Add(new Proportion(ProportionType.Pixels,50));
				panel.Widgets.Add(mapGrid);
				var mapLabel = new Label()
				{
					Text = "Official Maps",
					GridColumn = 0,
					GridRow = 0
				};
				mapGrid.Widgets.Add(mapLabel);
				var officialSelection = new ListBox()
				{
					GridColumn = 0,
					GridRow = 1
				};
				mapGrid.Widgets.Add(officialSelection);
				foreach (var path in GameManager.PreGameData.MapList)
				{
					var item = new ListItem()
					{
						Text = path,
					};
					officialSelection.Items.Add(item);
				}
				officialSelection.SelectedIndexChanged += (s, a) =>
				{
					GameManager.PreGameData.SelectedMap = officialSelection.Items[(int) officialSelection.SelectedIndex].Text;
					Networking.SendPreGameUpdate();
				};
				
				var commapLabel = new Label()
				{
					Text = "Recent Community Maps",
					GridColumn = 1,
					GridRow = 0
				};
				mapGrid.Widgets.Add(commapLabel);
				var customMapUpload = new TextButton()
				{
					Text = "Upload Local Map",
					GridColumn = 1,
					GridRow = 0,
					Top = 20,
				};
				mapGrid.Widgets.Add(customMapUpload);
				customMapUpload.Click += (i, a) =>
				{
					
					var file = new Myra.Graphics2D.UI.File.FileDialog(FileDialogMode.OpenFile)
					{
						
					};
					if (file == null) throw new ArgumentNullException(nameof(file));
					file.FilePath = "./Maps";
					file.ButtonOk.Click += (o, e) =>
					{
						var path = file.FilePath;
						Networking.UploadMap(path);
						file.Close();
					};
					panel.Widgets.Add(file);
				};
				var communitySelection = new ListBox()
				{
					GridColumn =1,
					GridRow = 1
				};
				mapGrid.Widgets.Add(communitySelection);
				foreach (var path in GameManager.PreGameData.CustomMapList)
				{
					var item = new ListItem()
					{
						Text = path,
					};
					communitySelection.Items.Add(item);
				}
				communitySelection.SelectedIndexChanged += (s, a) =>
				{
					GameManager.PreGameData.SelectedMap = communitySelection.Items[(int) communitySelection.SelectedIndex].Text;
					Networking.SendPreGameUpdate();
				};
				


			}


			var btn2 = new TextButton()
			{
				Text = "Quit",
				GridColumn = 1,
				GridRow = 2,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Top = 90,
			};
			btn2.Click += (s, a) =>
			{
				Networking.Disconnect();

			};
			panel.Widgets.Add(btn2);

			var label = new Label()
			{
				Text = GameManager.PreGameData.HostName,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Top = -25,
				Background = new SolidBrush(Color.Black)
			};
			var label2 = new Label()
			{
				Text = GameManager.PreGameData.Player2Name,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Top = 25,
				Background = new SolidBrush(Color.Black)
			};
			panel.Widgets.Add(label);
			panel.Widgets.Add(label2);
			
			root = panel;



		}


		private static int soldierCount = 0;
		private static int scoutCount = 0;
		private static int heavyCount = 0;
		public static void SetupUI()
		{


			var grid = new Grid
			{

				ColumnSpacing = 0,
				RowSpacing = 0,
				
			};

			
			//soldier counter
			var soldierButton = new TextButton
			{
			
				GridColumn = 2,
				GridRow = 1,
				Text = "Soldiers: "+soldierCount
			};
			grid.Widgets.Add(soldierButton);
			var freeslots = new Label()
			{
			
				GridColumn = 2,
				GridRow = 0,
				Text = "Free Units "+(WorldManager.Instance.CurrentMap.unitCount-soldierCount-scoutCount-heavyCount),
			};
			grid.Widgets.Add(freeslots);
			
			var soldierLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "<"
			};
			soldierLeft.Click += (s, a) =>
			{
				soldierCount--;
				if (soldierCount < 0)
				{
					soldierCount = 0;
				}

				soldierButton.Text = "Soldiers: " + soldierCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(soldierLeft);
			var soldierRight = new TextButton
			{
				GridColumn = 3,
				GridRow = 1,
				Text = ">"
			};
			soldierRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + heavyCount+1 >  WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}

				soldierCount++;
				soldierButton.Text = "Soldiers: " + soldierCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(soldierRight);
			
			//scout counter
			var scoutButton = new TextButton
			{
				GridColumn = 2,
				GridRow = 3,
				Text = "Scouts: "+scoutCount
			};
			grid.Widgets.Add(scoutButton);
			var scountLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 3,
				Text = "<"
			};
			scountLeft.Click += (s, a) =>
			{
				scoutCount--;
				if (scoutCount < 0)
				{
					scoutCount = 0;
				}

				scoutButton.Text = "Scouts: " + scoutCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(scountLeft);
			var scoutRight = new TextButton
			{
				GridColumn = 3,
				GridRow = 3,
				Text = ">"
			};
			scoutRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + heavyCount+ 1 > WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}

				scoutCount++;
				scoutButton.Text = "Scouts: " + scoutCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(scoutRight);
			
			
			//heavy counter
			var heavyButton = new TextButton
			{
				GridColumn = 2,
				GridRow = 2,
				Text = "Heavies: "+heavyCount
			};
			grid.Widgets.Add(heavyButton);
			var heavyLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "<"
			};
			heavyLeft.Click += (s, a) =>
			{
				heavyCount--;
				if (heavyCount < 0)
				{
					heavyCount = 0;
				}

				heavyButton.Text = "Heavies: " + heavyCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(heavyLeft);
			var heavyRight = new TextButton
			{
				GridColumn = 3,
				GridRow = 2,
				Text = ">"
			};
			heavyRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + heavyCount+ 1 >  WorldManager.Instance.CurrentMap.unitCount)
				{
					return;
				}

				heavyCount++;
				heavyButton.Text = "Heavies: " + heavyCount;
				freeslots.Text = "Free Units " + (WorldManager.Instance.CurrentMap.unitCount - soldierCount - scoutCount - heavyCount);
			};
			grid.Widgets.Add(heavyRight);


			var confirm = new TextButton
			{
				GridColumn = 2,
				GridRow = 4,
				Text = "Confirm"
			};
			confirm.Click += (s, a) =>
			{
				UnitStartDataPacket packet = new UnitStartDataPacket();
				packet.Scouts = scoutCount;
				packet.Soldiers = soldierCount;
				packet.Heavies = heavyCount;
				Networking.serverConnection.Send(packet);
				SetUI(GameUi);
			};
			grid.Widgets.Add(confirm);
			

			root = grid;
		}


		public static Dialog OptionMessage(string title, string content, string option1text, EventHandler option1,string option2text, EventHandler option2)
		{
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ButtonCancel.Text = option1text;
			messageBox.ButtonCancel.Click += option1;
			messageBox.ButtonOk.Text = option2text;
			messageBox.ButtonOk.Click += option2;
			messageBox.ShowModal(Desktop);
			return messageBox;
		}

		public static void ShowMessage(string title, string content)
		{
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ShowModal(Desktop);

		}
		
		public static void EditorMenu()
		{

			raycastDebug = true;
			
			var grid = new Grid
			{
				RowSpacing = 0,
				ColumnSpacing = 0
			};

			int ypos = 0;
			foreach (var prefabDictElement in PrefabManager.Prefabs)
			{
				
				
				var button = new TextButton
				{
					GridColumn = 0,
					GridRow = ypos,
					Text = prefabDictElement.Key
				};

				button.Click += (s, a) =>
				{ 
					WorldEditSystem.ActivePrefab = prefabDictElement.Key;
				};
				grid.Widgets.Add(button);
				
				
			
				ypos += 1;
			
			}
			var save = new TextButton
			{
				GridColumn = 5,
				GridRow = 0,
				Text = "save"
			};

			save.Click += (s, a) =>
			{ 			
				var panel = new Panel();
				var label = new Label()
				{
					Text = "Enter Map Name",
					Top = 0,
				};
				panel.Widgets.Add(label);
				
				var mapname = new TextBox()
				{
					Top = 25,
				};
				mapname.Text = WorldManager.Instance.CurrentMap.Name;
				panel.Widgets.Add(mapname);
				label = new Label()
				{
					Text = "Author Name",
					Top = 50
				};
				panel.Widgets.Add(label);
				var authorname = new TextBox()
				{
					Top = 75,
					Text = WorldManager.Instance.CurrentMap.Author,
				};
				panel.Widgets.Add(authorname);
				label = new Label()
				{
					Text = "Unit Count",
					Top = 100,
				};
				panel.Widgets.Add(label);
				var unitCount = new TextBox()
				{
					Top = 125,
					Text = WorldManager.Instance.CurrentMap.unitCount.ToString(),
				};
				panel.Widgets.Add(unitCount);
				
				
				
				var dialog = Dialog.CreateMessageBox("Save Map", panel);
				dialog.Height = 250;
				dialog.ButtonOk.Click += (sender, args) =>
				{
					
					WorldManager.Instance.CurrentMap = new MapData();
					WorldManager.Instance.CurrentMap.Name = mapname.Text;
					WorldManager.Instance.CurrentMap.Author = authorname.Text;
					int units = 6;
					bool result  = int.TryParse(unitCount.Text, out units);
					if (result)
					{
						WorldManager.Instance.CurrentMap.unitCount = units;
					}
					else
					{
						WorldManager.Instance.CurrentMap.unitCount = 6;
					}
					WorldManager.Instance.SaveCurrentMapTo("./Maps/"+mapname.Text+".mapdata");
				};
				dialog.ShowModal(Desktop);
				
			};
			
			var load = new TextButton
			{
				GridColumn = 5,
				GridRow = 1,
				Text = "load"
			};

			load.Click += (s, a) =>
			{ 
				//dropdown with all maps
				string[] filePaths = Directory.GetFiles("./Maps/", "*.mapdata");
				var panel = new Panel();
				var label = new Label()
				{
					Text = "Select a map to load"
				};
				panel.Widgets.Add(label);
				
				var selection = new ListBox();
				panel.Widgets.Add(selection);
				
				foreach (var path in filePaths)
				{
					var item = new ListItem()
					{
						Text = path.Split("/").Last().Split(".").First(),
					};
					selection.Items.Add(item);
				}
				var dialog = Dialog.CreateMessageBox("Load Map", panel);
				dialog.Width = 100;
				dialog.Height = (int) (1000f * globalScale.Y);
				dialog.ButtonOk.Click += (sender, args) =>
				{
					WorldManager.Instance.LoadMap("./Maps/"+selection.SelectedItem.Text+".mapdata");
				};
				dialog.ShowModal(Desktop);
				
			};
			grid.Widgets.Add(save);
			grid.Widgets.Add(load);
			
			var point = new TextButton
			{
				GridColumn = 1,
				GridRow = ypos,
				Text = "Point(1)"
			};
			point.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Point;
			};
			grid.Widgets.Add(point);
			var selection = new TextButton
			{
				GridColumn = 2,
				GridRow = ypos,
				Text = "Selection(2)"
			};
			selection.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Selection;
			};
			grid.Widgets.Add(selection);
			/*var line = new TextButton
			{
				GridColumn = 3,
				GridRow = ypos,
				Text = "Line(3)"
			};
			line.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Line;
			};
			grid.Widgets.Add(line);*/
			
			
			var rotateq = new TextButton
			{
				GridColumn = 2,
				GridRow = ypos-1,
				Text = "<- Rotate(Q)"
			};
			rotateq.Click += (s, a) =>
			{
				WorldEditSystem.ActiveDir--;
			};
			grid.Widgets.Add(rotateq);
			
			var rotatee = new TextButton
			{
				GridColumn = 3,
				GridRow = ypos-1,
				Text = "Rotate(E) -->"
			};
			rotatee.Click += (s, a) =>
			{
				WorldEditSystem.ActiveDir++;
			};
			grid.Widgets.Add(rotatee);
			
			
			
			root = grid;
			
			
		}

		private static Panel turnIndicator;
		private static Label scoreIndicator;
		private static VerticalStackPanel chatBox;
		private static ScrollViewer chatBoxViewer;
		
		public static void RecieveChatMessage(string msg)
		{
			var label = new Label
			{
				Text = msg,
				Wrap = true
			};
			lock (myrasyncobj)
			{
				if (chatBox != null)
				{
					chatBox.Widgets.Add(label);
					chatBoxViewer.ScrollPosition = chatBoxViewer.ScrollMaximum + new Point(0,50);
				}
			}
		}

		public static void SetMyTurn(bool myTurn)
		{
			if (myTurn)
			{
				UI.turnIndicator.Background = new SolidBrush(Color.Green);

			}
			else
			{
				UI.turnIndicator.Background = new SolidBrush(Color.Red);
			}
		}
		public static void SetScore(int score)
		{
			scoreIndicator.Text = "score: " + score;
		}

		public static bool MousePassthrough { get; private set; }

		private static void AttachChatBox(Panel parent)
		{
			lock (myrasyncobj)
			{
				if (chatBoxViewer == null)
				{
					chatBoxViewer = new ScrollViewer()
					{
						Left = 0,
						Width = 240,
						Height = 250,
						Top = 0,
						HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center,
					};
				}

				if (chatBox == null)
				{
					chatBox = new VerticalStackPanel()
					{
						VerticalAlignment = VerticalAlignment.Bottom,
						HorizontalAlignment = HorizontalAlignment.Left,
					};
				}

				chatBoxViewer.Content = chatBox;
				chatBoxViewer.ScrollPosition = chatBoxViewer.ScrollMaximum + new Point(0, 50);

				var input = new TextBox()
				{
					Width = 200,
					Height = 20,
					Top = 135,
					Left = 0,
					Text = "",
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Center,

				};
				input.KeyDown += (o, a) =>
				{
					if (a.Data == Keys.Enter)
					{
						if (input.Text != "")
						{
							if (Networking.serverConnection != null && Networking.serverConnection.IsAlive)
							{
								Networking.ChatMSG(input.Text);
							}
							else
							{
								MasterServerNetworking.ChatMSG(input.Text);
							}


							input.Text = "";
						}
					}
				};
				var inputbtn = new TextButton()
				{
					Width = 55,
					Height = 20,
					Top = 135,
					Left = 200,
					Text = "Send",
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Center,

				};
				inputbtn.Click += (o, a) =>
				{
					if (input.Text != "")
					{
						if (Networking.serverConnection != null && Networking.serverConnection.IsAlive)
						{
							Networking.ChatMSG(input.Text);
						}
						else
						{
							MasterServerNetworking.ChatMSG(input.Text);
						}

						input.Text = "";
					}
				};
				parent.Widgets.Add(inputbtn);
				parent.Widgets.Add(input);
				parent.Widgets.Add(chatBoxViewer);
			}
		}

		public static void GameUi()
		{
			
			
			var panel = new Panel
			{
				
			};
			if (!GameManager.spectating)
			{


				var end = new TextButton
				{
					Top = (int) (0f * globalScale.Y),
					Left = (int) (-10f * globalScale.X),
					Width = (int) (80 * globalScale.X),
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Top,
					Text = "End Turn",
					//Scale = globalScale
				};
				end.Click += (o, a) => GameManager.EndTurn(); 
				panel.Widgets.Add(end);
			}
			else
			{
				var swapTeam = new TextButton
				{
					Top = (int) (0f * globalScale.Y),
					Left = (int) (-10f * globalScale.X),
					Width = (int) (80 * globalScale.X),
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Top,
					Text = "Change POV",
					//Scale = globalScale
				};
				swapTeam.Click += (o, a) =>
				{
					GameManager.IsPlayer1 = !GameManager.IsPlayer1;
					WorldManager.Instance.MakeFovDirty();
					GameManager.CountMyUnits();
					
					UI.SetUI(null);
				};
				panel.Widgets.Add(swapTeam);
			}

			var quit = new TextButton
			{
				Top= (int)(200f*globalScale.Y),
				Left = (int)(-10f*globalScale.X),
				Width = (int)(80 * globalScale.X),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Text = "QUIT",
				//Scale = globalScale
			};
			quit.Click += (o, a) =>
			{
				Networking.Disconnect();
				
			};

			panel.Widgets.Add(quit);

			turnIndicator = new Panel()
			{
				Top= (int)(-1f*globalScale.Y),
				Left =(int)(-150f*globalScale.X),
				Height =50,
				Width = (int)(80 * globalScale.X),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Background = new SolidBrush(Color.Red),
				//Scale = globalScale
			};
			panel.Widgets.Add(turnIndicator);
			SetMyTurn(GameManager.IsMyTurn());
			if (scoreIndicator == null)
			{


				scoreIndicator = new Label()
				{
					Top=0,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				SetScore(0);
			}

			panel.Widgets.Add(scoreIndicator);

			AttachChatBox(panel);
		
			var UnitContainer = new Grid()
			{
				GridColumnSpan = 4,
				GridRowSpan = 1,
				RowSpacing = 10,
				ColumnSpacing = 10,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				MaxWidth = (int)(700f*globalScale.X),
				//ShowGridLines = true,
			};
			panel.Widgets.Add(UnitContainer);

			var column = 0;

			foreach (var unit in GameManager.MyUnits)
			{

				var unitPanel = new Panel()
				{
					Width = Math.Clamp((int) (100 * globalScale.X), 0, 110),
					Height = Math.Clamp((int) (200 * globalScale.Y), 0, 210),
					GridColumn = column,
					Background = new SolidBrush(Color.Black),

				};
				if (unit.Equals(SelectedControllable))
				{
					unitPanel.Background = new SolidBrush(Color.DimGray);
					unitPanel.Top = 25;
				}

				unitPanel.TouchDown += (sender, args) =>
				{
					Console.WriteLine("select");
					if (unit.Health > 0)
					{
						SelectControllable(unit);
					}
				};

				UnitContainer.Widgets.Add(unitPanel);
				var unitName = new Label()
				{
					Text = unit.Type.Name,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Center,
				};
				unitPanel.Widgets.Add(unitName);
				var unitImage = new Image()
				{
					Width = 80,
					Height = 80,
					Top = 20,
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Center,
					Renderable = new TextureRegion(TextureManager.GetTexture("UI/PortraitAlive"))
				};
				if (unit.Health <= 0)
				{
					unitImage.Renderable = new TextureRegion(TextureManager.GetTexture("UI/PortraitDead"));
					unitPanel.Top = -10;
					unitPanel.Background = new SolidBrush(Color.DarkRed);
				}unitPanel.Widgets.Add(unitImage);
			
				List<Texture2D> indicators1 = new List<Texture2D>();
				for (int i = 1; i <= unit.Type.MaxMovePoints; i++)
				{
					if (unit.MovePoints < i)
					{
						indicators1.Add(infoIndicator[0]);
					}
					else
					{
						indicators1.Add(infoIndicator[1]);
					}

				}
				List<Texture2D> indicators2 = new List<Texture2D>();
				for (int i = 1; i <= unit.Type.MaxTurnPoints; i++)
				{
				
					if (unit.TurnPoints < i)
					{
						indicators2.Add(infoIndicator[2]);
					}
					else
					{
						indicators2.Add(infoIndicator[3]);
					}

		
				}
				List<Texture2D> indicators3 = new List<Texture2D>();
				for (int i = 1; i <=  unit.Type.MaxActionPoints; i++)
				{
				
					if (unit.FirePoints < i)
					{
						indicators3.Add(infoIndicator[4]);
					}
					else
					{
						indicators3.Add(infoIndicator[5]);
					}

				}

				int xsize = Math.Clamp((int) (20 * globalScale.X), 0, 35);
				int ysize = Math.Clamp((int) (20 * globalScale.Y), 0, 35);
				
				int xpos = 0;
				int ypos = -ysize;
				List<List<Texture2D>> indicators = new List<List<Texture2D>>();
				indicators.Add(indicators1);
				indicators.Add(indicators2);
				indicators.Add(indicators3);
				foreach (var indicatorList in indicators)
				{
					
					xpos = 0;
					ypos += ysize;
					
					foreach (var indicator in indicatorList)
					{

						var icon = new Image()
						{
							Width = xsize,
							Height = ysize,
							Left = xpos,
							Top = -ypos,
							VerticalAlignment = VerticalAlignment.Bottom,
							HorizontalAlignment = HorizontalAlignment.Left,
							Renderable = new TextureRegion(indicator)
						};
						xpos += xsize;
						unitPanel.Widgets.Add(icon);
					}

				}

				column++;
			}

			
			root = panel;
			
		}


		private static string PreviewDesc;
		private static Label descBox;

		private static void SetPreviewDesc(string desc)
		{
			PreviewDesc = desc;
			if(descBox!= null){
				descBox.Text = desc;
			}
		}
		public static readonly object myrasyncobj = new object();

		public static void UnitUi()
		{
			if (SelectedControllable!=null && SelectedControllable.IsMyTeam())
			{
				
				GameUi();
				lock (myrasyncobj)
				{


					var localroot = (Panel) root;
					previewMoves = SelectedControllable.GetPossibleMoveLocations();


					descBox = new Label()
					{
						Top = -100,
						MaxWidth = (int) (600 * globalScale.X),
						MinHeight = 40,
						MaxHeight = 80,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Bottom,
						Background = new SolidBrush(Color.DimGray),
						Text = PreviewDesc,
						TextAlign = TextHorizontalAlignment.Center,
						Wrap = true
					};
					localroot.Widgets.Add(descBox);


					var buttonContainer = new Grid()
					{
						GridColumn = 1,
						GridRow = 3,
						GridColumnSpan = 4,
						GridRowSpan = 1,
						RowSpacing = 10,
						ColumnSpacing = 10,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Bottom,
						//ShowGridLines = true,
					};
					localroot.Widgets.Add(buttonContainer);

					buttonContainer.RowsProportions.Add(new Proportion(ProportionType.Pixels, 20));
					var fire = new ImageButton()
					{
						GridColumn = 0,
						GridRow = 1,

						Image = new TextureRegion(TextureManager.GetTexture("UI/Fire")),
						//	Scale = new Vector2(1.5f)
					};
					fire.Click += (o, a) => Action.SetActiveAction(ActionType.Attack);
					fire.MouseEntered += (o, a) => SetPreviewDesc("Shoot at a selected target. Anything in the blue area will get suppressed and lose determination. Cost: 1 action, 1 move");
					buttonContainer.Widgets.Add(fire);
					var watch = new ImageButton
					{
						GridColumn = 1,
						GridRow = 1,
						//	Text = "Overwatch",
						Image = new TextureRegion(TextureManager.GetTexture("UI/Overwatch"))
					};
					watch.Click += (o, a) => Action.SetActiveAction(ActionType.OverWatch);
					watch.MouseEntered += (o, a) => SetPreviewDesc("Watch Selected Area. First enemy to enter the area will be shot at automatically. Cost: 1 action, 1 move, 1 turn. Unit Cannot act anymore in this turn");
					buttonContainer.Widgets.Add(watch);
					var crouch = new ImageButton
					{
						GridColumn = 2,
						GridRow = 1,
						//		Text = "Crouch/Stand",
						Image = new TextureRegion(TextureManager.GetTexture("UI/Crouch"))
					};
					crouch.MouseEntered += (o, a) => SetPreviewDesc("Crouching improves benefits of cover and allows hiding under tall cover however you can move less tiles. Cost: 1 move");
					crouch.Click += (o, a) =>
					{
						if (SelectedControllable != null)
						{
							SelectedControllable.DoAction(Action.Actions[ActionType.Crouch], null);
						}
					};
					buttonContainer.Widgets.Add(crouch);
					int column = 3;
					foreach (var act in SelectedControllable.Type.extraActions)
					{
						var actBtn = new ImageButton
						{
							GridColumn = column,
							GridRow = 1,
							//	Text = act.Item1,
							Image = new TextureRegion(TextureManager.GetTexture("UI/" + act.Item1))
						};
						actBtn.Click += (o, a) => Action.SetActiveAction(act.Item2);
						actBtn.MouseEntered += (o, a) => SetPreviewDesc(Action.Actions[act.Item2].Description);
						buttonContainer.Widgets.Add(actBtn);
						column++;
					}

				}
			}
		}

		public static void DrawControllableHoverHud(SpriteBatch batch, Controllable controllable)
		{
	
			Debug.Assert(controllable != null, nameof(controllable) + " != null");


			Queue<Texture2D> indicators = new Queue<Texture2D>();
			for (int i = 1; i <= controllable.Type.MaxMovePoints; i++)
			{
				
				if (controllable.MovePoints < i)
				{
					indicators.Enqueue(infoIndicator[0]);
				}
				else
				{
					indicators.Enqueue(infoIndicator[1]);
				}

			}
			for (int i = 1; i <= controllable.Type.MaxTurnPoints; i++)
			{
				
				if (controllable.TurnPoints < i)
				{
					indicators.Enqueue(infoIndicator[2]);
				}
				else
				{
					indicators.Enqueue(infoIndicator[3]);
				}

		
			}
			for (int i = 1; i <=  controllable.Type.MaxActionPoints; i++)
			{
				
				if (controllable.FirePoints < i)
				{
					indicators.Enqueue(infoIndicator[4]);
				}
				else
				{
					indicators.Enqueue(infoIndicator[5]);
				}

			}

			int offset = 0;
			foreach (var indicator in indicators)
			{

				batch.Draw(indicator,Utility.GridToWorldPos((Vector2)controllable.worldObject.TileLocation.Position+new Vector2(-2f,-0.9f))+new Vector2(60*offset,0),Color.White);
				offset++;
			}
			
			for (int i = 0; i < controllable.Type.Maxdetermination; i++)
			{
				var indicator = healthIndicator[1];
				if (controllable.Type.Maxdetermination  - i  == controllable.determination+1 && !controllable.paniced)
				{
					indicator= healthIndicator[2];
				}
				else if (controllable.Type.Maxdetermination - i > controllable.determination)
				{
					indicator= healthIndicator[0];
				}

				batch.Draw(indicator,Utility.GridToWorldPos((Vector2)controllable.worldObject.TileLocation.Position+new Vector2(-1.5f,-0.8f))+new Vector2(45*i,0),Color.Green);	
				
			}

			for (int i = 0; i < controllable.Type.MaxHealth; i++)
			{
				var indicator = healthIndicator[1];
				if (i>= controllable.Health)
				{
					indicator= healthIndicator[0];
				}

				batch.Draw(indicator,Utility.GridToWorldPos((Vector2)controllable.worldObject.TileLocation.Position+new Vector2(-1.2f,-0.5f))+new Vector2(45*i,0),Color.White);	
				
			}
		}

		private static KeyboardState lastState;
		private static Dictionary<Vector2Int, Visibility> EnemyVisiblityCache = new Dictionary<Vector2Int, Visibility>();
		private static Controllable enemySelected;
		public static void Update(float deltatime)
		{
			var keyboardState = Keyboard.GetState();
			if(WorldEditSystem.enabled) return;
			if(Desktop.FocusedKeyboardWidget != null) return;
			if(GameManager.MyUnits.Count == 0) return;
			if (keyboardState.IsKeyDown(Keys.E) && lastState.IsKeyUp(Keys.E))
			{
				
				int fails = 0;
				do
				{
					var index = GameManager.MyUnits.FindIndex(i => i == SelectedControllable) + 1;
					if (index >= GameManager.MyUnits.Count)
					{
						index = 0;
					}

					SelectControllable(GameManager.MyUnits[index]);
					if(fails>GameManager.MyUnits.Count)
						break;
					fails++;
				} while (SelectedControllable.Health <= 0);


			}
			if (keyboardState.IsKeyDown(Keys.Q) && lastState.IsKeyUp(Keys.Q))
			{
				int fails = 0;
				do
				{
					var index = GameManager.MyUnits.FindIndex(i => i == SelectedControllable)-1;
					if (index < 0)
					{
						index = GameManager.MyUnits.Count-1;
					}
			
					SelectControllable(GameManager.MyUnits[index]);
					if(fails>GameManager.MyUnits.Count)
						break;
					fails++;
				} while (SelectedControllable.Health <= 0);

			}

			lastState = keyboardState;
		}

		private static bool raycastDebug;
		private static List<Vector2Int>[] previewMoves = new List<Vector2Int>[2];
		public static bool targeting = false;
		public static void Render(float deltaTime)
		{
			
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			//TileCoordinate = new Vector2(34, 33);
			TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
			var Mousepos = Utility.GridToWorldPos((Vector2)TileCoordinate+new Vector2(-1.5f,-0.5f));


			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred);
			
				for (int i = 0; i < 8; i++)
				{
					var indicator = coverIndicator[i];
					Color c = Color.White;
					switch ((Cover) WorldManager.Instance.GetTileAtGrid(TileCoordinate).GetCover((Direction) i))
					{
						case Cover.Full:
							c = Color.Red;
							break;
						case Cover.High:
							c = Color.Yellow;
							break;
						case Cover.Low:
							c = Color.Green;
							break;
					}

					//spriteBatch.DrawCircle(Mousepos, 5, 10, Color.Red, 200f);
					spriteBatch.Draw(indicator, Mousepos, c);
				}
				

			if (targeting)
			{
				spriteBatch.Draw(targetingCUrsor, Mousepos, Color.Red);
			}


			var count = 0;
			foreach (var moves in previewMoves.Reverse())
			{
				foreach (var path in moves)
				{


					if (path.X < 0 || path.Y < 0) break;
					var  pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

					Color c = Color.White;
					switch (count)
					{
						case 0:
							c = Color.Red;
							break;
						case 1:
							c = Color.Yellow;
							break;
						case 2:
							c = Color.Green;
							break;
						default:
							c = Color.LightGreen;
							break;

					}

					spriteBatch.DrawRectangle(pos, new Size2(20, 20), c, 5);


				}

				count++;
			}
			
			foreach (var obj in Controllables)
			{
				if (obj.worldObject.IsVisible())
				{
					DrawControllableHoverHud(spriteBatch, obj);
				}
			}



/* griddebug
			spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
				
					spriteBatch.DrawCircle(Utility.GridToWorldPos(new Vector2(x,y)), 5, 10, Color.Black, 5f);
				}
			}
			spriteBatch.End();
			
			*/
			targeting = false;
			var tile = WorldManager.Instance.GetTileAtGrid(TileCoordinate);
			if (SelectedControllable != null && Action.GetActiveActionType() != null)
			{
				Action.ActiveAction.Preview(SelectedControllable, TileCoordinate,spriteBatch);
			}else if (tile.ObjectAtLocation?.ControllableComponent != null && tile.ObjectAtLocation.IsVisible() && !tile.ObjectAtLocation.ControllableComponent.IsMyTeam())
			{
				if (tile.ObjectAtLocation.ControllableComponent != enemySelected)
				{
					EnemyVisiblityCache = WorldManager.Instance.GetVisibleTiles(tile.ObjectAtLocation.TileLocation.Position, tile.ObjectAtLocation.Facing,tile.ObjectAtLocation.ControllableComponent.GetSightRange(),tile.ObjectAtLocation.ControllableComponent.Crouching );
					enemySelected = tile.ObjectAtLocation.ControllableComponent;
				}

				foreach (var unit in GameManager.MyUnits)
				{
					if (unit.Health <= 0) continue;
					if (Camera.IsOnScreen(unit.worldObject.TileLocation.Position))
					{
						Action.Actions[ActionType.Attack].Preview(tile.ObjectAtLocation.ControllableComponent,unit.worldObject.TileLocation.Position,spriteBatch);
						if(EnemyVisiblityCache.ContainsKey(unit.worldObject.TileLocation.Position) && EnemyVisiblityCache[unit.worldObject.TileLocation.Position] >= unit.worldObject.GetMinimumVisibility())
						{
							spriteBatch.Draw(vissionIndicator[0],Utility.GridToWorldPos(unit.worldObject.TileLocation.Position),  Color.White);
						}
						else
						{
							spriteBatch.Draw(vissionIndicator[1],Utility.GridToWorldPos(unit.worldObject.TileLocation.Position),  Color.White);
						}
					}

				}
			}

			WorldEditSystem.Draw(spriteBatch);
			var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			spriteBatch.DrawString(Game1.SpriteFont,"X:"+MousePos.X+" Y:"+MousePos.Y,  Camera.GetMouseWorldPos(),Color.Wheat, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			spriteBatch.End();

			lock (myrasyncobj)
			{ 
				Desktop.Root = root;
				UI.Desktop.Render();
			}
			
		}

		
	
	}
}