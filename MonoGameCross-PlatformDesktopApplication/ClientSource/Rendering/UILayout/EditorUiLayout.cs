using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiplayerXeno;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class EditorUiLayout : MenuLayout
{


	private static List<WorldObjectData>[,] buffer;

	public EditorUiLayout()
	{
		DiscordManager.Client.UpdateState("In Level Editor");
		WorldManager.Instance.CurrentMap = new MapData();
		WorldManager.Instance.CurrentMap.Name = "Unnamed";
		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				WorldManager.Instance.MakeWorldObject("basicFloor",new Vector2Int(x,y),ActiveDir);
			}	
		}
	}

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
	
		WorldManager.Instance.MakeFovDirty(true);
		GameManager.GameState = GameState.Editor;
		var panel = new Panel();

		var stack = new VerticalStackPanel();
		stack.HorizontalAlignment = HorizontalAlignment.Left;
		panel.Widgets.Add(stack);
		foreach (var prefabDictElement in PrefabManager.WorldObjectPrefabs)
		{
			var button = new TextButton
			{
				Text = prefabDictElement.Key
			};

			button.Click += (s, a) =>
			{ 
				ActivePrefab = prefabDictElement.Key;
			};
			stack.Widgets.Add(button);
				
				

		}

		var save = new TextButton();
		save.Text = "save";
		save.HorizontalAlignment = HorizontalAlignment.Right;
		save.VerticalAlignment = VerticalAlignment.Top;
		save.Click += (s, a) =>
		{ 			
			var panel = new Panel();
			var stack = new VerticalStackPanel();
			panel.Widgets.Add(stack);
			stack.Spacing = 25;
			var label = new Label()
			{
				Text = "Enter Map Name",
				Top = 0,
			};
			stack.Widgets.Add(label);
				
			var mapname = new TextBox()
			{
				Top = 25,
			};
			mapname.Text = WorldManager.Instance.CurrentMap.Name;
			stack.Widgets.Add(mapname);
			label = new Label()
			{
				Text = "Author Name",
				Top = 50
			};
			stack.Widgets.Add(label);
			var authorname = new TextBox()
			{
				Top = 75,
				Text = WorldManager.Instance.CurrentMap.Author,
			};
			stack.Widgets.Add(authorname);
			label = new Label()
			{
				Text = "Unit Count",
				Top = 100,
			};
			stack.Widgets.Add(label);
			var unitCount = new TextBox()
			{
				Top = 125,
				Text = WorldManager.Instance.CurrentMap.unitCount.ToString(),
			};
			stack.Widgets.Add(unitCount);
				
				
				
			var dialog = Dialog.CreateMessageBox("Save Map", panel);
			dialog.Width = (int)(450f*globalScale.X);
			dialog.Height = (int) (500f * globalScale.Y);
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
			dialog.ShowModal(desktop);
				
		};

		var load = new TextButton();
		load.Text = "load";
		load.HorizontalAlignment = HorizontalAlignment.Right;
		load.VerticalAlignment = VerticalAlignment.Top;
		load.Left = (int)(-100*globalScale.X);

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
			selection.Top = (int)(30*globalScale.X);
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
			dialog.Width = (int)(450f*globalScale.X);
			dialog.Height = (int) (500f * globalScale.Y);
			dialog.ButtonOk.Click += (sender, args) =>
			{
				WorldManager.Instance.LoadMap("./Maps/"+selection.SelectedItem.Text+".mapdata");
			};
			dialog.ShowModal(desktop);
				
		};
		panel.Widgets.Add(save);
		panel.Widgets.Add(load);

		var optionstack = new HorizontalStackPanel();
		optionstack.HorizontalAlignment = HorizontalAlignment.Center;
		optionstack.VerticalAlignment = VerticalAlignment.Bottom;
		optionstack.Spacing = 10;
		panel.Widgets.Add(optionstack);
		var rotateq = new TextButton
		{
			Text = "<- Rotate(Q)"
		};
		rotateq.Click += (s, a) =>
		{
			ActiveDir--;
		};
		optionstack.Widgets.Add(rotateq);
		var point = new TextButton();
		point.Text = "Point(1)";
		point.Click += (s, a) =>
		{
			ActiveBrush = Brush.Point;
		};
		optionstack.Widgets.Add(point);
		var selection = new TextButton
		{
			Text = "Selection(2)"
		};
		selection.Click += (s, a) =>
		{
			ActiveBrush = Brush.Selection;
		};
		optionstack.Widgets.Add(selection);
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

		var rotatee = new TextButton
		{
			Text = "Rotate(E) -->"
		};
		rotatee.Click += (s, a) =>
		{
			ActiveDir++;
		};
		optionstack.Widgets.Add(rotatee);



		return panel;
	}

	private static bool lastMouseDown;
	private static bool lastLeftMouseDown;
	private static bool lastRghtMouseDown;
	private static Vector2Int lastMousePos;
	private static bool rightMouseDown;
	private static bool leftMouseDown;

	private static bool IsMouseDown => rightMouseDown || leftMouseDown;

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);
		if (rightclick)
		{
			rightMouseDown = true;
		}
		else
		{
			leftMouseDown = true;
		}
	}

	public override void MouseUp(Vector2Int position, bool righclick)
	{
		base.MouseUp(position, righclick);
		if (righclick)
		{
			rightMouseDown = false;
		}
		else
		{
			leftMouseDown = false;
		}
	}

	public enum Brush 
	{
		Point,
		Selection,
		Copy,
		Paste
			
	}
	
	public static Brush ActiveBrush = Brush.Point;
	public static string ActivePrefab = "basicFloor";
	public static Direction ActiveDir = Direction.North;
	
	
	private static Vector2Int startPoint = new Vector2Int(0,0);
	private static Vector2Int currentPoint = new Vector2Int(0,0);
	private static Vector2Int topLeftSelection = new Vector2Int(0,0);
	private static Vector2Int bottomRightSelection = new Vector2Int(0,0);
	private static bool IsValidPlacement(Vector2Int pos, string prefab, Direction dir)
	{
		if (!WorldManager.IsPositionValid(pos))
		{
			return false;
		}

		var tile = WorldManager.Instance.GetTileAtGrid(pos);
		WorldTile tile2;
		WorldObjectType type = PrefabManager.WorldObjectPrefabs[prefab];
		if (type.Surface)
		{
			return tile.Surface == null;
		}

		if (type.Edge)
		{
			switch (dir)
			{
				case Direction.North:
					return tile.NorthEdge == null;
				case Direction.West:
					return tile.WestEdge == null;
				case Direction.East:
					tile2 = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.East));
					return tile2.WestEdge == null;
				case Direction.South:
					tile2 = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.South));
					return tile2.NorthEdge == null;
				default:
					return false;
			}
		}

		return tile.ControllableAtLocation == null;
	}

	private float timeUntilNextSave = 30000;
	public override void Update(float deltatime)
	{
		base.Update(deltatime);
//		WorldManager.Instance.MakeFovDirty(true);
		timeUntilNextSave -= deltatime;
		
		if(timeUntilNextSave<= 0)
		{
			timeUntilNextSave = 30000;
			
			WorldManager.Instance.SaveCurrentMapTo("./Maps/autosaves/"+WorldManager.Instance.CurrentMap.Name+".mapdata");
		}
		
		if (JustPressed(Keys.E))
		{
			if (ActiveBrush == Brush.Paste)
			{
				var inputWidth = buffer.GetLength(0);
				var inputHeight = buffer.GetLength(1);

				// We swap the sizes because rotating a 3x4 yields a 4x3.
				var output =   new List<WorldObjectData>[inputHeight, inputWidth];

				var maxHeight = inputHeight - 1;

				for (int j = 0; j < output.GetLength(1); j++)
				{
					for (int i = 0; i < output.GetLength(0); i++)
					{
						output[i,j] = new List<WorldObjectData>();
					}
				}

				for (int j = 0; j < output.GetLength(1); j++)
				{
					for (int i = 0; i < output.GetLength(0); i++)
					{
						foreach (var data in buffer[j, maxHeight - i])
						{

							var worldObjectData = data;
							if (worldObjectData.Facing == Direction.North && worldObjectData.Fliped)
							{
								worldObjectData.Fliped = false;
								worldObjectData.Facing = Direction.West;
								if(i+1 <  output.GetLength(0))
									output[i+1, j].Add(worldObjectData);
									
							}
							else
							{
								worldObjectData.Facing += 2;
								worldObjectData.Facing = Utility.ClampFacing(worldObjectData.Facing);
								output[i, j].Add(worldObjectData);
							}

								
							
						}
					}
				}
				buffer = output;
			}
			else
			{
				ActiveDir += 1;
			}
			
		} else if (JustPressed(Keys.Q))
		{
			if(ActiveBrush == Brush.Paste)
			{
				var inputWidth = buffer.GetLength(0);
				var inputHeight = buffer.GetLength(1);

				// We swap the sizes because rotating a 3x4 yields a 4x3.
				var output = new List<WorldObjectData>[inputHeight, inputWidth];

				var maxWidth = inputWidth - 1;
				for (int j = 0; j < output.GetLength(1); j++)
				{
					for (int i = 0; i < output.GetLength(0); i++)
					{
						output[i,j] = new List<WorldObjectData>();
					}
				}

				for (int j = 0; j < output.GetLength(1); j++)
				{
					for (int i = 0; i < output.GetLength(0); i++)
					{
						foreach (var data in buffer[maxWidth - j, i])
						{
							
							var worldObjectData = data;
							worldObjectData.Facing-= 2;
							worldObjectData.Facing = Utility.ClampFacing(worldObjectData.Facing);
							output[i, j].Add(worldObjectData);
							
						}
					}
				}
			
				buffer = output;	
			}
			else
			{
				ActiveDir -= 1;
			}
		}

		if (JustPressed(Keys.F))
		{
			var inputWidth = buffer.GetLength(0);
			var inputHeight = buffer.GetLength(1);

			// We swap the sizes because rotating a 3x4 yields a 4x3.
			var output = new List<WorldObjectData>[inputWidth, inputHeight];

			var maxWidth = inputWidth - 1;

			for (int j = 0; j < output.GetLength(1); j++)
			{
				for (int i = 0; i < output.GetLength(0); i++)
				{
					output[i,j] = new List<WorldObjectData>();
				}
			}

			for (int j = 0; j < output.GetLength(1); j++)
			{
				for (int i = 0; i < output.GetLength(0); i++)
				{
					foreach (var data in  buffer[maxWidth-i, j])
					{

							var worldObjectData = data;
							if (worldObjectData.Facing == Direction.West || worldObjectData.Facing == Direction.East)
							{
								if (worldObjectData.Fliped)
								{
									worldObjectData.Fliped = false;
									if(i+1 <  output.GetLength(0))
										output[i+1, j].Add(worldObjectData);
									continue;
								}
							
								worldObjectData.Facing += 4;
								

								//worldObjectData.Fliped = !worldObjectData.Fliped;
								worldObjectData.Facing = Utility.ClampFacing(worldObjectData.Facing);
							}

							output[i, j].Add( worldObjectData);
						
						
					}
				}
			}
			
			buffer = output;	
			
		}

		if(currentKeyboardState.IsKeyDown(Keys.D1))
		{
			ActiveBrush = Brush.Point;
		}
		if(currentKeyboardState.IsKeyDown(Keys.D2))
		{
			ActiveBrush = Brush.Selection;
		}
		if(JustPressed(Keys.C) && currentKeyboardState.IsKeyDown(Keys.LeftControl))
		{
			ActiveBrush = Brush.Copy;
		}
		else if(JustPressed(Keys.V) && currentKeyboardState.IsKeyDown(Keys.LeftControl))
		{
			ActiveBrush = Brush.Paste;
		}
		
		ActiveDir = Utility.NormaliseDir(ActiveDir);


		Vector2Int mousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		
		
		switch (ActiveBrush)
		{
			case Brush.Point:
				if (leftMouseDown && IsValidPlacement(mousePos,ActivePrefab,ActiveDir))
				{
					WorldManager.Instance.MakeWorldObject(ActivePrefab,mousePos,ActiveDir);
				}else if ((rightMouseDown && !lastRghtMouseDown)||(rightMouseDown && lastMousePos!=mousePos))
				{
					DeletePrefab(mousePos);

				}

				break;
			case Brush.Copy:
			case Brush.Selection:
				if (IsMouseDown && !lastMouseDown)
				{
					startPoint = mousePos;
					break;
				}

				currentPoint = mousePos;
				if (startPoint.X > currentPoint.X)
				{
					topLeftSelection.X = currentPoint.X;
					bottomRightSelection.X = startPoint.X;

				}
				else
				{
					topLeftSelection.X = startPoint.X;
					bottomRightSelection.X = currentPoint.X;
				}
				if (startPoint.Y > currentPoint.Y)
				{
					topLeftSelection.Y = currentPoint.Y;
					bottomRightSelection.Y = startPoint.Y;

				}
				else
				{
					topLeftSelection.Y = startPoint.Y;
					bottomRightSelection.Y = currentPoint.Y;
				}
				if (lastRghtMouseDown && !rightMouseDown)
				{


					for (int x = topLeftSelection.X; x <= bottomRightSelection.X; x++)
					{
						for (int y = topLeftSelection.Y; y <= bottomRightSelection.Y; y++)
						{
							if (ActiveBrush == Brush.Selection)
							{
								DeletePrefab(new Vector2Int(x, y));
							}
						}
					}
				}else if (lastLeftMouseDown && !leftMouseDown)
				{
					if (ActiveBrush == Brush.Copy)
					{
						buffer = new List<WorldObjectData>[bottomRightSelection.X - topLeftSelection.X+1,bottomRightSelection.Y - topLeftSelection.Y+1];
					}

					for (int x = topLeftSelection.X; x <= bottomRightSelection.X; x++)
					{
						for (int y = topLeftSelection.Y; y <= bottomRightSelection.Y; y++)
						{
							if (ActiveBrush == Brush.Selection)
							{
								if (IsValidPlacement(new Vector2Int(x, y),ActivePrefab,ActiveDir))
								{
									WorldManager.Instance.MakeWorldObject(ActivePrefab, new Vector2Int(x, y), ActiveDir);
								}
							}else if (ActiveBrush == Brush.Copy)
							{
								WorldTile tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
								var data = tile.GetData();
								buffer[x - topLeftSelection.X, y - topLeftSelection.Y] = new List<WorldObjectData>();
								if (data.NorthEdge != null)
								{
									var worldObjectData = data.NorthEdge.Value;
									worldObjectData.Id = -1;
									buffer[x - topLeftSelection.X, y - topLeftSelection.Y].Add(worldObjectData);
								}
								
								if (data.WestEdge != null)
								{
									var worldObjectData = data.WestEdge.Value;
									worldObjectData.Id = -1;
									buffer[x - topLeftSelection.X, y - topLeftSelection.Y].Add(worldObjectData);
								}

								if (data.Surface != null)
								{
									var worldObjectData = data.Surface.Value;
									worldObjectData.Id = -1;
									buffer[x - topLeftSelection.X, y - topLeftSelection.Y].Add(worldObjectData);
								}
							}


						}
					}
					if(ActiveBrush == Brush.Copy)
					{
						ActiveBrush = Brush.Paste;
					}
				}
				break;
			case Brush.Paste:
				if (leftMouseDown && !lastLeftMouseDown)
				{
					if (buffer != null)
						for (int x = 0; x < buffer.GetLength(0); x++)
						{
							for (int y = 0; y < buffer.GetLength(1); y++)
							{
								if (!WorldManager.IsPositionValid(mousePos+new Vector2Int(x,y)))
								{
									continue;
									
								}

								var tileData = buffer[x, y];
								var pos = new Vector2Int(x,y) + mousePos;
								WorldManager.Instance.GetTileAtGrid(pos).Wipe();
								
								foreach (var data in tileData)
								{
									WorldManager.Instance.MakeWorldObjectFromData(data,	WorldManager.Instance.GetTileAtGrid(pos));
								}

							}
						}

					ActiveBrush = Brush.Point;
					leftMouseDown = false;
				}
	
				break;
		}
		//Console.WriteLine(MouseDown+" "+lastMouseDown);
		lastLeftMouseDown = leftMouseDown;
		lastMouseDown = IsMouseDown;
		lastRghtMouseDown = rightMouseDown;
		lastMousePos = mousePos;
	}
	private static void DeletePrefab(Vector2Int Pos)
	{
		if (!WorldManager.IsPositionValid(Pos))
		{
			return;
		}

		WorldTile tile = WorldManager.Instance.GetTileAtGrid(Pos);

		if (tile.ControllableAtLocation != null)
		{
			WorldManager.Instance.DeleteWorldObject(tile.ControllableAtLocation);
			return;
		}
		if (tile.NorthEdge != null)
		{
			WorldManager.Instance.DeleteWorldObject(tile.NorthEdge);
			return;
		}
		if (tile.WestEdge != null)
		{
			WorldManager.Instance.DeleteWorldObject(tile.WestEdge);
			return;
		}
		if(Pos.Y != 99){
			WorldTile southTile = WorldManager.Instance.GetTileAtGrid(Pos+Utility.DirToVec2(Direction.South));
			if (southTile.NorthEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(southTile.NorthEdge);
				return;
			}
		}

		if (Pos.X != 99)
		{
			WorldTile eastTile = WorldManager.Instance.GetTileAtGrid(Pos+Utility.DirToVec2(Direction.East));
			if (eastTile.WestEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(eastTile.WestEdge);
				return;
			}
		}

		


		if (tile.Surface != null)
		{
			WorldManager.Instance.DeleteWorldObject(tile.Surface);
			return;
		}




	}


	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		//	Console.WriteLine(Camera.GetMouseWorldPos());
		batch.DrawString(Game1.SpriteFont,"X:"+MousePos.X+" Y:"+MousePos.Y +" Mode: "+ActiveBrush,  Camera.GetMouseWorldPos(),Color.Wheat, 0, Vector2.Zero, 2/Camera.GetZoom(), new SpriteEffects(), 0);
		batch.DrawLine(Utility.GridToWorldPos(new Vector2(MousePos.X, 0)), Utility.GridToWorldPos(new Vector2(MousePos.X, 100)), Color.White, 5);
		batch.DrawLine(Utility.GridToWorldPos(new Vector2(MousePos.X+1, 0)), Utility.GridToWorldPos(new Vector2(MousePos.X+1, 100)), Color.White, 5);
		batch.DrawLine(Utility.GridToWorldPos(new Vector2(0,  MousePos.Y)), Utility.GridToWorldPos(new Vector2(100, MousePos.Y)), Color.White, 5);
		batch.DrawLine(Utility.GridToWorldPos(new Vector2(0,  MousePos.Y+1)), Utility.GridToWorldPos(new Vector2(100, MousePos.Y+1)), Color.White, 5);
		switch (ActiveBrush)
		{
			case Brush.Copy:
			case Brush.Selection:
				if (IsMouseDown)
				{
					batch.DrawLine(Utility.GridToWorldPos(new Vector2(topLeftSelection.X, topLeftSelection.Y)), Utility.GridToWorldPos(new Vector2(topLeftSelection.X, bottomRightSelection.Y + 1)), Color.Peru, 5);
					batch.DrawLine(Utility.GridToWorldPos(new Vector2(topLeftSelection.X, topLeftSelection.Y)), Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, topLeftSelection.Y)), Color.Peru, 5);
					batch.DrawLine(Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, bottomRightSelection.Y + 1)), Utility.GridToWorldPos(new Vector2(topLeftSelection.X, bottomRightSelection.Y + 1)), Color.Peru, 5);
					batch.DrawLine(Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, bottomRightSelection.Y + 1)), Utility.GridToWorldPos(new Vector2(bottomRightSelection.X + 1, topLeftSelection.Y)), Color.Peru, 5);
				}
				break;
			case Brush.Paste:
				for (int x = 0; x < buffer.GetLength(0); x++)
				{
					for (int y = 0; y < buffer.GetLength(1); y++)
					{
						var tileData = buffer[x, y];
						var pos = new Vector2Int(x, y) + MousePos;
						foreach (var data in tileData)
						{
							batch.DrawPrefab(Utility.GridToWorldPos(pos), data.Prefab, data.Facing,data.Fliped);
						}
					}
				}
				
				break;
		}

		if (ActiveBrush == Brush.Selection || ActiveBrush == Brush.Point)
		{
			batch.DrawPrefab(Utility.GridToWorldPos(MousePos),ActivePrefab,ActiveDir);
		}

		batch.End();
		
	}


}