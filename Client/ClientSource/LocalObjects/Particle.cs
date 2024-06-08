using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
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
	public readonly int LifeTime;
	public int AliveTime;
	private readonly float _damping;
	private float _rotationVel;
	private readonly bool _fade;
	private int _particleSpawnCounter = 0;
	private Vector2 originalScale;
	private List<SpawnParticle.RandomisedParticleParams> _particleSpawn = new List<SpawnParticle.RandomisedParticleParams>();

	public static readonly ConcurrentBag<Particle> Objects = new ConcurrentBag<Particle>();
	private readonly bool _scaleDown;


	public Particle(Texture2D sprite,Vector2 position, Vector2 velocity, Vector2 Scale, Vector2 acceleration, float rotationVel, float damping, int lifeTime, bool fade = true, bool scaleDown = true)
	{

		_transform = new Transform2();
		_sprite = sprite;//inefficient but fuck it
		_transform.Position = position;
		_transform.Scale = Scale;
		originalScale = Scale;
		_velocity = velocity;
		_acceleration = acceleration;
		LifeTime = lifeTime;
		_fade = fade;
		_scaleDown = scaleDown;
		_damping = damping;
		_rotationVel = rotationVel;

		Objects.Add(this);
		
	}

	public Particle(Texture2D texture, Vector2 start, Vector2 end, float particleSpeed, List<SpawnParticle.RandomisedParticleParams> spawnParticleParams)
	{
		_sprite = texture;
		_transform = new Transform2();
		_transform.Position = start;
		_velocity = (end - start) * particleSpeed;
		
		_fade = false;
		_acceleration = new Vector2(0, 0);
		_transform.Scale = new Vector2(1, 1);
		_damping = 1f;
		_transform.Rotation = (float)Math.Atan2(_velocity.Y, _velocity.X);
		
		_particleSpawn = spawnParticleParams;
		
		Objects.Add(this);
		LifeTime = (int)((end - start).Length() / particleSpeed);
	}

	public static void Update(int deltaTime)
	{
		Particle? obj;
		List<Particle> toReturn = new List<Particle>();
		while (Objects.TryTake(out obj))
		{
			// Update velocity by acceleration
			obj._velocity += obj._acceleration * deltaTime*deltaTime;
			//apply damping
			obj._velocity *= obj._damping;
			// Update position by velocity
			obj._transform.Position += obj._velocity * deltaTime;

				
			obj.AliveTime += deltaTime;

			obj._rotationVel *= obj._damping;
				
			obj._transform.Rotation += obj._rotationVel;

			if (obj._scaleDown)
			{
				obj._transform.Scale = obj.originalScale * (1 - (float)obj.AliveTime / obj.LifeTime);
			}
				


			List<SpawnParticle.RandomisedParticleParams> updatedParticleSpawn = new List<SpawnParticle.RandomisedParticleParams>();

			foreach(var p in obj._particleSpawn)
			{
				
				var newP = p with {SpawnCounter = p.SpawnCounter + deltaTime};
				if (newP.SpawnCounter > newP.SpawnDelay)
				{
					newP = newP with {SpawnCounter = 0};
					//create new particle based on parameters
					newP.MakeParticles(obj._transform.Position).ForEach(x => x._velocity+=obj._velocity);
				}

				updatedParticleSpawn.Add(newP);
			}

			obj._particleSpawn = updatedParticleSpawn;
			if (obj.AliveTime <= obj.LifeTime)
			{
				toReturn.Add(obj);
			}
		}

		foreach (var p in toReturn)
		{
			Objects.Add(p);
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
		if (_fade)
		{
		
			return Color.White * (1 - (float)AliveTime / LifeTime);
		}
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