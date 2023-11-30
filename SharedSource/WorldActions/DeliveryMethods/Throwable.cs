using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.WorldActions.DeliveryMethods;

public class Throwable : DeliveryMethod
{
	
	readonly int throwRange;
	public Throwable(int throwRange)
	{
		this.throwRange = throwRange;
	}
	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref  WorldObject target)
	{
		if (target.TileLocation.Position == actor.WorldObject.TileLocation.Position)
		{
			return new List<SequenceAction>();
		}

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.WorldObject.TileLocation.Position, target.TileLocation.Position, Cover.Full,visibilityCast: false,ignoreControllables: true);
		target = WorldManager.Instance.GetTileAtGrid(outcome.CollisionPointShort).Surface!;
		return new List<SequenceAction>();
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return throwRange+margin;
	}


	public override Tuple<bool, string> CanPerform(Unit actor, WorldObject target, int dimension = -1)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target.TileLocation.Position) > throwRange)
		{
			return new Tuple<bool, string>(false, "Too Far");
		}
		
		return new Tuple<bool, string>(true, "");
	}

}