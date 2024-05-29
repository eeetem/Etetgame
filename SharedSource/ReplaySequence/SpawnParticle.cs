using Riptide;

namespace DefconNull.ReplaySequence;

public class SpawnParticle : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.SpawnParticle;
	}
	string textureName;
	Vector2Int Position;
	Vector2Int Velocity;
	Vector2Int Acceleration;
	float LifeTime;
	
	public static SpawnParticle Make(Vector2Int position,Vector2Int velocity,Vector2Int acceleration, float lifeTime, string textureName) 
	{
		SpawnParticle t = (GetAction(SequenceType.SpawnParticle) as SpawnParticle)!;
		t.textureName = textureName;
		t.Position = position;
		t.Velocity = velocity;
		t.LifeTime = lifeTime;
		return t;
	} 


	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		new LocalObjects.Particle(Rendering.TextureManager.GetTexture(textureName), Position, Velocity, LifeTime);
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(textureName);
		message.Add(Position);
		message.Add(Velocity);
		message.Add(LifeTime);
		message.Add(Acceleration);
	}

	protected override void DeserializeArgs(Message message)
	{
		textureName = message.GetString();
		Position = message.GetSerializable<Vector2Int>();
		Velocity = message.GetSerializable<Vector2Int>();
		LifeTime = message.GetFloat();
		Acceleration = message.GetSerializable<Vector2Int>();
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
		return WorldManager.Instance.GetTileAtGrid(Position).IsVisible(Visibility.Partial,player1);
	}
#endif

}