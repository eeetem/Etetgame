using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Sprint : Action
{
	public Sprint() :base(ActionType.Sprint)
	{
		Description = "Regain all of your action points. Cost: 2 Awareness";
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{
		if (actor.Awareness == actor.Type.MaxAwareness)
		{
			return true;
		}

		return false;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.Awareness = 0;
		actor.TurnPoints = actor.Type.MaxTurnPoints;
		actor.MovePoints = actor.Type.MaxMovePoints;
		actor.ActionPoints = actor.Type.MaxActionPoints;
	}
#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.DoAction(this,target);
		SetActiveAction(null);
	}
#endif




}

