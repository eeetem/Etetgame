using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.LocalObjects;

public class LocalObject : Rendering.IDrawable
{
	private Transform2 Transform;
	private Vector2 Velocity;
	private Texture2D sprite;
	private float lifeTime;
	private float aliveTime;
	
	public static readonly object syncobj = new object();

	

	public static List<LocalObject> Objects = new List<LocalObject>();

	public LocalObject(Texture2D sprite,Vector2 position, Vector2 velocity,float lifeTime)
	{
		Transform = new Transform2();
		this.sprite = sprite;//inefficient but fuck it
		Transform.Position = position;
		Transform.Scale = new Vector2(6, 6);
		Velocity = velocity;
		this.lifeTime = lifeTime;
		lock (syncobj)
		{
			Objects.Add(this);
		}
	}

	public static void Update(float deltaTime)
	{
		lock (syncobj)
		{

			foreach (var obj in new List<LocalObject>(Objects))
			{

				obj.Transform.Position += obj.Velocity * deltaTime;
				obj.aliveTime += deltaTime;

				if (obj.lifeTime != -1 && obj.aliveTime > obj.lifeTime)
				{
					Objects.Remove(obj);
				}

			}
		}
	}

	public Transform2 GetDrawTransform()
	{
		return Transform;
	}

	public Vector2Int GetGridPos()
	{
		return Utility.WorldPostoGrid(Transform.Position);
	}

	public float GetDrawOrder()
	{
		Vector2Int gridpos = Utility.WorldPostoGrid(Transform.Position);
		return gridpos.X + gridpos.Y;

	}

	public Texture2D GetTexture()
	{
		return sprite;
	}

	public Color GetColor()
	{
		return Color.White;
	}

	public Visibility GetMinimumVisibility()
	{
		return Visibility.Partial;
	}

	public bool IsVisible()
	{
		
		Vector2Int pos = GetGridPos();
		if (!WorldManager.IsPositionValid(pos)) return false;
		WorldTile tile = WorldManager.Instance.GetTileAtGrid(pos);
		return GetMinimumVisibility() <= tile.GetVisibility();
	}


	public bool IsTransparentUnderMouse()
	{
		return false;
	}
}