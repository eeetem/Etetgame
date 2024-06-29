using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;

namespace DefconNull.WorldActions.DeliveryMethods;

public class VissionCast : DeliveryMethod
{
	
	readonly int _range;
	public VissionCast(int range)
	{
		this._range = range;
	}


	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref WorldObject target)
	{
		Vector2Int vectarget =  target!.TileLocation.Position;
		if (Visibility.None == WorldManager.Instance.VisibilityCast(actor.WorldObject.TileLocation.Position, vectarget, 200,actor.Crouching))
		{
			throw new Exception("Trying to execute sight beyond sight");
			return new List<SequenceAction>();
		}
		return new List<SequenceAction>();
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return _range+margin;
	}


	public override Tuple<bool,bool, string>  CanPerform(Unit actor, WorldObject target, int dimension = -1)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target.TileLocation.Position) >= _range)
		{
			return new Tuple<bool,bool, string> (false, false,"Too Far");
		}
		if (Visibility.None == WorldManager.Instance.VisibilityCast(actor.WorldObject.TileLocation.Position, target.TileLocation.Position, _range,actor.Crouching))
		{
			return new Tuple<bool,bool, string> (false, false,"No Sight");
		}
		
		return new Tuple<bool,bool, string> (true, true, "");
	}


}