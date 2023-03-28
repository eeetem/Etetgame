using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommonData;
using FontStashSharp;
using HeartSignal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using MultiplayerXeno.UILayouts;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop { get; private set; }
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		public static void Init(ContentManager content, GraphicsDevice graphicsdevice)
		{
			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			MyraEnvironment.Game = Game1.instance;
			//	MyraEnvironment.DrawWidgetsFrames = true; MyraEnvironment.DrawTextGlyphsFrames = true;
	

			Desktop = new Desktop();
			Desktop.TouchDown += MouseDown;
			Desktop.TouchUp += MouseUp;

		
		}



		public delegate void UIGen();
		

		private static UiLayout CurrentUI;
		private static Widget root;
		public static void SetUI(UiLayout? newUI)
		{

			UiLayout.SetScale(new Vector2((Game1.resolution.X / 500f) * 1f, (Game1.resolution.Y / 500f) * 1f));
			
		
			if (newUI != null)
			{
				root = newUI.Generate(Desktop, CurrentUI);
				CurrentUI = newUI;
			}
			else
			{
				root = CurrentUI.Generate(Desktop, CurrentUI);
			}

			Console.WriteLine("Changing UI to: "+CurrentUI);
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
				CurrentUI.MouseDown(gridClick, false);
				
			}

			if (mouseState.RightButton == ButtonState.Pressed)
			{
				CurrentUI.MouseDown(gridClick, true); 
			}

			lastMouseState = mouseState;
		}

		public static void MouseUp(object? sender, EventArgs e)
		{
			if (UI.Desktop.IsMouseOverGUI)
			{
				return; //let myra do it's thing
			}

			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (lastMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
			{
				 CurrentUI.MouseUp(gridClick, false);

			}

			if (lastMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
			{
				CurrentUI.MouseUp(gridClick, true);
			}

		}


	
	


	
		public static Dialog OptionMessage(string title, string content, string option1text, EventHandler option1,string option2text, EventHandler option2)
		{
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ButtonCancel.Text = option1text;
			messageBox.ButtonCancel.Click += option1;
			messageBox.ButtonOk.Text = option2text;
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
			CurrentUI.Update(deltatime);
		}

		public static void Render(float deltaTime)
		{
		
			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			CurrentUI.Render(spriteBatch,deltaTime);
			spriteBatch.End();

			
			Desktop.Root = root;
			if (Game1.instance.IsActive)
			{
				Desktop.Render();
			}
			else
			{
				Desktop.RenderVisual();
			}


		}

		
	
	}
}