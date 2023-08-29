﻿using System;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
#endif
namespace DefconNull.World.WorldObjects.Units.Actions;

public class Crouch : Action
{
	public Crouch() :base(ActionType.Crouch)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Unit actor, Vector2Int position)
	{
		if (actor.MovePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points!");
		}
		
		return new Tuple<bool, string>(true, "");
	}
#if CLIENT
	public override void ExecuteClientSide(Unit actor, Vector2Int target)
	{
		base.ExecuteClientSide(actor, target);
	}

#else
	public override Queue<SequenceAction> GetConsiquenes(Unit actor,Vector2Int target)
	{

		Visibility vis = Visibility.Full;//inverted
		if (actor.Crouching)
		{
			vis = Visibility.Partial;
		}
		var shooters = ((WorldTile)WorldManager.Instance.GetTileAtGrid(actor.WorldObject.TileLocation.Position)).GetOverWatchShooters(actor,vis);

		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ChangeUnitValues(actor.WorldObject.ID,0,-1,0,0));
		queue.Enqueue(new CrouchUnit(actor.WorldObject.ID));

		foreach (var shooter in shooters)
		{
			UseAbility.AbilityIndex = -0;
			var res = Actions[ActionType.UseAbility].GetConsiquenes(shooter,actor.WorldObject.TileLocation.Position);
			foreach (var a in res)
			{
				queue.Enqueue(a);
			}
		
		}
		
		return queue;
	}
#endif


#if CLIENT
	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new NotImplementedException();
	}


#endif
}