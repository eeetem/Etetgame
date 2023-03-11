using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameLayout : UiLayout
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
	public override Widget Generate(Desktop desktop)
	{
		Texture2D indicatorSpriteSheet = TextureManager.GetTexture("UI/indicators");
		var infoIndicator = Utility.SplitTexture(indicatorSpriteSheet, indicatorSpriteSheet.Width / 6, indicatorSpriteSheet.Height);//this is inneficient but it'll be gone once new unit bar gets made
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
						indicators1.Add(infoIndicator[0]);
					}
					else
					{
						indicators1.Add(infoIndicator[1]);
					}

				}
				List<Texture2D> indicators3 = new List<Texture2D>();
				for (int i = 1; i <=  unit.Type.MaxFirePoints; i++)
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
}