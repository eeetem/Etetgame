using System;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Sprint : Action
{
	public Sprint() :base(ActionType.Sprint)
	{
		Description = "Regain all of your action points. Cost: 2 determination";
	}

	
	public override Tuple<bool,string> CanPerform(Controllable actor, Vector2Int position)
	{
		if (actor.determination == actor.Type.Maxdetermination)
		{
			return new Tuple<bool, string>(true, "");
		}

		return new Tuple<bool, string>(false, "Not enough determination");
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.determination = 0;
		actor.TurnPoints = actor.Type.MaxTurnPoints;
		actor.MovePoints = actor.Type.MaxMovePoints;
		actor.FirePoints = actor.Type.MaxActionPoints;
	}
#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.DoAction(this,target);
		SetActiveAction(null);
	}
#endif




}

