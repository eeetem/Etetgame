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
	readonly int spot;
	public Projectile(int range, int spot)
	{
		this.range = range;
		this.spot = spot;
	}

	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref  WorldObject target)
	{
		List<SequenceAction> list = new List<SequenceAction>();
		list.Add(MoveCamera.Make(target.TileLocation.Position,true,spot));
		if (target.TileLocation.Position == actor.WorldObject.TileLocation.Position)
		{
			return list;
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
		return list;
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return range+margin;
	}


	public override Tuple<bool,bool, string>  CanPerform(Unit actor, WorldObject target, int dimension = -1)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target.TileLocation.Position) > range)
		{
			return new Tuple<bool,bool, string> (false,false, "Too Far");
		}
		
		return new Tuple<bool,bool, string> (true,true, "");
	}

}