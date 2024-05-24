using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.LocalObjects;
using DefconNull.Networking;
using DefconNull.Rendering;
using DefconNull.Rendering.PostProcessing;
using DefconNull.Rendering.UILayout;
using DefconNull.Rendering.UILayout.GameLayout;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Riptide;
using Salaros.Configuration;
using Action = DefconNull.WorldObjects.Units.Actions.Action;

namespace DefconNull;

public class Game1 : Game
{
	public GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;

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
		Log.Init();
		spriteBatch = new SpriteBatch(GraphicsDevice);
		Camera.Init(GraphicsDevice,Window);
		//WorldEditSystem.Init();

		SequenceAction.InitialisePools();
		WorldManager.Instance.Init();
		Action.Init();
		RenderSystem.Init(GraphicsDevice);
		Utility.Init();
		Message.MaxPayloadSize = 2048 * (int)Math.Pow(2, 5);
		
		

		base.Initialize();

        
		DiscordManager.Init();
		UI.SetUI(new MainMenuLayout());
		UpdateGameSettings();
		MainMenuLayout.GradientPos = new Vector2(-1000000, 0);
	}

	public static RenderTarget2D GlobalRenderTarget;
	public static ConfigParser config;
	

	protected override void LoadContent()
	{
			
		GlobalRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
		GraphicsDevice.SetRenderTarget(GlobalRenderTarget);
		GraphicsDevice.SetRenderTarget(null);
		TextureManager.Init(Content);
		UI.Init(GraphicsDevice);
		UiLayout.Init(GraphicsDevice);
		GameLayout.Init();
		Tracer.Init(Content);

		Audio.Init(Content);
		
		_spriteBatch = new SpriteBatch(GraphicsDevice);
		PostProcessing.Init(Content, GraphicsDevice);
		PrefabManager.MakePrefabs();
		ConfigParserSettings st = new ConfigParserSettings();
		st.Culture = System.Globalization.CultureInfo.InvariantCulture;
		config = new ConfigParser("config.txt",st);

		_graphics.ApplyChanges();
			
		Audio.OnGameStateChange(GameState.Lobby);
			
	
	}

	public static Vector2Int resolution = new Vector2Int(1280, 720);
	public void UpdateGameSettings()
	{
		_graphics.SynchronizeWithVerticalRetrace = true;
		//_graphics.HardwareModeSwitch = false;
		_graphics.PreferredBackBufferWidth = int.Parse(config.GetValue("settings", "Resolution", "1280x720").Split("x")[0]);
		resolution.X = int.Parse(config.GetValue("settings", "Resolution", "1280x720").Split("x")[0]);
		_graphics.PreferredBackBufferHeight = int.Parse(config.GetValue("settings", "Resolution", "1280x720").Split("x")[1]);
		resolution.Y = int.Parse(config.GetValue("settings", "Resolution", "1280x720").Split("x")[1]);
		_graphics.IsFullScreen = bool.Parse(config.GetValue("settings", "fullscreen", "false"));
		if (_graphics.IsFullScreen)
		{
			_graphics.PreferredBackBufferWidth= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			resolution.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			resolution.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
		}

		_graphics.ApplyChanges();
		GlobalRenderTarget = new RenderTarget2D(GraphicsDevice, _graphics.PreferredBackBufferWidth,_graphics.PreferredBackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
		GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
		Camera.Init(GraphicsDevice,Window);
		Audio.MusicVolume = float.Parse(config.GetValue("settings", "musicVol", "0.2"), System.Globalization.CultureInfo.InvariantCulture);
		Audio.SoundVolume = float.Parse(config.GetValue("settings", "sfxVol", "0.7"), System.Globalization.CultureInfo.InvariantCulture);
		PostProcessing.RemakeRenderTarget();
		UI.SetUI(null);

	}

	protected override void Update(GameTime gameTime)
	{
		NetworkingManager.Update();
		MasterServerNetworking.Update();
		GameManager.Update(gameTime.ElapsedGameTime.Milliseconds);
		WorldManager.Instance.Update(gameTime.ElapsedGameTime.Milliseconds);
		SequenceManager.Update();
		Audio.Update(gameTime.ElapsedGameTime.Milliseconds);
		Camera.Update(gameTime);
		LocalObject.Update(gameTime.ElapsedGameTime.Milliseconds);
		Tracer.Update(gameTime.ElapsedGameTime.Milliseconds);
		PopUpText.Update(gameTime.ElapsedGameTime.Milliseconds);
		UI.Update(gameTime.ElapsedGameTime.Milliseconds);
		DiscordManager.Update();
		Chat.Update(gameTime.ElapsedGameTime.Milliseconds);
		//if (Mouse.GetState().LeftButton == ButtonState.Pressed)
		//{
		//	new Tracer(new Vector2(100,100),Camera.GetMouseWorldPos());
		//}

		base.Update(gameTime);
	}
	private static SpriteBatch spriteBatch;
		
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.SetRenderTarget(GlobalRenderTarget);

		GraphicsDevice.Clear(Color.Black);
			
		RenderSystem.Draw(spriteBatch);
		UI.Render(gameTime.ElapsedGameTime.Milliseconds,spriteBatch);//potentially move this into the render system!	Long live Forg!
		PopUpText.Draw(spriteBatch);

		PostProcessing.Apply(gameTime.ElapsedGameTime.Milliseconds);
		GraphicsDevice.SetRenderTarget(null);
		base.Draw(gameTime);
	}

}