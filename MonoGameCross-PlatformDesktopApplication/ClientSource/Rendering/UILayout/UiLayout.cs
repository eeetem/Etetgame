using System.IO;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;

namespace MultiplayerXeno.UILayouts;

public abstract class UiLayout
{
	public static void Init()
	{
		DefaultFont = new FontSystem();
		DefaultFont.AddFont(File.ReadAllBytes("Content/GradientVector.ttf"));
		
		Stylesheet.Current.ButtonStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.TextBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ButtonStyle.LabelStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.TextBoxStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.LabelStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.LabelStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ComboBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ComboBoxStyle.LabelStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.ComboBoxStyle.LabelStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ListBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ScrollViewerStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ScrollViewerStyle.VerticalScrollKnob = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),new Color(31,81,255,240));
		Stylesheet.Current.ScrollViewerStyle.VerticalScrollBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")), Color.Black);
		Stylesheet.Current.WindowStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.WindowStyle.TitleStyle.TextColor = new Color(31,81,255,240);
		Stylesheet.Current.HorizontalSliderStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.HorizontalSliderStyle.KnobStyle.ImageStyle.PressedImage = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),new Color(31,81,255,240));

	}

	protected static FontSystem DefaultFont { get; set; }
	public static void SetScale(Vector2 scale)
	{
		globalScale = scale;
		FontSize = globalScale.Y * 35;
		Stylesheet.Current.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ButtonStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.TextBoxStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ComboBoxStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
	}
	protected static float FontSize = 35;
	protected static Vector2 globalScale = new Vector2(1, 1);
	public abstract Widget Generate(Desktop desktop);
	
	
	//probably shouldnt be here but idk where to put it
	private static VerticalStackPanel chatBox;
	private static ScrollViewer chatBoxViewer;
	protected static void AttachSideChatBox(Panel parent)
	{
		var viewer = new ScrollViewer()
		{
			Left = 0,
			Width = (int)(200*globalScale.X),
			Height = 250,
			Top = 0,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			
		};
    
		AddChatBoxToViewer(viewer);
    					
		var input = new TextBox()
		{
			Width = (int)(200*globalScale.X),
			Height = 20,
			Top = 135,
			Left = 0,
			Text = "",
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Font = DefaultFont.GetFont(20),
			Border = new SolidBrush(new Color(31,81,255,240)),
			BorderThickness = new Thickness(2)
		};
		input.KeyDown += (o, a) =>
		{
			if (a.Data == Keys.Enter)
			{ 
				if (input.Text != "")
				{ if (Networking.serverConnection != null && Networking.serverConnection.IsAlive)
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
			Width = 100,
			Height = 30,
			Top = 135,
			Left = (int)(200*globalScale.X),
			Text = "Send",
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Font = DefaultFont.GetFont(25)
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
		parent.Widgets.Add(inputbtn);
		parent.Widgets.Add(input);
		parent.Widgets.Add(chatBoxViewer);
    			
	}
	protected static void AddChatBoxToViewer(ScrollViewer viewer)
	{
		chatBoxViewer = viewer;
		if (chatBox == null)
		{
			chatBox = new VerticalStackPanel()
			{
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
		}

		viewer.Content = chatBox;
		viewer.ScrollPosition = viewer.ScrollMaximum + new Point(0, 50);
	}
	public static void RecieveChatMessage(string msg)
	{
		var label = new Label
		{
			Text = msg,
			Wrap = true,
			TextAlign = TextHorizontalAlignment.Left,
			Font = DefaultFont.GetFont(20)
		};

		if (chatBox != null) { 
			chatBox.Widgets.Add(label);
			chatBoxViewer.ScrollPosition = chatBoxViewer.ScrollMaximum + new Point(0,50);
		}
		
	}


	
}