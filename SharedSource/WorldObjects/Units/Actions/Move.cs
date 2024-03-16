using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.WorldObjects.Units.Actions;

public class Move : Action
{
	public Move() :base(ActionType.Move)
	{
	}
	
	public override Tuple<bool,string> CanPerform(Unit actor, ActionExecutionParamters args)
	{
		
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, args.Target!.Value);
		if (result.Cost == 0)
		{
			return new Tuple<bool, string>(false, "No path found");
		}

		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange()*moveUse)
		{
			moveUse++;
		}
		if (moveUse > actor.MovePoints)
		{
			return new Tuple<bool, string>(false, "Not enough move points");
		}
		if (actor.Paniced)
		{
			return new Tuple<bool, string>(false, "Cannot Move while Paniced");
		}

		if (WorldManager.Instance.GetTileAtGrid(args.Target!.Value).UnitAtLocation != null)
		{
			return new Tuple<bool, string>(false, "Tile is occupied");
		}

		return new Tuple<bool, string>(true, "");
	}


	public override Queue<SequenceAction>[] GetConsequenes(Unit actor, ActionExecutionParamters args)
	{
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, args.Target!.Value);
		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange()*moveUse)
		{
			moveUse++;
		}

		List<Unit> alreadyShot = new List<Unit>();
		
		List<Tuple<List<Unit>,Vector2Int>> shootingSpots = new List<Tuple<List<Unit>, Vector2Int>>();
		
		foreach (var tile in result.Path)
		{
			var shooters = WorldManager.Instance.GetTileAtGrid(tile).GetOverWatchShooters(actor,actor.WorldObject.GetMinimumVisibility());
			
			shooters.RemoveAll(u => alreadyShot.Contains(u));
			
			if (shooters.Count > 0)
			{
				shootingSpots.Add(new Tuple<List<Unit>, Vector2Int>(shooters,tile));
				alreadyShot.AddRange(shooters);
			}
		}

		List<List<Vector2Int>> paths = new List<List<Vector2Int>>();
		int i = 0;
		paths.Add(new List<Vector2Int>());
		
		//seperate paths between each shot into seperate lists
		foreach (var tile in result.Path)
		{
			if(shootingSpots.Find((t) => t.Item2 == tile) != null)
			{
				paths[i].Add(tile);
				paths.Add(new List<Vector2Int>());
				i++;
			}
			paths[i].Add(tile);
		}
		Debug.Assert(paths.Count == shootingSpots.Count + 1);


		var queue = new Queue<SequenceAction>();
		queue.Enqueue(ChangeUnitValues.Make(actor.WorldObject.ID,0,-moveUse,0,0));
		for (int j = 0; j < paths.Count; j++)
		{
			Log.Message("UNITS","moving from: "+paths[j][0]+" to:" + paths[j].Last());
			queue.Enqueue(UnitMove.Make(actor.WorldObject.ID,paths[j]));
			if (j < shootingSpots.Count)
			{
				Log.Message("UNITS","shooting at:" + shootingSpots[j].Item2 +" with: "+ shootingSpots[j].Item1.Count+" units");
				foreach (var attacker in shootingSpots[j].Item1)
				{
					
	
					var act = DelayedAbilityUse.Make(attacker.WorldObject.ID,attacker.Overwatch.Item2,shootingSpots[j].Item2);
					queue.Enqueue(act);
				}
			}
			
		}
		return new Queue<SequenceAction>[] {queue};

	}


	
#if CLIENT

	private static PathFinding.PathFindResult previewPath = new PathFinding.PathFindResult();

	private Vector2Int lastTarget = new Vector2Int(0,0);

	
	public override void Preview(Unit actor, ActionExecutionParamters args,SpriteBatch spriteBatch)
	{
		if (SequenceManager.SequenceRunning) return;
		previewPath = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, args.Target!.Value);
		

		for (int index = 0; index < previewPath.Path.Count - 1; index++)
		{
			var path = previewPath.Path[index];
			if (path.X < 0 || path.Y < 0) break;
			var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));
			var nextpos = Utility.GridToWorldPos((Vector2) previewPath.Path[index + 1] + new Vector2(0.5f, 0.5f));

			Color c = Color.Green;
			float mul = (float) WorldManager.Instance.GetTileAtGrid(previewPath.Path[index + 1]).TraverseCostFrom(path);
			if (mul > 1.5f)
			{
				c = Color.Yellow;

			}

			spriteBatch.DrawLine(pos, nextpos, c, 8f);
		}


		int moveUse = 1;
		while (previewPath.Cost > actor.GetMoveRange() * moveUse)
		{
			moveUse++;
		}

		for (int i = 0; i < moveUse; i++)
		{
			spriteBatch.Draw(TextureManager.GetTexture("HoverHud/movepoint"), Utility.GridToWorldPos(args.Target.Value) + new Vector2(-20 * moveUse, -30) + new Vector2(50, 0) * i, null, Color.White, 0f, Vector2.Zero, 4.5f, SpriteEffects.None, 0f);
		}
		
	}
#endif
}