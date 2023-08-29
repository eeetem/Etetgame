using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using DefconNull.WorldObjects.Units.ReplaySequence;

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
		
	}
	
	protected static AbilityUse GetWorstPossibleAttack(Unit Attacker, Vector2Int TargetPosition, Unit pseudoUnitAtLocation, bool nextTurn =false, int dimension = -1)
	{

		AbilityUse worstAttack = new AbilityUse();
		Parallel.ForEach(Attacker.Abilities, ability =>
		{
			Parallel.ForEach(WorldManager.Instance.GetTilesAround(TargetPosition, 2, dimension), attackTile =>
			{
				int damage = 0;
				int supression = 0;

				if(!ability.CanPerform(Attacker, attackTile.Position,nextTurn,dimension).Item1) return;
                    
				var cons = ability.GetConsequences(Attacker, attackTile.Position,dimension);
				foreach (var c in cons)
				{
					if (c.GetType() == typeof(TakeDamage) )
					{
						TakeDamage tkdmg =  (TakeDamage)c;
						Unit? hitUnit = null;
						if (tkdmg.objID != -1)
						{
							hitUnit = WorldManager.Instance.GetObject(((TakeDamage)c).objID)!.UnitComponent;
						}
						else if(tkdmg.position != new Vector2Int(-1, -1))
						{
							if(tkdmg.position == pseudoUnitAtLocation.WorldObject.TileLocation.Position)
							{
								hitUnit = pseudoUnitAtLocation;
							}
							else
							{
								hitUnit = WorldManager.Instance.GetTileAtGrid(((TakeDamage)c).position).UnitAtLocation;
							}
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
								
							if (dmgThisAttack > hitUnit.Health)
							{
								dmgThisAttack *= 3;
							}
							if(hitUnit.IsPlayer1Team != Attacker.IsPlayer1Team)
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
							
						Unit? hitUnit = null;
						if (sup.Requirements.ActorID != -1)
						{
							hitUnit = WorldManager.Instance.GetObject(((Suppress)c).Requirements.ActorID)!.UnitComponent;
						}
						else if(sup.Requirements.Position != new Vector2Int(-1, -1))
						{
							if(sup.Requirements.Position  == pseudoUnitAtLocation.WorldObject.TileLocation.Position)
							{
								hitUnit = pseudoUnitAtLocation;
							}
							else
							{
								hitUnit = WorldManager.Instance.GetTileAtGrid(((Suppress)c).Requirements.Position).UnitAtLocation;
							}
							
						}
                            
						if (hitUnit != null)
						{
							int supressionThisAttack = 0;
							supressionThisAttack += ((Suppress) c).detDmg;
							if(supressionThisAttack > hitUnit.Determination)
							{
								supressionThisAttack += ((Suppress) c).detDmg*2;
							}
							if (hitUnit.IsPlayer1Team != Attacker.IsPlayer1Team)
							{
								supression += supressionThisAttack;
							}
							else
							{
								supression -= supressionThisAttack;
							}
						}
							
					}
				}
                    
				AbilityUse attackResult = new AbilityUse();
				attackResult.Ability = ability;
				attackResult.target = attackTile.Position;
				attackResult.Dmg = damage;
				attackResult.Supression = supression;
					
				if (attackResult.GetTotalValue() > worstAttack.GetTotalValue())
				{
					worstAttack = attackResult;
				}
				
			});
		});
			


		return worstAttack;
	}

	protected static bool CanPotentiallyAttack(Unit unit, Unit target, int dimension = -1)
	{
		var attack = GetWorstPossibleAttack(unit,target.WorldObject.TileLocation.Position,target,true,dimension);
		return attack.GetTotalValue() > 0;
	}
	protected static AbilityUse GetWorstPossibleAttackOnEnemyTeam(Unit attacker, bool nextTurn, bool onlyVisible)
	{
		var otherTeam = GameManager.GetTeamUnits(!attacker.IsPlayer1Team);
		AbilityUse bestAttack = new AbilityUse();
		foreach (var enemy in otherTeam)
		{
			if (!onlyVisible || WorldManager.Instance.CanTeamSee(enemy.WorldObject.TileLocation.Position, attacker.IsPlayer1Team) >= enemy.WorldObject.GetMinimumVisibility())
			{
				var res =GetWorstPossibleAttack(attacker,enemy.WorldObject.TileLocation.Position, enemy,nextTurn);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
				if(res > bestAttack)
				{
					bestAttack = res;
				}
			}

		}

		return bestAttack;
	}


	public abstract void Execute(Unit unit);
	public abstract int GetScore(Unit unit);
	
	
	public struct AbilityUse
	{
		public IUnitAbility? Ability;
		public Vector2Int target = new Vector2Int();
		public int Dmg=0;
		public int Supression=0;

		public AbilityUse()
		{
		}

		public int GetTotalValue()
		{
			return Dmg + Supression;
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