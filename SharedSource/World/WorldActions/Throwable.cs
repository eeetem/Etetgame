using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.World.WorldActions;

public class Throwable : DeliveryMethod
{
	
	readonly int throwRange;
	public Throwable(int throwRange)
	{
		this.throwRange = throwRange;
	}

	private static Vector2Int? lastReturned;
	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref Vector2Int? target)
	{
		if (target!.Value == actor.WorldObject.TileLocation.Position)
		{
			return new List<SequenceAction>();
		}

		if (Vector2.Distance(target.Value, actor.WorldObject.TileLocation.Position) > throwRange)
		{
			target = lastReturned;
			return new List<SequenceAction>();
		}

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.WorldObject.TileLocation.Position, target!.Value, Cover.Full,false,true);
		lastReturned = outcome.EndPoint;
		target= outcome.CollisionPointShort;
		return new List<SequenceAction>();
	}

	public override float GetOptimalRangeAI(float margin)
	{
		return throwRange+margin;
	}


	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target) > throwRange)
		{
			if (lastReturned != null && Vector2.Distance(actor.WorldObject.TileLocation.Position, lastReturned.Value) <= throwRange)
			{
				target = lastReturned.Value;
				return new Tuple<bool, string>(true, "");
			}

			return new Tuple<bool, string>(false, "Too Far");
		}
		
		return new Tuple<bool, string>(true, "");
	}

}