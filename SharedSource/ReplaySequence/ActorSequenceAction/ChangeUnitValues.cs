using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.SharedSource.Units.ReplaySequence;

public class ChangeUnitValues : UnitSequenceAction
{
	private ValueChange actChange;
	private ValueChange moveChange;
	private ValueChange detChange;
	private ValueChange moveRangeEffect;

	public ChangeUnitValues(int actorID, int actChange=0 , int moveChange=0 ,int detChange =0,int moveRangeEffect =0 ) : this(actorID, new ValueChange(actChange), new ValueChange(moveChange), new ValueChange(detChange), new ValueChange(moveRangeEffect))
	{
		
	}


	public ChangeUnitValues(int actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null) : base(new TargetingRequirements(actorID),  SequenceType.ChangeUnitValues)
	{
		if (actChange.HasValue)
		{
			this.actChange = actChange.Value;
		}
		
		if (moveChange.HasValue)
		{
			this.moveChange = moveChange.Value;
		}
		
		if (detChange.HasValue)
		{
			this.detChange = detChange.Value;
		}
		
		if (moveRangeEffect.HasValue)
		{
			this.moveRangeEffect = moveRangeEffect.Value;
		}
	}
	public ChangeUnitValues(TargetingRequirements actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null) : base(actorID,  SequenceType.ChangeUnitValues)
	{
		if (actChange.HasValue)
		{
			this.actChange = actChange.Value;
		}
		
		if (moveChange.HasValue)
		{
			this.moveChange = moveChange.Value;
		}
		
		if (detChange.HasValue)
		{
			this.detChange = detChange.Value;
		}
		
		if (moveRangeEffect.HasValue)
		{
			this.moveRangeEffect = moveRangeEffect.Value;
		}
	}
    
	public ChangeUnitValues(TargetingRequirements actorID, Message msg) : base(actorID, SequenceType.ChangeUnitValues)
	{
		actChange = msg.GetSerializable<ValueChange>();
		moveChange = msg.GetSerializable<ValueChange>();
		detChange = msg.GetSerializable<ValueChange>();
		moveRangeEffect = msg.GetSerializable<ValueChange>();
	}
	

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			
			actChange.Apply(ref Actor.ActionPoints);
			moveChange.Apply(ref Actor.MovePoints);
			detChange.Apply(ref Actor.Determination);
			moveRangeEffect.Apply(ref Actor.MoveRangeEffect);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(actChange);
		message.Add(moveChange);
		message.Add(detChange);
		message.Add(moveRangeEffect);
	}

#if CLIENT

	protected override void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}
#endif
}