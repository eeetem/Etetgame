using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class Crouch : SequenceAction
{
	
	public Crouch(int actorID) : base(actorID, SequenceType.Crouch)
	{
	}
	public Crouch(int actorID,Message msg) : base(actorID, SequenceType.Crouch)
	{
	}

	public override void Do()
	{
		Actor.canTurn = true;
		Actor.Crouching = !Actor.Crouching;
		Actor.WorldObject.TileLocation.OverWatchTrigger();
	}

	protected override void SerializeArgs(Message message)
	{
		return;
	}
}