using System;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public abstract class SequenceAction : IMessageSerializable
{
	public enum SequenceType
	{
		Attack=1,
		Move=2,
		Face=3,
		Crouch=4,
		OverWatch = 5,
		UseItem = 6,
		WorldEffect = 9,
		PlayAnimation =10,
	}
	public readonly SequenceType SqcType;

	protected int ActorID;
	protected Unit Actor => WorldManager.Instance.GetObject(ActorID).UnitComponent;
	public SequenceAction(int actorID,SequenceType tp)
	{
		SqcType = tp;
		ActorID = actorID;
	}
	public abstract void Do();
	
	
	protected abstract void SerializeArgs(Message message);

	public void Serialize(Message message)
	{
		message.Add(ActorID);
		SerializeArgs(message);
	}

	public void Deserialize(Message message)
	{
		throw new Exception("cannot deserialize abstract SequenceAction");
	}
}