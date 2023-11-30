using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;

namespace DefconNull.WorldActions.DeliveryMethods;

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