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
		OverWatch = 5,
		UseAbility = 7,

		
	}
	public static readonly object attackLock = new object();
	protected static AbilityUse GetWorstPossibleAttack(Vector2Int hypotheticalAttackerPos, Unit Attacker, Vector2Int TargetPosition, Unit Target, bool nextTurn =false)
	{

		AbilityUse worstAttack = new AbilityUse();
		lock (attackLock)
		{
			
		
			Vector2Int? oldPos =null;
			if(hypotheticalAttackerPos != Attacker.WorldObject.TileLocation.Position)
			{
				oldPos= Attacker.WorldObject.TileLocation.Position;
				Attacker.WorldObject.Move(hypotheticalAttackerPos);
			}

		
			Parallel.ForEach(Attacker.Abilities, ability =>
			{
				
				Parallel.ForEach(WorldManager.Instance.GetTilesAround(TargetPosition, 2), attackTile =>
				{
					int damage = 0;
					int supression = 0;

					if(!ability.CanPerform(Attacker, attackTile.Position,nextTurn).Item1) return;
                    
					var cons = ability.GetConsequences(Attacker, attackTile.Position);
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
								hitUnit = WorldManager.Instance.GetTileAtGrid(((TakeDamage)c).position).UnitAtLocation;
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
								hitUnit = WorldManager.Instance.GetTileAtGrid(((Suppress)c).Requirements.Position).UnitAtLocation;
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
			

			if (oldPos.HasValue)
			{
				Attacker.WorldObject.Move(oldPos.Value);
			}
		}
		return worstAttack;
	}

	protected static List<Unit> GetMyTeamUnits(bool playerOne)
	{
#if SERVER
		var myTeamUnitsIds = playerOne ? GameManager.T1Units : GameManager.T2Units;
		

		List<Unit> myTeamUnits = new List<Unit>();
		
		foreach (var id in myTeamUnitsIds)
		{
			myTeamUnits.Add( WorldManager.Instance.GetObject(id).UnitComponent);
		}
        
#else
		var myTeamUnits = GameLayout.MyUnits;//this assumes we're getting movement score for our own units
			
#endif

		return myTeamUnits;
	}

	protected static List<Unit> GetOtherTeamUnits(bool playerOne)
	{
		
#if SERVER
		var otherTeamUnitsIds = playerOne? GameManager.T2Units : GameManager.T1Units;
		List<Unit> otherTeamUnits = new List<Unit>();
		foreach (var id in otherTeamUnitsIds)
		{
			otherTeamUnits.Add( WorldManager.Instance.GetObject(id).UnitComponent);
		}
		
#else
		var otherTeamUnits = GameLayout.EnemyUnits;

#endif
		return otherTeamUnits;
	}

	protected static AbilityUse GetWorstPossibleAttackOnEnemyTeam(Unit attacker, bool nextTurn)
	{
		var otherTeam = GetOtherTeamUnits(attacker.IsPlayer1Team);
		AbilityUse bestAttack = new AbilityUse();
		foreach (var enemy in otherTeam)
		{
			var res =GetWorstPossibleAttack(attacker.WorldObject.TileLocation.Position,attacker,enemy.WorldObject.TileLocation.Position,enemy, nextTurn);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
			if(res > bestAttack)
			{
				bestAttack = res;
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