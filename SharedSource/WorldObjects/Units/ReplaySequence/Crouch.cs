using System.Threading.Tasks;
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

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.canTurn = true;
			Actor.Crouching = !Actor.Crouching;
			Actor.WorldObject.TileLocation.OverWatchTrigger();
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		return;
	}
}