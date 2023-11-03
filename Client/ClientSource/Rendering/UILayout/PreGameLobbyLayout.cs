﻿using System;
using DefconNull.Networking;
using DefconNull.Rendering.CustomUIElements;
using DefconNull.World;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;

namespace DefconNull.Rendering.UILayout;

public class PreGameLobbyLayout : MenuLayout
{
	private Panel gameOptionsMenu;
	private bool swapingMap;

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		swapingMap = false;
		WorldManager.Instance.MakeFovDirty();
		var grid1 = new Grid()
		{

		};
		grid1.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
		grid1.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
		grid1.ColumnsProportions.Add(new Proportion(ProportionType.Auto));



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
			Border = new SolidBrush(new Color(31, 81, 255, 240)),
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

		btn.Click += (s, a) => { NetworkingManager.SendStartGame(); };
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
		options.Click += (s, a) =>
		{
			if (UI.Desktop.Widgets.Contains(gameOptionsMenu)) return;
			gameOptionsMenu = new Panel();
			gameOptionsMenu.HorizontalAlignment = HorizontalAlignment.Center;
			gameOptionsMenu.VerticalAlignment = VerticalAlignment.Center;
			gameOptionsMenu.Background = new SolidBrush(Color.Black);
			gameOptionsMenu.BorderThickness = new Thickness(1);
			var stack = new VerticalStackPanel();
			gameOptionsMenu.Widgets.Add(stack);
			stack.Spacing = 25;
			var label = new Label()
			{
				Text = "Seconds Per Turn(0 for infinite):",
				Top = 10,
			};
			stack.Widgets.Add(label);
			var time = new TextBox()
			{
				Top = 25,
			};
			time.Text = "" + GameManager.PreGameData.TurnTime;
			stack.Widgets.Add(time);
			var btn = new SoundButton()
			{
				Text = "Apply",
				Top = 25,
				HorizontalAlignment = HorizontalAlignment.Center,
			};
			if (!GameManager.IsPlayer1)
			{
				btn.Text = "Close";
			}

			btn.Click += (s, a) =>
			{
				if (int.TryParse(time.Text, out var timeLimit))
				{
					var data = GameManager.PreGameData;
					data.TurnTime = timeLimit;
					GameManager.PreGameData = data;
					NetworkingManager.SendPreGameUpdate();
				}

				UI.Desktop.Widgets.Remove(gameOptionsMenu);
				gameOptionsMenu = null;
			};
			stack.Widgets.Add(btn);

			UI.Desktop.Widgets.Add(gameOptionsMenu);
		};

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

		if (GameManager.PreGameData.Player2Name != "None")
		{
			
			var kick = new ImageButton()
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Right,
				Left = -10,
				Width = 50,
				Height = 50,
				Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/kick")), Color.White),
			};
			kick.Click += (s, a) => { NetworkingManager.KickRequest(); };
			lablekick.Widgets.Add(kick);
		}
		else if (GameManager.PreGameData.SinglePLayerFeatures)
		{
			var addAI = new ImageButton()
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Right,
				Left = -10,
				Width = 50,
				Height = 50,
				Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/AI")), Color.White),
			};
			addAI.Click += (s, a) => { NetworkingManager.AddAI(); };
			var addPractice = new ImageButton()
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Right,
				Left = -10,
				Width = 50,
				Height = 50,
				Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/pract")), Color.White),
			};
			addPractice.Click += (s, a) => { NetworkingManager.PracticeMode();};
			lablekick.Widgets.Add(addAI);
			lablekick.Widgets.Add(addPractice);
		}
	
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
		if (!swapingMap)
		{
			var data = GameManager.PreGameData;
			data.SelectedMap = GameManager.MapList[officialSelection.Items[(int) officialSelection.SelectedIndex].Text];
			GameManager.PreGameData = data;
			NetworkingManager.SendPreGameUpdate();
			var lbl = new Label();
			lbl.Text = "Swaping Map";
			lbl.HorizontalAlignment = HorizontalAlignment.Center;
			lbl.VerticalAlignment = VerticalAlignment.Center;
			leftpanel.Widgets.Add(lbl);
			swapingMap = true;
		}
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
	if (!swapingMap)
	{
		var data = GameManager.PreGameData;
		data.SelectedMap = GameManager.CustomMapList[communitySelection.Items[(int) communitySelection.SelectedIndex].Text];
		GameManager.PreGameData = data;
					
		NetworkingManager.SendPreGameUpdate();
		var lbl = new Label();
		lbl.Text = "Swaping Map";
		lbl.HorizontalAlignment = HorizontalAlignment.Center;
		lbl.VerticalAlignment = VerticalAlignment.Center;
		leftpanel.Widgets.Add(lbl);
		swapingMap = true;
	}
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
	var file = new FileDialog(FileDialogMode.OpenFile)
	{
						
	};
	if (file == null) throw new ArgumentNullException(nameof(file));
	file.FilePath = "./Maps";
	file.ButtonOk.Click += (o, e) =>
	{
		var path = file.FilePath;
		NetworkingManager.UploadMap(path);
		file.Close();
	};
	grid1.Widgets.Add(file);

};

				
return grid1;
}
}