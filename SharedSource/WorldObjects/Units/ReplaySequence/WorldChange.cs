using System;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class WorldChange : SequenceAction
{
	private WorldEffect effect;
	private Vector2Int target;
	public WorldChange(int actorID, Vector2Int target, WorldEffect effect) : base(actorID, SequenceType.WorldEffect)
	{
		this.target = target;
		this.effect = effect;
	}
	public WorldChange(int actorID, Message args) : base(actorID, SequenceType.WorldEffect)
	{
		this.target = args.GetSerializable<Vector2Int>();
		this.effect = args.GetSerializable<WorldEffect>();
	}
	
	public override void Do()
	{
		effect.Apply(target, Actor.WorldObject);
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(target);
		message.Add(effect);
	}

	
}