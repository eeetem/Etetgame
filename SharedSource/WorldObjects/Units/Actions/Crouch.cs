using System;
using Microsoft.Xna.Framework.Graphics;
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
		actor.MovePoints--;
		actor.canTurn = true;
		actor.Crouching = !actor.Crouching;
		actor.WorldObject.TileLocation.OverWatchTrigger();
	}

#else
		public override void ExecuteServerSide(Unit actor,Vector2Int target)
	{
		
		actor.MovePoints--;
		actor.canTurn = true;
		actor.Crouching = !actor.Crouching;
		actor.WorldObject.TileLocation.OverWatchTrigger();

	}
#endif


#if CLIENT
	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new NotImplementedException();
	}

	public override void Animate(Unit actor, Vector2Int target)
	{
		base.Animate(actor,target);
		return;
	}
#endif
}