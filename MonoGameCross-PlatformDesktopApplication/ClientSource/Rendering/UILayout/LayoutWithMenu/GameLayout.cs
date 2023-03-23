using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MultiplayerXeno;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameLayout : MenuLayout
{
	private static Panel turnIndicator;
	private static Label scoreIndicator;
	
	public static void SetMyTurn(bool myTurn)
	{
		if (myTurn)
		{
			turnIndicator.Background = new SolidBrush(Color.Green);

		}
		else
		{
			turnIndicator.Background = new SolidBrush(Color.Red);
		}
	}
	public static void SetScore(int score)
	{
		scoreIndicator.Text = "score: " + score;
	}

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		var panel = new Panel ();
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
				Font = DefaultFont.GetFont(FontSize/2)
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

		AttachSideChatBox(panel);
		
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
			if (unit.Equals(UI.SelectedControllable))
			{
				unitPanel.Background = new SolidBrush(Color.DimGray);
				unitPanel.Top = 25;
			}

			unitPanel.TouchDown += (sender, args) =>
			{
				Console.WriteLine("select");
				if (unit.Health > 0)
				{
					UI.SelectControllable(unit);
				}
			};

			UnitContainer.Widgets.Add(unitPanel);
			var unitName = new Label()
			{
				Text = unit.Type.Name,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Center,
				Font = DefaultFont.GetFont(FontSize/2)
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
					indicators1.Add(UI.infoIndicator[0]);
				}
				else
				{
					indicators1.Add(UI.infoIndicator[1]);
				}

			}
			List<Texture2D> indicators3 = new List<Texture2D>();
			for (int i = 1; i <=  unit.Type.MaxFirePoints; i++)
			{
				
				if (unit.FirePoints < i)
				{
					indicators3.Add(UI.infoIndicator[4]);
				}
				else
				{
					indicators3.Add(UI.infoIndicator[5]);
				}

			}

			int xsize = Math.Clamp((int) (20 * globalScale.X), 0, 35);
			int ysize = Math.Clamp((int) (20 * globalScale.Y), 0, 35);
				
			int xpos = 0;
			int ypos = -ysize;
			List<List<Texture2D>> indicators = new List<List<Texture2D>>();
			indicators.Add(indicators1);
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
			
		return panel;
	}

	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		
		if (lastKeyboardState.IsKeyDown(Keys.Tab))
		{
			UI.Desktop.FocusedKeyboardWidget = null;//override myra focus switch functionality
		}
		if(UI.Desktop.FocusedKeyboardWidget != null) return;
		if (GameManager.MyUnits.Count != 0)
		{
			if (currentKeyboardState.IsKeyDown(Keys.E) && lastKeyboardState.IsKeyUp(Keys.E))
			{
				
				int fails = 0;
				do
				{
					var index = GameManager.MyUnits.FindIndex(i => i == UI.SelectedControllable) + 1;
					if (index >= GameManager.MyUnits.Count)
					{
						index = 0;
					}

					UI.SelectControllable(GameManager.MyUnits[index]);
					if(fails>GameManager.MyUnits.Count)
						break;
					fails++;
				} while (UI.SelectedControllable.Health <= 0);


			}
			if (currentKeyboardState.IsKeyDown(Keys.Q) && lastKeyboardState.IsKeyUp(Keys.Q))
			{
				int fails = 0;
				do
				{
					var index = GameManager.MyUnits.FindIndex(i => i == UI.SelectedControllable)-1;
					if (index < 0)
					{
						index = GameManager.MyUnits.Count-1;
					}
			
					UI.SelectControllable(GameManager.MyUnits[index]);
					if(fails>GameManager.MyUnits.Count)
						break;
					fails++;
				} while (UI.SelectedControllable.Health <= 0);

			}
		}
		if (currentKeyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
		{
			
			if (Attack.targeting == TargetingType.Auto)
			{
				Attack.targeting = TargetingType.High;
			}
			else if (Attack.targeting == TargetingType.High)
			{
				Attack.targeting = TargetingType.Low;
			}
			else if (Attack.targeting == TargetingType.Low)
			{
				Attack.targeting = TargetingType.Auto;
			}

		}
	}

	public override void MouseDown(Vector2Int position, bool righclick)
	{
		base.MouseDown(position, righclick);
		
		var Tile = WorldManager.Instance.GetTileAtGrid(position);

		WorldObject obj = Tile.ObjectAtLocation;
		if (obj!=null&&obj.ControllableComponent != null&& obj.GetMinimumVisibility() <= obj.TileLocation.Visible && Action.GetActiveActionType() == null) { 
			UI.SelectControllable(obj.ControllableComponent);
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
					UI.SelectedControllable?.DoAction(Action.ActiveAction,position);
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
					if (UI.SelectedControllable != null)
					{
						Action.SetActiveAction(ActionType.Move);
					}

					break;
				default:
					UI.SelectedControllable?.DoAction(Action.ActiveAction, position);
					break;


			}

		}
	}
}