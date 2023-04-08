﻿using System;
using CommonData;

namespace MultiplayerXeno;

public class Fire : Attack
{
	public Fire() :base(ActionType.Attack)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Controllable actor, Vector2Int position)
	{
		
		var parentReply = base.CanPerform(actor, position);
		if (!parentReply.Item1)
		{
			return parentReply;
		}

		if (actor.overWatch)
		{
			return new Tuple<bool, string>(true, "");
		}

		if (actor.ActionPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough fire points");
		}
		if (actor.MovePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points");
		}

		return new Tuple<bool, string>(true, "");

	}

	public override void Execute(Controllable actor,Vector2Int target)
	{
		base.Execute(actor,target);
			actor.ActionPoints--;
			actor.MovePoints--;

#if CLIENT
		Camera.SetPos(target);
		if (actor.Type.WeaponRange < 8)
		{
		
			ObjectSpawner.ShotGun(actor.worldObject.TileLocation.Position,target);	
		}
		else if (actor.Type.MaxFirePoints == 2)
		{
			ObjectSpawner.MG(actor.worldObject.TileLocation.Position, target);
		}
		else
		{
			ObjectSpawner.Burst(actor.worldObject.TileLocation.Position, target);
		}
#endif

	}

	protected override int GetDamage(Controllable actor)
	{
		return actor.Type.WeaponDmg;
	}

	protected override int GetSupressionRange(Controllable actor)
	{
		return  actor.Type.SupressionRange;
	}

	protected override int GetdeterminationResistanceEffect(Controllable actor)
	{
		return 1;
	}
}

