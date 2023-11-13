using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.Actions;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.AI;

public class Overwatch : AIAction
{
	public Overwatch() : base(AIActionType.OverWatch)
	{
		
	}



	public override void Execute(Unit unit)
	{

		var abl = GetRandomOverWatchAbility(unit);
		if (abl is null) throw new Exception("No Overwatch Abilities Found!");
		var highestTile = GetBestOverWatchTile(unit,abl);

		unit.DoAction(Action.ActionType.OverWatch,highestTile.Item1,new List<string> {abl.Index.ToString()});
		
		
	}

	private static (Vector2Int, int) GetBestOverWatchTile(Unit unit, UnitAbility unitAbility)
	{
		ConcurrentDictionary<Vector2Int, int> tileScores = new();
		var units = GameManager.GetTeamUnits(!unit.IsPlayer1Team);
		var myTeam = GameManager.GetTeamUnits(unit.IsPlayer1Team);
		List<Vector2Int> tilesList = new();
		foreach (var u in units)
		{
			foreach (var possibleMoveLocationList in u.GetPossibleMoveLocations(moveOverride: u.MovePoints.Max))
			{
				tilesList.AddRange(possibleMoveLocationList);
			}
		}

		Parallel.ForEach(tilesList, (tile) =>
		{
			if (WorldManager.Instance.VisibilityCast(unit.WorldObject.TileLocation.Position, tile, 99, unit.Crouching) == Visibility.None) return;
			float score = CoverScore(tile, myTeam);
			float distance = Vector2.Distance(tile, unit.WorldObject.TileLocation.Position);
			float range = unitAbility.GetOptimalRangeAI();
			while (distance > range)
			{
				score /= 1.5f;
				distance -= range;
			}
			

			tileScores.AddOrUpdate(tile, (int)score, (key, oldValue) => (int) (oldValue + score));
		});
		ValueTuple<Vector2Int, int> highestTile = new ValueTuple<Vector2Int, int>(new Vector2Int(-1, -1), -100);
		foreach (var s in tileScores)
		{
			if (s.Value > highestTile.Item2)
				highestTile = new ValueTuple<Vector2Int, int>(s.Key, s.Value);
		}

		return highestTile;
	}

	public override int GetScore(Unit unit)
	{
		if(base.GetScore(unit) <= 0) return -100;
		var abl = GetRandomOverWatchAbility(unit);
		if(abl is null) return -100;
		if(CanSeeAnyEnemy(unit)) return -100;//no overwatch when there's someone right in front of us
		return GetBestOverWatchTile(unit,abl).Item2;
	}


	private UnitAbility? GetRandomOverWatchAbility(Unit unit)
	{
		var randomList = new List<UnitAbility>(unit.Abilities);
		randomList.Shuffle(Random.Shared);
		foreach (var ability in randomList)
		{
			if(ability.AIExempt) continue; 
			if(!ability.CanOverWatch) continue;
			if (ability.ImmideateActivation) continue;
			if(!ability.HasEnoughPointsToPerform(unit).Item1) continue;
			return ability;
		}

		return null;
	}

}