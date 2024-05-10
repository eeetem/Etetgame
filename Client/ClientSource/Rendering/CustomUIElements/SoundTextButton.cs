
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
	public void ForceSelect()
	{
		Background = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
		PressedBackground = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
		OverBackground = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
	}
	public void ForceDeselect()
	{
		Background = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelection"));
		PressedBackground = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
		OverBackground = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/mainSelectionSelected"));
	}
	public void GenerateText()
	{
		Task.Run((() =>
		{
			while (Parent== null)
			{
				Thread.Sleep(100);
			}
			var panel = new TextLabel();
			Content = panel;
			panel.Height = Height;
			panel.Width = Width;
		
			panel.Text = _text;
		}));

		

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