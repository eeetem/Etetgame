using System;
using System.Threading.Tasks;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class ChangeUnitValues : UnitSequenceAction
{
	public ValueChange ActChange;
	public ValueChange MoveChange;
	public ValueChange DetChange;
	public ValueChange MoveRangeeffectChange;

	public override BatchingMode Batching => BatchingMode.Sequential;

	protected bool Equals(ChangeUnitValues other)
	{
		return base.Equals(other) && ActChange.Equals(other.ActChange) && MoveChange.Equals(other.MoveChange) && DetChange.Equals(other.DetChange) && MoveRangeeffectChange.Equals(other.MoveRangeeffectChange);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((ChangeUnitValues) obj);
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), ActChange, MoveChange, DetChange, MoveRangeeffectChange);
	}

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
		}else
		{
			t.ActChange = new ValueChange(0);
		}
		
		if (moveChange.HasValue)
		{
			t.MoveChange = moveChange.Value;
		}else
		{
			t.MoveChange = new ValueChange(0);
		}
		
		if (detChange.HasValue)
		{
		
			t.DetChange = detChange.Value;
		}else
		{
			t.DetChange = new ValueChange(0);
		}
		
		if (moveRangeEffect.HasValue)
		{
			t.MoveRangeeffectChange = moveRangeEffect.Value;
		}
		else
		{
			t.MoveRangeeffectChange = new ValueChange(0);
		}

		return t;
	}

	public override SequenceType GetSequenceType()
	{
		return SequenceType.ChangeUnitValues;
	}

	protected override void RunSequenceAction()
	{
		
		ActChange.Apply(ref Actor.ActionPoints);
		MoveChange.Apply(ref Actor.MovePoints);
		DetChange.Apply(ref Actor.Determination);
		if(MoveRangeeffectChange.Value != 0)
		{
			MoveRangeeffectChange.Apply(ref Actor.MoveRangeEffect);
		}
		
	}

	public override string ToString()
	{
		return $"{base.ToString()}, {nameof(ActChange)}: {ActChange}, {nameof(MoveChange)}: {MoveChange}, {nameof(DetChange)}: {DetChange}, {nameof(MoveRangeeffectChange)}: {MoveRangeeffectChange}";
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

	public override void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}


#endif

}