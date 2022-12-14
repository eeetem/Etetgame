using System;
using System.Linq;
using CommonData;
using MultiplayerXeno.Pathfinding;

namespace MultiplayerXeno;

public class Move : Action
{
	public Move() :base(ActionType.Move)
	{
	}
	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{
		
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.worldObject.TileLocation.Position, position);
		if (result.Cost == 0)
		{
			Console.WriteLine("move action rejected: cost is 0");

			return false;//no path
		}

		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange()*moveUse)
		{
			moveUse++;
		}
		if (moveUse > actor.MovePoints)
		{

			Console.WriteLine("client attempted to move past move points at: "+actor.worldObject.TileLocation.Position +" to "+result.Path.Last());
				
			return false;
		}

		if (Controllable.moving)
		{
			return false;
		}


		return true;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.worldObject.TileLocation.Position, target);
		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange()*moveUse)
		{
			moveUse++;
		}

		actor.MovePoints -= moveUse;
		actor.MoveAnimation(result.Path);

	}
}