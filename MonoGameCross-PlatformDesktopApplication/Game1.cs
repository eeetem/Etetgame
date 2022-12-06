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

			WorldManager.Instance.Init();
			RenderSystem.Init(GraphicsDevice);
//
			PathFinding.GenerateNodes();
			base.Initialize();
			UI.SetUI(UI.MainMenu);
			

		}

		public RenderTarget2D renderTarget;
		protected override void LoadContent()
		{
			this.renderTarget = new RenderTarget2D(GraphicsDevice, 200, 200, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			UI.Init(Content,GraphicsDevice);
			Audio.Init(Content);
			TextureManager.Init(Content);
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			
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
			LocalObject.Update(gameTime.ElapsedGameTime.Milliseconds);
			UI.Update(gameTime.ElapsedGameTime.Milliseconds);
			
			base.Update(gameTime);
		}
		private static SpriteBatch spriteBatch;
		
		protected override void Draw(GameTime gameTime)
		{
			
			GraphicsDevice.SetRenderTarget(renderTarget);
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(Color.Gray);
			
			RenderSystem.Draw();
			
			UI.Render(gameTime.ElapsedGameTime.Milliseconds);//potentially move this into the render system!	Long live Forg!
		

			base.Draw(gameTime);
		}

	}
}