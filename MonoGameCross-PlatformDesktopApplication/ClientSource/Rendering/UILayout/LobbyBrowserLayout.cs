using System;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class LobbyBrowserLayout : MenuLayout
{
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		
			var Grid = new Grid();
			Grid.Background = new TextureRegion(TextureManager.GetTexture("UI/background"));
			Grid.ColumnsProportions.Add(Proportion.Auto);
			Grid.ColumnsProportions.Add(Proportion.Fill);
			Grid.ColumnsProportions.Add(Proportion.Auto);
			var chatPanel = new Panel()
			{
				GridColumn = 0,
				GridRow = 0,
				Width = (int)(100*globalScale.X),
			};
			Grid.Widgets.Add(chatPanel);
		
			var input = new TextBox()
			{
				Width = (int)(100*globalScale.X),
				Height = 40,
				Top = 0,
				Left = 0,
				Text = "",
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Bottom,
				Font = DefaultFont.GetFont(20),
				Border = new SolidBrush(new Color(31,81,255,240)),
				BorderThickness = new Thickness(2)

			};
			input.KeyDown += (o, a) =>
			{
				if (a.Data == Keys.Enter)
				{
					if (input.Text != "")
					{
						if (Networking.Connected)
						{
							Networking.ChatMSG(input.Text);
						}
						else
						{
							//MasterServerNetworking.ChatMSG(input.Text);
						}


						input.Text = "";
					}
				}
			};
				
		

			var midPanel = new Panel()
			{
				GridColumn = 1,
				GridRow = 0,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			Grid.Widgets.Add(midPanel);
			var lobbyViewer = new ScrollViewer()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Stretch,

			};
			midPanel.Widgets.Add(lobbyViewer);

			var lobbies = new Grid()
			{
				VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Stretch,
					ColumnSpacing = 10,
					ShowGridLines = true,
					GridLinesColor = new Color(255,255,255,100), 
				//	GridRowSpan = 20
					//Padding = new Thickness(10),
					//Margin = new Thickness(10),
					

			};
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Pixels,(int)(80*globalScale.X)));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Pixels,(int)(60*globalScale.X)));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			lobbies.DefaultRowProportion = new Proportion(ProportionType.Auto);



			lobbies.Widgets.Add(new Label()
			{
				Text = "Name",
				GridRow = 0,
				GridColumn = 1,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left,
				Font = DefaultFont.GetFont(FontSize*0.65f)
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "Map",
				GridRow = 0,
				GridColumn = 2,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left,
				Font = DefaultFont.GetFont(FontSize*0.65f)
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "Players",
				GridRow = 0,
				GridColumn = 3,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left,
				Font = DefaultFont.GetFont(FontSize*0.65f)
			});
			lobbies.Widgets.Add(new Label()
			{
				Text = "State",
				GridRow = 0,
				GridColumn = 4,
				TextAlign = TextHorizontalAlignment.Left,
				HorizontalAlignment = HorizontalAlignment.Left,
				Font = DefaultFont.GetFont(FontSize*0.65f)
			});
			
			
			int row = 1;
			foreach (var lobby in MasterServerNetworking.Lobbies)
			{
								
				var lobbybtn = new TextButton()
				{
					Text = "Join",
					GridRow = row,
					Font = DefaultFont.GetFont(FontSize*0.6f),
					GridColumn =0,
					
				};

				lobbybtn.Click += (sender, args) =>
				{
					var ip = Game1.config.GetValue("config", "MasterServerIP", "");
					ip = ip.Substring(0, ip.LastIndexOf(':'));
					ip += ":" + lobby.Port;
					var result = Networking.Connect(ip, Game1.config.GetValue("config", "Name", "Operative#"+Random.Shared.Next(1000)));
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
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.Name,
					GridRow = row,
					GridColumn = 1,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left,
					Font = DefaultFont.GetFont(FontSize*0.4f),
					Margin = new Thickness(0,5,0,5)
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.MapName,
					GridRow = row,
					GridColumn = 2,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left,
					Font = DefaultFont.GetFont(FontSize*0.4f),
					Margin = new Thickness(0,5,0,5)
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.PlayerCount+"/2",
					GridRow = row,
					GridColumn = 3,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left,
					Font = DefaultFont.GetFont(FontSize*0.5f),
					Margin = new Thickness(0,5,0,5)
				});
				lobbies.Widgets.Add(new Label()
				{
					Text = lobby.GameState,
					GridRow = row,
					GridColumn = 4,
					TextAlign = TextHorizontalAlignment.Left,
					HorizontalAlignment = HorizontalAlignment.Left,
					Font = DefaultFont.GetFont(FontSize*0.5f),
					Margin = new Thickness(0,5,0,5)
				});

				row++;
			}
			lobbyViewer.Content = lobbies;

			var btnstack = new HorizontalStackPanel();
			btnstack.VerticalAlignment = VerticalAlignment.Bottom;
			btnstack.HorizontalAlignment = HorizontalAlignment.Center;
			btnstack.Spacing = 20;
			midPanel.Widgets.Add(btnstack);
			var btn = new TextButton()
			{
				Text = "Create Lobby",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
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

					MasterServerNetworking.CreateLobby(txt.Text);
				};
				
				dialog.ShowModal(desktop);
			};
		
			btnstack.Widgets.Add(btn);
			
			var btn2 = new SoundButton()
			{
				Text = "Refresh",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
			};
			btn2.Click += (sender, args) =>
			{
				MasterServerNetworking.RefreshServers();
			};
			btnstack.Widgets.Add(btn2);


			var rightpanel = new Panel()
			{
				GridColumn = 2,
				GridRow = 0,
			};
			Grid.Widgets.Add(rightpanel);
			var players = new VerticalStackPanel()
			{
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			var playerlbl2 = new Label()
			{
				Text = "Players Online",
				HorizontalAlignment = HorizontalAlignment.Center,
				Font = DefaultFont.GetFont(FontSize/1.5f)
		
			};
			players.Widgets.Add(playerlbl2);	
			foreach (var player in MasterServerNetworking.Players)
			{
				var playerlbl = new Label()
				{
					Text = player,
					HorizontalAlignment = HorizontalAlignment.Center,
					Font = DefaultFont.GetFont(FontSize/1.5f)
					
				};
				players.Widgets.Add(playerlbl);			
			}
			rightpanel.Widgets.Add(players);
			lobbyViewer.Content = lobbies;

			return Grid;
	}

	protected override void HandleMenuQuit()
	{
		base.HandleMenuQuit();
		MasterServerNetworking.Disconnect();
	}
}