using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;

namespace DefconNull.World.WorldActions.DeliveryMethods;

public class VissionCast : DeliveryMethod
{
	
	readonly int range;
	public VissionCast(int range)
	{
		this.range = range;
	}


	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref Vector2Int? target)
	{
		Vector2Int vectarget =  target!.Value;
		if (Visibility.None == WorldManager.Instance.CanSee(actor, vectarget, true))
		{
			target = null;
			return new List<SequenceAction>();
		}
		return new List<SequenceAction>();
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return range+margin;
	}


	public override Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target) >= range)
		{
			return new Tuple<bool, string>(false, "Too Far");
		}
		if (Visibility.None == WorldManager.Instance.CanSee(actor, target, true))
		{
			return new Tuple<bool, string>(false, "No Sight");
		}
		
		return new Tuple<bool, string>(true, "");
	}


}