
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MultiplayerXeno;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Network.Converter;

namespace MultiplayerXeno.UILayouts;

public class PreGameLobbyLayout : UiLayout
{
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		
			var grid1 = new Grid()
			{

			};
			grid1.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
			grid1.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
			grid1.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

			if (GameManager.PreGameData == null) return grid1;

			var rightStack = new VerticalStackPanel()
			{
				GridColumn = 2,
				GridRow = 0,
				Spacing = 0,
				Top = 0,
				Left = 0,
				Background = new SolidBrush(Color.Black),
				Width = (int) (140 * globalScale.X),
				Height = (int) (800 * globalScale.Y),
				Margin = new Thickness(0),
				Padding = new Thickness(0)
			};
			grid1.Widgets.Add(rightStack);
			var middlePanel = new Panel()
			{
				GridColumn = 1,
				GridRow = 0,
				Top = 0,
				Left = 0,
				Background = new SolidBrush(Color.Black),
				Width = (int) (120 * globalScale.X),
				Height = (int) (1000 * globalScale.Y),
				Border = new SolidBrush(new Color(31,81,255,240)),
				BorderThickness = new Thickness(2)
			};
			grid1.Widgets.Add(middlePanel);
			var leftpanel = new Panel()
			{
				GridColumn = 0,
				GridRow = 0,
				Top = 0,
				Left = 0,
				Background = new SolidBrush(Color.Black),
				Width = (int) (600 * globalScale.X),
				Height = (int) (200 * globalScale.Y),
				VerticalAlignment = VerticalAlignment.Bottom

			};
			grid1.Widgets.Add(leftpanel);



			var btn = new TextButton()
			{
				Text = "GO",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Top = 0,
				Width = (int) (200 * globalScale.X),
				Height = (int) (100 * globalScale.Y),
				Background = new TextureRegion(TextureManager.GetTexture("UI/button")),
				OverBackground = new TextureRegion(TextureManager.GetTexture("UI/button")),
				//	Font =DefaultFont.GetFont(50)

			};
			if (!GameManager.IsPlayer1)
			{
				btn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/button")), Color.DimGray);
				btn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/button")), Color.DimGray);
			}

			btn.Click += (s, a) => { Networking.SendStartGame(); };
			rightStack.Widgets.Add(btn);
			var linebreak = new Label()
			{
				Text = "______________",
				HorizontalAlignment = HorizontalAlignment.Center,
				Width = (int) (400 * globalScale.X),
			};
			rightStack.Widgets.Add(linebreak);
			var options = new TextButton()
			{
				Text = "Game Options",
				Width = (int) (400 * globalScale.X),
				Height = (int) (100 * globalScale.Y),
				Padding = Thickness.Zero,
				Margin = Thickness.Zero,

			};
			rightStack.Widgets.Add(options);
			linebreak = new Label()
			{
				Text = "______________",
				HorizontalAlignment = HorizontalAlignment.Center,
				Width = (int) (400 * globalScale.X),
			};
			rightStack.Widgets.Add(linebreak);


			var lablekick = new HorizontalStackPanel();
			lablekick.HorizontalAlignment = HorizontalAlignment.Center;
			rightStack.Widgets.Add(lablekick);
			

			var label = new Label()
			{
						Text = "Opponent:",
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center,
					//	Height = (int)(80*globalScale.Y),
						Background = new SolidBrush(Color.Black),
			};
		
					var kick = new ImageButton()
					{
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Right,
						Left = -10,
						Width = 50,
						Height = 50,
						Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/kick")),Color.White),
					};
					kick.Click += (s, a) =>
					{
						Networking.serverConnection.SendRawData(RawDataConverter.FromBoolean("kick",true));
					};
					lablekick.Widgets.Add(kick);
					lablekick.Widgets.Add(label);
					string oponentName = GameManager.PreGameData.Player2Name;
				if (!GameManager.IsPlayer1)
				{
					oponentName = GameManager.PreGameData.HostName;
				}

				var namelable = new Label()
				{
					Text = oponentName,
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					Background = new SolidBrush(Color.Black),
					Top = 0,
					Wrap = true
				};
				rightStack.Widgets.Add(namelable);
				


				linebreak = new Label()
				{
					Text = "______________",
					HorizontalAlignment = HorizontalAlignment.Center,
					Width = (int)(400*globalScale.X),
				};
				rightStack.Widgets.Add(linebreak);

				
				var spectatorViewer = new ScrollViewer();
				var spectatorList = new VerticalStackPanel();
				spectatorViewer.Content = spectatorList;
				spectatorViewer.Height = (int) (80 * globalScale.Y);

				var lbl = new Label()
				{
					Text = "SPECTATORS:",
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
					Background = new SolidBrush(Color.Black),
					Top = 0,
					Wrap = true
				};
				rightStack.Widgets.Add(lbl);
				foreach (var spectator in GameManager.PreGameData.Spectators)
				{
					var spec = new Label()
					{
						Text = spectator,
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Center,
						Background = new SolidBrush(Color.Black),
						Top = 0,
						
						Wrap = true
					};
					spectatorList.Widgets.Add(spec);
				}
		
				rightStack.Widgets.Add(spectatorViewer);
				var chatViewer = new ScrollViewer();
				chatViewer.VerticalAlignment = VerticalAlignment.Bottom;
				chatViewer.Top = -40;
				AddChatBoxToViewer(chatViewer);
				middlePanel.Widgets.Add(chatViewer);
				
			

				var input = new TextBox()
				{
					Width = (int)(145*globalScale.X),
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
					Width = 50,
					Height = 25,
					Top = 0,
					Left = 0,
					Text = "Send",
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Bottom,
					Font = DefaultFont.GetFont(15)
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
				middlePanel.Widgets.Add(input);
				middlePanel.Widgets.Add(inputbtn);
				
				
				///LEFT STACK///
				var officialSelection = new ListBox()
				{
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Bottom
				};
				officialSelection.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(20);
				foreach (var path in GameManager.MapList)
				{
					var item = new ListItem()
					{
						Text = path.Key,
					};
					officialSelection.Items.Add(item);
				}
				
				officialSelection.SelectedIndexChanged += (s, a) =>
				{
					GameManager.PreGameData.SelectedMap = GameManager.MapList[officialSelection.Items[(int) officialSelection.SelectedIndex].Text];
					Networking.SendPreGameUpdate();
				};
				var communitySelection = new ListBox()
				{
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalAlignment = VerticalAlignment.Bottom
				};
				communitySelection.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(20);
				foreach (var path in GameManager.CustomMapList)
				{
					var item = new ListItem()
					{
						Text = path.Key,
						
					};
					
					communitySelection.Items.Add(item);
				}
				communitySelection.SelectedIndexChanged += (s, a) =>
				{
					GameManager.PreGameData.SelectedMap = GameManager.CustomMapList[communitySelection.Items[(int) communitySelection.SelectedIndex].Text];
					Networking.SendPreGameUpdate();
				};

				var tab1 = new TextButton()
				{
					Text = "Official",
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top,
					Border = new SolidBrush(new Color(31,81,255,240)),
					BorderThickness = new Thickness(1)
				};
				var tab2 = new TextButton()
				{
					Text = "Community",
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Top,
					Border = new SolidBrush(new Color(31,81,255,240)),
					BorderThickness = new Thickness(1)
				};
				var tab3 = new TextButton()
				{
					Text = "Local",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Top,
					Left = -12,
					Border = new SolidBrush(new Color(31,81,255,240)),
					BorderThickness = new Thickness(1)
				};
				leftpanel.Widgets.Add(tab1);
				leftpanel.Widgets.Add(tab2);
				leftpanel.Widgets.Add(tab3);
				leftpanel.Widgets.Add(officialSelection);
				tab1.Click += (i, a) =>
				{
					leftpanel.Widgets.Remove(officialSelection);
					leftpanel.Widgets.Remove(communitySelection);
					leftpanel.Widgets.Add(officialSelection);
				};
				tab2.Click += (i, a) =>
				{
					leftpanel.Widgets.Remove(communitySelection);
					leftpanel.Widgets.Remove(officialSelection);
					leftpanel.Widgets.Add(communitySelection);
				};
				tab3.Click += (i, a) =>
				{
					leftpanel.Widgets.Remove(communitySelection);
					leftpanel.Widgets.Remove(officialSelection);
					leftpanel.Widgets.Add(communitySelection);
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
					grid1.Widgets.Add(file);

				};

				
				return grid1;
	}
}