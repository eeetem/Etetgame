using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Action = DefconNull.WorldObjects.Units.Actions.Action;
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
		var bestMove = best[r].Item4;
		Vector2Int target = best[r].Item1;
		Console.WriteLine("moving to tile with score: "+best[r].Item3 + "at postion: "+target);
		File.AppendAllText("aidebug.txt","moving to: "+target+" with: "+  bestMove.ToString()+"\n");
		if(best[r].Item1 == unit.WorldObject.TileLocation.Position)
		{
			Console.WriteLine("already at best tile");
			return;
		}
		bool needToDoCrouchAction = best[r].Item2 != unit.Crouching;

		if (unit.Crouching && needToDoCrouchAction)
		{
			unit.DoAction(Action.ActionType.Crouch, new Action.ActionExecutionParamters());
			needToDoCrouchAction = false;
		}

		if (target != unit.WorldObject.TileLocation.Position)
		{
			Console.WriteLine("ordering move action from: "+unit.WorldObject.TileLocation.Position+" to: "+target+" with score: "+best[r].Item3);
			unit.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(target));
		}
		do
		{
			Thread.Sleep(500);
		} while (SequenceManager.SequenceRunning);
		if (!unit.Crouching && needToDoCrouchAction)
		{
			unit.DoAction(Action.ActionType.Crouch, new Action.ActionExecutionParamters());
			needToDoCrouchAction = false;
		}

	
		
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
			Thread.Sleep(250);
		} while (SequenceManager.SequenceRunning);
		unit.DoAction(Action.ActionType.Face, new Action.ActionExecutionParamters(vec));
	}

	public override int GetScore(Unit unit)
	{
		if(base.GetScore(unit) <= 0) return -100;
		if(unit.MovePoints.Current <= 0 || unit.Paniced)
		{
			return 0;
		}

		float worseThanAverage = GetWorseThanAverage(unit);
		if(worseThanAverage <= 0)
		{
			return (int)worseThanAverage;
		}
		
		float actionScore = 2+worseThanAverage;//diffference bwteen current tile and average of all other tiles
		if (unit.MovePoints >= unit.MovePoints.Max)//we should probably move first then do something
		{
			actionScore *= 1.5f;
		}

		return (int)actionScore;
	}

	public static float GetWorseThanAverage(Unit unit)
	{
		int movesToUse = GetMovesToUse(unit);
		var locs = GetMovementLocations(unit, movesToUse);
		int scoreForCurrentTile = GetTileMovementScore((unit.WorldObject.TileLocation.Position,new PathFinding.PathFindResult()), 0,unit.Crouching,unit, out _);

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
		float percentile = Utility.CalculatePercentile(scores, 90);
		float worseThanAverage = percentile - scoreForCurrentTile;

		return worseThanAverage;
	}


	private static ConcurrentBag<Tuple<Vector2Int, bool, int,MoveCalcualtion>> GetMovementLocations(Unit unit, int distance)
	{
		List<(Vector2Int,PathFinding.PathFindResult)>[] allLocations = unit.GetPossibleMoveLocations();
	
		var scoredLocations = new ConcurrentBag<Tuple<Vector2Int,bool, int,MoveCalcualtion>>();

		MoveCalcualtion m;
		int score = GetTileMovementScore((unit.WorldObject.TileLocation.Position,new PathFinding.PathFindResult()),0,unit.Crouching, unit, out m);
		scoredLocations.Add(new Tuple<Vector2Int, bool,int,MoveCalcualtion>(unit.WorldObject.TileLocation.Position, unit.Crouching,score,m));
		
		//move locations where we do not change stance
		for (int i = 0; i < Math.Min(distance,allLocations.Length); i++)
		{
			int moveUse = i+1;
			//Parallel.ForEach(allLocations[i],l =>
			foreach (var loc in allLocations[i])
			{
				int scr = GetTileMovementScore(loc,moveUse,unit.Crouching, unit, out var mv);

				scoredLocations.Add(new Tuple<Vector2Int, bool,int,MoveCalcualtion>(loc.Item1, unit.Crouching,scr,mv));
			}//);
		}

		if (unit.Crouching)
		{
			//stand up then move
			List<(Vector2Int,PathFinding.PathFindResult)>[] standUpLocations = unit.GetPossibleMoveLocations(unit.GetMoveRange()+2);//offset the courch penalty since we're gonna be standing up
			standUpLocations[0].Add((unit.WorldObject.TileLocation.Position, new PathFinding.PathFindResult()));
			for (int i = 0; i < Math.Min(distance-1, standUpLocations.Length); i++)
			{
				int i1 = i;
				Parallel.ForEach(standUpLocations[i], l =>
				{
					MoveCalcualtion m;
					int score = GetTileMovementScore(l,i1+2,false, unit, out m);
					//discourage changing stance by doing -1 if the scores are equal
					scoredLocations.Add(new Tuple<Vector2Int, bool,int,MoveCalcualtion>(l.Item1, false,score-1,m));
				});
			}
		}
		else
		{
			//move then crouch
			List<(Vector2Int,PathFinding.PathFindResult)>[] crouchLocations = unit.GetPossibleMoveLocations();
			crouchLocations[0].Add((unit.WorldObject.TileLocation.Position, new PathFinding.PathFindResult()));
			for (int i = 0; i < Math.Min(distance-1, crouchLocations.Length); i++)
			{
				int i1 = i;
				Parallel.ForEach(crouchLocations[i], l =>
				{
					MoveCalcualtion m;
					int score = GetTileMovementScore(l,i1+2,true, unit, out m);
					//discourage changing stance by doing -1 if the scores are equal
					scoredLocations.Add(new Tuple<Vector2Int, bool,int,MoveCalcualtion>(l.Item1, true,score-1,m));
				});
			}
		}
		

		return scoredLocations;
	}





}