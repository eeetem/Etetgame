using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

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




	public override Queue<SequenceAction>[] GetConsequenes(Unit actor, ActionExecutionParamters args)
	{
		UnitAbility action = actor.Abilities[args.AbilityIndex];
		
		var queue1 = new Queue<SequenceAction>();
		
		MoveCamera m = MoveCamera.Make(actor.WorldObject.TileLocation.Position,false,1);
		queue1.Enqueue(m);
		
		var turnact = FaceUnit.Make(actor.WorldObject.ID, args.Target,false);
		queue1.Enqueue(turnact);

		var res = action.GetConsequences(actor, args.GetTarget());
		var queue2 = new Queue<SequenceAction>();
		foreach (var sequenceAction in res)
		{
			queue2.Enqueue(sequenceAction);
		}
		return new Queue<SequenceAction>[] {queue1,queue2};


	}




#if CLIENT
		private ActionExecutionParamters _lastArgs = new ActionExecutionParamters();
		private Unit? _lastActor = null;
		private Vector2Int? _lastActorPos = null;
		List<SequenceAction> _previewCache = new List<SequenceAction>();

		public override void Preview(Unit actor, ActionExecutionParamters args, SpriteBatch spriteBatch)
		{
			UnitAbility action = actor.Abilities[args.AbilityIndex];
			if (_lastActor == null || !Equals(_lastArgs, args) || !Equals(_lastActor, actor) || _lastActorPos != actor.WorldObject.TileLocation.Position)
			{
				_previewCache = action.GetConsequences(actor, args.GetTarget());
				_lastActor = actor;
				_lastArgs = args;
				_lastActorPos = actor.WorldObject.TileLocation.Position;
			}
			var pos = actor.WorldObject.TileLocation.Position;
			spriteBatch.DrawLine(Utility.GridToWorldPos(actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(pos+ new Vector2(0.5f, 0.5f)), Color.Red, 2);

			foreach (var c in _previewCache)
			{
				c.Preview(spriteBatch);
			}
			
			
		}


#endif

}