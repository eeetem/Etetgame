using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.WorldObjects.Units.Actions;

public class UseAbility : Action
{
	
	public UseAbility() : base(ActionType.UseAbility)
	{
	}

	public override Tuple<bool, string> CanPerform(Unit actor, ActionExecutionParamters args)
	{
		UnitAbility action = actor.Abilities[args.AbilityIndex];
		return action.HasEnoughPointsToPerform(actor,false);
	}




#if SERVER
	public override Queue<SequenceAction>[] GetConsiquenes(Unit actor, ActionExecutionParamters args)
	{
		UnitAbility action = actor.Abilities[args.AbilityIndex];
		var res = action.GetConsequences(actor, args.TargetObj!);
		var queue1 = new Queue<SequenceAction>();
		
		MoveCamera m = MoveCamera.Make(actor.WorldObject.TileLocation.Position,false,1);
		queue1.Enqueue(m);
		var turnact = FaceUnit.Make(actor.WorldObject.ID, args.TargetObj!.TileLocation.Position);
		queue1.Enqueue(turnact);

		var queue2 = new Queue<SequenceAction>();
		foreach (var sequenceAction in res)
		{
			queue2.Enqueue(sequenceAction);
		}
		return new Queue<SequenceAction>[] {queue1,queue2};


	}
#endif
	



#if CLIENT

		public override List<SequenceAction> Preview(Unit actor, ActionExecutionParamters args, SpriteBatch spriteBatch)
		{
			UnitAbility action = actor.Abilities[args.AbilityIndex];
			return action.Preview(actor, args.TargetObj!,spriteBatch);
			
		}


#endif

}