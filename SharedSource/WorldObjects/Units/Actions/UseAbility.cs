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

	public override Tuple<bool, string> CanPerform(Unit actor,  Vector2Int target, List<string> args)
	{
		var abilityIndex= int.Parse(args[0]);
		UnitAbility action = actor.Abilities[(abilityIndex)];
		return action.CanPerform(actor, target,false,false);

	}




#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, Vector2Int target, List<string> args)
	{

		var abilityIndex= int.Parse(args[0]);
		UnitAbility action = actor.Abilities[(abilityIndex)];
		var res = action.GetConsequences(actor, target);
		var queue = new Queue<SequenceAction>();
		
		MoveCamera m = (MoveCamera) SequenceAction.GetAction(SequenceAction.SequenceType.MoveCamera);
		m.location = actor.WorldObject.TileLocation.Position;
		m.doAlways = false;
		m.scatter = 1;
		queue.Enqueue(m);
		

		
		foreach (var sequenceAction in res)
		{
			queue.Enqueue(sequenceAction);
		}
		return queue;


	}
#endif
	



#if CLIENT

		public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch, List<string> Args)
		{
			var abilityIndex= int.Parse(Args[0]);
			UnitAbility action = actor.Abilities[abilityIndex];
			action.Preview(actor, target,spriteBatch);
		}


#endif

}