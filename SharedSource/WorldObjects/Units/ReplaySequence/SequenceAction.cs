namespace MultiplayerXeno.ReplaySequence;

public abstract class SequenceAction
{
	protected int ActorID;
	public SequenceAction(int actorID)
	{
		ActorID = actorID;
	}
	public abstract void Do();
}