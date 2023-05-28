using System;
using MultiplayerXeno;
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

	
	public override Tuple<bool,string> CanPerform(Controllable actor, Vector2Int position)
	{
		if (actor.MovePoints <= 0 )
		{
			return new Tuple<bool, string>(false, "Not enough move points!");
		}
		
		return new Tuple<bool, string>(true, "");
	}

	public override void Execute(Controllable actor,Vector2Int target)
	{

		actor.MovePoints--;
		actor.canTurn = true;
		actor.Crouching = !actor.Crouching;
		actor.worldObject.TileLocation.OverWatchTrigger();
#if CLIENT
		GameLayout.ReMakeMovePreview();
#endif
	}

#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new NotImplementedException();
	}

	public override void Animate(Controllable actor, Vector2Int target)
	{
		base.Animate(actor,target);
		return;
	}
#endif
}

