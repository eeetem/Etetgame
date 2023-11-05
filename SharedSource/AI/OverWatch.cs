using System;
using System.Collections.Generic;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.Actions;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.AI;

public class Overwatch : AIAction
{
	public Overwatch() : base(AIActionType.OverWatch)
	{
		
	}



	public override void Execute(Unit unit)
	{

		var atk = GetBestPossibleAbility(unit,false,false,false,-1,true);
		var args = new List<string>();
		args.Add(atk.AbilityIndex.ToString());
		unit.DoAction(Action.ActionType.OverWatch,atk.TargetPosition,args);
		
		
	}

	public override int GetScore(Unit unit)
	{
		return 5;
	}	
	

}