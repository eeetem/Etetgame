using System;
using System.Collections.Generic;
using System.Linq;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno;

public class Fire : Attack
{
	public Fire() :base(ActionType.Attack)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{
		
		if (position == actor.worldObject.TileLocation.Position)
		{
			return false;
		}

		if (actor.overWatch)
		{
			return true;//can overwatch fire without points
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
		base.Execute(actor,target);
			actor.ActionPoints--;
			actor.MovePoints--;

#if CLIENT
		Camera.SetPos(target);
		if (actor.Type.WeaponRange < 6)
		{
		
			ObjectSpawner.ShotGun(actor.worldObject.TileLocation.Position,target);	
		}
		else if (actor.Type.MaxActionPoints == 2)
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

	protected override int GetAwarenessResistanceEffect(Controllable actor)
	{
		return 1;
	}
}

