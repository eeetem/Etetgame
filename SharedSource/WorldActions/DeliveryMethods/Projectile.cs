using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.WorldActions.DeliveryMethods;

public class Projectile : DeliveryMethod
{
	
	readonly int range;
	public Projectile(int range)
	{
		this.range = range;
	}
	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref  WorldObject target)
	{
		if (target.TileLocation.Position == actor.WorldObject.TileLocation.Position)
		{
			return new List<SequenceAction>();
		}

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.WorldObject.TileLocation.Position, target.TileLocation.Position, Cover.Full,visibilityCast: false);
		target = WorldManager.Instance.GetTileAtGrid(outcome.CollisionPointShort).Surface!;
		if(outcome.HitObjId != -1)
		{
			var obj = WorldObjectManager.GetObject(outcome.HitObjId)!;
			if(obj.UnitComponent != null)
			{
				target = obj;
			}
		}
		return new List<SequenceAction>();
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return range+margin;
	}


	public override Tuple<bool, string> CanPerform(Unit actor, WorldObject target, int dimension = -1)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target.TileLocation.Position) > range)
		{
			return new Tuple<bool, string>(false, "Too Far");
		}
		
		return new Tuple<bool, string>(true, "");
	}

}