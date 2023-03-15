using System;
using CommonData;
using FontStashSharp.RichText;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using MultiplayerXeno;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Network;

namespace MultiplayerXeno.UILayouts;

public class LobbyBrowserLayout : UiLayout
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
			};
			Grid.Widgets.Add(chatPanel);
			AttachSideChatBox(chatPanel);
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
					var result = Networking.Connect(ip, Game1.config.GetValue("config", "Name", "Operative#"+Random.Shared.Next(1000)));
					if (result == ConnectionResult.Connected)
					{
						UI.ShowMessage("Connection Notice", "Connected to server!");
						UI.SetUI(new PreGameLobbyLayout());
						DiscordManager.client.UpdateState("In Battle");
					}
					else
					{
						UI.ShowMessage("Connection Notice", "Failed to connect: " + result);

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
				
				dialog.ShowModal(desktop);
			};
		
			Grid.Widgets.Add(btn);
			
			var btn2 = new SoundButton()
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
			
			var btn3 = new SoundButton()
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
				UI.SetUI(new MainMenuLayout());
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

			return Grid;
	}
}