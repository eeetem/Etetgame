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

			actor.ActionPoints--;
			actor.MovePoints--;
			actor.ClearOverWatch();
			
		//client shouldnt be allowed to judge what got hit
		//fire packet just makes the unit "shoot"
		//actual damage and projectile is handled elsewhere
#if SERVER
			Projectile p = MakeProjectile(actor, target);
			p.Fire();
			Networking.DoAction(new ProjectilePacket(p.result,p.covercast,actor.Type.WeaponDmg,p.dropoffRange));

#endif
#if CLIENT
		Camera.SetPos(target);
		if (actor.Type.WeaponRange > 6)
		{
			ObjectSpawner.Burst(actor.worldObject.TileLocation.Position, target);
		}
		else
		{
			ObjectSpawner.ShotGun(actor.worldObject.TileLocation.Position,target);	
		}
#endif
		actor.worldObject.Face(Utility.ToClampedDirection( actor.worldObject.TileLocation.Position-target));

	}

	protected override int GetDamage(Controllable actor)
	{
		return actor.Type.WeaponDmg;
	}

	protected override int GetAwarenessResistanceEffect(Controllable actor)
	{
		return 1;
	}
}

