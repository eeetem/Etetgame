using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MultiplayerXeno.Pathfinding;


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
			
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += (s, a) =>
			{
				_graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
				_graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			//	GraphicsDevice.Viewport = new Viewport(0,0,Window.ClientBounds.Width, Window.ClientBounds.Height);
				_graphics.ApplyChanges();

			};

			Window.ClientSizeChanged += (s, a) =>
			{
				Camera.Init(GraphicsDevice,Window);

						};
			/*		//Window.ClientSizeChanged += UI.RemakeUi;
			
			*/
		}

		protected override void Initialize()
		{

			spriteBatch = new SpriteBatch(GraphicsDevice);
			Camera.Init(GraphicsDevice,Window);
			//WorldEditSystem.Init();

			WorldManager.Instance.Init(GraphicsDevice);
//
			PathFinding.GenerateNodes();
			base.Initialize();
			UI.SetUI(UI.MainMenu);

		}

		

		public static Texture2D[] Floors;
		public static  Texture2D[] Walls;
		public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
		public RenderTarget2D renderTarget;
		protected override void LoadContent()
		{
			this.renderTarget = new RenderTarget2D(GraphicsDevice, 200, 200, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			UI.Init(Content,GraphicsDevice);
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			Textures.Add("basicFloor",Content.Load<Texture2D>("basicFloor"));
			Textures.Add("capturePoint",Content.Load<Texture2D>("capturePoint"));
			Textures.Add("spawnPoint",Content.Load<Texture2D>("spawnPoint"));
			Textures.Add("Human",Content.Load<Texture2D>("Human"));
			Textures.Add("Scout",Content.Load<Texture2D>("Scout"));
			Textures.Add("basicWall",Content.Load<Texture2D>("basicWall"));
			Textures.Add("basicHalfWall",Content.Load<Texture2D>("basicHalfWall"));
			Textures.Add("basicHalfWallLight",Content.Load<Texture2D>("basicHalfWallLight"));
			SpriteFont = Content.Load<SpriteFont>("font");

			PrefabManager.MakePrefabs();
			WorldEditSystem.GenerateUI();
			
			



			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			
			

			
			Camera.Update(gameTime);
			//MouseManager.Update(gameTime);
			WorldEditSystem.Update(gameTime);
			WorldManager.Instance.Update(gameTime.ElapsedGameTime.Milliseconds);
			UI.Update(gameTime.ElapsedGameTime.Milliseconds);
			
			base.Update(gameTime);
		}
		private static SpriteBatch spriteBatch;
		protected override void Draw(GameTime gameTime)
		{
			
			GraphicsDevice.SetRenderTarget(renderTarget);
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(Color.Gray);
			WorldManager.Instance.Draw(gameTime);
			//GraphicsDevice.Clear(Color.Black);
			UI.Render(gameTime.ElapsedGameTime.Milliseconds);

			base.Draw(gameTime);
		}

	}
}