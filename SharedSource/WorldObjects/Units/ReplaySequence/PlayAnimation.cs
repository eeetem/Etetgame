using System;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class PlayAnimation : SequenceAction
{
	public override void Do()
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