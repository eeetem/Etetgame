using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public abstract class DeliveryMethod
{
	public abstract Tuple<bool, string> CanPerform(Unit actor, Vector2Int target,int dimension = -1);

	//do NOT convert this to old system, keep sequence actions for things like breaking windows with throwables
	public List<SequenceAction> ExectuteAndProcessLocation(Unit actor,ref Vector2Int? target)
	{

		var res = CanPerform(actor, target.Value);
		if (!res.Item1)
		{
			throw new Exception("Execution of action that can't be performed: "+res.Item2);
		}


		return ExectuteAndProcessLocationChild(actor, ref target);
	}

	public abstract List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref Vector2Int? target);

	public abstract float GetOptimalRangeAI(float margin);


}