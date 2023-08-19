using System;
using DefconNull.ReplaySequence;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.World.WorldObjects.Units.Actions;

public class UseItem : Action
{
	public UseItem() : base(ActionType.UseItem)
	{
	}
	
	public override Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{

		if (actor.SelectedItem == null)
		{
			return new Tuple<bool, string>(false, "No Item Selected");
		}

		if (actor.ActionPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough action points!");
		}
		return actor.SelectedItem.CanPerform(actor, target);

	}

	#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, Vector2Int target)
	{
		var queue = new Queue<SequenceAction>();
		
        
		var m = new MoveCamera(actor.WorldObject.TileLocation.Position, false, 0);
		queue.Enqueue(m);

		var item = actor.SelectedItem;
		var cons = item.GetConsequences(actor, target);


		ValueChange valueChange = new ValueChange();
		valueChange.Value = -1;
		queue.Enqueue(new UnitSelectItem(actor.WorldObject.ID,actor.SelectedItemIndex));
		queue.Enqueue(new UseUpSelectedItem(actor.WorldObject.ID,target));
		queue.Enqueue(new ChangeUnitValues(actor.WorldObject.ID,valueChange));
		if (actor.WorldObject.TileLocation.Position != target)
		{
			queue.Enqueue(new FaceUnit(actor.WorldObject.ID, target));
		}

		foreach (var c in cons)
		{
			queue.Enqueue(c);
		}

		return queue;
	}
	#endif
	
#if CLIENT

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.SelectedItem?.Preview(actor, target,spriteBatch);
	}

#endif

}