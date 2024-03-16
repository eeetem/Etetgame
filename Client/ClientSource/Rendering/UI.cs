using System;

using DefconNull.Rendering.UILayout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering;

public static class UI
{
	public static Desktop Desktop { get; private set; } = null!;
	private static GraphicsDevice graphicsDevice = null!;
	public static void Init(GraphicsDevice graphicsdevice)
	{
		graphicsDevice = graphicsdevice;
		MyraEnvironment.Game = Game1.instance;
		//	MyraEnvironment.DrawWidgetsFrames = true; MyraEnvironment.DrawTextGlyphsFrames = true;
	

		Desktop = new Desktop();
		Desktop.TouchDown += MouseDown;
		Desktop.TouchUp += MouseUp;

		
	}



	public delegate void UIGen();
		

	public static UiLayout currentUi = new MainMenuLayout();
	private static UiLayout? lastDifferentUI = new MainMenuLayout();
	private static Widget root = null!;
	public static void SetUI(UiLayout? newUI)
	{

			UiLayout.SetScale(new Vector2(Game1.resolution.X / 800f * 1f, Game1.resolution.Y / 800f * 1f));
			
		
			if (newUI != null)
			{
			
				if (currentUi.GetType() != newUI.GetType())
				{
					lastDifferentUI = currentUi;
					root = newUI.Generate(Desktop, lastDifferentUI);
					currentUi = newUI;
				}
				else
				{
					root = currentUi.Generate(Desktop,lastDifferentUI);
				}
			}
			else
			{
				root = currentUi.Generate(Desktop, lastDifferentUI);
			}
		
		Console.WriteLine("Changing UI to: "+currentUi);
	}
	private static MouseState lastMouseState;

	public static void MouseDown(object? sender, EventArgs e)
	{
		if (!Game1.instance.IsActive) return;
		if (Desktop.IsMouseOverGUI)
		{
			return; //let myra do it's thing
		}
		var mouseState = Mouse.GetState();
		Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		if (mouseState.LeftButton == ButtonState.Pressed)
		{
			currentUi.MouseDown(gridClick, false);
				
		}

		if (mouseState.RightButton == ButtonState.Pressed)
		{
			currentUi.MouseDown(gridClick, true); 
		}

		lastMouseState = mouseState;
	}

	public static void MouseUp(object? sender, EventArgs e)
	{
		if (Desktop.IsMouseOverGUI)
		{
			return; //let myra do it's thing
		}

		var mouseState = Mouse.GetState();
		Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		if (lastMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
		{
			currentUi.MouseUp(gridClick, false);

		}

		if (lastMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
		{
			currentUi.MouseUp(gridClick, true);
		}

	}


	
	


	
	public static Dialog OptionMessage(string title, string content, string option1text, EventHandler? option1,string option2text, EventHandler? option2)
	{
		var messageBox = Dialog.CreateMessageBox(title,content);
		messageBox.ButtonCancel.Content = new Label() {Text = option1text};
		messageBox.ButtonCancel.Click += option1;
		messageBox.ButtonOk.Content = new Label() {Text = option2text};
		messageBox.ButtonOk.Click += option2;
		messageBox.ShowModal(Desktop);
		return messageBox;
	}

	public static void ShowMessage(string title, string content)
	{
		var messageBox = Dialog.CreateMessageBox(title,content);
		messageBox.ShowModal(Desktop);

	}

		

	public static void Update(float deltatime)
	{
		if(!Game1.instance.IsActive) return;
		currentUi.Update(deltatime);
	}

	public static void Render(float deltaTime, SpriteBatch spriteBatch)
	{

		try
		{
			currentUi.RenderBehindHud(spriteBatch, deltaTime);


			Desktop.Root = root;
			if (Game1.instance.IsActive)
			{
				Desktop.Render();
			}
			else
			{
				Desktop.RenderVisual();
			}

			currentUi.RenderFrontHud(spriteBatch, deltaTime);
		}
		catch (Exception e)
		{
			Log.Message("ERROR", "Error rendering UI: " + e.Message + " " + e.StackTrace);
			throw;
		}


	}

		
	
}