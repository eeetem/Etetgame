using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.ReplaySequence;

namespace MultiplayerXeno;

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
		throw new NotImplementedException();
		actor.SelectedItemIndex = target.X;
	}
#endif

#if CLIENT

	public override void ExecuteClientSide(Unit actor, Vector2Int target)
	{
		actor.SelectedItemIndex = target.X;
		base.ExecuteClientSide(actor,target);
	}

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		return;
	}
#endif

}