using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;
#if CLIENT
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.AI;

public class Move : AIAction
{
	public Move() : base(AIActionType.Move)
	{
	}

	public override void Execute(Unit unit)
	{
		int movesToUse = 0;
		if (unit.MovePoints.Current > unit.ActionPoints.Current)
		{
			movesToUse = unit.MovePoints.Current - unit.ActionPoints.Current;//use our free moves
		}
		else
		{
			movesToUse = unit.MovePoints.Current; // we're out of free moves, use all our moves
		}

		var locs = GetMovementLocations(unit,movesToUse);

		int bestOf = Math.Min(locs.Count, 1);
						
						
		var result = locs
			.OrderByDescending(x => x.Item2)
			.Take(bestOf)
			.ToArray();
		//pick random location out of top [bestOf]
		int r = Random.Shared.Next(bestOf);
		Vector2Int target = result[r].Item1;

		if (target == unit.WorldObject.TileLocation.Position)
		{
			Console.WriteLine("AI decided to stay put");
			return;
		}
						

		Console.WriteLine("ordering move action from: "+unit.WorldObject.TileLocation.Position+" to: "+target+" with score: "+result[r].Item2);
		unit.DoAction(Action.Actions[Action.ActionType.Move], target);
		Console.WriteLine(" waiting for sequence to clear.....");
	}
	
	
	private static ConcurrentBag<Tuple<Vector2Int, int>> GetMovementLocations(Unit unit,int distance = 1)
	{
		List<Vector2Int>[] allLocations = unit.GetPossibleMoveLocations();
		allLocations[0].Add(unit.WorldObject.TileLocation.Position);
		var scoredLocations = new ConcurrentBag<Tuple<Vector2Int, int>>();

		for (int i = 0; i < Math.Min(distance,allLocations.Length); i++)
		{
			Parallel.ForEach(allLocations[i], l =>
			{
				int score = GetTileMovementScore(l, unit, out _);
				scoredLocations.Add(new Tuple<Vector2Int, int>(l, score));
			});
		}

		return scoredLocations;
	}
	public struct MoveCalcualtion
	{
		public float closestDistance;
		public int distanceReward;
		public int protectionPentalty;
		public List<Tuple<string,int,int>> EnemyAttackScores;
		public int visibilityScore;
		public int clumpingPenalty;
		public int damagePotential;
	}
	
	public static int GetTileMovementScore(Vector2Int vec, Unit unit, out MoveCalcualtion details)
	{
		if(WorldManager.Instance.GetTileAtGrid(vec).UnitAtLocation != null)
		{
			details = new MoveCalcualtion();
			return -1000;
		}
		details = new MoveCalcualtion();
		int score = 0;
		bool team = unit.IsPlayer1Team;
		var tile = WorldManager.Instance.GetTileAtGrid(vec);
#if SERVER
		var myTeamUnitsIds = team ? GameManager.T1Units : GameManager.T2Units;
		var otherTeamUnitsIds = team ? GameManager.T2Units : GameManager.T1Units;

		List<Unit> myTeamUnits = new List<Unit>();
		List<Unit> otherTeamUnits = new List<Unit>();
		foreach (var id in myTeamUnitsIds)
		{
			myTeamUnits.Add( WorldManager.Instance.GetObject(id).UnitComponent);
		}

		foreach (var id in otherTeamUnitsIds)
		{
			otherTeamUnits.Add( WorldManager.Instance.GetObject(id).UnitComponent);
		}

#else
			var myTeamUnits = GameLayout.MyUnits;//this assumes we're getting movement score for our own units
			var otherTeamUnits = GameLayout.EnemyUnits;
#endif
			
			
			
			
		//add points for being in range of your primiary attack
		float closestDistance=1000;
			
		foreach (var u in otherTeamUnits)
		{
			var enemyLoc = u.WorldObject.TileLocation.Position;
			var dist = Vector2.Distance(enemyLoc, vec);
			if(dist< closestDistance){
				closestDistance = dist;
			}
		}
			

		closestDistance -= unit.GetAction(-1).GetOptimalRangeAI();
        
		details.closestDistance = closestDistance;
		closestDistance = Math.Max(0, closestDistance);
		int distanceReward = 20;
		distanceReward -= Math.Min(distanceReward, (int)closestDistance);//if we're futher than our optimal range, we get less points

		score += distanceReward;
		details.distanceReward = distanceReward;
			
        
		List<Tuple<string,int,int>> enemyAttackScores = new List<Tuple<string, int,int>>();
		//add points for being protected
		int protectionPentalty = 0;
		foreach (var u in otherTeamUnits)
		{
			var res =GetWorstPossibleAttack(u.WorldObject.TileLocation.Position,u,vec,unit);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
			protectionPentalty -= res.Item2;
			protectionPentalty -= res.Item3;
			enemyAttackScores.Add(new Tuple<string, int,int>(res.Item1, res.Item2,res.Item3));
			
			
		}

		protectionPentalty *= 2;//cover is quite important
		score += protectionPentalty;
		details.protectionPentalty = protectionPentalty;
		details.EnemyAttackScores = enemyAttackScores;

		
		int visibilityScore = -30;
		int visibleEnemies = 0;
		foreach (var u in otherTeamUnits)
		{
			if (WorldManager.Instance.CanSee(unit, u.WorldObject.TileLocation.Position) >= u.WorldObject.GetMinimumVisibility())
			{
				visibleEnemies++;
			}
		}
		if (visibleEnemies == 1)
		{
			visibilityScore += 50;//good to see 1
		}
		visibilityScore -= (visibleEnemies-1) * 20;//reduce for more
		
		score += visibilityScore;
		details.visibilityScore = visibilityScore;

		int clumpingPenalty = 0;
		foreach (var u in myTeamUnits)
		{
			if(Equals(u, unit)) continue;
			var friendLoc = u.WorldObject.TileLocation.Position;
			var dist = Vector2.Distance(friendLoc, vec);
			clumpingPenalty -= (int)(5/dist);
		}
		score += clumpingPenalty;
		details.clumpingPenalty = clumpingPenalty;
		
		Tuple<string,int,int> BestAttack = new Tuple<string, int, int>("",0,0);
		int damagePotential = 0;
		foreach (var enemy in otherTeamUnits)
		{
			var res =GetWorstPossibleAttack(vec,unit,enemy.WorldObject.TileLocation.Position,enemy);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
			if(res.Item2 + res.Item3 > BestAttack.Item2 + BestAttack.Item3)
			{
				BestAttack = res;
			}
		}
		damagePotential = BestAttack.Item2 + BestAttack.Item3;
		damagePotential *= 2;//damage is important
		details.damagePotential = damagePotential;
	
		score += damagePotential;
        
        
		//account for good positions on to buff tamemates from
		return score;
	}

	public static readonly object attackLock = new object();
	private static Tuple<string,int,int> GetWorstPossibleAttack(Vector2Int hypotheticalAttackerPos, Unit Attacker, Vector2Int hypotheticalTargetPosition, Unit HypotheticalTarget)
	{
		
		Tuple<string,int,int> worstAttack = new Tuple<string, int, int>("",0,0);
		lock (attackLock)
		{
			
		
			Vector2Int? oldPos =null;
			if(hypotheticalAttackerPos != Attacker.WorldObject.TileLocation.Position)
			{
				oldPos= Attacker.WorldObject.TileLocation.Position;
				Attacker.WorldObject.Move(hypotheticalAttackerPos);
			}

		
			Parallel.ForEach(Attacker.GetFullAbilityList(), ability =>
			{
				Parallel.ForEach(WorldManager.Instance.GetTilesAround(hypotheticalTargetPosition, 2), attackTile =>
				{
					int Damage = 0;
					int Supression = 0;
					bool canPerform = true;
					foreach (var eff in ability.Effects)
					{
						if (!eff.CanPerform(Attacker, attackTile.Position).Item1)
						{
							canPerform = false;
							break;
						}
					}

					if (!canPerform) return;

					var cons = ability.GetConsequences(Attacker, attackTile.Position);
					foreach (var c in cons)
					{
						if (c.GetType() == typeof(TakeDamage) && ((TakeDamage) c).position == hypotheticalTargetPosition)
						{
							if (HypotheticalTarget.Determination > 0)
							{
								Damage += ((TakeDamage) c).dmg - ((TakeDamage) c).detResistance;
							}
							else
							{
								Damage += ((TakeDamage) c).dmg;
							}

						}
						else if (c.GetType() == typeof(Suppress) && ((Suppress) c).Requirements.Position == hypotheticalTargetPosition)
						{
							Supression += ((Suppress) c).detDmg;
						}
					}



					if (Damage > HypotheticalTarget.Health)
					{
						Damage *= 3;
					}
					else if (Supression > HypotheticalTarget.Determination)
					{
						Supression *= 2;
					}

					if (Damage + Supression > worstAttack.Item2 + worstAttack.Item3)
					{
						worstAttack = new Tuple<string, int, int>(ability.Name, Damage, Supression);
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
}