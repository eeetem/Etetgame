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
	public Move(Unit u) : base(AIActionType.Move,u)
	{
		
	}
	
	
	public override void Execute()
	{


		var locs = GetTileScores();

		int bestOf = Math.Min(locs.Count, 3);

		var oredredResults = locs.OrderByDescending(x => x.Item3);
	
		var best=	oredredResults.Take(bestOf)
			.ToArray();
			
		//pick random location out of top [bestOf]
		int r = Random.Shared.Next(bestOf);
		var bestMove = best[r].Item4;
		Vector2Int target = best[r].Item1;
		Log.Message("AI","moving to tile with score: "+best[r].Item3 + "at postion: "+target);
		Log.Message("AI","moving to: "+target+" with: "+  bestMove.ToString()+"\n");
		if(best[r].Item1 == Unit.WorldObject.TileLocation.Position)
		{
			Log.Message("AI","already at best tile");
			return;
		}
		bool needToDoCrouchAction = best[r].Item2 != Unit.Crouching;

		if (Unit.Crouching && needToDoCrouchAction)
		{
			Unit.DoAction(Action.ActionType.Crouch, new Action.ActionExecutionParamters());
			needToDoCrouchAction = false;
		}

		if (target != Unit.WorldObject.TileLocation.Position)
		{
			Log.Message("AI","ordering move action from: "+Unit.WorldObject.TileLocation.Position+" to: "+target+" with score: "+best[r].Item3);
			Unit.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(target));
		}
		do
		{
			Thread.Sleep(500);
		} while (SequenceManager.SequenceRunning);
		if (!Unit.Crouching && needToDoCrouchAction)
		{
			Unit.DoAction(Action.ActionType.Crouch, new Action.ActionExecutionParamters());
			needToDoCrouchAction = false;
		}

	
		
		var otherTeamUnits = GameManager.GetTeamUnits(!Unit.IsPlayer1Team);
	
		
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
		Unit.DoAction(Action.ActionType.Face, new Action.ActionExecutionParamters(vec));
	}

	ConcurrentBag<Tuple<Vector2Int, bool, int, MoveCalcualtion>> calculatedMoves = new ConcurrentBag<Tuple<Vector2Int, bool, int, MoveCalcualtion>>();
	private ConcurrentBag<Tuple<Vector2Int, bool, int, MoveCalcualtion>> GetTileScores()
	{
		if(calculatedMoves.Count == 0)
			calculatedMoves = GetMovementLocations(Unit,Unit.MovePoints.Current);
		return calculatedMoves;
	}
	public override int GetScore()
	{
		if(base.GetScore() <= 0) return -100;
		if(Unit.MovePoints.Current <= 0 || Unit.Panicked)
		{
			return 0;
		}

		(float, bool) result;
		result	= GetWorseThanAverage();
		if(result.Item1 <= 0)
		{
			if(result.Item2)
			{
				return 1;//if there's a better tile out there we should have really low priority to move there if there's nothing else to do
			}
	
			return (int)result.Item1;
			
			
		}
		
		float actionScore = 2+result.Item1;//diffference bwteen current tile and average of all other tiles
		if (Unit.MovePoints >= Unit.MovePoints.Max)//we should probably move first then do something
		{
			actionScore *= 1.5f;
		}

		return (int)actionScore;
	}

	public (float,bool) GetWorseThanAverage()
	{
		var locs = GetTileScores();
		bool betterTileExists = false;
		MoveCalcualtion details;
		int scoreForCurrentTile = GetTileMovementScore((Unit.WorldObject.TileLocation.Position,new PathFinding.PathFindResult()), 0,Unit.Crouching,Unit, out details);

		int countedLocs = 0;
		List<int> scores = new List<int>();
		foreach (var loc in locs)
		{
			if (loc.Item3 != -1000)
			{
				countedLocs++;
				scores.Add(loc.Item3);
				if (loc.Item3 > scoreForCurrentTile+1)
				{
					betterTileExists = true;
				}
			}
		}

		if (countedLocs == 0)
		{
			return (0,false);
		}


		//int averageScore = totalScore / countedLocs;
		float percentile = Utility.CalculatePercentile(scores, 65);
		float worseThanAverage = percentile - scoreForCurrentTile;

		return (worseThanAverage,betterTileExists);
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
			Parallel.ForEach(allLocations[i], loc =>
			{
				int scr = GetTileMovementScore(loc, moveUse, unit.Crouching, unit, out var mv);

				scoredLocations.Add(
					new Tuple<Vector2Int, bool, int, MoveCalcualtion>(loc.Item1, unit.Crouching, scr, mv));
			});
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