using System;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class SecondWind : Action
{
	public SecondWind() :base(ActionType.Sprint)
	{
		Description = "Regain all of your action points. Cost: 2 determination";
	}

	
	public override Tuple<bool,string> CanPerform(Controllable actor, Vector2Int position)
	{
		
		if (actor.overWatch)
		{
			return new Tuple<bool, string>(false, "You cannot user second wind while overwatching");
		}
		if (actor.Determination != actor.Type.Maxdetermination)
		{
			return new Tuple<bool, string>(false, "Not enough determination");
		
		}

	
		return new Tuple<bool, string>(true, "");
	
	}

	public override void Execute(Controllable actor,Vector2Int target)
	{
		actor.Suppress(actor.Determination, true);
		actor.canTurn = true;
		actor.MovePoints = actor.Type.MaxMovePoints;
		actor.ActionPoints = actor.Type.MaxFirePoints;
	}
#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.DoAction(this,target);
		SetActiveAction(null);
	}
	public override void Animate(Controllable actor, Vector2Int target)
	{
		return;
	}
#endif




}

