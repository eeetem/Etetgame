using System.IO;
using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.AI;

public class Attack : AIAction
{
	public Attack(Unit u) : base(AIActionType.Attack,u)
	{
		
	}



	public override void Execute()
	{
		var atk = GetBestPossibleAbility(Unit,true,false,false);
		Log.Message("AI","attacking with: "+atk.AbilityIndex+" at: "+atk.Target.TileLocation.Position+ "with score: "+atk.GetTotalValue()+" damage: "+atk.Dmg+" Suppression: "+atk.Supression+" scorechange: "+atk.TotalChangeScore+"\n");
		
		Unit.DoAbility(atk.Target,atk.AbilityIndex);
	}

	public override int GetScore()
	{
		if(base.GetScore() <= 0) return -100;
		//only what you see
		var atk = GetBestPossibleAbility(Unit,true,false,false);
		Log.Message("AI","calcualted attack score: "+(atk.GetTotalValue()*2) + " for unit: "+Unit.WorldObject.ID + " "+atk);
		return atk.GetTotalValue()*2;
	}	
	

}