using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

	private static int GetMovesToUse(Unit u)
	{
		
		int movesToUse = 0;
		if (u.MovePoints.Current > u.ActionPoints.Current)
		{
			movesToUse = u.MovePoints.Current - u.ActionPoints.Current;//use our free moves
			if(movesToUse == 1 && u.Crouching)//not doing this will always make them stay crouching if the only have 1 free move
			{
				movesToUse = u.MovePoints.Current;
			}
		}
		else
		{
			movesToUse = u.MovePoints.Current; // we're out of free moves, use all our moves
		}
		
		return movesToUse;
	}

	public override void Execute(Unit unit)
	{

		int movesToUse = unit.MovePoints.Current;

		var locs = GetMovementLocations(unit,movesToUse);

		int bestOf = Math.Min(locs.Count, 2);

		var oredredResults = locs.OrderByDescending(x => x.Item3);
	
		var best=	oredredResults.Take(bestOf)
			.ToArray();
			
		//pick random location out of top [bestOf]
		int r = Random.Shared.Next(bestOf);
		Vector2Int target = best[r].Item1;
		bool needToDoCrouchAction = best[r].Item2 != unit.Crouching;

		if (unit.Crouching && needToDoCrouchAction)
		{
			unit.DoAction(Action.Actions[Action.ActionType.Crouch], new Vector2Int(0,0));
			needToDoCrouchAction = false;
		}

		if (target != unit.WorldObject.TileLocation.Position)
		{
			Console.WriteLine("ordering move action from: "+unit.WorldObject.TileLocation.Position+" to: "+target+" with score: "+best[r].Item2);
			unit.DoAction(Action.Actions[Action.ActionType.Move], target);
		}
		do
		{
			Thread.Sleep(500);
		} while (WorldManager.Instance.SequenceRunning);
		if (!unit.Crouching && needToDoCrouchAction)
		{
			unit.DoAction(Action.Actions[Action.ActionType.Crouch], new Vector2Int(0,0));
			needToDoCrouchAction = false;
		}

		Console.WriteLine("waiting for sequence to clear.....");
		
		var otherTeamUnits = GameManager.GetTeamUnits(!unit.IsPlayer1Team);
	
		
		float closestDistance = 1000;
		Vector2Int vec = new Vector2Int(0, 0);
		foreach (var u in otherTeamUnits)
		{
			var enemyLoc = u.WorldObject.TileLocation.Position;
			var dist = Vector2.Distance(enemyLoc, target);
			if(dist< closestDistance){
				closestDistance = dist;
				vec = enemyLoc;
			}
		}
		do
		{
			Thread.Sleep(500);
		} while (WorldManager.Instance.SequenceRunning);
		unit.DoAction(Action.Actions[Action.ActionType.Face], vec);//face the enemy closesy to you
	}

	public override int GetScore(Unit unit)
	{
		if(unit.MovePoints.Current <= 0)
		{
			return 0;
		}

		if (unit.MovePoints.Current == 1 && unit.Crouching)
		{
			return 0;
		}

		float worseThanAverage = GetWorseThanAverage(unit);
		if(worseThanAverage <= 0)
		{
			return 1;
		}
		
		float actionScore = 1+worseThanAverage;//diffference bwteen current tile and average of all other tiles
		if (unit.MovePoints == unit.MovePoints.Max)//we should probably move first then do something
		{
			actionScore *= 1.5f;
		}

		return (int)actionScore;
	}

	public static float GetWorseThanAverage(Unit unit)
	{
		int movesToUse = GetMovesToUse(unit);
		var locs = GetMovementLocations(unit, movesToUse);
		int scoreForCurrentTile = GetTileMovementScore(unit.WorldObject.TileLocation.Position, 0,unit.Crouching,unit, out _);

		int countedLocs = 0;
		List<int> scores = new List<int>();
		foreach (var loc in locs)
		{
			if (loc.Item3 != -1000)
			{
				countedLocs++;
				scores.Add(loc.Item3);
			}
		}

		if (countedLocs == 0)
		{
			return 0;
		}


		//int averageScore = totalScore / countedLocs;
		float percentile = Utility.CalculatePercentile(scores, 80);
		float worseThanAverage = percentile - scoreForCurrentTile;

		return worseThanAverage;
	}


	private static ConcurrentBag<Tuple<Vector2Int, bool, int>> GetMovementLocations(Unit unit, int distance)
	{
		List<Vector2Int>[] allLocations = unit.GetPossibleMoveLocations();
		allLocations[0].Add(unit.WorldObject.TileLocation.Position);

		var scoredLocations = new ConcurrentBag<Tuple<Vector2Int,bool, int>>();

		int lowestScore = 1000;
		
		//move locations where we do not change stance
		for (int i = 0; i < Math.Min(distance,allLocations.Length); i++)
		{
			int i1 = i;
			Parallel.ForEach(allLocations[i], l =>
			{
				int score = GetTileMovementScore(l,i1,unit.Crouching, unit, out _);
				if(score< lowestScore)
				{
					lowestScore = score;
				}
				scoredLocations.Add(new Tuple<Vector2Int, bool,int>(l, unit.Crouching,score));
			});
		}

		if (unit.Crouching)
		{
			//stand up then move
			List<Vector2Int>[] standUpLocations = unit.GetPossibleMoveLocations(unit.GetMoveRange()+2);//offset the courch penalty since we're gonna be standing up
			standUpLocations[0].Add(unit.WorldObject.TileLocation.Position);
			for (int i = 0; i < Math.Min(distance-1, standUpLocations.Length); i++)
			{
				int i1 = i;
				Parallel.ForEach(standUpLocations[i], l =>
				{
					int score = GetTileMovementScore(l,i1+1,false, unit, out _);
					if(score< lowestScore)
					{
						lowestScore = score;
					}
					scoredLocations.Add(new Tuple<Vector2Int, bool,int>(l, false,score));
				});
			}
		}
		else
		{
			//move then crouch
			List<Vector2Int>[] crouchLocations = unit.GetPossibleMoveLocations();
			crouchLocations[0].Add(unit.WorldObject.TileLocation.Position);
			for (int i = 0; i < Math.Min(distance-1, crouchLocations.Length); i++)
			{
				int i1 = i;
				Parallel.ForEach(crouchLocations[i], l =>
				{
					int score = GetTileMovementScore(l,i1+1,true, unit, out _);
					if(score< lowestScore)
					{
						lowestScore = score;
					}
					scoredLocations.Add(new Tuple<Vector2Int, bool,int>(l, true,score));
				});
			}
		}








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

	public static int GetTileMovementScore(Vector2Int tilePosition, int movesToReach, bool crouch, Unit realUnit, out MoveCalcualtion details)
	{
		if(WorldManager.Instance.GetTileAtGrid(tilePosition).UnitAtLocation != null && !Equals(WorldManager.Instance.GetTileAtGrid(tilePosition).UnitAtLocation, realUnit))
		{
			details = new MoveCalcualtion();
			return -1000;
		}
		details = new MoveCalcualtion();
		int score = 0;

		var otherTeamUnits = GameManager.GetTeamUnits(!realUnit.IsPlayer1Team);
		Unit hypotheticalUnit;
		int dimension = WorldManager.Instance.PlaceUnitInPseudoWorld(realUnit,tilePosition, out hypotheticalUnit);
		hypotheticalUnit.Crouching = crouch;
		hypotheticalUnit.MovePoints -= movesToReach;
		
		
	
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
			
        
		List<AbilityUse> enemyAttackScores = new List<AbilityUse>();
		//add points for being protected
		int protectionPentalty = 0;
		Parallel.ForEach(otherTeamUnits, u =>
		{
			var res = GetWorstPossibleAttack(u, tilePosition,hypotheticalUnit,true,dimension); 
			protectionPentalty -= res.GetTotalValue();
			enemyAttackScores.Add(res);
		});
		
		protectionPentalty *= 4;//cover is VERY important
		score += protectionPentalty;
		details.protectionPentalty = protectionPentalty;
		details.EnemyAttackScores = enemyAttackScores;


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
		
		AbilityUse bestAttack = new AbilityUse();
		int damagePotential = 0;
		foreach (var enemy in otherTeamUnits)
		{
			var res =GetWorstPossibleAttack(hypotheticalUnit,enemy.WorldObject.TileLocation.Position,enemy,false,dimension:dimension);//potential improvement - consider nearby tiles as grenades/supression dont need direct LOS
			if (res.GetTotalValue() > bestAttack.GetTotalValue()) 
			{
				bestAttack = res;
			}
		}

		damagePotential = bestAttack.GetTotalValue();
		damagePotential *= 2;//damage is important
		details.damagePotential = damagePotential; 
	
		score += damagePotential;

		
	
		WorldManager.Instance.WipePseudoLayer(dimension);
        
		var myTeamUnits = GameManager.GetTeamUnits(realUnit.IsPlayer1Team);
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


}