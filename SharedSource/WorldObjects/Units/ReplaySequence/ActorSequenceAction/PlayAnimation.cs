using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class PlayAnimation : UnitSequenceAction
{

	protected override Task GenerateTask()
	{
		throw new NotImplementedException();
	}

	protected override void SerializeArgs(Message message)
	{
		throw new NotImplementedException();
	}

	public PlayAnimation(int actorID) : base(actorID, SequenceType.PlayAnimation)
	{
	}
	
	
}