using System;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class UseItem : Action
{
	public UseItem() : base(ActionType.UseItem)
	{
	}

	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
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

	public override void Execute(Controllable actor, Vector2Int target)
	{
		actor.ActionPoints--;
		actor.SelectedItem!.Execute(actor, target);
		actor.RemoveItem(actor.SelectedItemIndex);
		actor.worldObject.Face(Utility.GetDirection(actor.worldObject.TileLocation.Position,target));
	}

#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.SelectedItem?.Preview(actor, target,spriteBatch);
	}

	public override void Animate(Controllable actor, Vector2Int target)
	{
		actor.SelectedItem?.Animate(actor,target);
	}
#endif

}