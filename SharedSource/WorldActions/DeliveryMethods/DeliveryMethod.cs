using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public abstract class DeliveryMethod
{
	public abstract Tuple<bool, string> CanPerform(Unit actor, WorldObject target,int dimension = -1);

	//do NOT convert this to old system, keep sequence actions for things like breaking windows with throwables
	public List<SequenceAction> ExectuteAndProcessLocation(Unit actor,ref WorldObject target)
	{
		
		return ExectuteAndProcessLocationChild(actor, ref target);
	}

	public abstract List<SequenceAction> ExectuteAndProcessLocationChild(Unit actor,ref WorldObject target);

	public abstract float GetOptimalRangeAI(float margin);


}