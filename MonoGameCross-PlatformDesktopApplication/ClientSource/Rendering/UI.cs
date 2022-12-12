using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommonData;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MultiplayerXeno.Pathfinding;
using Myra;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Network;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop { get; private set; }
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		private static Texture2D[] coverIndicator = new Texture2D[8];
		private static Texture2D[] infoIndicator = new Texture2D[6];
		private static Texture2D[] healthIndicator = new Texture2D[2];
		
		public static readonly List<Controllable> Controllables = new List<Controllable>();


		public static void Init(ContentManager content, GraphicsDevice graphicsdevice)
		{
			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			MyraEnvironment.Game = Game1.instance;
			


			Desktop = new Desktop();
			Desktop.TouchDown += MouseDown;
			Desktop.TouchUp += MouseUp;


			Texture2D coverIndicatorSpriteSheet = content.Load<Texture2D>("textures/coverIndicator");
			coverIndicator = Utility.SplitTexture(coverIndicatorSpriteSheet, coverIndicatorSpriteSheet.Width / 3, coverIndicatorSpriteSheet.Width / 3);

			Texture2D indicatorSpriteSheet = content.Load<Texture2D>("textures/indicators");
			infoIndicator = Utility.SplitTexture(indicatorSpriteSheet, indicatorSpriteSheet.Width / 6, indicatorSpriteSheet.Height);

			Texture2D healthIndicatorSpriteSheet = content.Load<Texture2D>("textures/healthbar");
			healthIndicator = Utility.SplitTexture(healthIndicatorSpriteSheet, healthIndicatorSpriteSheet.Width / 2, healthIndicatorSpriteSheet.Height);

			previewMoves[0] = new List<Vector2Int>();
			previewMoves[1] = new List<Vector2Int>();
		}

		
		public static void RemakeUi(object sender, EventArgs e)
		{
			Desktop = new Desktop();
			Desktop.TouchDown += MouseDown;
			Desktop.TouchUp += MouseUp;
			SetUI(null);
		}

		public delegate void UIGen();

		private static UIGen currentUI;
		public static void SetUI(UIGen? uiMethod) {
			if (uiMethod != null)
			{
				currentUI = uiMethod;
			}
			currentUI.Invoke();

			Grid grid = (Grid)Desktop.Root;
				//	grid.ColumnSpacing = 10;
			foreach (var obj in grid.Widgets)
			{
				if (obj is TextButton)
				{
					obj.HorizontalAlignment = HorizontalAlignment.Center;
				}

				obj.Scale = new Vector2(Game1.instance.Window.ClientBounds.Width/1000f, Game1.instance.Window.ClientBounds.Width/1000f);
			}
			
		}

		public delegate void MouseClick(Vector2Int gridPos);

		public static event MouseClick RightClick;
		public static event MouseClick LeftClick;
		public static event MouseClick RightClickUp;
		public static event MouseClick LeftClickUp;

		private static MouseState lastState;

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

			lastState = mouseState;
		}

		public static void MouseUp(object? sender, EventArgs e)
		{
			if (UI.Desktop.IsMouseOverGUI)
			{
				return; //let myra do it's thing
			}

			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (lastState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
			{
				LeftClickUp?.Invoke(gridClick);
			}

			if (lastState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
			{
				RightClickUp?.Invoke(gridClick);
			}

		}





		public static void MainMenu()
		{
			var grid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8
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
				RowSpacing = 8,
				ColumnSpacing = 8
			};

			var textBox = new TextBox()
			{
				GridColumn = 1,
				GridRow = 1,
				Text = "localhost"
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
		public static void SetupUi()
		{


			var grid = new Grid
			{

				ColumnSpacing = 8,
				RowSpacing = 8,
				
			};

			
			//soldier counter
			var soldierButton = new TextButton
			{
			
				GridColumn = 2,
				GridRow = 1,
				Text = "Soldiers: 0"
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
				if (soldierCount + scoutCount + 1 > 6)
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
				GridRow = 2,
				Text = "Scouts: 0"
			};
			grid.Widgets.Add(scoutButton);
			var scountLeft = new TextButton
			{
				GridColumn = 1,
				GridRow = 2,
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
				GridRow = 2,
				Text = ">"
			};
			scoutRight.Click += (s, a) =>
			{
				if (soldierCount + scoutCount + 1 > 6)
				{
					return;
				}

				scoutCount++;
				scoutButton.Text = "Scouts: " + scoutCount;
			};
			grid.Widgets.Add(scoutRight);
			
			
			var confirm = new TextButton
			{
				GridColumn = 1,
				GridRow = 3,
				Text = "Confirm"
			};
			confirm.Click += (s, a) =>
			{
				StartDataPacket packet = new StartDataPacket();
				packet.Scouts = scoutCount;
				packet.Soldiers = soldierCount;
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
				RowSpacing = 8,
				ColumnSpacing = 8
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
			
			
			var grid = new Grid
			{
				RowSpacing = 8,
				ColumnSpacing = 8
			};

			var end = new TextButton
			{
				GridColumn = 8,
				GridRow = 0,
				Text = "End Turn"
			};
			end.Click += (o,a) => GameManager.EndTurn();		
			grid.Widgets.Add(end);
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
				GridColumn = 7,
				GridRow = 0,
				Background = new SolidBrush(Color.Red)
			};
			grid.Widgets.Add(turnIndicator);
			SetMyTurn(GameManager.IsMyTurn());

			if (scoreIndicator == null)
			{


				scoreIndicator = new Label()
				{
					GridColumn = 4,
					GridRow = 0,
					GridColumnSpan = 2,
				};
				SetScore(0);
			}

			grid.Widgets.Add(scoreIndicator);
	
			var PassThroughToggle = new TextButton
			{
				GridColumn = 7,
				GridRow = 8,
				Text = "TogglePassthrough"
			};
			PassThroughToggle.Click += (o, a) => { MousePassthrough = !MousePassthrough;};
			
			Desktop.Root = grid;





		}

		private static List<Vector2Int>[] previewMoves = new List<Vector2Int>[2];
		public static void UnitUI(WorldObject worldObject)
		{
			if (worldObject.ControllableComponent.IsMyTeam())
			{
				previewMoves = worldObject.ControllableComponent.GetPossibleMoveLocations();
				GameUi();
				var root = (Grid) Desktop.Root;


				var fire = new TextButton
				{
					GridColumn = 2,
					GridRow = 8,
					Text = "Fire"
				};
				fire.Click += (o, a) => Controllable.ToggleTarget();
				root.Widgets.Add(fire);
				var crouch = new TextButton
				{
					GridColumn = 3,
					GridRow = 8,
					Text = "Crouch/Stand"
				};
				crouch.Click += (o, a) =>
				{
					if (Controllable.Selected != null)
					{
						Controllable.Selected.CrouchAction();
					}
				};
				root.Widgets.Add(crouch);
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
			for (int i = 1; i <= 1; i++)
			{
				
				if (controllable.ActionPoints < i)
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
			
			for (int i = 0; i < controllable.Type.MaxAwareness; i++)
			{
				var indicator = healthIndicator[1];
				if (controllable.Type.MaxAwareness - i > controllable.Awareness)
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
		
		 static Vector2Int lastMousePos;
		static List<Vector2Int> previewPath = new List<Vector2Int>();
		private static Projectile previewShot = new Projectile(new Vector2Int(0,0),new Vector2Int(0,0),0,0);


		public static bool validShot;
		public static bool showPath = false;
		public static void Update(float deltatime)
		{
			if (Controllable.Selected != null)
			{
				Vector2Int currentPos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
				if (lastMousePos !=currentPos)
				{

					if (Controllable.Targeting)
					{
						bool lowShot = false;
						if (Controllable.Selected.Crouching)
						{
							lowShot = true;
						}else
						{
							WorldTile tile = WorldManager.Instance.GetTileAtGrid(currentPos);
							if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null && tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent.Crouching)
							{
								lowShot = true;
							}
						}

						Vector2 shotDir = Vector2.Normalize(currentPos - Controllable.Selected.worldObject.TileLocation.Position);
						previewShot = new Projectile(Controllable.Selected.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),currentPos+new Vector2(0.5f,0.5f),Controllable.Selected.Type.WeaponDmg,Controllable.Selected.Type.WeaponRange,lowShot);
						if (previewShot.result.hit && WorldManager.Instance.GetObject(previewShot.result.hitObjID)?.ControllableComponent != null)
						{
							validShot = true;
						}
						else
						{
							validShot = false;
						}

					}
					else
					{
						showPath = false;
						previewPath = PathFinding.GetPath(Controllable.Selected.worldObject.TileLocation.Position, currentPos).Path;
						if (previewPath == null)
						{
							previewPath = new List<Vector2Int>();
						}
					}

					
					lastMousePos = currentPos;
				}
			}
		}

		private static bool raycastDebug;
		public static void Render(float deltaTime)
		{
			
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			
			

			bool found = true;
			
			
			
			var Mousepos = Utility.GridToWorldPos((Vector2)TileCoordinate+new Vector2(-1.5f,-0.5f));

			
			UI.Desktop.Render();
			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred);
			
			
			
			
			

			if (WorldManager.IsPositionValid(TileCoordinate))
			{


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

			}

			
			foreach (var obj in Controllables)
			{
				if (obj.worldObject.IsVisible())
				{
					DrawControllableHoverHud(spriteBatch, obj);
				}
			}
			//raycastdebug
		int count;
		if (raycastDebug)
		{
			var templist = new List<RayCastOutcome>(WorldManager.Instance.RecentFOVRaycasts);
			if (Controllable.Selected != null)
			{
				templist.Add(WorldManager.Instance.Raycast((Vector2) Controllable.Selected.worldObject.TileLocation.Position,(Vector2)Controllable.Selected.worldObject.TileLocation.Position + new Vector2(2,-1), Cover.Full));
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
					spriteBatch.DrawLine(Utility.GridToWorldPos(cast.EndPoint), Utility.GridToWorldPos(cast.CollisionPoint), Color.Yellow, 2);
					spriteBatch.DrawLine(Utility.GridToWorldPos(cast.CollisionPoint), Utility.GridToWorldPos(cast.CollisionPoint) + (Utility.GridToWorldPos(cast.VectorToCenter) / 2f), Color.Red, 5);
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

			if (Controllable.Selected != null)
			{
				count = 0;
				foreach (var moves in previewMoves.Reverse())
				{
					foreach (var path in moves)
					{


						if (path.X < 0 || path.Y < 0) break;
						Mousepos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

						Color c = Color.White;
						switch (count)
						{
							case 0:
								c = Color.Red;
								break;
							case 1:
								c = Color.Yellow;
								break;
							default:
								c = Color.Green;
								break;

						}

						spriteBatch.DrawRectangle(Mousepos, new Size2(20, 20), c, 5);


					}

					count++;
				}
			

				if (Controllable.Targeting && previewShot!= null && previewShot.result!=null && Controllable.Selected != null)
				{
				
					List<WorldTile> tiles = WorldManager.Instance.GetTilesAround(new Vector2Int((int)previewShot.result.EndPoint.X, (int)previewShot.result.EndPoint.Y));
					foreach (var tile in tiles)
					{
						if (tile.Surface == null) continue;
							
						Texture2D sprite = tile.Surface.GetTexture();

						spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, Color.Cyan*0.3f);
					}
					
					var startPoint = Utility.GridToWorldPos(previewShot.result.StartPoint);
					var endPoint = Utility.GridToWorldPos(previewShot.result.EndPoint);

					Vector2 point1 = startPoint;
					Vector2 point2;
					int k = 0;
					var dmg = Controllable.Selected.Type.WeaponDmg;
					foreach (var dropOff in previewShot.dropOffPoints)
					{
						if (dropOff == previewShot.dropOffPoints.Last())
						{
							point2 = Utility.GridToWorldPos(previewShot.result.EndPoint);
							
						}
						else
						{
							point2 = Utility.GridToWorldPos(dropOff);
						}

						Color c;
						switch (k)
						{
							case 0:
								c = Color.DarkGreen;
								break;
							case 1:
								c = Color.Orange;
								break;
							case 2:
								c = Color.DarkRed;
								break;
							default:
								c = Color.Purple;
								break;

						}
						
						spriteBatch.DrawString(Game1.SpriteFont,"Damage: "+dmg,  point1,c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
						spriteBatch.DrawLine(point1.X,point1.Y,point2.X,point2.Y,c,25);
						dmg = (int)Math.Ceiling(dmg/2f);
						k++;
						point1 = point2;
					
							
						
					}
					
				
					spriteBatch.DrawLine(startPoint.X,startPoint.Y,endPoint.X,endPoint.Y,Color.White,15);
					int coverModifier = 0;
					
					var hitobj = WorldManager.Instance.GetObject(previewShot.result.hitObjID);
					if (previewShot.covercast != null && previewShot.covercast.hit)
					{
						Color c = Color.Green;
						string hint = "";
						var coverPoint = Utility.GridToWorldPos(previewShot.covercast.CollisionPoint);
						Cover cover = WorldManager.Instance.GetObject(previewShot.covercast.hitObjID).GetCover();
						if (hitobj?.ControllableComponent != null && hitobj.ControllableComponent.Crouching)
						{
							if (cover != Cover.Full)
							{ 
								cover++;
							}
						}

						switch (cover)
						{
							case Cover.None:
								c = Color.Green;
								Console.WriteLine("How: Cover object has no cover");
								break;
							case Cover.Low:
								c = Color.Gray;
								coverModifier = 1;
								hint = "Cover: -1 DMG";
								break;
							case Cover.High:
								c = Color.Black;
								coverModifier = 2;
								hint = "Cover: -2 DMG";
								break;
							case Cover.Full:
								c = Color.Black;
								coverModifier = 10;
								hint = "Full Cover: -10 DMG";
								break;
							default:
							
								break;

						}
						spriteBatch.DrawString(Game1.SpriteFont,hint, coverPoint+new Vector2(1f,1f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
						spriteBatch.DrawLine(coverPoint.X,coverPoint.Y,endPoint.X,endPoint.Y,c,9);
						spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.StartPoint), 15, 10, Color.Red, 25f);
						
						var coverobj = WorldManager.Instance.GetObject(previewShot.covercast.hitObjID);
						var coverobjtransform = coverobj.Type.Transform;
						Texture2D yellowsprite = coverobj.GetTexture();

						spriteBatch.Draw(yellowsprite, coverobjtransform.Position + Utility.GridToWorldPos(coverobj.TileLocation.Position), Color.Yellow);
						//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
						spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.CollisionPoint), 15, 10, Color.Yellow, 25f);

					}
				
					
						
					if (hitobj != null)
					{
						var transform = hitobj.Type.Transform;
						Texture2D redSprite = hitobj.GetTexture();
						

						spriteBatch.Draw(redSprite, transform.Position + Utility.GridToWorldPos(hitobj.TileLocation.Position), Color.Red);
						spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.result.CollisionPoint), 15, 10, Color.Red, 25f);
						//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
						if (hitobj.ControllableComponent != null)
						{
							if (hitobj.ControllableComponent.Awareness > 0)
							{
								spriteBatch.DrawString(Game1.SpriteFont, "Final Damage: " + (previewShot.dmg - coverModifier) / 2 + "(Saved By Awareness)", Utility.GridToWorldPos(previewShot.result.CollisionPoint + new Vector2(-0.5f, -0.5f)), Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
							}
							else
							{
								spriteBatch.DrawString(Game1.SpriteFont, "Final Damage: " + (previewShot.dmg - coverModifier), Utility.GridToWorldPos(previewShot.result.CollisionPoint + new Vector2(-0.5f, -0.5f)), Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
							}
						}
					}

					
						
		


				}
				else
				{
					if(showPath){
						foreach (var path in previewPath)
						{
					
							if(path.X < 0 || path.Y < 0) break;
							Mousepos = Utility.GridToWorldPos((Vector2)path + new Vector2(0.5f,0.5f));
				
							spriteBatch.DrawCircle(Mousepos,20,10,Color.Green,20f);
				
				
						}
				
					}
				}

			}

			WorldEditSystem.Draw(spriteBatch);
			
			spriteBatch.End();
		}

		
	
	}
}