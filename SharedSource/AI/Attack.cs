using System.IO;
using DefconNull.WorldObjects;
using Riptide;

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
		Log.Message("AI","calcualted attack score: "+(atk.GetTotalValue()*2) + " for unit: "+unit.WorldObject.ID + " "+atk);
		return atk.GetTotalValue()*2;
	}	
	

}