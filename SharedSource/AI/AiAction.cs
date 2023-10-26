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

	public struct PotentialAbilityActivation
	{
		public List<SequenceAction> Consequences;
		public int abilityIndex;
		public Vector2Int targetPosition;
		public string Name;
		public Unit User;
		public PotentialAbilityActivation(string name, int abilityIndex, Unit user,List<SequenceAction> consequences, Vector2Int targetPosition)
		{
			this.Name = name;
			this.User = user;
			Consequences = consequences;
			this.abilityIndex = abilityIndex;
			this.targetPosition = targetPosition;
		}
	}

	protected static List<PotentialAbilityActivation> IterateAllAbilities(Unit attacker, Vector2Int targetPosition, bool excludeExempt, int dimension = -1)
	{
		List<PotentialAbilityActivation> ret = new List<PotentialAbilityActivation>();
		ret.AddRange(IterateTargetedAbilities(attacker,targetPosition,excludeExempt,dimension));
		ret.AddRange(IterateImmideateAbilities(attacker,excludeExempt,dimension));
		return ret;
	}
	protected static List<PotentialAbilityActivation>  IterateTargetedAbilities(Unit attacker, Vector2Int targetPosition, bool excludeExempt, int dimension = -1)
	{

		List<PotentialAbilityActivation> ret = new List<PotentialAbilityActivation>();
		
		foreach (var ability in attacker.Abilities)
		{
			if(ability.AIExempt&&excludeExempt) continue; 
			if (ability.ImmideateActivation) continue;
			foreach (var attackTile in WorldManager.Instance.GetTilesAround(targetPosition, 1, dimension)){
				
				var cons = ability.GetConsequences(attacker, attackTile.Position,dimension);
				ret.Add(new PotentialAbilityActivation(ability.Name,ability.Index,attacker,cons,attackTile.Position));
			}
		}

		return ret;
	}
	protected static List<PotentialAbilityActivation>  IterateImmideateAbilities(Unit attacker, bool excludeExempt, int dimension = -1)
	{
		List<PotentialAbilityActivation> ret = new List<PotentialAbilityActivation>();
		
		
		foreach (var ability in attacker.Abilities)
		{
			if(ability.AIExempt&&excludeExempt) continue; 
			if (!ability.ImmideateActivation) continue;
			foreach (var attackTile in WorldManager.Instance.GetTilesAround(attacker.WorldObject.TileLocation.Position, 1, dimension))
			{
				
				var cons = ability.GetConsequences(attacker, attackTile.Position,dimension);
				ret.Add(new PotentialAbilityActivation(ability.Name,ability.Index,attacker,cons,attackTile.Position));
			}
		}

		return ret;
	}

	public static AbilityUse ScoreAbility(PotentialAbilityActivation ability, Unit attacker, int dimension, bool noRecursion, bool nextTurnUse)
	{
		int damage = 0;
		int supression = 0;
		int totalChangeScore =0;

		if (attacker.Abilities[ability.abilityIndex].CanPerform(attacker, ability.targetPosition, true,nextTurnUse, dimension).Item1)
		{
			foreach(var c in ability.Consequences){
				var consiquence = ScoreConsequence(c,dimension,attacker,noRecursion);
				damage += consiquence.Item1;
				supression += consiquence.Item2;
				totalChangeScore += consiquence.Item3;
			}
		}


		
                    
		AbilityUse attackResult = new AbilityUse();
		attackResult.Dmg = damage;
		attackResult.Supression = supression;
		attackResult.TotalChangeScore = totalChangeScore;
		attackResult.AbilityIndex = ability.abilityIndex;
		attackResult.TargetPosition = ability.targetPosition;
		return attackResult;
	}

	public static Tuple<int,int,int> ScoreConsequence(SequenceAction c,int dimension, Unit attacker, bool noRecursion)
	{
		int damage = 0;
		int supression = 0;
		int totalChangeScore = 0;
		
		if (c.GetType() == typeof(TakeDamage) )
		{
			TakeDamage tkdmg =  (TakeDamage)c;
			Unit? hitUnit = null;
			if (tkdmg.ObjID != -1)
			{
				hitUnit = WorldManager.Instance.GetObject(((TakeDamage)c).ObjID,dimension)!.UnitComponent;
			}
			else if(tkdmg.Position != new Vector2Int(-1, -1))
			{

				hitUnit = WorldManager.Instance.GetTileAtGrid(((TakeDamage)c).Position,dimension).UnitAtLocation;
				
			}
			if (hitUnit != null)
			{
				int dmgThisAttack = 0;
				if (hitUnit.Determination > 0)
				{
					dmgThisAttack += ((TakeDamage) c).Dmg - ((TakeDamage) c).DetResistance;
				}
				else
				{
					dmgThisAttack += ((TakeDamage) c).Dmg;
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
				var prechangeScore = GetBestPossibleAbility(pseudoUnit, false,true,true,newDim);
				
				var aChange =change.ActChange.GetChange(pseudoUnit.ActionPoints);
				var mchanghe = change.MoveChange.GetChange(pseudoUnit.MovePoints);
				var dchanghe = change.DetChange.GetChange(pseudoUnit.Determination);
				var mrchanghe = change.MoveRangeeffectChange.GetChange(pseudoUnit.MoveRangeEffect);
				
				change.ActChange.Apply(ref pseudoUnit.ActionPoints);
				change.MoveChange.Apply(ref pseudoUnit.MovePoints);
				change.DetChange.Apply(ref pseudoUnit.Determination);
				change.MoveRangeeffectChange.Apply(ref pseudoUnit.MoveRangeEffect);
				
				var postchangeScore = GetBestPossibleAbility(pseudoUnit, false,true,true,newDim);
				

				
				var outcome =postchangeScore.GetTotalValue()- prechangeScore.GetTotalValue();
				int defenceOutcome = 0;
				if (outcome > 0)//if we get better attack oppotunities only then check if expose ourselves
				{
									
					pseudoUnit.ActionPoints.Current -= aChange;
					pseudoUnit.MovePoints.Current -= mchanghe;
					pseudoUnit.Determination.Current -= dchanghe;
					pseudoUnit.MoveRangeEffect.Current -= mrchanghe;
					
					
					var prechangeDefenceScore = ProtectionPentalty(pseudoUnit,newDim, true);
					
					change.ActChange.Apply(ref pseudoUnit.ActionPoints);
					change.MoveChange.Apply(ref pseudoUnit.MovePoints);
					change.DetChange.Apply(ref pseudoUnit.Determination);
					change.MoveRangeeffectChange.Apply(ref pseudoUnit.MoveRangeEffect);
					
					var postchangeDefenceScore = ProtectionPentalty(pseudoUnit,newDim, true);

					defenceOutcome = postchangeDefenceScore - prechangeDefenceScore;
				}

				outcome *= 3;
				defenceOutcome *= 2;
				outcome += defenceOutcome;
				if (outcome == 0 && mchanghe>0)
				{
					outcome += 1 ;//if there is no change in score, we still want to encourage movement buffs
				}

				WorldManager.Instance.WipePseudoLayer(newDim);
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
					var res= ScoreConsequence(cact, dimension, attacker,noRecursion);
					damage += res.Item1;
					supression += res.Item2;
					totalChangeScore += res.Item3;
				}
			}
			
		}

		return new Tuple<int, int, int>(damage,supression,totalChangeScore);
	}




	public abstract void Execute(Unit unit);
	public abstract int GetScore(Unit unit);
	
	public struct MoveCalcualtion
	{
		public float ClosestDistance;
		public int DistanceReward;
		public int ProtectionPentalty;
		public int StartingMovePoints;
		public int EndMovePoints = -99;
		public int ClumpingPenalty;
		public int DamagePotential;
		public int CoverBonus;

		public MoveCalcualtion()
		{
			ClosestDistance = 0;
			DistanceReward = 0;
			ProtectionPentalty = 0;
			ClumpingPenalty = 0;
			DamagePotential = 0;
		}

		public override string ToString()
		{
			return "closestDistance: "+ClosestDistance+" distanceReward: "+DistanceReward+" protectionPentalty: "+ProtectionPentalty+" clumpingPenalty: "+ClumpingPenalty+" damagePotential: "+DamagePotential + " coverBonus: "+CoverBonus +" start moves:"+StartingMovePoints+" end moves: "+EndMovePoints +" total: "+GetTotalValue();
		}

		private float GetTotalValue()
		{
			return (ClosestDistance + DistanceReward + ProtectionPentalty + ClumpingPenalty + DamagePotential + CoverBonus);
		}
	}

	public static int GetTileMovementScore(Vector2Int tilePosition, int movesToUse, bool crouch, Unit realUnit, out MoveCalcualtion details)
	{
#if DEBUG && !CLIENT
		if (crouch == realUnit.Crouching)//crouch moves are a bit more compilcated, skip this check
		{

			var result = PathFinding.GetPath(realUnit.WorldObject.TileLocation.Position, tilePosition);

			int calcMovesToUse = 0;
			int moveRange = realUnit.GetMoveRange();
			while (result.Cost > moveRange*calcMovesToUse)
			{
				calcMovesToUse++;
			}
			
			if (calcMovesToUse!=movesToUse)
			{
				throw new Exception("moves to use not equal to cost of path");
			}
		}
#endif
		return GetTileMovementScore(tilePosition, new ChangeUnitValues(-1,0,-movesToUse),crouch, realUnit, out details);
	}

	
	public static int GetTileMovementScore(Vector2Int tilePosition, ChangeUnitValues? valueMod,  bool crouch, Unit realUnit, out MoveCalcualtion details)
	{
		if(WorldManager.Instance.GetTileAtGrid(tilePosition).UnitAtLocation != null && !Equals(WorldManager.Instance.GetTileAtGrid(tilePosition).UnitAtLocation, realUnit))
		{
			details = new MoveCalcualtion();
			return -1000;
		}
		details = new MoveCalcualtion();
		int score = 0;
		details.StartingMovePoints = realUnit.MovePoints.Current;


		Unit hypotheticalUnit;
		int dimension = WorldManager.Instance.CreatePseudoWorldWithUnit(realUnit,tilePosition, out hypotheticalUnit);
		
		var otherTeamUnits = GameManager.GetTeamUnits(!realUnit.IsPlayer1Team,dimension);
		hypotheticalUnit.Crouching = crouch;

		
		if (valueMod is not null)
		{
			details.EndMovePoints = realUnit.MovePoints.Current + valueMod.MoveChange.GetChange(realUnit.MovePoints);
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
        
		details.ClosestDistance = closestDistance;
		closestDistance = Math.Max(0, closestDistance);
		int distanceReward = 40;
		distanceReward -= Math.Min(distanceReward, (int)closestDistance);//if we're futher than our optimal range, we get less points
		score += distanceReward;
		details.DistanceReward = distanceReward;
		

		int protectionPentalty = ProtectionPentalty(hypotheticalUnit, dimension, false);
		
		var bestAttack = GetBestPossibleAbility(hypotheticalUnit, false, false,true,dimension);
		
		int damagePotential = bestAttack.GetTotalValue();


		
	
		protectionPentalty *=2;//cover is VERY important
		

		score += protectionPentalty;
		details.ProtectionPentalty = protectionPentalty;
		//	details.EnemyAttackScores = enemyAttackScores;
	
		
		damagePotential *= 3; //damage is important
		
		if(damagePotential <= 0)
		{
			damagePotential = -80;//discourage damageless tiles, we do this instead of vission checks
		}

		details.DamagePotential = damagePotential; 
	
		score += damagePotential;

		
		WorldManager.Instance.WipePseudoLayer(dimension);


		var myTeamUnits = GameManager.GetTeamUnits(realUnit.IsPlayer1Team,dimension);
		int clumpingPenalty = 0;
		foreach (var u in myTeamUnits)
		{
			if(Equals(u, realUnit)) continue;
			var friendLoc = u.WorldObject.TileLocation.Position;
			var dist = Vector2.Distance(friendLoc, tilePosition);
			clumpingPenalty -= (int)(6/dist);
		}
		score += clumpingPenalty;
		details.ClumpingPenalty = clumpingPenalty;
		
		int coverBonus = 0;
		float stackingRatio = 1;
		
		foreach(var enemy in otherTeamUnits){
			
			var enemyDirection = Utility.Vec2ToDir( enemy.WorldObject.TileLocation.Position-tilePosition);
			Cover coverInDirection = WorldManager.Instance.GetCover(tilePosition,enemyDirection, false);
			float bonus = 0;
			if (coverInDirection == Cover.High)
			{
				bonus += 5;
			}
			else if (coverInDirection == Cover.Low)
			{
				bonus += 2;
			}

			coverBonus += (int) (bonus * stackingRatio);
			stackingRatio *= 0.3f;
		}
        
		score += coverBonus;
		details.CoverBonus = coverBonus;
        
		

		return score;
	}

	
	protected static AbilityUse GetBestPossibleAbility(Unit attacker, bool onlyVisible, bool noRecursion, bool excludeExempt, int dimension = -1)
	{
		var allUnits = GameManager.GetTeamUnits(!attacker.IsPlayer1Team);
		allUnits.AddRange(GameManager.GetTeamUnits(attacker.IsPlayer1Team));
		AbilityUse bestAttack = new AbilityUse();
		List<PotentialAbilityActivation> attacks = new List<PotentialAbilityActivation>();
		foreach (var enemy in allUnits)
		{
			if (!onlyVisible || WorldManager.Instance.CanTeamSee(enemy.WorldObject.TileLocation.Position, attacker.IsPlayer1Team) >= enemy.WorldObject.GetMinimumVisibility())
			{
				attacks.AddRange(IterateTargetedAbilities(attacker, enemy.WorldObject.TileLocation.Position,excludeExempt,dimension));
			}
		}
		attacks.AddRange( IterateImmideateAbilities(attacker,excludeExempt,dimension));

		foreach (var a in attacks)
		{
			var res = ScoreAbility(a, attacker, dimension, noRecursion,false);
			if (res.GetTotalValue() > bestAttack.GetTotalValue())
			{
				bestAttack = res;
			}
		}
		
		return bestAttack;
	}
	private static int ProtectionPentalty(Unit unit, int dimension, bool noRecursion)
	{
		var otherTeamUnits = GameManager.GetTeamUnits(!unit.IsPlayer1Team,dimension);
		int protectionPentalty = 0;
		List<PotentialAbilityActivation> enemyAttacks = new List<PotentialAbilityActivation>();

		bool r = WorldManager.Instance.GetCachedAttacksInDimension(ref enemyAttacks, dimension, false);
		if (!r)
		{
			foreach (var enemy in otherTeamUnits)
			{
				enemyAttacks.AddRange(IterateAllAbilities(enemy, unit.WorldObject.TileLocation.Position, true, dimension));
			}
		}

		WorldManager.Instance.CacheAttacksInDimension(enemyAttacks, dimension, false);

		foreach (var a in enemyAttacks)
		{
			var res = ScoreAbility(a, a.User, dimension, noRecursion, true);
			if (res.GetTotalValue() > 0)//dont considere enem attacks that are beneficial to us as the would  never do  them
			{
				protectionPentalty -= res.GetTotalValue();
			}
		}
		return protectionPentalty;
	}

	


	public struct AbilityUse
	{
		public int Dmg=0;
		public int Supression=0;
		public int TotalChangeScore=0;
		public int AbilityIndex;
		public Vector2Int TargetPosition;
		
		public AbilityUse()
		{
		}

		public int GetTotalValue()
		{
			return Dmg + Supression + TotalChangeScore;
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