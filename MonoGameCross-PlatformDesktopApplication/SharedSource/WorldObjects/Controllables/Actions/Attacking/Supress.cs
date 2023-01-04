using System;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Supress : Attack
{
	public Supress() : base(ActionType.Suppress)
	{
		Description = "Suppress a big area. All units in the area will instantly panic(reach 0 determination). Cost: 2 Action, 1 Move, 4 determination";
	}


	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int position)
	{
		var parentReply = base.CanPerform(actor, position);
		if (!parentReply.Item1)
		{
			return parentReply;
		}

		if (actor.determination != actor.Type.Maxdetermination)
		{
			return new Tuple<bool, string>(false, "Not enough determination!");
		}

		if (actor.FirePoints <= 1)
		{
			return new Tuple<bool, string>(false, "Not enough action points!");
		}

		if (actor.MovePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points!");
		}

		if (Vector2.Distance(actor.worldObject.TileLocation.Position, position) < 5)
		{
			return new Tuple<bool, string>(false, "Target is too close!");
		}

		return new Tuple<bool, string>(true, "");
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		base.Execute(actor,target);
		actor.FirePoints-=2;
		actor.determination=0;
		actor.MovePoints--;
	
		
#if CLIENT
		Camera.SetPos(target);
		ObjectSpawner.MG2(actor.worldObject.TileLocation.Position, target);
		
#endif

	
	}

	protected override int GetDamage(Controllable actor)
	{
		return 5;
	}

	protected override int GetSupressionRange(Controllable actor)
	{
		return 4;
	}
	protected override int GetSupressionStrenght(Controllable actor)
	{
		return 5;
	}

	protected override int GetdeterminationResistanceEffect(Controllable actor)
	{
		return 1;
	}

}

