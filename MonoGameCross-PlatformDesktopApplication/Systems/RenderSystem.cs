using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public class RenderSystem : EntityDrawSystem
	{
		private static SpriteBatch _spriteBatch { get; set; }

		private static GraphicsDevice _graphicsDevice { get; set; }
		private static ComponentMapper<Sprite> _spriteMapper { get; set; }

		private static ComponentMapper<Transform2> _transformMapper { get; set; }
		private static ComponentMapper<WorldObject> _worldobjMapper { get; set; }
		
		
		
		public RenderSystem(GraphicsDevice graphicsDevice)
			: base(Aspect.All(typeof(Sprite), typeof(Transform2),typeof(WorldObject)))
		{
			 _graphicsDevice = graphicsDevice;
			_spriteBatch = new SpriteBatch(graphicsDevice);
		}


		public override void Draw(GameTime gameTime)
		{


			List<int>[] DrawOrderSortedEntities = new List<int>[5];
			foreach (var entity in ActiveEntities)
			{
				WorldObject grid = _worldobjMapper.Get(entity);
				if (DrawOrderSortedEntities[grid.drawLayer] == null)
				{
					DrawOrderSortedEntities[grid.drawLayer] = new List<int>();
				}
				DrawOrderSortedEntities[grid.drawLayer].Add(entity);
			
			}
			

			foreach (var list in DrawOrderSortedEntities)
			{
				if (list == null) continue;
				list.Sort(new EntityDrawOrderCompare(_worldobjMapper,_transformMapper));
				_spriteBatch.Begin(transformMatrix: CameraSystem.Camera.GetViewMatrix());

				foreach (var entity in list)
				{
					var transform = _transformMapper.Get(entity);
					var sprite = _spriteMapper.Get(entity);
					WorldObject grid = _worldobjMapper.Get(entity);

					_spriteBatch.Draw(sprite, transform.Position + WorldObjectManager.GridToWorldPos(grid.Position),transform.Rotation, transform.Scale);
				}

				_spriteBatch.End();
			}
		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_transformMapper = mapperService.GetMapper<Transform2>();
			_spriteMapper = mapperService.GetMapper<Sprite>();
			_worldobjMapper = mapperService.GetMapper<WorldObject>();
		}

	
		public class EntityDrawOrderCompare : Comparer<int>
		{

			private static ComponentMapper<Transform2> TransformMapper { get; set; }
			private static ComponentMapper<WorldObject> WorldobjMapper { get; set; }
			
			public EntityDrawOrderCompare(ComponentMapper<WorldObject> worldobjMapper,ComponentMapper<Transform2> transformMapper)
			{
				TransformMapper = transformMapper;
				WorldobjMapper = worldobjMapper;
			}
			// Compares by Length, Height, and Width.
			public override int Compare(int x, int y)
			{
				WorldObject worldObjectx = _worldobjMapper.Get(x);
				WorldObject worldObjecty = _worldobjMapper.Get(y);
				
				var transformx = _transformMapper.Get(x);
				var transformy = _transformMapper.Get(y);


				return (transformx.Position + WorldObjectManager.GridToWorldPos(worldObjectx.Position)).Y.CompareTo((transformy.Position + WorldObjectManager.GridToWorldPos(worldObjecty.Position)).Y);
			}
		}
	}
}