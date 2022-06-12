using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public class UiSystem : EntityDrawSystem
	{
		private static SpriteBatch _spriteBatch { get; set; }

		private static GraphicsDevice _graphicsDevice { get; set; }
		private static ComponentMapper<Sprite> _spriteMapper { get; set; }
		private static ComponentMapper<Text> _textMapper { get; set; }

		private static ComponentMapper<UiObject> _UiObjMapper { get; set; }
		
		
		
		public UiSystem(GraphicsDevice graphicsDevice)
			: base(Aspect.All(typeof(UiObject)).One(typeof(Sprite),typeof(Text)))
		{
			 _graphicsDevice = graphicsDevice;
			_spriteBatch = new SpriteBatch(graphicsDevice);
		}

		private bool LastMousePressed = false;
		public override void Draw(GameTime gameTime)
		{
			var mouse = Mouse.GetState();
			_spriteBatch.Begin();
			foreach (var entity in ActiveEntities)
			{

					var sprite = _spriteMapper.Get(entity);
					var text = _textMapper.Get(entity);
					UiObject UIobj = _UiObjMapper.Get(entity);
					int totalHeight = Game1.instance._graphics.GraphicsDevice.Viewport.Height;
					int totalWidth = Game1.instance._graphics.GraphicsDevice.Viewport.Width;

					float requiredHeight = totalHeight * ((float) UIobj.height / 100f);
					float requiredWidth = totalWidth * ((float) UIobj.width / 100f);
					float positionX = (totalWidth / 100f) * UIobj.xPos;
					float positionY = (totalHeight / 100f) * UIobj.yPos;

					
					var buttonRect = new Rectangle(new Point((int)(positionX-requiredWidth/2), (int) (positionY-requiredHeight/2)), new Point((int)requiredWidth, (int)requiredHeight));
					if (buttonRect.Contains(mouse.Position))
					{
						UIobj.OnHover();
						if (mouse.LeftButton == ButtonState.Released && LastMousePressed)
						{
							
							UIobj.OnClick();
						}
					}
					
					
					if (sprite != null)
					{
						float scaleX = requiredWidth / sprite.TextureRegion.Width;
						float scaleY = requiredHeight / sprite.TextureRegion.Height;

						_spriteBatch.Draw(sprite, new Vector2(positionX,positionY),0, new Vector2(scaleX,scaleY));
					}

					if (text != null)
					{
						_spriteBatch.DrawString(Game1.SpriteFont,text.text,new Vector2(positionX,positionY),Color.Red);
								
						_spriteBatch.DrawRectangle(buttonRect,Color.Red);
					}
		

				
			}
			_spriteBatch.End();
			if (mouse.LeftButton == ButtonState.Pressed)
			{
				LastMousePressed = true;
			}
			else
			{
				LastMousePressed = false;
			}

		}

		public override void Initialize(IComponentMapperService mapperService)
		{
			_spriteMapper = mapperService.GetMapper<Sprite>();
			_UiObjMapper = mapperService.GetMapper<UiObject>();
			_textMapper = mapperService.GetMapper<Text>();
		}

	
	}
}