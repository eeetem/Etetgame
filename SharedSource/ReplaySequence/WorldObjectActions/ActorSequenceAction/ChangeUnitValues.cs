using System;
using System.Collections.Generic;
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
	public ValueChange MoveRangeEffectChange;

	public override BatchingMode Batching => BatchingMode.Sequential;

	protected bool Equals(ChangeUnitValues other)
	{
		return base.Equals(other) && ActChange.Equals(other.ActChange) && MoveChange.Equals(other.MoveChange) && DetChange.Equals(other.DetChange) && MoveRangeEffectChange.Equals(other.MoveRangeEffectChange);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((ChangeUnitValues) obj);
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), ActChange, MoveChange, DetChange, MoveRangeEffectChange);
	}

	public static ChangeUnitValues Make(int actorID, int actChange = 0, int moveChange = 0, int detChange = 0, int moveRangeEffect = 0)
	{
		return Make(actorID, new ValueChange(actChange), new ValueChange(moveChange), new ValueChange(detChange), new ValueChange(moveRangeEffect));
	}


	public static ChangeUnitValues Make(int actorID, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null) 
	{
		return Make(new TargetingRequirements(actorID), actChange, moveChange, detChange, moveRangeEffect);
		
	}

	public static ChangeUnitValues Make(Vector2 pos, ValueChange? actChange = null, ValueChange? moveChange = null, ValueChange? detChange = null, ValueChange? moveRangeEffect = null)
	{
		return Make(new TargetingRequirements(pos), actChange, moveChange, detChange, moveRangeEffect);
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
			t.MoveRangeEffectChange = moveRangeEffect.Value;
		}
		else
		{
			t.MoveRangeEffectChange = new ValueChange(0);
		}

		return t;
	}

	public override SequenceType GetSequenceType()
	{
		return SequenceType.ChangeUnitValues;
	}

	protected override void RunSequenceAction()
	{
		var detChange = DetChange.GetChange(Actor.Determination);
		ActChange.Apply(ref Actor.ActionPoints);
		MoveChange.Apply(ref Actor.MovePoints);
		DetChange.Apply(ref Actor.Determination);
		if(Actor.Determination > 0 && Actor.Paniced)
		{
			Actor.Paniced = false;
		}
		if(MoveRangeEffectChange.Value != 0)
		{
			MoveRangeEffectChange.Apply(ref Actor.MoveRangeEffect);
		}

#if CLIENT
		if(detChange > 0)
		{
			var t = new PopUpText("Determination:"+detChange, Actor.WorldObject.TileLocation.Position,Color.Green);	
		}

#endif

	}

	public override string ToString()
	{
		return $"{base.ToString()}, {nameof(ActChange)}: {ActChange}, {nameof(MoveChange)}: {MoveChange}, {nameof(DetChange)}: {DetChange}, {nameof(MoveRangeEffectChange)}: {MoveRangeEffectChange}";
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(ActChange);
		message.Add(MoveChange);
		message.Add(DetChange);
		message.Add(MoveRangeEffectChange);
	}

	protected override void DeserializeArgs(Message msg)
	{
		base.DeserializeArgs(msg);
		ActChange = msg.GetSerializable<ValueChange>();
		MoveChange = msg.GetSerializable<ValueChange>();
		DetChange = msg.GetSerializable<ValueChange>();
		MoveRangeEffectChange = msg.GetSerializable<ValueChange>();
	}

#if CLIENT

	public override void DrawConsequence(Vector2 pos, SpriteBatch batch)
	{
		Texture2D upArrow = TextureManager.GetTexture("HoverHud/Consequences/upArrow");
		Texture2D downArrow = TextureManager.GetTexture("HoverHud/Consequences/downArrow");
	
		
		Vector2 offset = new Vector2(upArrow.Width-2,0);
		int i = 3;
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
				batch.Draw(upArrow, pos+offset*i, Color.White);
			}
			else
			{
				batch.Draw(downArrow, pos+offset*i, Color.White);
			}
			batch.DrawNumberedIcon(Math.Abs(detChange).ToString(),TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"),pos+offset*i+new Vector2(10,0),Color.White);
			i--;
		}
	
	}

	public override void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
	{
		batch.DrawText("" +
		               "           Value Change:\n" +
		               "  Move Point [Yellow]Change[-]\n" +
		               "  Action Point [Yellow]Change[-]\n" +
		               "  Determination [Yellow]Change[-]\n" +
		               "  Move Range [Yellow]Change[-]\n" +
		               "  todo sight\n", pos, scale, Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/movePoint"), pos + new Vector2(0, 5)*scale,scale/2f,Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/circlePoint"), pos + new Vector2(0, 16)*scale,scale/2f,Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"), pos + new Vector2(0, 28)*scale,scale/2f,Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/genericDamage"), pos + new Vector2(0, 40)*scale,scale/2f,Color.White);

	
	}

	public override void Preview(SpriteBatch spriteBatch)
	{
		if (Requirements.Position != new Vector2Int(-1, -1))
		{
			var t = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
			if(t.Surface is null)return;
			Texture2D sprite = t.Surface.GetTexture();
			spriteBatch.Draw(sprite, t.Surface.GetDrawTransform().Position, Color.Yellow * 0.2f);
			spriteBatch.DrawOutline(new List<WorldTile>(){t}, Color.Green, 0.5f);
		}
	}


#endif

}