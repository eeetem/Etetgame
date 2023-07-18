using System;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
#endif
namespace DefconNull.World.WorldObjects.Units.Actions;

public class Crouch : Action
{
	public Crouch() :base(ActionType.Crouch)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Unit actor, ref Vector2Int position)
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
	public override Queue<SequenceAction> ExecuteServerSide(Unit actor,Vector2Int target)
	{

		Visibility vis = Visibility.Full;//inverted
		if (actor.Crouching)
		{
			vis = Visibility.Partial;
		}
		var shooters = WorldManager.Instance.GetTileAtGrid(actor.WorldObject.TileLocation.Position).GetOverWatchShooters(actor,vis);
		WorldEffect w = new WorldEffect();
		w.Move.Value = -1;
		w.TargetFriend = true;
		w.TargetSelf = true;
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new WorldChange(actor.WorldObject.ID,actor.WorldObject.TileLocation.Position,w));
		queue.Enqueue(new ReplaySequence.Crouch(actor.WorldObject.ID));

		foreach (var shooter in shooters)
		{
			queue.Enqueue(new DoAction(shooter.WorldObject.ID,actor.WorldObject.TileLocation.Position,-1));
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