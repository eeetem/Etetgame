using System;
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
		return action.HasEnoughPointsToPerform(actor,false);

	}




#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, Vector2Int target, List<string> args)
	{

		var abilityIndex= int.Parse(args[0]);
		UnitAbility action = actor.Abilities[(abilityIndex)];
		var res = action.GetConsequences(actor, target);
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

		public override string Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch, List<string> Args)
		{
			var abilityIndex= int.Parse(Args[0]);
			UnitAbility action = actor.Abilities[abilityIndex];
			return action.Preview(actor, target,spriteBatch);
		}


#endif

}