using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.SharedSource.Units.ReplaySequence;

public class ChangeUnitValues : UnitSequenceAction
{
	public readonly ValueChange ActChange;
	public readonly ValueChange MoveChange;
	public readonly ValueChange DetChange;
	public readonly ValueChange MoveRangeeffectChange;
	

	public ChangeUnitValues(int actorID, int actChange=0 , int moveChange=0 ,int detChange =0,int moveRangeEffect =0 ) : this(actorID, new ValueChange(actChange), new ValueChange(moveChange), new ValueChange(detChange), new ValueChange(moveRangeEffect))
	{
		
	}


	public ChangeUnitValues(int actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null) : base(new TargetingRequirements(actorID),  SequenceType.ChangeUnitValues)
	{
		if (actChange.HasValue)
		{
			ActChange = actChange.Value;
		}
		
		if (moveChange.HasValue)
		{
			MoveChange = moveChange.Value;
		}
		
		if (detChange.HasValue)
		{
			DetChange = detChange.Value;
		}
		
		if (moveRangeEffect.HasValue)
		{
			MoveRangeeffectChange = moveRangeEffect.Value;
		}
	}
	public ChangeUnitValues(TargetingRequirements actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null) : base(actorID,  SequenceType.ChangeUnitValues)
	{
		if (actChange.HasValue)
		{
			ActChange = actChange.Value;
		}
		
		if (moveChange.HasValue)
		{
			MoveChange = moveChange.Value;
		}
		
		if (detChange.HasValue)
		{
			DetChange = detChange.Value;
		}
		
		if (moveRangeEffect.HasValue)
		{
			MoveRangeeffectChange = moveRangeEffect.Value;
		}
	}
    
	public ChangeUnitValues(TargetingRequirements actorID, Message msg) : base(actorID, SequenceType.ChangeUnitValues)
	{
		ActChange = msg.GetSerializable<ValueChange>();
		MoveChange = msg.GetSerializable<ValueChange>();
		DetChange = msg.GetSerializable<ValueChange>();
		MoveRangeeffectChange = msg.GetSerializable<ValueChange>();
	}
	

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			ActChange.Apply(ref Actor.ActionPoints);
			MoveChange.Apply(ref Actor.MovePoints);
			DetChange.Apply(ref Actor.Determination);
			MoveRangeeffectChange.Apply(ref Actor.MoveRangeEffect);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(ActChange);
		message.Add(MoveChange);
		message.Add(DetChange);
		message.Add(MoveRangeeffectChange);
	}



#if CLIENT

	protected override void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}
#endif
}