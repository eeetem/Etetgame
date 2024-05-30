using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.LocalObjects;

public class Particle : Rendering.IDrawable
{
	private readonly Transform2 _transform;
	private Vector2 _velocity;
	private readonly Vector2 _acceleration;
	private readonly Texture2D _sprite;
	private readonly float _lifeTime;
	private float _aliveTime;
	
	public static readonly object syncobj = new object();

	

	public static readonly List<Particle> Objects = new List<Particle>();

	public Particle(Texture2D sprite,Vector2 position, Vector2 velocity, Vector2 acceleration, float lifeTime)
	{
		_transform = new Transform2();
		this._sprite = sprite;//inefficient but fuck it
		_transform.Position = position;
		_transform.Scale = new Vector2(1, 1);
		_velocity = velocity;
		_acceleration = acceleration;
		this._lifeTime = lifeTime;
		lock (syncobj)
		{
			Objects.Add(this);
		}
	}

	public static void Update(float deltaTime)
	{
		lock (syncobj)
		{

			foreach (var obj in new List<Particle>(Objects))
			{

				// Update velocity by acceleration
				obj._velocity += obj._acceleration * deltaTime;

				// Update position by velocity
				obj._transform.Position += obj._velocity * deltaTime;

				
				obj._aliveTime += deltaTime;

				if (obj._aliveTime > obj._lifeTime)
				{
					Objects.Remove(obj);
				}

			}
		}
	}

	public Transform2 GetDrawTransform()
	{
		return _transform;
	}

	public Vector2Int GetGridPos()
	{
		return Utility.WorldPostoGrid(_transform.Position);
	}

	public float GetDrawOrder()
	{
		Vector2Int gridpos = Utility.WorldPostoGrid(_transform.Position);
		return gridpos.X + gridpos.Y+5f;

	}

	public Texture2D GetTexture()
	{
		return _sprite;
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
		return true;
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