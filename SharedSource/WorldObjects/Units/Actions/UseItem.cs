using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.ReplaySequence;

#if CLIENT
using MultiplayerXeno.UILayouts;
#endif


namespace MultiplayerXeno;

public class UseItem : Action
{
	public UseItem() : base(ActionType.UseItem)
	{
	}
	
	public override Tuple<bool, string> CanPerform(Unit actor, ref  Vector2Int target)
	{

		if (actor.SelectedItem == null)
		{
			return new Tuple<bool, string>(false, "No Item Selected");
		}

		if (actor.ActionPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough action points!");
		}
		return actor.SelectedItem.CanPerform(actor,ref  target);

	}

	#if SERVER
	public override Queue<SequenceAction> ExecuteServerSide(Unit actor, Vector2Int target)
	{
		
		
		WorldEffect w = new WorldEffect();
		w.Act.Value = -1;
		w.TargetFriend = true;
		w.TargetSelf = true;
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ReplaySequence.WorldChange(actor.WorldObject.ID,actor.WorldObject.TileLocation.Position,w));
		queue.Enqueue(new ReplaySequence.UseSelectedItem(actor.WorldObject.ID,target));
		return queue;
	}
	#endif
	
#if CLIENT
	public override void InitAction()
	{
		//bad
		GameLayout.SelectedUnit?.SelectedItem?.InitPreview();
		base.InitAction();
	}

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.SelectedItem?.Preview(actor, target,spriteBatch);
	}

#endif

}