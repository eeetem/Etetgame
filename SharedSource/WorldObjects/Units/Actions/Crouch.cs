using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.WorldObjects.Units.Actions;

public class Crouch : Action
{
	public Crouch() :base(ActionType.Crouch)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Unit actor, ActionExecutionParamters args)
	{
		if (actor.MovePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points!");
		}
		
		return new Tuple<bool, string>(true, "");
	}
#if SERVER

public override Queue<SequenceAction> GetConsiquenes(Unit actor, ActionExecutionParamters args)
	{

		Visibility vis = Visibility.Full;//inverted
		if (actor.Crouching)
		{
			vis = Visibility.Partial;
		}
		var shooters = ((WorldTile)WorldManager.Instance.GetTileAtGrid(actor.WorldObject.TileLocation.Position)).GetOverWatchShooters(actor,vis);

		var queue = new Queue<SequenceAction>();
		ChangeUnitValues c = ChangeUnitValues.Make(actor.WorldObject.ID,0,-1);
		queue.Enqueue(c);
		
		CrouchUnit crouch = CrouchUnit.Make(actor.WorldObject.ID);
		queue.Enqueue(crouch);

		foreach (var shooter in shooters)
		{
			var act = DelayedAbilityUse.Make(shooter.WorldObject.ID,shooter.Overwatch.Item2,actor.WorldObject.TileLocation.Position);
			queue.Enqueue(act);
		
		}
		
		return queue;
	}

#endif
	


#if CLIENT
	public override List<SequenceAction> Preview(Unit actor, ActionExecutionParamters args,SpriteBatch spriteBatch)
	{
return new List<SequenceAction>();
	}
	
#endif
}