using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Sprites;

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
		
			
			WorldManager.MakeWorld(GraphicsDevice,Window);
			
			base.Initialize();
/*
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
					WorldObjectManager.MakeGridEntity("basicFloor",new Vector2Int(x, y));
				}
			}

			WorldObjectManager.MakeGridEntity("baiscWallE",new Vector2Int(5, 5));
			WorldObjectManager.MakeGridEntity("baiscWallE",new Vector2Int(5, 6));
			WorldObjectManager.MakeGridEntity("baiscWallE",new Vector2Int(5, 7));
			WorldObjectManager.MakeGridEntity("baiscWallS",new Vector2Int(5, 7));
			WorldObjectManager.MakeGridEntity("baiscWallN",new Vector2Int(5, 7));
			WorldObjectManager.MakeGridEntity("baiscWallW",new Vector2Int(5, 7));

			*/
//move this to networking or menu or something
		WorldObjectManager.LoadData();


		}

		

		public static Texture2D[] Floors;
		public static  Texture2D[] Walls;
		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			int countx=0;
			int county=0;
			Floors = Utility.SplitTexture(Content.Load<Texture2D>("Floors"),256,128, out countx, out county);
			Walls = Utility.SplitTexture(Content.Load<Texture2D>("Walls"),128,192, out countx, out county);
			SpriteFont = Content.Load<SpriteFont>("text");
			WorldObjectManager.MakePrefabs();

			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			
			WorldManager.World.Update(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			WorldManager.World.Draw(gameTime);

			base.Draw(gameTime);
		}
	}
}