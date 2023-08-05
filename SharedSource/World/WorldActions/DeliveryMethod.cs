using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public abstract class DeliveryMethod
{
	public Vector2Int offset = new Vector2Int(0,0);
	public abstract Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target);

	public List<SequenceAction> ExectuteAndProcessLocation(Unit actor,ref Vector2Int? target)
	{
		if (target == null)
		{
			target = offset;
		}
		else
		{
			target = (Vector2Int)target + offset;
		}

		
		return ExectuteAndProcessLocationChild(actor, ref target);
	}

	public abstract List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref Vector2Int? target);

	//todo preview
	public abstract float GetOptimalRangeAI(float margin);

}