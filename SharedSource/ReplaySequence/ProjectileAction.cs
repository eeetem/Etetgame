using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;


namespace DefconNull.ReplaySequence;

public class ProjectileAction : SequenceAction
{
	
	public Vector2Int From;
	public Vector2Int To;
	private string particleName;
	private float particleSpeed;
	private List<SpawnParticle.RandomisedParticleParams> _particleSpawn = new List<SpawnParticle.RandomisedParticleParams>();

	protected bool Equals(ProjectileAction other)
	{
		return From.Equals(other.From) && To.Equals(other.To);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((ProjectileAction) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = From.GetHashCode();
			hashCode = (hashCode * 397) ^ To.GetHashCode();
			return hashCode;
		}
	}

	public static ProjectileAction Make(Vector2Int from,Vector2Int to, string particleName, float particleSpeed, List<SpawnParticle.RandomisedParticleParams> particleSpawn)
	{
		ProjectileAction t = (GetAction(SequenceType.Projectile) as ProjectileAction)!;
		t.From = from;
		t.To = to;
		t.particleName = particleName;
		t.particleSpeed = particleSpeed;
		t._particleSpawn = particleSpawn;
		return t;
	}


	
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return WorldManager.Instance.GetTileAtGrid(From).IsVisible(Visibility.Partial,player1) || WorldManager.Instance.GetTileAtGrid(To).IsVisible(Visibility.Partial,player1);
	}
#endif


	public override BatchingMode Batching => BatchingMode.NonBlockingAlone;

	public override SequenceType GetSequenceType()
	{
		return SequenceType.Projectile;
	}

	
	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		if (particleName != "")
		{
			var p = new LocalObjects.Particle(Rendering.TextureManager.GetTextureFromPNG("Particles/" + particleName),
				Utility.GridToWorldPos(From + new Vector2(0.5f, 0.5f)),
				Utility.GridToWorldPos(To + new Vector2(0.5f, 0.5f)), particleSpeed,_particleSpawn);
			while (p.AliveTime <= p.LifeTime)
			{
				Thread.Sleep(100);
				
			}

		}

		
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(From);
		message.Add(To);
		message.Add(particleName);
		message.Add(particleSpeed);
		message.AddSerializables(_particleSpawn.ToArray());

	}

	protected override void DeserializeArgs(Message message)
	{
		From = message.GetSerializable<Vector2Int>();
		To = message.GetSerializable<Vector2Int>();
		particleName = message.GetString();
		particleSpeed = message.GetFloat();
		_particleSpawn = message.GetSerializables<SpawnParticle.RandomisedParticleParams>().ToList();


	}
#if CLIENT
	override public void Preview(SpriteBatch spriteBatch)
	{
		spriteBatch.DrawLine(Utility.GridToWorldPos(From + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(To+ new Vector2(0.5f, 0.5f)), Color.Red, 2);
	}
#endif
}