using System;
using System.Threading.Tasks;

using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

#if CLIENT
using DefconNull.Rendering;
#endif

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

	public override void DrawDesc(Vector2 pos, SpriteBatch batch)
	{
		Texture2D upArrow = TextureManager.GetTexture("HoverHud/Consequences/upArrow");
		Texture2D downArrow = TextureManager.GetTexture("HoverHud/Consequences/downArrow");
	
		
		Vector2 offset = new Vector2(upArrow.Width-2,0);
		int i = 4;
		var moveChange = MoveChange.GetChange(Actor.MovePoints);
		if (moveChange != 0)
		{
			if (moveChange > 0)
			{
				batch.Draw(upArrow, pos + offset*i, Color.White);
			}
			else
			{
				batch.Draw(downArrow, pos + offset*i, Color.White);
			}

			batch.DrawNumberedIcon(Math.Abs(moveChange).ToString(), TextureManager.GetTexture("HoverHud/Consequences/movePoint"), pos + offset*i + new Vector2(10, 0), Color.White);
			i--;
		}

		var actChange = ActChange.GetChange(Actor.ActionPoints);
		if (actChange != 0)
		{
			if (actChange > 0)
			{
				batch.Draw(upArrow, pos + offset*i, Color.White);
			}
			else
			{
				batch.Draw(downArrow, pos + offset*i, Color.White);
			}
			batch.DrawNumberedIcon(Math.Abs(actChange).ToString(), TextureManager.GetTexture("HoverHud/Consequences/circlePoint"), pos +offset*i + new Vector2(10, 0), Color.White);
			i--;
		}

		var detChange = DetChange.GetChange(Actor.Determination);
		if(detChange != 0){
			if(detChange > 0)
			{
				batch.Draw(upArrow, pos+offset*3, Color.White);
			}
			else
			{
				batch.Draw(downArrow, pos+offset*3, Color.White);
			}
			batch.DrawNumberedIcon(Math.Abs(detChange).ToString(),TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"),pos+offset*3+new Vector2(10,0),Color.White);
			i--;
		}
	
	}
	public override void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}


#endif

}