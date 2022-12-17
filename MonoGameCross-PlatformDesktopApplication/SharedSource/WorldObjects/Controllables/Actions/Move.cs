using System;
using System.Collections.Generic;
using System.Linq;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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

	

#if CLIENT
	
	private static List<Vector2Int> previewPath = new List<Vector2Int>();

	private Vector2Int lastTarget = new Vector2Int(0,0);

	public override void InitAction()
	{

		lastTarget = new Vector2Int(0, 0);
		base.InitAction();
	}

	public override void Preview(Controllable actor, Vector2Int target,SpriteBatch spriteBatch)
	{
		if (lastTarget == new Vector2Int(0, 0))
		{
			previewPath = PathFinding.GetPath(actor.worldObject.TileLocation.Position, target).Path;
			if (previewPath == null)
			{
				previewPath = new List<Vector2Int>();
			}
			lastTarget = target;
		}

		if (lastTarget != target)
		{
			Action.SetActiveAction(null);
			
		}
	
		foreach (var path in previewPath)
		{
					
			if(path.X < 0 || path.Y < 0) break;
			var pos = Utility.GridToWorldPos((Vector2)path + new Vector2(0.5f,0.5f));
				
			spriteBatch.DrawCircle(pos,20,10,Color.Green,20f);
				
				
		}
	}
#endif
}