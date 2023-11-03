﻿using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.SharedSource.Units.ReplaySequence;

public class ChangeUnitValues : UnitSequenceAction
{
	public ValueChange ActChange;
	public ValueChange MoveChange;
	public ValueChange DetChange;
	public ValueChange MoveRangeeffectChange;


	public static ChangeUnitValues Make(int actorID, int actChange = 0, int moveChange = 0, int detChange = 0, int moveRangeEffect = 0)
	{
		return Make(actorID, new ValueChange(actChange), new ValueChange(moveChange), new ValueChange(detChange), new ValueChange(moveRangeEffect));
	}


	public static ChangeUnitValues Make(int actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null) 
	{
		return Make(new TargetingRequirements(actorID), actChange, moveChange, detChange, moveRangeEffect);
		
	}
	
	public static ChangeUnitValues Make(TargetingRequirements actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null)
	{
		var t = GetAction(SequenceType.ChangeUnitValues) as ChangeUnitValues;
		t.Requirements = actorID;
		if (actChange.HasValue)
		{
			t.ActChange = actChange.Value;
		}
		
		if (moveChange.HasValue)
		{
			t.MoveChange = moveChange.Value;
		}
		
		if (detChange.HasValue)
		{
			t.DetChange = detChange.Value;
		}
		
		if (moveRangeEffect.HasValue)
		{
			t.MoveRangeeffectChange = moveRangeEffect.Value;
		}
		return t;
	}

	public override SequenceType GetSequenceType()
	{
		return SequenceType.ChangeUnitValues;
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

	protected override void DeserializeArgs(Message msg)
	{
		base.DeserializeArgs(msg);
		ActChange = msg.GetSerializable<ValueChange>();
		MoveChange = msg.GetSerializable<ValueChange>();
		DetChange = msg.GetSerializable<ValueChange>();
		MoveRangeeffectChange = msg.GetSerializable<ValueChange>();
	}

#if CLIENT

	protected override void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}


#endif

}