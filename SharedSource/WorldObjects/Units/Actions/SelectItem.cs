using System;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldObjects.Units.Actions;

public class SelectItem : Action
{
	public SelectItem() : base(ActionType.SelectItem)
	{
		
	}

	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		if(target.X>=actor.Inventory.Length || target.X < -1)
			return new Tuple<bool, string>(false, "Invalid Item");
		
		return  new Tuple<bool, string>(true, "");
	}
#if SERVER
	public override Queue<SequenceAction> ExecuteServerSide(Unit actor, Vector2Int target)
	{
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ReplaySequence.SelectItem(actor.WorldObject.ID,target.X));
		return queue;
	}
#endif

#if CLIENT

	public override void ExecuteClientSide(Unit actor, Vector2Int target)
	{
		base.ExecuteClientSide(actor,target);
actor.SelectedItemIndex = target.X;
	}

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		return;
	}
#endif

}