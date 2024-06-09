using System;
using System.Collections.Generic;

using DefconNull.WorldActions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Riptide;

#if CLIENT
using DefconNull.LocalObjects;
#endif

namespace DefconNull.ReplaySequence;

public class SpawnParticle : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.SpawnParticle;
	}
	public struct RandomisedParticleParams : IMessageSerializable
	{
		public string TextureName;
		public int Count;
		public float VelocityXMin, VelocityXMax;
		public float VelocityYMin, VelocityYMax;
		public float ScaleMin, ScaleMax;
		public float AccelerationXMin, AccelerationXMax;
		public float AccelerationYMin, AccelerationYMax;
		public int LifetimeMin, LifetimeMax;
		public float Damping;
		public float RotationMax;
		public float RotationMin;
		public int SpawnDelay;
		public float SpawnCounter;

		public void Serialize(Message message)
		{
			message.Add(TextureName);
			message.Add(Count);
			message.Add(VelocityXMin);
			message.Add(VelocityXMax);
			message.Add(VelocityYMin);
			message.Add(VelocityYMax);
			message.Add(AccelerationXMin);
			message.Add(AccelerationXMax);
			message.Add(AccelerationYMin);
			message.Add(AccelerationYMax);
			message.Add(LifetimeMin);
			message.Add(LifetimeMax);
			message.Add(Damping);
			message.Add(RotationMax);
			message.Add(RotationMin);
			message.Add(SpawnDelay);
			message.Add(ScaleMin);
			message.Add(ScaleMax);
		}

		public void Deserialize(Message message)
		{
			TextureName = message.GetString();
			Count = message.GetInt();
			VelocityXMin = message.GetFloat();
			VelocityXMax = message.GetFloat();
			VelocityYMin = message.GetFloat();
			VelocityYMax = message.GetFloat();
			AccelerationXMin = message.GetFloat();
			AccelerationXMax = message.GetFloat();
			AccelerationYMin = message.GetFloat();
			AccelerationYMax = message.GetFloat();
			LifetimeMin = message.GetInt();
			LifetimeMax = message.GetInt();
			Damping = message.GetFloat();
			RotationMax = message.GetFloat();
			RotationMin = message.GetFloat();
			SpawnDelay = message.GetInt();
			ScaleMin = message.GetFloat();
			ScaleMax = message.GetFloat();
		}
#if CLIENT
		

		public List<Particle> MakeParticles(Vector2Int position)
		{
			var list = new List<Particle>();
			for (int i = 0; i < Count; i++)
			{
				Vector2 velocity = new Vector2(Random.Shared.NextSingle(VelocityXMin / 500f, VelocityXMax / 500f), Random.Shared.NextSingle(VelocityYMin / 500f, VelocityYMax / 500f));
				Vector2 acceleration = new Vector2(Random.Shared.NextSingle(AccelerationXMin / 5000f, AccelerationXMax / 5000f), Random.Shared.NextSingle(AccelerationYMin / 5000f, AccelerationYMax / 5000f));
				int LifeTime = Random.Shared.Next(LifetimeMin, LifetimeMax);
				float rotationVel = Random.Shared.NextSingle(RotationMin, RotationMax);
				float scale = Random.Shared.NextSingle(ScaleMin, ScaleMax);
				list.Add(new LocalObjects.Particle(Rendering.TextureManager.GetTextureFromPNG("Particles/" + TextureName), position, velocity, new Vector2(scale, scale), acceleration, rotationVel, Damping, LifeTime));
			}

			return list;
		}
	
#endif
	}
	RandomisedParticleParams _paramsList;
	Vector2Int _position;

	public static SpawnParticle Make(RandomisedParticleParams randomisedParticleParamsList, Vector2Int position) 
	{
		SpawnParticle t = (GetAction(SequenceType.SpawnParticle) as SpawnParticle)!;
		t._paramsList = randomisedParticleParamsList;
		t._position = position;
		return t;
	}



	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else

		_paramsList.MakeParticles(Utility.GridToWorldPos(_position));

		
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_paramsList);
		message.Add(_position);
	}

	protected override void DeserializeArgs(Message message)
	{
		_paramsList = message.GetSerializable<RandomisedParticleParams>();
		_position = message.GetSerializable<Vector2Int>();
	}

	public override bool ShouldDo()
	{
#if SERVER
		return false;
#else
		return true;
#endif
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{ 
		return WorldManager.Instance.GetTileAtGrid(_position).IsVisible(Visibility.Partial,player1);
	}
#endif

}