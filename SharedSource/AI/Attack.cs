using System;
using System.Collections.Generic;
using System.IO;
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
		var atk = GetBestPossibleAbility(unit,true,false,false);
		File.AppendAllText("aidebug.txt","attacking with: "+atk.AbilityIndex+" at: "+atk.Target.TileLocation.Position+ "with score: "+atk.GetTotalValue()+" damage: "+atk.Dmg+" Suppression: "+atk.Supression+" scorechange: "+atk.TotalChangeScore+"\n");
		
		unit.DoAbility(atk.Target,atk.AbilityIndex);
	}

	public override int GetScore(Unit unit)
	{
		if(base.GetScore(unit) <= 0) return -100;
		//only what you see
		var atk = GetBestPossibleAbility(unit,true,false,false);
		return atk.GetTotalValue()*2;
	}	
	

}