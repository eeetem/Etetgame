using System;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class SelectItem : Action
{
	public SelectItem() : base(ActionType.SelectItem)
	{
		
	}

	public override Tuple<bool, string> CanPerform(Controllable actor, ref Vector2Int target)
	{
		if(target.X>=actor.Inventory.Length || target.X < -1)
			return new Tuple<bool, string>(false, "Invalid Item");
		
		return  new Tuple<bool, string>(true, "");
	}

	public override void Execute(Controllable actor, Vector2Int target)
	{
		actor.SelectedItemIndex = target.X;
	}
#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		return;
	}
#endif

}