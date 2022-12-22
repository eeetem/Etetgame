﻿using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Supress : Attack
{
	public Supress() : base(ActionType.Suppress)
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

		if (Vector2.Distance(actor.worldObject.TileLocation.Position, position) < 6) ;
		{
			return false;
		}

		return true;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		base.Execute(actor,target);
		actor.ActionPoints--;
		actor.Awareness=0;
		actor.MovePoints--;
	
		
#if CLIENT
		Camera.SetPos(target);
		ObjectSpawner.MG2(actor.worldObject.TileLocation.Position, target);
		
#endif

	
	}

	protected override int GetDamage(Controllable actor)
	{
		return 4;
	}

	protected override int GetSupressionRange(Controllable actor)
	{
		return 5;
	}
	protected override int GetSupressionStrenght(Controllable actor)
	{
		return 2;
	}

	protected override int GetAwarenessResistanceEffect(Controllable actor)
	{
		return 1;
	}

}

