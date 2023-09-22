using System;
using System.Collections.Generic;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.Actions;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.AI;

public class Attack : AIAction
{
	public Attack() : base(AIActionType.Attack)
	{
		
	}



	public override void Execute(Unit unit)
	{
		//todo only waht you see
		var atk = GetBestPossibleAbility(unit,true,false,false);
		UseAbility.AbilityIndex = atk.AbilityIndex;
		unit.DoAction(Action.Actions[Action.ActionType.UseAbility],atk.TargetPosition);
		
	}

	public override int GetScore(Unit unit)
	{
		//only what you see
		var atk = GetBestPossibleAbility(unit,true,false,false);
		return atk.GetTotalValue()*2;
	}	
	

}