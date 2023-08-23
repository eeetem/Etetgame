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
		var atk = GetWorstPossibleAttackOnEnemyTeam(unit,false);
		UseAbility.AbilityIndex = atk.Ability!.Index;
		unit.DoAction(Action.Actions[Action.ActionType.UseAbility],atk.target);
	}

	public override int GetScore(Unit unit)
	{
		
		//only what you see
		var atk = GetWorstPossibleAttackOnEnemyTeam(unit,false);
		return atk.GetTotalValue()*2;
	}	
}