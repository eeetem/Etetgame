using System.Collections.Concurrent;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

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
				int score = GetTileMovementScore(l, unit);
				scoredLocations.Add(new Tuple<Vector2Int, int>(l, score));
			});
		}

		return scoredLocations;
	}
	
	
	public static int GetTileMovementScore(Vector2Int vec, Unit unit)
	{
		int score = 0;
		bool team = unit.IsPlayerOneTeam;
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
		int distanceReward = 30;

		while (closestDistance > 0)//subtract points for being too far away
		{
			distanceReward--;
			closestDistance--;
		}
		
		score += distanceReward;
			
			
		//add points for being protected
		foreach (var u in otherTeamUnits)
		{
			//u.GetAction(-1)
		}



		return score;
	}


	private static int GetWorstPossibleAttack(Unit u, Vector2Int position)
	{
		foreach (var ability in u.GetFullAbilityList())
		{
			if (ability.CanPerform(u, position).Item1)
			{
				var cons = ability.GetConsequences(u,position);
			}
			
		}

		return -1;
	}
}