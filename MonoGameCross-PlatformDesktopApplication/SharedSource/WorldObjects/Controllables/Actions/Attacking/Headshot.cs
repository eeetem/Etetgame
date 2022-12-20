using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Headshot : Attack
{
	public Headshot() : base(ActionType.HeadShot)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{

		if (position == actor.worldObject.TileLocation.Position)
		{
			return false;
		}
		if (actor.Awareness != actor.Type.MaxAwareness)
		{
			return false;
		}
		if (actor.ActionPoints <= 0)
		{
			return false;
		}
		if (actor.MovePoints <= 0)
		{
			return false;
		}

		return true;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.ActionPoints--;
		actor.Awareness=0;
		actor.MovePoints--;
		
	}

	protected override int GetDamage(Controllable actor)
	{
		return 10;
	}

	protected override int GetAwarenessResistanceEffect(Controllable actor)
	{
		return 10;
	}

}

