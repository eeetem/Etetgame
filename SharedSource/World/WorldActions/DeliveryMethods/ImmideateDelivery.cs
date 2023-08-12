using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public class ImmideateDelivery : DeliveryMethod
{
	public override Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{
		return new Tuple<bool, string>(true, "");
		
	}
	public override List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref Vector2Int? target)
	{
		target = actor.WorldObject.TileLocation.Position;
		return new List<SequenceAction>();
	}
	public override float GetOptimalRangeAI(float margin)
	{
		return margin;
	}


}