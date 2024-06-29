using System;
using System.Linq;
using DefconNull.Networking;
using DefconNull.Rendering.CustomUIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering.UILayout;

public class PreGameLobbyLayout : MenuLayout
{

	private bool swapingMap;

	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		base.RenderBehindHud(batch, deltatime);
		batch.Begin(samplerState: SamplerState.PointClamp);
		var chatPos = Game1.instance.GraphicsDevice.Viewport.Height - 200 * GlobalScale.X;
		batch.Draw(TextureManager.GetTexture(""), new Vector2(0, chatPos), null, new Color(242f / 255f, 190f / 255f, 0f / 255f)*0.2f, 0, Vector2.Zero, new Vector2(150,200)*GlobalScale.X, SpriteEffects.None, 0);

		string chatmsg = "";
		int extraLines = 0;
		int width = 35;
		foreach (var msg in Chat.Messages)
		{
			chatmsg += msg + "\n";
			if (msg.Length > width)
			{
				extraLines++;
			}

			extraLines++;
		}

		batch.DrawText(chatmsg, new Vector2(5, chatPos),  GlobalScale.X*0.6f, width, Color.White);
		
		var bannerScale = GlobalScale.X*0.8f;
		var banner = TextureManager.GetTexture("Lobby/banner1");
		batch.Draw(banner, new Vector2(188*bannerScale, Game1.resolution.Y-banner.Height*bannerScale), null, Color.White, 0, Vector2.Zero, bannerScale, SpriteEffects.None, 0);
		batch.DrawText(GameManager.PreGameData.HostName, new Vector2(188*bannerScale, Game1.resolution.Y-(banner.Height-5)*bannerScale), bannerScale*2f, 20, Color.White);
		
		batch.DrawText("VS", new Vector2(490*bannerScale, Game1.resolution.Y-(banner.Height-5)*bannerScale), bannerScale*3f, 20, Color.White);
		
		banner = TextureManager.GetTexture("Lobby/banner2");
		batch.Draw(banner, new Vector2(545*bannerScale, Game1.resolution.Y-banner.Height*bannerScale), null, Color.White, 0, Vector2.Zero, bannerScale, SpriteEffects.None, 0);
		batch.DrawText(GameManager.PreGameData.Player2Name, new Vector2(545*bannerScale, Game1.resolution.Y-(banner.Height-5)*bannerScale), bannerScale*2f, 20, Color.White);
		
		//centered
		var pos = new Vector2(Game1.resolution.X / 2f - (WorldManager.Instance.CurrentMap.Name.Length) * GlobalScale.X * 20f, 0);
		batch.DrawText(WorldManager.Instance.CurrentMap.Name,pos, GlobalScale.X*2f, 20, Color.White);
		batch.DrawText("By "+WorldManager.Instance.CurrentMap.Author,pos+new Vector2(0,20f*GlobalScale.X),GlobalScale.X, 20, Color.DarkGray);
		batch.End();
	}

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
        Background = new SolidBrush(new Color(242f / 255f, 190f / 255f, 0f / 255f) * 0.2f),
        Width = (int)(140 * GlobalScale.X),
        Height = (int)(800 * GlobalScale.Y),
        Margin = new Thickness(0),
        Padding = new Thickness(0)
    };
    grid1.Widgets.Add(rightStack);

    var leftpanel = new Panel()
    {
        Top = 0,
        Left = 0,
        Background = new SolidBrush(new Color(242f / 255f, 190f / 255f, 0f / 255f)*0.2f),
        Width = (int)(150 * GlobalScale.X),
        Height = (int)(440 * GlobalScale.Y),
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Left,
    };
    grid1.Widgets.Add(leftpanel);

    var chat = new TextBox()
    {
	    Background = new SolidBrush(Color.Black),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Width = (int)(150 * GlobalScale.X),
        Height = (int)(10 * GlobalScale.X),
        Left = (int)(0 * GlobalScale.X),
        Font = UiLayout.DefaultFont.GetFont(5 * GlobalScale.X)
    };
    chat.KeyDown += (s, a) =>
    {
        if (a.Data == Keys.Enter)
        {
            Chat.SendMessage(chat.Text);
            chat.Text = "";
        }
    };
    grid1.Widgets.Add(chat);

    
    /*var overlayLeftPanel = new Image()
    {
        GridColumn = 0,
        GridRow = 0,
        Top = 0,
        Left = 0,
        Width = leftpanel.Width,
        Height = leftpanel.Height,
        Renderable = new TextureRegion(TextureManager.GetTexture("Lobby/LeftPanelOverlay"))
    };
    grid1.Widgets.Add(overlayLeftPanel);
	*/
    var overlayRightPanel = new Image()
    {
        GridColumn = 2,
        GridRow = 0,
        Top = 170,
        Left = 0,
        Width = rightStack.Width,
        Height = rightStack.Height,
	    Renderable = new TextureRegion(TextureManager.GetTexture("Lobby/RightPanelOverlay"))
    };
    grid1.Widgets.Add(overlayRightPanel);
	
    /*
    var overlayChat = new Image()
    {
        GridColumn = 0,
        GridRow = 0,
        Top = chat.Top,
        Left = chat.Left,
        Width = chat.Width,
        Height = chat.Height,
        Renderable = new TextureRegion(TextureManager.GetTexture("Lobby/ChatOverlay"))
    };
    grid1.Widgets.Add(overlayChat);
    */
    

		if (GameManager.PreGameData.Player2Name != "None" && GameManager.IsPlayer1)
		{
			var kick = new ImageButton()
			{
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Right,
				Top = (int)(-30*GlobalScale.X),
				Left = (int)(0*GlobalScale.X),
				Width = (int)(35*GlobalScale.X),
				Height = (int)(35*GlobalScale.X),
				ImageHeight = (int)(35*GlobalScale.X),
				ImageWidth = (int)(35*GlobalScale.X),
				Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/kick")), Color.White),
				Background = new SolidBrush(Color.Transparent),
			};
			kick.Click += (s, a) => { NetworkingManager.KickRequest(); };
			grid1.Widgets.Add(kick);
		}
		else if (GameManager.PreGameData.SinglePLayerFeatures && GameManager.IsPlayer1)
		{
			var addAI = new ImageButton()
			{
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Right,
				Top = (int)(-30*GlobalScale.X),
				Left = (int)(-35*GlobalScale.X),
				Width = (int)(35*GlobalScale.X),
				Height = (int)(35*GlobalScale.X),
				ImageHeight = (int)(35*GlobalScale.X),
				ImageWidth = (int)(35*GlobalScale.X),
				Background = new SolidBrush(Color.Transparent),
				Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/ai")), Color.White),
			};
			addAI.Click += (s, a) => { NetworkingManager.AddAI(); };
			var addPractice = new ImageButton()
			{
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Right,
				Top = (int)(-30*GlobalScale.X),
				Left = (int)(0*GlobalScale.X),
				Width = (int)(35*GlobalScale.X),
				Height = (int)(35*GlobalScale.X),
				ImageHeight = (int)(35*GlobalScale.X),
				ImageWidth = (int)(35*GlobalScale.X),
				Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/tutorial")), Color.White),
				Background = new SolidBrush(Color.Transparent)
			};
			addPractice.Click += (s, a) => { NetworkingManager.PracticeMode();};
			grid1.Widgets.Add(addAI);
			grid1.Widgets.Add(addPractice);
		}


		var btn = new ImageButton()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Top = 0,
			Width = (int) (200 * GlobalScale.X),
			Height = (int) (100 * GlobalScale.Y),
			PressedBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/startButton")),Color.DarkGoldenrod),
			Background = new TextureRegion(TextureManager.GetTexture("Lobby/startButton")),
			OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/startButton")),Color.DarkGoldenrod),
			Padding = new Thickness(0,0,0,(int)(20*GlobalScale.X)),
			Margin = new Thickness(0,0,0,(int)(20*GlobalScale.X)),


		};
		if (!GameManager.IsPlayer1)
		{
			btn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/startButton")), Color.DimGray);
			btn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("Lobby/startButton")), Color.DimGray);
		}

		btn.Click += (s, a) =>
		{
			
			NetworkingManager.SendStartGame();
		};
		rightStack.Widgets.Add(btn);
		
		var label = new TextLabel()
		{
			Text = "Seconds Per Turn",
			HorizontalAlignment = HorizontalAlignment.Center,
			Height = (int)(10*GlobalScale.X),
	
		};
		
		rightStack.Widgets.Add(label);
		label = new TextLabel()
		{
			Text = "(0 for infinite)",
			HorizontalAlignment = HorizontalAlignment.Center,
			Height = (int)(10*GlobalScale.X),
	
		};
		rightStack.Widgets.Add(label);
		var time = new TextBox()
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			Width = (int)(120f*GlobalScale.X)
		};
		time.Text = "" + GameManager.PreGameData.TurnTime;
		rightStack.Widgets.Add(time);
		time.KeyboardFocusChanged += (s, a) =>
		{
			if (time.Text != "" && GameManager.PreGameData.TurnTime != int.Parse(time.Text))
			{
				GameManager.PreGameData = GameManager.PreGameData with {TurnTime = uint.Parse(time.Text)};
				NetworkingManager.SendPreGameUpdate();
			}
		};

	
		

	var linebreak = new TextLabel()
	{
		Text = "______________",
		HorizontalAlignment = HorizontalAlignment.Center,
		Width = (int)(400*GlobalScale.X),
		Height = (int)(20*GlobalScale.X)
	};
	rightStack.Widgets.Add(linebreak);

				
	var spectatorViewer = new ScrollViewer();
	var spectatorList = new VerticalStackPanel();
	spectatorViewer.Background = new SolidBrush(Color.Transparent);
	spectatorViewer.Content = spectatorList;
	spectatorViewer.Height = (int) (80 * GlobalScale.Y);

	var lbl = new TextLabel()
	{
		Text = "SPECTATORS:",
		VerticalAlignment = VerticalAlignment.Center,
		HorizontalAlignment = HorizontalAlignment.Center,
		Height = (int) (10 * GlobalScale.X),
		Top = 0,
	};
	rightStack.Widgets.Add(lbl);
	foreach (var spectator in GameManager.PreGameData.Spectators)
	{
		var spec = new TextLabel()
		{
			Text = spectator,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
			Top = 0,
		};
		spectatorList.Widgets.Add(spec);
	}
		
	rightStack.Widgets.Add(spectatorViewer);
		
				
				
				
	///LEFT STACK///
	var officialSelection = new ListBox()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch,
		VerticalAlignment = VerticalAlignment.Bottom,
	};
	officialSelection.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(20);
	foreach (var path in GameManager.MapList)
	{
		var item = new ListItem()
		{
			Text = path.Key,
			
		};
		var image = new ImageButton()
		{
			Image = new TextureRegion(TextureManager.GetTexture("Lobby/MapBackground")),
			
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
			var lbl = new TextLabel();
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
		Image = new TextureRegion(TextureManager.GetTexture("Lobby/MapBackground")),
		ImageTextSpacing = 150
						
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
		var lbl = new TextLabel();
		lbl.Text = "Swaping Map";
		lbl.HorizontalAlignment = HorizontalAlignment.Center;
		lbl.VerticalAlignment = VerticalAlignment.Center;
		leftpanel.Widgets.Add(lbl);
		swapingMap = true;
	}
};
var lbl1 = new TextLabel()
{
	Text = "Maps Selection",
	VerticalAlignment = VerticalAlignment.Top,
	HorizontalAlignment = HorizontalAlignment.Center,
	Top = (int)(10*GlobalScale.X),
	Height = (int)(10*GlobalScale.X)

};
var tab1 = new SoundTextButton()
{
	Text = "Official",
	HorizontalAlignment = HorizontalAlignment.Left,
	VerticalAlignment = VerticalAlignment.Top,
	Height = (int)(8*GlobalScale.X),
	Width = (int)(80*GlobalScale.X),
	Top = (int)(20*GlobalScale.X)
};
var tab2 = new SoundTextButton()
{
	Text = "Community",
	HorizontalAlignment = HorizontalAlignment.Right,
	VerticalAlignment = VerticalAlignment.Top,
	Height = (int)(8*GlobalScale.X),
	Width = (int)(80*GlobalScale.X),
	Top = (int)(20*GlobalScale.X)

};
leftpanel.Widgets.Add(lbl1);
leftpanel.Widgets.Add(tab1);
leftpanel.Widgets.Add(tab2);

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
/*
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

};*/

				
return grid1;
}
}