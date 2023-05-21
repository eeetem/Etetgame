using System;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class UseItem : Action
{
	public UseItem() : base(ActionType.UseItem)
	{
	}

	public static int ItemIndex = -1;
	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{

		if (ItemIndex == -1 || actor.Inventory.Length <= ItemIndex || actor.Inventory[ItemIndex] == null)
		{
			return new Tuple<bool, string>(false, "No Item Selected");
		}

		if (actor.ActionPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough action points!");
		}
		return actor.Inventory[ItemIndex]!.CanPerform(actor, target);

	}

	
	public override void Execute(Controllable actor, Vector2Int target)
	{
		actor.ActionPoints--;
		actor.Inventory[ItemIndex]!.Execute(actor, target);
		Console.WriteLine("Using Item "+actor.Inventory[ItemIndex]!.Name);
		actor.RemoveItem(ItemIndex);
		actor.worldObject.Face(Utility.GetDirection(actor.worldObject.TileLocation.Position,target));
	}

	public override void ToPacket(Controllable actor, Vector2Int target)
	{
		var packet = new GameActionPacket(actor.worldObject.Id,target,ActionType);
		packet.args.Add(ItemIndex.ToString());

		
		Networking.DoAction(packet);
	}

#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.SelectedItem?.Preview(actor, target,spriteBatch);
	}

	public override void Animate(Controllable actor, Vector2Int target)
	{
		actor.Inventory[ItemIndex]!.Animate(actor,target);
	}
#endif

}