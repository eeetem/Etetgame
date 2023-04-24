using System;
using MultiplayerXeno;

namespace MultiplayerXeno;

public class Headshot : Attack
{
	public Headshot() : base(ActionType.HeadShot)
	{
		Description = "Shoot for 15 Damage. Can only hit targets with 0 determination. Cost: 1 Action, 1 Move, 2 determination";
	}

	
	public override Tuple<bool,string> CanPerform(Controllable actor, Vector2Int position)
	{
		var parentReply = base.CanPerform(actor, position);
		if (!parentReply.Item1)
		{
			return parentReply;
		}

	
		if (actor.Determination != actor.Type.Maxdetermination)
		{
			return new Tuple<bool, string>(false, "Not enough determination!");
		}
		if (actor.FirePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough fire points!");
		}
		if (actor.MovePoints <= 0)
		{ 
			return new Tuple<bool, string>(false, "Not enough move points!");
		}

		return new Tuple<bool, string>(true, "");
	}

	public override void Execute(Controllable actor,Vector2Int target)
	{
		base.Execute(actor,target);
		actor.FirePoints--;
		actor.Suppress(actor.Determination, true);
		actor.MovePoints--;
	
		
#if CLIENT
		Camera.SetPos(target);
		ObjectSpawner.Single(actor.worldObject.TileLocation.Position, target);
		
#endif

	
	}

	protected override int GetDamage(Controllable actor)
	{
		return 15;
	}

	protected override int GetSupressionRange(Controllable actor)
	{
		return 0;
	}

	protected override int GetdeterminationResistanceEffect(Controllable actor)
	{
		return GetDamage(actor);
	}

}

