﻿using HeartSignal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Pathfinding;
using MultiplayerXeno.UILayouts;
using Salaros.Configuration;


namespace MultiplayerXeno
{
	public class Game1 : Game
	{
		public GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		public static SpriteFont SpriteFont;

		public static Game1 instance;

		public Game1()
		{
			instance = this;
			_graphics = new GraphicsDeviceManager(this);
			_graphics.HardwareModeSwitch = false;
			//_graphics.PreferredBackBufferWidth = 100;
			_graphics.ApplyChanges();
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

			
			Window.AllowUserResizing = false;


		}

		public float GetWindowWidth()
		{
			return Window.ClientBounds.Width;
		}

		protected override void Initialize()
		{

			spriteBatch = new SpriteBatch(GraphicsDevice);
			Camera.Init(GraphicsDevice,Window);
			//WorldEditSystem.Init();

			WorldManager.Instance.Init();
			Action.Init();
			RenderSystem.Init(GraphicsDevice);
			PopUpText.Init(GraphicsDevice);
//
			PathFinding.GenerateNodes();

			base.Initialize();

			

			DiscordManager.Init();
			UpdateGameSettings();

		}

		public static RenderTarget2D GlobalRenderTarget;
		public static ConfigParser config;
		protected override void LoadContent()
		{
			
			GlobalRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			GraphicsDevice.SetRenderTarget(GlobalRenderTarget);
			GraphicsDevice.SetRenderTarget(null);
			TextureManager.Init(Content);
			UI.Init(Content,GraphicsDevice);
			UiLayout.Init();
			UI.SetUI(new MainMenuLayout());
			Audio.Init(Content);
		
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			PostPorcessing.Init(Content, GraphicsDevice);
			SpriteFont = Content.Load<SpriteFont>("font");
			PrefabManager.MakePrefabs();
			WorldEditSystem.GenerateUI();
			config = new ConfigParser("config.txt");

			_graphics.ApplyChanges();
			
			Audio.PlayMenu();
		}

		public void UpdateGameSettings()
		{
			_graphics.PreferredBackBufferWidth = int.Parse(config.GetValue("settings", "Resolution", "1920x1080").Split("x")[0]);
			_graphics.PreferredBackBufferHeight = int.Parse(config.GetValue("settings", "Resolution", "1920x1080").Split("x")[1]);
			_graphics.ApplyChanges();
			GlobalRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
			Camera.Init(GraphicsDevice,Window);
			Audio.MusicVolume = float.Parse(config.GetValue("settings", "musicVol", "0.5"));
			Audio.SoundVolume = float.Parse(config.GetValue("settings", "sfxVol", "0.5"));
			PostPorcessing.RemakeRenderTarget();
			UI.SetUI(null);
		}

		protected override void Update(GameTime gameTime)
		{

			
			Camera.Update(gameTime);
			WorldEditSystem.Update(gameTime);
			WorldManager.Instance.Update(gameTime.ElapsedGameTime.Milliseconds);
			LocalObject.Update(gameTime.ElapsedGameTime.Milliseconds);
			PopUpText.Update(gameTime.ElapsedGameTime.Milliseconds);
			UI.Update(gameTime.ElapsedGameTime.Milliseconds);
			DiscordManager.Update();
			
			base.Update(gameTime);
		}
		private static SpriteBatch spriteBatch;
		
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.SetRenderTarget(GlobalRenderTarget);

			GraphicsDevice.Clear(Color.Gray);
			
			RenderSystem.Draw();
			UI.Render(gameTime.ElapsedGameTime.Milliseconds);//potentially move this into the render system!	Long live Forg!
			PopUpText.Draw();

			PostPorcessing.Apply(gameTime.ElapsedGameTime.Milliseconds);
			GraphicsDevice.SetRenderTarget(null);
			base.Draw(gameTime);
		}

	}
}