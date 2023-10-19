using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.Rendering;

public class PopUpText
{
	private Transform2 Transform;
	private float aliveTime;
	private string text;
	public float scale = 1;
	public Color Color = Color.White;
	
	public static readonly object syncobj = new object();
	

	public static List<PopUpText> Objects = new List<PopUpText>();

	public PopUpText(string text,Vector2 position, Color c)
	{
		Transform = new Transform2();
		Transform.Position = Utility.GridToWorldPos(position);
		this.text = text;
		Color = c;
		lock (syncobj)
		{
			Objects.Add(this);
		}
	}



	public static void Update(float deltaTime)
	{
		lock (syncobj)
		{

			foreach (var obj in new List<PopUpText>(Objects))
			{

				obj.Transform.Position += new Vector2(0,-0.2f) * deltaTime;
				obj.aliveTime += deltaTime;

				if (obj.aliveTime > 1000)
				{
					Objects.Remove(obj);
					
				}

			}
		}
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		lock (syncobj)
		{


			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Texture);
			foreach (var obj in new List<PopUpText>(Objects))
			{


				spriteBatch.DrawText(obj.text, obj.Transform.Position, 5*obj.scale, 100, obj.Color * ((1000-obj.aliveTime) / 1000));


			}

			spriteBatch.End();
		}
	}

}