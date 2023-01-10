using System;
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
using Myra.Utility;
using Network;
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
			
			targetingCUrsor = TextureManager.GetTexture("UI/targetingCursor");
			
			LeftClick += LeftClickAtPosition;
			RightClick += RightClickAtPosition;

			previewMoves[0] = new List<Vector2Int>();
			previewMoves[1] = new List<Vector2Int>();
		}


		public delegate void UIGen();

		private static UIGen currentUI;
		private static Vector2 globalScale = new Vector2(1, 1);
		public static readonly object myrasyncobj = new object();

		public static void SetUI(UIGen? uiMethod) {
			lock (myrasyncobj)
			{
				globalScale = new Vector2((Game1.instance.Window.ClientBounds.Width / 1000f) * 1f, (Game1.instance.Window.ClientBounds.Width / 1000f) * 1f);
				if (uiMethod != null)
				{
					currentUI = uiMethod;
				}

				currentUI.Invoke();
			}
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
			
			SelectedControllable = controllable;
			if(controllable==null) return;
			SetUI(UnitUi);
			
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

	

		public static void MainMenu()
		{
			var grid = new Grid
			{
				RowSpacing = 0,
				ColumnSpacing = 0
			};


			var button = new TextButton
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "Join Server"
			};

			button.Click += (s, a) => { SetUI(ConnectionMenu); };

			grid.Widgets.Add(button);

			var button2 = new TextButton
			{
				GridColumn = 1,
				GridRow = 2,
				Text = "Map Editor"
			};

			button2.Click += (s, a) =>
			{
				WorldEditSystem.Init();
				WorldEditSystem.GenerateUI();

			};

			grid.Widgets.Add(button2);



			Desktop.Root = grid;


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
				Text = "46.7.175.47"
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

			button.Click += (s, a) =>
			{
				ConnectionResult result = Networking.Connect(textBox.Text.Trim(), textBox2.Text.Trim());
				if (result == ConnectionResult.Connected)
				{
					ShowMessage("Connection Notice", "Connected to server!");

					grid.Widgets.Remove(button);
					grid.Widgets.Remove(textBox);
					SetUI(SetupUi);
					DiscordManager.client.UpdateState("In Battle");
				}
				else
				{
					ShowMessage("Connection Notice", "Failed to connect: " + result);

				}

			};

			grid.Widgets.Add(button);



			Desktop.Root = grid;



		}


		private static int soldierCount = 0;
		private static int scoutCount = 0;
		private static int heavyCount = 0;
		public static void SetupUi()
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
				if (soldierCount + scoutCount + heavyCount+1 > 6)
				{
					return;
				}

				soldierCount++;
				soldierButton.Text = "Soldiers: " + soldierCount;
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
				if (soldierCount + scoutCount + heavyCount+ 1 > 6)
				{
					return;
				}

				scoutCount++;
				scoutButton.Text = "Scouts: " + scoutCount;
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
				if (soldierCount + scoutCount + heavyCount+ 1 > 6)
				{
					return;
				}

				heavyCount++;
				heavyButton.Text = "Heavies: " + heavyCount;
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
				StartDataPacket packet = new StartDataPacket();
				packet.Scouts = scoutCount;
				packet.Soldiers = soldierCount;
				packet.Heavies = heavyCount;
				Networking.serverConnection.Send(packet);
				SetUI(GameUi);
			};
			grid.Widgets.Add(confirm);
			

			Desktop.Root = grid;
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
				WorldManager.Instance.SaveData("map.mapdata");
			};
			
			var load = new TextButton
			{
				GridColumn = 5,
				GridRow = 1,
				Text = "load"
			};

			load.Click += (s, a) =>
			{ 
				WorldManager.Instance.LoadData(File.ReadAllBytes("map.mapdata"));
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
			var line = new TextButton
			{
				GridColumn = 3,
				GridRow = ypos,
				Text = "Line(3)"
			};
			line.Click += (s, a) =>
			{
				WorldEditSystem.ActiveBrush = WorldEditSystem.Brush.Line;
			};
			grid.Widgets.Add(line);
			
			
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
			
			
			
			Desktop.Root = grid;
			
			
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
			if (chatBox != null)
			{
				chatBox.Widgets.Add(label);
				chatBoxViewer.ScrollPosition = chatBoxViewer.ScrollMaximum + new Point(0,50);
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

		public static void GameUi()
		{
			
			
			var panel = new Panel
			{
				
			};

			var end = new TextButton
			{
				Top= (int)(0f*globalScale.Y),
				Left = (int)(-10f*globalScale.X),
				Width = (int)(80 * globalScale.X),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Text = "End Turn",
				//Scale = globalScale
			};
			end.Click += (o,a) => GameManager.EndTurn();		
			panel.Widgets.Add(end);
		/*	
			var debug = new TextButton
			{
				GridColumn = 8,
				GridRow = 1,
				Text = "Raycast Toggle"
			};
			debug.Click += (o,a) => raycastDebug = !raycastDebug;
			grid.Widgets.Add(debug);
			*/
			
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
					HorizontalAlignment = HorizontalAlignment.Center
				};
				SetScore(0);
			}

			panel.Widgets.Add(scoreIndicator);

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

			
			if(chatBox== null)
			{
				chatBox = new VerticalStackPanel()
				{
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Left,
				};
			}
			chatBoxViewer.Content = chatBox;

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
						Networking.ChatMSG(input.Text);
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
			inputbtn.Click += (o,a) =>
			{
				if (input.Text != "")
				{
					Networking.ChatMSG(input.Text);
					input.Text = "";
				}
			};
			panel.Widgets.Add(inputbtn);
			panel.Widgets.Add(input);
			panel.Widgets.Add(chatBoxViewer);
		
			var UnitContainer = new Grid()
			{
				GridColumnSpan = 4,
				GridRowSpan = 1,
				RowSpacing = 10,
				ColumnSpacing = 10,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				//ShowGridLines = true,
			};
			panel.Widgets.Add(UnitContainer);

			var column = 0;
			
			foreach (var unit in GameManager.MyUnits)
			{
				var unitPanel = new Panel()
				{
					Width = 100,
					Height = 150,
					GridColumn = column,
					Background = new SolidBrush(Color.Black),

				};
				if (SelectedControllable == unit)
				{
					unitPanel.Background = new SolidBrush(Color.Black);
					unitPanel.Top = 50;
				}
				

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
					Width = 100,
					Height = 100,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					Renderable = new TextureRegion(TextureManager.GetTexture("UI/PortraitAlive"))
				};
				unitPanel.Widgets.Add(unitImage);
				column++;
			}
			
			
			Desktop.Root = panel;
		}

		public static bool ffmode { get; private set; } = false;
		private static string PreviewDesc;
		private static Label descBox;
		private static TextButton ffmodebtn;
		private static void SetPreviewDesc(string desc)
		{
			PreviewDesc = desc;
			if(descBox!= null){
				descBox.Text = desc;
			}
		}
		public static void UnitUi()
		{
			if (SelectedControllable!=null && SelectedControllable.IsMyTeam())
			{
				
				GameUi();
				var root = (Panel) Desktop.Root;
				previewMoves = SelectedControllable.GetPossibleMoveLocations();


				descBox = new Label()
				{
					Top = -100,
					MaxWidth = (int)(600 * globalScale.X),
					MinHeight = 40,
					MaxHeight = 80,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Bottom,
					Background = new SolidBrush(Color.DimGray),
					Text = PreviewDesc,
					TextAlign = TextHorizontalAlignment.Center,
					Wrap = true
				};
				root.Widgets.Add(descBox);


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
				root.Widgets.Add(buttonContainer);
				ffmodebtn = new TextButton()
				{
					Text = "FreeFire Mode: " + (ffmode ? "On" : "Off"),
					GridColumn = 0,
					GridRow = 0,
				
				};
				ffmodebtn.Click += (o, a) =>
				{
					ffmode = !ffmode;
					ffmodebtn.Text = "FreeFire Mode: " + (ffmode ? "On" : "Off");
				};
				
				buttonContainer.RowsProportions.Add(new Proportion(ProportionType.Pixels,20));
				buttonContainer.Widgets.Add(ffmodebtn);
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
						SelectedControllable.DoAction(Action.Actions[ActionType.Crouch],null);
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
						Image = new TextureRegion(TextureManager.GetTexture("UI/"+act.Item1))
					};
					actBtn.Click += (o, a) => Action.SetActiveAction(act.Item2);
					actBtn.MouseEntered += (o, a) => SetPreviewDesc(Action.Actions[act.Item2].Description);
					buttonContainer.Widgets.Add(actBtn);
					column++;
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
		public static void Update(float deltatime)
		{
			var keyboardState = Keyboard.GetState();
			if (keyboardState.IsKeyDown(Keys.Tab) && lastState.IsKeyUp(Keys.Tab))
			{
				ffmode = !ffmode;
				ffmodebtn.Text = "FreeFire Mode: " + (ffmode ? "On" : "Off");
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

			lock (myrasyncobj)
			{
				UI.Desktop.Render();
			}

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
			//raycastdebug
			if (raycastDebug)
			{
				var templist = new List<RayCastOutcome>(WorldManager.Instance.RecentFOVRaycasts);
				if (SelectedControllable != null)
				{
					templist.Add(WorldManager.Instance.Raycast((Vector2) UI.SelectedControllable.worldObject.TileLocation.Position,(Vector2)UI.SelectedControllable.worldObject.TileLocation.Position + new Vector2(2,-1), Cover.Full));
					templist.Reverse();
				}

				count = 0;
				foreach (var cast in templist)
				{
				
					if (count > 20000)
					{
						break;
					}
					spriteBatch.DrawLine(Utility.GridToWorldPos(cast.StartPoint), Utility.GridToWorldPos(cast.EndPoint), Color.Green, 1);
					if (cast.hit)
					{
						count++;
					
						spriteBatch.DrawCircle(Utility.GridToWorldPos(cast.EndPoint), 5, 10, Color.Red, 5f);
						spriteBatch.DrawLine(Utility.GridToWorldPos(cast.EndPoint), Utility.GridToWorldPos(cast.CollisionPointLong), Color.Yellow, 2);
						spriteBatch.DrawLine(Utility.GridToWorldPos(cast.CollisionPointLong), Utility.GridToWorldPos(cast.CollisionPointLong) + (Utility.GridToWorldPos(cast.VectorToCenter) / 2f), Color.Red, 5);
					}
					else
					{
						spriteBatch.DrawCircle(Utility.GridToWorldPos(cast.EndPoint), 5, 10, Color.Green, 5f);
					}
				




					//foreach (var point in cast.CollisionPoint)
					//	{
					//		spriteBatch.DrawCircle(Utility.GridToWorldPos(point), 5, 10, Color.Green, 5f);
					//	}
				

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
			if (SelectedControllable != null && Action.GetActiveActionType() != null)
			{
				Action.ActiveAction.Preview(SelectedControllable, TileCoordinate,spriteBatch);
			}

			WorldEditSystem.Draw(spriteBatch);
			var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			spriteBatch.DrawString(Game1.SpriteFont,"X:"+MousePos.X+" Y:"+MousePos.Y,  Camera.GetMouseWorldPos(),Color.Wheat, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			spriteBatch.End();
		}

		
	
	}
}