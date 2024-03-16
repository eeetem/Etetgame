using System;
using Riptide;

namespace DefconNull.ReplaySequence;

public class UnitUpdate : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.UnitUpdate;
	}

	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#endif
		
	}

	protected override void SerializeArgs(Message message)
	{
		throw new System.NotImplementedException();
	}

	protected override void DeserializeArgs(Message message)
	{
		throw new System.NotImplementedException();
	}
}