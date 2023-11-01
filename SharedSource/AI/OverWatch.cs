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
		var owr = GetBestPossibleAbility(unit,true,false,false,-1,true);
		if(owr.GetTotalValue()<=0) return 0;//no good overwatch oportunities
		
		var atk = GetBestPossibleAbility(unit,true,false,false);
		if (atk.Dmg == 0 && atk.Supression == 0)//passive ability, maybe best to overwatch
		{
			return Random.Shared.Next(5, 20);
		}

		return 5;
	}	
	

}