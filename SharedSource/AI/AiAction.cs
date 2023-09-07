using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;
using Microsoft.Xna.Framework;

#if CLIENT
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.AI;

public abstract class AIAction
{

	public readonly AIActionType Type;
    
	public AIAction(AIActionType? type)
	{
		if(type==null) return;
		Type = (AIActionType)type;
	}

    
	public enum AIActionType
	{
		Attack=1,
		Move=2,
		SupportAbility =3,
	}
	protected static AbilityUse IterateAllAbilities(Unit attacker, Vector2Int targetPosition,  bool nextTurn =false, int dimension = -1,bool noRecursion = true)
	{

		AbilityUse topAttack = new AbilityUse();
		var res = IterateTargetedAbilities(attacker,targetPosition,nextTurn,dimension,noRecursion);
		if(res > topAttack)
		{
			topAttack = res;
		}
		res = IterateImmideateAbilities(attacker,nextTurn,dimension,noRecursion);
		if(res > topAttack)
		{
			topAttack = res;
		}
		
		return topAttack;
	}
	protected static AbilityUse IterateTargetedAbilities(Unit attacker, Vector2Int targetPosition,  bool nextTurn =false, int dimension = -1,bool noRecursion = true)
	{

		AbilityUse topAttack = new AbilityUse();
		//Parallel.ForEach(attacker.Abilities, ability =>
	//		{
		foreach (var ability in attacker.Abilities)
		{
			if (ability.ImmideateActivation) continue;
			foreach (var attackTile in WorldManager.Instance.GetTilesAround(targetPosition, 1, dimension))
			{
				var attackResult = SimulateAbility(attacker, ability, attackTile, nextTurn, dimension,noRecursion);

				if (attackResult.GetTotalValue() > topAttack.GetTotalValue())
				{
					topAttack = attackResult;
				}
			}
			//	Parallel.ForEach(WorldManager.Instance.GetTilesAround(targetPosition, 2, dimension), attackTile =>
				//{
					

			//	});
		}
			
		//	});
		
		return topAttack;
	}
	protected static AbilityUse IterateImmideateAbilities(Unit attacker,  bool nextTurn =false, int dimension = -1,bool noRecursion = true)
	{

		AbilityUse topAttack = new AbilityUse();
		Parallel.ForEach(attacker.Abilities, ability =>
		{
		//foreach (var ability in attacker.Abilities)
		//{
			if (!ability.ImmideateActivation) return;
		//	foreach (var attackTile in WorldManager.Instance.GetTilesAround(attacker.WorldObject.TileLocation.Position, 1, dimension))
		//{

			//}
	//	}
			Parallel.ForEach(WorldManager.Instance.GetTilesAround(attacker.WorldObject.TileLocation.Position, 1, dimension), attackTile =>
			{
				var attackResult = SimulateAbility(attacker, ability, attackTile, nextTurn, dimension,noRecursion);

				if (attackResult.GetTotalValue() > topAttack.GetTotalValue())
				{
					topAttack = attackResult;
				}

			});
		});
		
		return topAttack;
	}

	public static AbilityUse SimulateAbility(Unit attacker, IUnitAbility ability, IWorldTile attackTile, bool nextTurn, int dimension, bool noRecursion)
	{
		int damage = 0;
		int supression = 0;
		int totalChangeScore =0;

		if(!ability.CanPerform(attacker, attackTile.Position,nextTurn,dimension).Item1) return new AbilityUse();
                    
		var cons = ability.GetConsequences(attacker, attackTile.Position,dimension);
		Parallel.ForEach(cons, c =>
		{
			var consiquence = ScoreConsequence(c,dimension,attacker,ability.Index,noRecursion);
			damage += consiquence.Item1;
			supression += consiquence.Item2;
			totalChangeScore += consiquence.Item3;
		});
		
                    
		AbilityUse attackResult = new AbilityUse();
		attackResult.Ability = ability;
		attackResult.target = attackTile.Position;
		attackResult.Dmg = damage;
		attackResult.Supression = supression;
		attackResult.totalChangeScore = totalChangeScore;
		return attackResult;
	}

	public static Tuple<int,int,int> ScoreConsequence(SequenceAction c,int dimension, Unit attacker,int abilityIndex, bool noRecursion)
	{
		int damage = 0;
		int supression = 0;
		int totalChangeScore = 0;
		
		if (c.GetType() == typeof(TakeDamage) )
		{
			TakeDamage tkdmg =  (TakeDamage)c;
			Unit? hitUnit = null;
			if (tkdmg.objID != -1)
			{
				hitUnit = WorldManager.Instance.GetObject(((TakeDamage)c).objID,dimension)!.UnitComponent;
			}
			else if(tkdmg.position != new Vector2Int(-1, -1))
			{

				hitUnit = WorldManager.Instance.GetTileAtGrid(((TakeDamage)c).position,dimension).UnitAtLocation;
				
			}
			if (hitUnit != null)
			{
				int dmgThisAttack = 0;
				if (hitUnit.Determination > 0)
				{
					dmgThisAttack += ((TakeDamage) c).dmg - ((TakeDamage) c).detResistance;
				}
				else
				{
					dmgThisAttack += ((TakeDamage) c).dmg;
				}
				if (dmgThisAttack >= hitUnit.Health)
				{
					dmgThisAttack *= 2;
				}
				if(hitUnit.IsPlayer1Team != attacker.IsPlayer1Team)
				{
					damage += dmgThisAttack;
				}else
				{
					damage -= dmgThisAttack;
				}
			}
		}
		else if (c.GetType() == typeof(Suppress))
		{
			Suppress sup = (Suppress)c;

			var hitUnit = sup.GetAffectedActor(dimension);
			if (hitUnit != null)
			{
				int supressionThisAttack = 0;
				supressionThisAttack += Math.Min(((Suppress) c).detDmg, hitUnit.Determination.Current);
				if(supressionThisAttack >= hitUnit.Determination && !hitUnit.Paniced)
				{
					supressionThisAttack += ((Suppress) c).detDmg*2;
				}
				if (hitUnit.IsPlayer1Team != attacker.IsPlayer1Team)
				{
					supression += supressionThisAttack;
				}
				else
				{
					supression -= supressionThisAttack;
				}
			}
		}
		else if (c.GetType() == typeof(ChangeUnitValues))
		{
			if (noRecursion)
			{
				return new Tuple<int, int, int>(0,0,0);
			}

			ChangeUnitValues change = (ChangeUnitValues)c;
			var hitUnit = change.GetAffectedActor(dimension);
			if (hitUnit != null)
			{
				if(hitUnit.WorldObject.ID == attacker.WorldObject.ID && change.ActChange.GetChange(attacker.ActionPoints) <= 0 && change.MoveChange.GetChange(attacker.MovePoints) <= 0 && change.DetChange.GetChange(attacker.Determination) == 0 && change.MoveRangeeffectChange.GetChange(attacker.MoveRangeEffect) == 0)
				{
					//we're just using points for an ability, no need to do extensive calculations
					return new Tuple<int, int, int>(0,0,0);
				}
				Unit pseudoUnit;
				int newDim = WorldManager.Instance.CreatePseudoWorldWithUnit(hitUnit, hitUnit.WorldObject.TileLocation.Position, out pseudoUnit, dimension);
				var prechangeScore = GetProtectionAndAtackScore(pseudoUnit,newDim, true);
				
				change.ActChange.Apply(ref pseudoUnit.ActionPoints);
				change.MoveChange.Apply(ref pseudoUnit.MovePoints);
				change.DetChange.Apply(ref pseudoUnit.Determination);
				change.MoveRangeeffectChange.Apply(ref pseudoUnit.MoveRangeEffect);
				
				var postchangeScore = GetProtectionAndAtackScore(pseudoUnit,newDim, true);
				WorldManager.Instance.WipePseudoLayer(newDim);
				var outcome = (postchangeScore.Item1+postchangeScore.Item2) - (prechangeScore.Item1+prechangeScore.Item2);
				if(hitUnit.IsPlayer1Team == attacker.IsPlayer1Team)
				{
					totalChangeScore += outcome;
				}
				else
				{
					totalChangeScore -= outcome;
				}
			}
		}else if (c.GetType() == typeof(UnitStatusEffect))
		{
			UnitStatusEffect status = (UnitStatusEffect)c;
			
			var hitUnit = status.GetAffectedActor(dimension);
			if (hitUnit != null)
			{
				var consiq =PrefabManager.StatusEffects[status.effectName].Conseqences.GetApplyConsiqunces(hitUnit.WorldObject.TileLocation.Position);
				foreach (var cact in consiq)
				{
					var res= ScoreConsequence(cact, dimension, attacker,abilityIndex,noRecursion);
					damage += res.Item1;
					supression += res.Item2;
					totalChangeScore += res.Item3;
				}
			}
			
		}

		return new Tuple<int, int, int>(damage,supression,totalChangeScore);
	}

	protected static bool CanPotentiallyAttack(Unit attacker, Unit target, int dimension = -1)
	{
	
		foreach (var ability in attacker.Abilities)
		{
			if (ability.ImmideateActivation) continue;
			foreach (var attackTile in WorldManager.Instance.GetTilesAround(target.WorldObject.TileLocation.Position, 2, dimension))
			{
				var attackResult = SimulateAbility(attacker, ability, attackTile, false, dimension,true);

				if (attackResult.GetTotalValue() > 0) return true;
			}
	
		}

		return false;
	}
	protected static AbilityUse GetWorstPossibleAttackOnEnemyTeam(Unit attacker, bool nextTurn, bool onlyVisible, int dimension = -1)
	{
		var otherTeam = GameManager.GetTeamUnits(!attacker.IsPlayer1Team);
		AbilityUse bestAttack = new AbilityUse();
		foreach (var enemy in otherTeam)
		{
			if (!onlyVisible || WorldManager.Instance.CanTeamSee(enemy.WorldObject.TileLocation.Position, attacker.IsPlayer1Team) >= enemy.WorldObject.GetMinimumVisibility())
			{
				var res =IterateTargetedAbilities(attacker,enemy.WorldObject.TileLocation.Position,nextTurn,dimension);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
				if(res > bestAttack)
				{
					bestAttack = res;
				}
			}
		}
		var res2 =IterateImmideateAbilities(attacker,nextTurn,dimension);
		if(res2 > bestAttack)
		{
			bestAttack = res2;
		}

		return bestAttack;
	}


	public abstract void Execute(Unit unit);
	public abstract int GetScore(Unit unit);
	
	public struct MoveCalcualtion
	{
		public float closestDistance;
		public int distanceReward;
		public int protectionPentalty;
		public List<AbilityUse> EnemyAttackScores =  new List<AbilityUse>();
		public int visibilityScore;
		public int clumpingPenalty;
		public int damagePotential;
		public int coverBonus;

		public MoveCalcualtion()
		{
			closestDistance = 0;
			distanceReward = 0;
			protectionPentalty = 0;
			visibilityScore = 0;
			clumpingPenalty = 0;
			damagePotential = 0;
		}
	}

	public static int GetTileMovementScore(Vector2Int tilePosition, int movesToUse, bool crouch, Unit realUnit, out MoveCalcualtion details)
	{
		return GetTileMovementScore(tilePosition, new ChangeUnitValues(-1,0,-movesToUse),crouch, realUnit, out details);
	}

	
	public static int GetTileMovementScore(Vector2Int tilePosition, ChangeUnitValues? valueMod,  bool crouch, Unit realUnit, out MoveCalcualtion details, int copyDimension = -1)
	{
		if(WorldManager.Instance.GetTileAtGrid(tilePosition).UnitAtLocation != null && !Equals(WorldManager.Instance.GetTileAtGrid(tilePosition).UnitAtLocation, realUnit))
		{
			details = new MoveCalcualtion();
			return -1000;
		}
		details = new MoveCalcualtion();
		int score = 0;



		Unit hypotheticalUnit;
		int dimension = WorldManager.Instance.CreatePseudoWorldWithUnit(realUnit,tilePosition, out hypotheticalUnit, copyDimension);
		
		var otherTeamUnits = GameManager.GetTeamUnits(!realUnit.IsPlayer1Team,dimension);
		hypotheticalUnit.Crouching = crouch;
	
		/*if (abilityToExclude != null)
		{
			if (abilityToExclude.Item1.WorldObject.ID == realUnit.WorldObject.ID)
			{
				hypotheticalUnit.Abilities[abilityToExclude.Item2].Disable();
			}
			else
			{
				Unit unit;
				WorldManager.Instance.AddUnitToPseudoWorld(abilityToExclude.Item1, tilePosition, out unit, dimension);
				unit.Abilities[abilityToExclude.Item2].Disable();
			}
		}*/


		
		if (valueMod is not null)
		{
			valueMod.ActChange.Apply(ref hypotheticalUnit.ActionPoints);
			valueMod.MoveChange.Apply(ref hypotheticalUnit.MovePoints);
			valueMod.DetChange.Apply(ref hypotheticalUnit.Determination);
			valueMod.MoveRangeeffectChange.Apply(ref hypotheticalUnit.MoveRangeEffect);
		}

	
		//add points for being in range of your primiary attack
		float closestDistance = 1000;
		foreach (var u in otherTeamUnits)
		{
			var enemyLoc = u.WorldObject.TileLocation.Position;
			var dist = Vector2.Distance(enemyLoc, tilePosition);
			if(dist< closestDistance){
				closestDistance = dist;
			}
		}

		closestDistance -= hypotheticalUnit.Abilities[0].GetOptimalRangeAI();
        
		details.closestDistance = closestDistance;
		closestDistance = Math.Max(0, closestDistance);
		int distanceReward = 40;
		distanceReward -= Math.Min(distanceReward, (int)closestDistance);//if we're futher than our optimal range, we get less points
		score += distanceReward;
		details.distanceReward = distanceReward;
		

		
		int visibilityScore = -40;
		int visibleEnemies = 0;
		
		foreach (var u in otherTeamUnits)
		{
			if (WorldManager.Instance.CanSee(hypotheticalUnit.WorldObject.TileLocation.Position, u.WorldObject.TileLocation.Position,hypotheticalUnit.GetSightRange(),hypotheticalUnit.Crouching) >= u.WorldObject.GetMinimumVisibility() && CanPotentiallyAttack(hypotheticalUnit, u,dimension))	
			{
				visibleEnemies++;
			}
		}

		if (visibleEnemies == 1)
		{
			visibilityScore = 50;//good to see 1
		}
		visibilityScore -= (visibleEnemies-1) * 25;//reduce for more
		
		score += visibilityScore;
		details.visibilityScore = visibilityScore;


		(int protectionPentalty, int damagePotential) = GetProtectionAndAtackScore(hypotheticalUnit, dimension, copyDimension != -1);
		
		if (copyDimension == -1)//only do this at top layer of recursion
		{
			protectionPentalty *=3;//cover is VERY important
		}

		score += protectionPentalty;
		details.protectionPentalty = protectionPentalty;
	//	details.EnemyAttackScores = enemyAttackScores;
	
		if (copyDimension == -1) //only do this at top layer of recursion
		{
			damagePotential *= 3; //damage is important
		}

		details.damagePotential = damagePotential; 
	
		score += damagePotential;


				
	
		//	if (copyDimension == -1)//only wwipe if we ourselves picked this dimension
		//	{
		WorldManager.Instance.WipePseudoLayer(dimension);
		//	}
		//	else
		//	{
		//		WorldManager.Instance.DeletePseudoUnit(hypotheticalUnit.WorldObject.ID,dimension);
		//	}

		var myTeamUnits = GameManager.GetTeamUnits(realUnit.IsPlayer1Team,dimension);
		int clumpingPenalty = 0;
		foreach (var u in myTeamUnits)
		{
			if(Equals(u, realUnit)) continue;
			var friendLoc = u.WorldObject.TileLocation.Position;
			var dist = Vector2.Distance(friendLoc, tilePosition);
			clumpingPenalty -= (int)(3/dist);
		}
		score += clumpingPenalty;
		details.clumpingPenalty = clumpingPenalty;
		
		int coverBonus = 0;
		float stackingRatio = 1;
		Parallel.ForEach(otherTeamUnits, enemy =>
		{
			
			var enemyDirection = Utility.Vec2ToDir( enemy.WorldObject.TileLocation.Position-tilePosition);
			Cover coverInDirection = WorldManager.Instance.GetCover(tilePosition,enemyDirection, true);
			float bonus = 0;
			if (coverInDirection == Cover.High)
			{
				bonus += 5;
			}
			else if (coverInDirection == Cover.Low)
			{
				bonus += 2;
			}

			//bonus *= 3 / Vector2.Distance(tilePosition, enemy.WorldObject.TileLocation.Position);
			coverBonus += (int) (bonus * stackingRatio);
			stackingRatio *= 0.5f;
		});
        
		score += coverBonus;
		details.coverBonus = coverBonus;

        
		
//account for good positions on to buff tamemates from

		return score;
	}

	public static Tuple<int,int> GetProtectionAndAtackScore(Unit unit, int dimension, bool noRecursion)
	{			
		var otherTeamUnits = GameManager.GetTeamUnits(!unit.IsPlayer1Team,dimension);
	
		//add points for being protected
		int protectionPentalty = 0;
		//Parallel.ForEach(otherTeamUnits, u =>
		//{--
		foreach (var enemy in otherTeamUnits)
		{
			var res = IterateAllAbilities(enemy, unit.WorldObject.TileLocation.Position,true,dimension, noRecursion:noRecursion);
			if (res.GetTotalValue() > 0)
			{
				protectionPentalty -= res.GetTotalValue();
			}
			//enemyAttackScores.Add(res);
		}
			
		//});
		
		AbilityUse bestAttack = new AbilityUse();
		int damagePotential = 0;
		foreach (var enemy in otherTeamUnits)
		{
			var res =IterateTargetedAbilities(unit,enemy.WorldObject.TileLocation.Position,false,dimension:dimension,noRecursion:noRecursion);
			if (res.GetTotalValue() > bestAttack.GetTotalValue()) 
			{
				bestAttack = res;
			}
		}
		var res2 =IterateImmideateAbilities(unit,false,dimension:dimension,noRecursion:noRecursion);
		if (res2.GetTotalValue() > bestAttack.GetTotalValue())
		{
			bestAttack = res2;
		}

		damagePotential = bestAttack.GetTotalValue();

		return new Tuple<int, int>(protectionPentalty, damagePotential);
	}



	public struct AbilityUse
	{
		public IUnitAbility? Ability;
		public Vector2Int target = new Vector2Int();
		public int Dmg=0;
		public int Supression=0;
		public int totalChangeScore;

		public AbilityUse()
		{
		}

		public int GetTotalValue()
		{
			return Dmg + Supression + totalChangeScore;
		}

		public static bool operator >(AbilityUse a, AbilityUse b)
		{
			return a.GetTotalValue() > b.GetTotalValue();
		}

		public static bool operator <(AbilityUse a, AbilityUse b)
		{
			return a.GetTotalValue() < b.GetTotalValue();	
		}
	}
}