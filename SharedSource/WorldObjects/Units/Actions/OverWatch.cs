using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using FontStashSharp;
#endif
namespace DefconNull.WorldObjects.Units.Actions;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}


	public override Queue<SequenceAction>[] GetConsequenes(Unit actor, ActionExecutionParamters args)
	{
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(UnitOverWatch.Make(actor.WorldObject.ID,args.Target!.Value,args.AbilityIndex));
		return new Queue<SequenceAction>[] {queue};
	}





	public override Tuple<bool, string> CanPerform(Unit actor, ActionExecutionParamters args)
	{
	
		UnitAbility action = actor.Abilities[args.AbilityIndex];
		return action.HasEnoughPointsToPerform(actor, false);
	}

#if CLIENT

	public override void Preview(Unit actor, ActionExecutionParamters args,SpriteBatch spriteBatch)
	{
		foreach (var loc in actor.GetOverWatchPositions(args.Target!.Value,args.AbilityIndex))
		{
			var tile = WorldManager.Instance.GetTileAtGrid(loc);
			if (tile.Surface == null) continue;
			Color c = Color.Green * 0.45f;
			
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position, c);
		}
	}

#endif
}