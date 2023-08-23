using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World;
using DefconNull.World.WorldActions;
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

	private int GetMovesToUse(Unit u)
	{
		int movesToUse = 0;
		if (u.MovePoints.Current > u.ActionPoints.Current)
		{
			movesToUse = u.MovePoints.Current - u.ActionPoints.Current;//use our free moves
		}
		else
		{
			movesToUse = u.MovePoints.Current; // we're out of free moves, use all our moves
		}
		
		return movesToUse;
	}

	public override void Execute(Unit unit)
	{

		int movesToUse = GetMovesToUse(unit);
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

	public override int GetScore(Unit unit)
	{
		if(unit.MovePoints.Current <= 0)
		{
			return 0;
		}
		int movesToUse = GetMovesToUse(unit);
		var locs = GetMovementLocations(unit,movesToUse);
		int scoreForCurrentTile = GetTileMovementScore(unit.WorldObject.TileLocation.Position, unit, out _);
		
		int totalScore = 0;
		int countedLocs = 0;
		foreach (var loc in locs)
		{
			if (loc.Item2 != -1000)
			{
				countedLocs++;
				totalScore += loc.Item2;
			}
		}
		if (countedLocs == 0)
		{
			return 0;
		}
		int averageScore = totalScore / countedLocs;

		int worseThanAverage = (averageScore - scoreForCurrentTile);
		if(worseThanAverage < 0)
		{
			worseThanAverage = 0;
		}
		int actionScore = 1 + worseThanAverage;//diffference bwteen current tile and average of all other tiles
		if (unit.MovePoints > unit.ActionPoints)
		{
			actionScore *= 2;
		}

		return actionScore;
	}


	private static ConcurrentBag<Tuple<Vector2Int, int>> GetMovementLocations(Unit unit,int distance = 1)
	{
		List<Vector2Int>[] allLocations = unit.GetPossibleMoveLocations();
		allLocations[0].Add(unit.WorldObject.TileLocation.Position);
		var scoredLocations = new ConcurrentBag<Tuple<Vector2Int, int>>();
	//	var normalisedScoredLocations = new ConcurrentBag<Tuple<Vector2Int, int>>();
		
		int lowestScore = 1000;
		for (int i = 0; i < Math.Min(distance,allLocations.Length); i++)
		{
			Parallel.ForEach(allLocations[i], l =>
			{
				int score = GetTileMovementScore(l, unit, out _);
				if(score< lowestScore)
				{
					lowestScore = score;
				}
				scoredLocations.Add(new Tuple<Vector2Int, int>(l, score));
			});
		}

		//normalise all socres to be above 0
	//	foreach (var loc in scoredLocations)
	//	{
		//	normalisedScoredLocations.Add(new Tuple<Vector2Int, int>(loc.Item1, loc.Item2 + lowestScore));
	//}
		
		

		return scoredLocations;
	}
	public struct MoveCalcualtion
	{
		public float closestDistance;
		public int distanceReward;
		public int protectionPentalty;
		public List<AbilityUse> EnemyAttackScores =  new List<AbilityUse>();
		public int visibilityScore;
		public int clumpingPenalty;
		public int damagePotential;

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
	
	public static int GetTileMovementScore(Vector2Int vec, Unit unit, out MoveCalcualtion details)
	{
		if(WorldManager.Instance.GetTileAtGrid(vec).UnitAtLocation != null && WorldManager.Instance.GetTileAtGrid(vec).UnitAtLocation != unit)
		{
			details = new MoveCalcualtion();
			return -1000;
		}
		details = new MoveCalcualtion();
		int score = 0;
		bool team = unit.IsPlayer1Team;
		var tile = WorldManager.Instance.GetTileAtGrid(vec);


		var otherTeamUnits = GetOtherTeamUnits(unit.IsPlayer1Team);
			
			
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
			

		closestDistance -= unit.Abilities[0].GetOptimalRangeAI();
        
		details.closestDistance = closestDistance;
		closestDistance = Math.Max(0, closestDistance);
		int distanceReward = 40;
		distanceReward -= Math.Min(distanceReward, (int)closestDistance);//if we're futher than our optimal range, we get less points

		score += distanceReward;
		details.distanceReward = distanceReward;
			
        
		List<AbilityUse> enemyAttackScores = new List<AbilityUse>();
		//add points for being protected
		int protectionPentalty = 0;
		foreach (var u in otherTeamUnits)
		{
			var res =GetWorstPossibleAttack(u.WorldObject.TileLocation.Position,u,vec,unit);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
			protectionPentalty -= res.GetTotalValue();
			enemyAttackScores.Add(res);
			
			
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

		
		var myTeamUnits = GetMyTeamUnits(unit.IsPlayer1Team);
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
		
		AbilityUse bestAttack = new AbilityUse();
		int damagePotential = 0;
		foreach (var enemy in otherTeamUnits)
		{
			var res =GetWorstPossibleAttack(vec,unit,enemy.WorldObject.TileLocation.Position,enemy);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
			if (res.GetTotalValue() > bestAttack.GetTotalValue()) 
			{
				bestAttack = res;
			}
		}

		damagePotential = bestAttack.GetTotalValue();
		damagePotential *= 2;//damage is important
		details.damagePotential = damagePotential; 
	
		score += damagePotential;
        
        
		//account for good positions on to buff tamemates from
		return score;
	}


	
}