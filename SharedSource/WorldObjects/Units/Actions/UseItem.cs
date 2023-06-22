using System;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

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
	public override void ExecuteServerSide(Unit actor, Vector2Int target)
	{
		actor.ActionPoints--;
		actor.SelectedItem?.Execute(actor, target);
		Console.WriteLine("Using Item "+actor.SelectedItem?.Name);
		actor.LastItem = actor.SelectedItem;
		actor.RemoveItem(actor.SelectedItemIndex);
		actor.WorldObject.Face(Utility.GetDirection(actor.WorldObject.TileLocation.Position,target));
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

	public override void Animate(Unit actor, Vector2Int target)
	{
		base.Animate(actor,target);
		actor.SelectedItem?.Animate(actor,target);
	}
#endif

}