using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			
	
		}

		protected override void Initialize()
		{
		
			Camera.Init(GraphicsDevice,Window);
			//WorldEditSystem.Init();

			WorldManager.Instance.Init(GraphicsDevice);
//
			PathFinding.GenerateNodes();
			base.Initialize();
			UI.MainMenu();

		}

		

		public static Texture2D[] Floors;
		public static  Texture2D[] Walls;
		public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
		protected override void LoadContent()
		{
			
			UI.Init(Content,GraphicsDevice);
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			Textures.Add("basicFloor",Content.Load<Texture2D>("basicFloor"));
			Textures.Add("capturePoint",Content.Load<Texture2D>("capturePoint"));
			Textures.Add("spawnPoint",Content.Load<Texture2D>("spawnPoint"));
			Textures.Add("Human",Content.Load<Texture2D>("Human"));
			Textures.Add("basicWall",Content.Load<Texture2D>("basicWall"));
			Textures.Add("basicHalfWall",Content.Load<Texture2D>("basicHalfWall"));

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

		protected override void Draw(GameTime gameTime)
		{
		
			GraphicsDevice.Clear(Color.CornflowerBlue);

			WorldManager.Instance.Draw(gameTime);
			//GraphicsDevice.Clear(Color.Black);
			UI.Render(gameTime.ElapsedGameTime.Milliseconds);

			base.Draw(gameTime);
		}

	}
}