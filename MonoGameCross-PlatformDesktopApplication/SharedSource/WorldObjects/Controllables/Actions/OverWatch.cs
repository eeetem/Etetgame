using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{

		if (actor.TurnPoints <= 0)
		{
			return false;
		}
		if (actor.MovePoints <= 0)
		{
			return false;
		}
		if (actor.ActionPoints <= 0)
		{
			return false;
		}
	

		return false;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.TurnPoints--;
		actor.ActionPoints--;
		actor.MovePoints--;
		
		
	}

	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new System.NotImplementedException();
	}
}

