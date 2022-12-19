using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Crouch : Action
{
	public Crouch() :base(ActionType.Crouch)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{

		if (actor.MovePoints > 0)
		{
			return true;
		}
	

		return false;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.MovePoints--;
		actor.Crouching = !actor.Crouching;
	}

#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new System.NotImplementedException();
	}
#endif
}

