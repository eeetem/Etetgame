
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering.CustomUIElements;

public sealed class SoundTextButton : Button
{
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			GenerateText();
		}
	}
	private string _text = "";

	public SoundTextButton() : base()
	{

		Background = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelection"));
		PressedBackground = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
		OverBackground = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
	}
	public void GenerateText()
	{
		var panel = new HorizontalStackPanel();
		panel.HorizontalAlignment = HorizontalAlignment.Stretch;
		panel.VerticalAlignment = VerticalAlignment.Stretch;
		Content = panel;
		Task.Run(() =>
		{
			Thread.Sleep(100);
			foreach (char c in _text)
			{
				Thread.Sleep(100);
				var charImage = new Image();
				charImage.Background = new SolidBrush(Color.Transparent);
				charImage.Renderable = new TextureRegion(TextureManager.GetTextTexture(c));
				if(c == ' ')
					charImage.Renderable = new ColoredRegion((TextureRegion) charImage.Renderable,Color.Transparent);
				charImage.VerticalAlignment = VerticalAlignment.Center;
				charImage.Left = 1;
				charImage.Height = Height-5;
				charImage.Width = Height-5;
				panel.Widgets.Add(charImage);
			}
		});

		

	}

	
	public override void OnMouseEntered()
	{
		Audio.PlaySound("UI/select",null,0.5f);
		base.OnMouseEntered();
	}

	public override void OnTouchDown()
	{
		Audio.PlaySound("UI/press",null,0.5f);
		base.OnTouchDown();
	}
}