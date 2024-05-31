using System;
using DefconNull.WorldActions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Riptide;

namespace DefconNull.ReplaySequence;

public class SpawnParticle : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.SpawnParticle;
	}
	public struct ParticleParams : IMessageSerializable
	{
		public string TextureName;
		public int Count;
		public float VelocityXMin, VelocityXMax;
		public float VelocityYMin, VelocityYMax;
		public float AccelerationXMin, AccelerationXMax;
		public float AccelerationYMin, AccelerationYMax;
		public float LifetimeMin, LifetimeMax;
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
			LifetimeMin = message.GetFloat();
			LifetimeMax = message.GetFloat();
		}
	}
	ParticleParams _paramsList;
	Vector2Int _position;

	public static SpawnParticle Make(ParticleParams particleParamsList, Vector2Int position) 
	{
		SpawnParticle t = (GetAction(SequenceType.SpawnParticle) as SpawnParticle)!;
		t._paramsList = particleParamsList;
		t._position = position;
		return t;
	}



	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		for (int i = 0; i < _paramsList.Count; i++)
		{
			Vector2 velocity = new Vector2(Random.Shared.NextSingle(_paramsList.VelocityXMin/500f, _paramsList.VelocityXMax/500f), Random.Shared.NextSingle(_paramsList.VelocityYMin/500f, _paramsList.VelocityYMax/500f));
			Vector2 acceleration = new Vector2(Random.Shared.NextSingle(_paramsList.AccelerationXMin/500f, _paramsList.AccelerationXMax/500f), Random.Shared.NextSingle(_paramsList.AccelerationYMin/500f, _paramsList.AccelerationYMax/500f));
			float LifeTime = Random.Shared.NextSingle(_paramsList.LifetimeMin, _paramsList.LifetimeMax);
			new LocalObjects.Particle(Rendering.TextureManager.GetTextureFromPNG("Particles/"+_paramsList.TextureName), Utility.GridToWorldPos(_position), velocity,acceleration, LifeTime);
		}
		
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_paramsList);
		message.Add(_position);
	}

	protected override void DeserializeArgs(Message message)
	{
		_paramsList = message.GetSerializable<ParticleParams>();
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