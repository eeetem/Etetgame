﻿using System;
using System.Threading.Tasks;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class PlayAnimation : SequenceAction
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