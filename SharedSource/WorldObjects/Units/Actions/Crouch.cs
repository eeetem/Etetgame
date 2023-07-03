using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.ReplaySequence;
#if CLIENT
using MultiplayerXeno.UILayouts;
#endif
namespace MultiplayerXeno;

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
		WorldEffect w = new WorldEffect();
		w.Move.Value = -1;
		w.TargetFriend = true;
		w.TargetSelf = true;
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ReplaySequence.WorldChange(actor.WorldObject.ID,actor.WorldObject.TileLocation.Position,w));
		queue.Enqueue(new ReplaySequence.Crouch(actor.WorldObject.ID));
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