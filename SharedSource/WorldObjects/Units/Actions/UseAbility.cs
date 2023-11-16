﻿using System;
using System.Collections.Generic;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects.Units.ReplaySequence;

using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldObjects.Units.Actions;

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
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, ActionExecutionParamters args)
	{
		UnitAbility action = actor.Abilities[args.AbilityIndex];
		var res = action.GetConsequences(actor, args.TargetObj!);
		var queue = new Queue<SequenceAction>();
		
		MoveCamera m = MoveCamera.Make(actor.WorldObject.TileLocation.Position,false,1);
		queue.Enqueue(m);
		

		
		foreach (var sequenceAction in res)
		{
			queue.Enqueue(sequenceAction);
		}
		return queue;


	}
#endif
	



#if CLIENT

		public override void Preview(Unit actor, ActionExecutionParamters args, SpriteBatch spriteBatch)
		{
			UnitAbility action = actor.Abilities[args.AbilityIndex];
			action.Preview(actor, args.TargetObj!,spriteBatch);
		}


#endif

}