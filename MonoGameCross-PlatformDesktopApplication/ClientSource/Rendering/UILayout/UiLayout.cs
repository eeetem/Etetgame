using System.IO;
using MultiplayerXeno;
using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;

namespace MultiplayerXeno.UILayouts;

public abstract class UiLayout
{
	protected static GraphicsDevice graphicsDevice;
	public static void Init(GraphicsDevice g)
	{
		graphicsDevice = g;
		graphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;//shitcode
		DefaultFont = new FontSystem();
		DefaultFont.AddFont(File.ReadAllBytes("Content/GradientVector.ttf"));

		Stylesheet.Current.ButtonStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.TextBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.TextBoxStyle.BorderThickness = new Thickness(1);
		Stylesheet.Current.TextBoxStyle.Border = new SolidBrush(new Color(31, 81, 255, 240));
		Stylesheet.Current.CheckBoxStyle.Border = new SolidBrush(new Color(31, 81, 255, 240));
		Stylesheet.Current.CheckBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.CheckBoxStyle.ImageStyle.PressedImage = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),new Color(31,81,255,240));
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
		FontSize = globalScale.Y * 30;
		Stylesheet.Current.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ButtonStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.TextBoxStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ComboBoxStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
	}
	protected static float FontSize = 35;
	protected static Vector2 globalScale = new Vector2(1, 1);
	public abstract Widget Generate(Desktop desktop, UiLayout? lastLayout);


	public virtual void MouseDown(Vector2Int position, bool rightclick)
	{

	}
	public virtual void MouseUp(Vector2Int position, bool righclick)
	{

	}


	protected static KeyboardState lastKeyboardState;
	protected static KeyboardState currentKeyboardState;
	public virtual void Update(float deltatime)
	{
		lastKeyboardState = currentKeyboardState;
		currentKeyboardState = Keyboard.GetState();
		
#if DEBUG
		if (currentKeyboardState.IsKeyDown(Keys.L) && lastKeyboardState.IsKeyUp(Keys.L))
		{
			UI.SetUI(null);
		}
#endif
		
		
		
	}
	public virtual void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
			
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		batch.DrawText("X:"+MousePos.X+" Y:"+MousePos.Y,  Camera.GetMouseWorldPos(),  2/Camera.GetZoom(),Color.Wheat);
		batch.End();
	}
	public virtual void RenderFrontHud(SpriteBatch batch, float deltatime)
	{
		
	}

	protected static bool JustPressed(Keys k)
	{
		return lastKeyboardState.IsKeyUp(k) && currentKeyboardState.IsKeyDown(k);
	}

}