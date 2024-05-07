using System;
using System.Globalization;
using System.IO;
using System.Linq;
using DefconNull.Rendering.CustomUIElements;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;

namespace DefconNull.Rendering.UILayout;

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
		Stylesheet.Current.TextBoxStyle.Border = new SolidBrush(Color.White);
		Stylesheet.Current.CheckBoxStyle.Border = new SolidBrush(Color.White);
		Stylesheet.Current.CheckBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.CheckBoxStyle.ImageStyle.PressedImage = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),new Color(31,81,255,240));
		Stylesheet.Current.ButtonStyle.LabelStyle.TextColor = Color.White;
		Stylesheet.Current.TextBoxStyle.TextColor = Color.White;
		Stylesheet.Current.LabelStyle.TextColor = Color.White;
		Stylesheet.Current.LabelStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ComboBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ComboBoxStyle.LabelStyle.TextColor = Color.White;
		Stylesheet.Current.ComboBoxStyle.LabelStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.TextColor = Color.White;
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ListBoxStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.TextColor = Color.White;
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ScrollViewerStyle.Background =  new SolidBrush(Color.Black);
		Stylesheet.Current.ScrollViewerStyle.VerticalScrollKnob = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),new Color(31,81,255,240));
		Stylesheet.Current.ScrollViewerStyle.VerticalScrollBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")), Color.Black);
		Stylesheet.Current.WindowStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.WindowStyle.TitleStyle.TextColor = Color.White;
		Stylesheet.Current.HorizontalSliderStyle.Background = new SolidBrush(Color.Black);
		Stylesheet.Current.HorizontalSliderStyle.KnobStyle.ImageStyle.PressedImage = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),new Color(31,81,255,240));

	}

	protected static FontSystem DefaultFont { get; set; }
	public static void SetScale(Vector2 scale)
	{
		GlobalScale = scale;
		FontSize = GlobalScale.Y * 30;
		Stylesheet.Current.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ButtonStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.TextBoxStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ComboBoxStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
		Stylesheet.Current.ComboBoxStyle.ListBoxStyle.ListItemStyle.LabelStyle.Font = DefaultFont.GetFont(FontSize);
	}
	protected static float FontSize = 35;
	public static Vector2 GlobalScale = new Vector2(1, 1);
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
		if (currentKeyboardState.IsKeyDown(Keys.F5) && lastKeyboardState.IsKeyUp(Keys.F5))
		{
			UI.SetUI(null);
		}
#endif
		
		
		
	}
	public virtual void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
			
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		//Console.WriteLine(MousePos.X + " " + MousePos.Y);
		batch.DrawText("\nX:"+MousePos.X+" Y:"+MousePos.Y,  Camera.GetMouseWorldPos(),  2/Camera.GetZoom(),Color.Wheat);
		batch.End();
	}
	public virtual void RenderFrontHud(SpriteBatch batch, float deltatime)
	{
		
	}

	protected static bool JustPressed(Keys k)
	{
		return lastKeyboardState.IsKeyUp(k) && currentKeyboardState.IsKeyDown(k);
	}

	protected static Widget SettingsMenu(Action quitEvent)
	{
		Panel p = new Panel();
		p.Widgets.Add(new TextLabel()
		{
			Text = "Settings",
			Top = (int)(5*GlobalScale.X),
			Height = (int) (25 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Top,
			Background = new SolidBrush(Color.Transparent),
		});
		var stack = new VerticalStackPanel();
		stack.HorizontalAlignment = HorizontalAlignment.Stretch;
		stack.Top = (int) (50 * GlobalScale.X);
		stack.Spacing = (int) (10 * GlobalScale.X);
		p.Widgets.Add(stack);
		
		
		//volume
		var option = new HorizontalStackPanel();
		option.HorizontalAlignment = HorizontalAlignment.Stretch;
		option.VerticalAlignment = VerticalAlignment.Center;
		option.Widgets.Add(new TextLabel()
		{
			Text = "Sound Volume",
			Height = (int) (15 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Background = new SolidBrush(Color.Transparent),
		});
		
		var sfxVolume = new HorizontalSlider()
		{
			ImageButton =
			{
				Background = new SolidBrush(Color.Transparent),
				OverBackground = new SolidBrush(Color.Transparent),
				Content = new Widget()
				{
					Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),Color.White),
					Height = 50,
					Width = 15,
				},
				Width = 15,
				Height = 50,
				Padding = new Thickness(0),
				Margin = new Thickness(0),
				BorderThickness = Thickness.Zero,
			},
			Background = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/slide")),
			Height = (int) (15 * GlobalScale.X),
			Width = (int)(100*GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			Minimum = 0,
			Maximum = 1,
			Value = float.Parse(Game1.config.GetValue("settings", "sfxVol", "0.5"), CultureInfo.InvariantCulture)
		};
		option.Widgets.Add(sfxVolume);
		option.Spacing = (int) (5f*GlobalScale.X);
		stack.Widgets.Add(option);
		//music
		option = new HorizontalStackPanel();
		option.HorizontalAlignment = HorizontalAlignment.Stretch;
		option.VerticalAlignment = VerticalAlignment.Center;
		option.Widgets.Add(new TextLabel()
		{
			Text = "Music Volume",
			Height = (int) (15 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Background = new SolidBrush(Color.Transparent),
		});
		var musicVolume = new HorizontalSlider()
		{
			ImageButton =
			{
				Background = new SolidBrush(Color.Transparent),
				OverBackground = new SolidBrush(Color.Transparent),
				Content = new Widget()
				{
					Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),Color.White),
					Height = 50,
					Width = 15,
				},
				Width = 15,
				Height = 50,
				Padding = new Thickness(0),
				Margin = new Thickness(0),
				BorderThickness = Thickness.Zero,
			},
			Background = new TextureRegion(TextureManager.GetTexture("MainMenu/butts/slide")),
			Height = (int) (15 * GlobalScale.X),
			Width = (int)(100*GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			Minimum = 0,
			Maximum = 1,
			Value = float.Parse(Game1.config.GetValue("settings", "musicVol", "0.5"), CultureInfo.InvariantCulture)
		};
		option.Widgets.Add(musicVolume);
		option.Spacing = (int) (5*GlobalScale.X);
		stack.Widgets.Add(option);
		//resolution
		option = new HorizontalStackPanel();
		option.HorizontalAlignment = HorizontalAlignment.Stretch;
		option.VerticalAlignment = VerticalAlignment.Center;
		option.Widgets.Add(new TextLabel()
		{
			Text = "Resolution",
			Height = (int) (15 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Background = new SolidBrush(Color.Transparent),
		});

		var resoultion = new ComboBox();
		
		string currentRes = Game1.config.GetValue("settings", "Resolution", "800x600");
		foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes) {
			if (mode.AspectRatio < 1.6f)
			{
				continue;
			}
			resoultion.Items.Add(new ListItem(mode.Width + "x" + mode.Height));
			if (mode.Width + "x" + mode.Height == currentRes)
			{
				resoultion.SelectedItem = resoultion.Items.Last();
			}
		}
		if (resoultion.SelectedItem == null)
		{
			resoultion.Items.Add(new ListItem(currentRes));
			resoultion.SelectedItem = resoultion.Items.Last();
		}
		resoultion.HorizontalAlignment = HorizontalAlignment.Right;
		option.Widgets.Add(resoultion);
		option.Spacing = (int) (6f*GlobalScale.X);
		stack.Widgets.Add(option);
		
		//fullscreen
		option = new HorizontalStackPanel();
		option.HorizontalAlignment = HorizontalAlignment.Stretch;
		option.VerticalAlignment = VerticalAlignment.Center;
		option.Widgets.Add(new TextLabel()
		{
			Text = "FullScreen",
			Height = (int) (15 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Background = new SolidBrush(Color.Transparent),
		});

		var fullscren = new CheckBox
		{
			Width = 50,
			Height = 50,
			ImageHeight = 50,
			ImageWidth = 50,
			Padding = new Thickness(0),
			Margin = new Thickness(0),
			BorderThickness = Thickness.Zero,
			PressedTextColor = Color.Green,
			PressedBackground = new SolidBrush(Color.Green),
			HorizontalAlignment = HorizontalAlignment.Right,
			PressedImage = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("")),Color.Green),
#if DEBUG
			IsChecked = bool.Parse(Game1.config.GetValue("settings", "fullscreen", "false"))
#else
			IsChecked = bool.Parse(Game1.config.GetValue("settings", "fullscreen", "true"))
#endif
	
		};
		option.Widgets.Add(fullscren);
		option.Spacing = (int) (50f * GlobalScale.X);
		stack.Widgets.Add(option);

		
		
		var cancel = new SoundTextButton
		{
			Text = "Cancel",
			Height = (int)(12 * GlobalScale.X),
			Width = (int)(66 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Bottom,
			Left = -(int)(60 * GlobalScale.X),
			Top = -(int)(10 * GlobalScale.X),
		};
		cancel.Click += (sender, args) =>
		{
			quitEvent.Invoke();
		};
		var ok = new SoundTextButton
		{
			Text = "Ok",
			Height = (int)(12 * GlobalScale.X),
			Width = (int)(25 * GlobalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Bottom,
			Left = -(int)(10 * GlobalScale.X),
			Top = -(int)(10 * GlobalScale.X),
		};
		ok.Click += (sender, args) =>
		{
			Game1.config.SetValue("settings", "Resolution", resoultion.SelectedItem.Text);
			Game1.config.SetValue("settings", "musicVol", musicVolume.Value);
			Game1.config.SetValue("settings", "sfxVol", sfxVolume.Value);
			Game1.config.SetValue("settings", "fullscreen", fullscren.IsChecked.ToString());
			Game1.config.Save();
			Game1.instance.UpdateGameSettings();
			quitEvent.Invoke();
		};
		p.Widgets.Add(cancel);
		p.Widgets.Add(ok);
		return p; 
	}

}