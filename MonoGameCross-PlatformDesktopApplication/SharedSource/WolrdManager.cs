using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;

namespace MultiplayerXeno
{
	public partial class WorldManager
	{
		public static World World { get; private set; }


		public static void MakeWorld(
#if CLIENT
			GraphicsDevice graphicsDevice, GameWindow window
#endif
	)

	{
			World = new WorldBuilder()
				
				.AddSystem(new WorldObjectManager())
				#if CLIENT
				.AddSystem(new CameraSystem(graphicsDevice,window))
				//.AddSystem(new WorldEditSystem())
				.AddSystem(new RenderSystem(graphicsDevice))
				.AddSystem(new UiSystem(graphicsDevice))
				
				#endif
				.Build();

			
		}
	}
	
}
	