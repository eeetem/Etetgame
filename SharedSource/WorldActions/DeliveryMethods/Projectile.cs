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
	private readonly bool ignoreUnits;
	public Projectile(int range, int spot, bool ignoreUnits = false)
	{
		this.range = range;
		this.spot = spot;
		this.ignoreUnits = ignoreUnits;
	}

	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref  WorldObject target)
	{
		List<SequenceAction> list = new List<SequenceAction>();
		if (target.TileLocation.Position == actor.WorldObject.TileLocation.Position)
		{
			list.Add(MoveCamera.Make(target.TileLocation.Position,true,spot));
			return list;
		}

		//if out of range pick furthest point in range
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target.TileLocation.Position) > range)
		{

			Vector2 direction = Vector2.Normalize(target.TileLocation.Position - actor.WorldObject.TileLocation.Position);
			Vector2 newTargetPosition = actor.WorldObject.TileLocation.Position + direction * range;
			if (WorldManager.Instance.GetTileAtGrid(newTargetPosition).Surface != null)
			{
				target = WorldManager.Instance.GetTileAtGrid(newTargetPosition).Surface!;
			}
			else
			{
				target = actor.WorldObject; //there's no tile so just shoot yourself isntead lmao
				return list;
			}
		}

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.WorldObject.TileLocation.Position, target.TileLocation.Position, Cover.Full,visibilityCast: false, ignoreControllables:ignoreUnits);
		target = WorldManager.Instance.GetTileAtGrid(outcome.CollisionPointShort).Surface!;
		if(outcome.HitObjId != -1)
		{
			var obj = WorldObjectManager.GetObject(outcome.HitObjId)!;
			if(obj.UnitComponent != null)
			{
				target = obj;
			}
		}
		list.Add(MoveCamera.Make(target.TileLocation.Position,true,spot));
		list.Add( ProjectileAction.Make(actor.WorldObject.TileLocation.Position, target.TileLocation.Position));
		return list;
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return range+margin;
	}


	public override Tuple<bool,bool, string>  CanPerform(Unit actor, WorldObject target, int dimension = -1)
	{

		
		return new Tuple<bool,bool, string> (true,true, "");
	}

}