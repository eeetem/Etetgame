using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.World.WorldObjects.Units.Actions;

public class Move : Action
{
	public Move() :base(ActionType.Move)
	{
	}
	
	public override Tuple<bool,string> CanPerform(Unit actor, ref Vector2Int position)
	{
		
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, position);
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

		return new Tuple<bool, string>(true, "");
	}

	
#if SERVER
	public override Queue<SequenceAction> ExecuteServerSide(Unit actor,Vector2Int target)
	{
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, target);
		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange()*moveUse)
		{
			moveUse++;
		}

		List<Unit> alreadyShot = new List<Unit>();
		
		List<Tuple<List<Unit>,Vector2Int>> ShootingSpots = new List<Tuple<List<Unit>, Vector2Int>>();
		
		foreach (var tile in result.Path)
		{
			var shooters = WorldManager.Instance.GetTileAtGrid(tile).GetOverWatchShooters(actor,actor.WorldObject.GetMinimumVisibility());
			
			List<Unit> exclude = new List<Unit>();
			
			shooters.ForEach((s) =>
			{
				if (alreadyShot.Contains(s))
				{
					exclude.Add(s);
				}
			});
			
			foreach (var unit in exclude)
			{
				shooters.Remove(unit);
			}
			
			
			if (shooters.Count > 0)
			{
				ShootingSpots.Add(new Tuple<List<Unit>, Vector2Int>(shooters,tile));
				alreadyShot.AddRange(shooters);
			}
		}

		List<List<Vector2Int>> paths = new List<List<Vector2Int>>();
		int i = 0;
		paths.Add(new List<Vector2Int>());
		
		foreach (var tile in result.Path)
		{
			if(ShootingSpots.Find((t) => t.Item2 == tile) != null)
			{
				paths[i].Add(tile);
				paths.Add(new List<Vector2Int>());
				i++;
			}
			paths[i].Add(tile);
		}
		Debug.Assert(paths.Count == ShootingSpots.Count + 1);

		WorldEffect w = new WorldEffect();
		w.Move.Value = -moveUse;
		w.TargetFriend = true;
		w.TargetSelf = true;
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new WorldChange(actor.WorldObject.ID,actor.WorldObject.TileLocation.Position,w));
		for (int j = 0; j < paths.Count; j++)
		{
			Console.WriteLine("moving from: "+paths[j][0]+" to:" + paths[j].Last());
			queue.Enqueue(new ReplaySequence.Move(actor.WorldObject.ID,paths[j]));
			if (j < ShootingSpots.Count)
			{
				Console.WriteLine("shooting at:" + ShootingSpots[j].Item2);
				foreach (var attacker in ShootingSpots[j].Item1)
				{
					queue.Enqueue(new DoAction(attacker.WorldObject.ID, ShootingSpots[j].Item2, -1));
				}
			}
			
		}
		return queue;

	}
#endif


	
#if CLIENT
	
	private static List<Vector2Int>? previewPath = new List<Vector2Int>();

	private Vector2Int lastTarget = new Vector2Int(0,0);

	public override void InitAction()
	{

		lastTarget = new Vector2Int(0, 0);
		base.InitAction();
	}

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if(WorldManager.Instance.SequenceRunning) return;
		if (lastTarget == new Vector2Int(0, 0))
		{
			previewPath = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, target).Path;
			if (previewPath == null)
			{
				previewPath = new List<Vector2Int>();
			}

			lastTarget = target;
		}

		if (lastTarget != target)
		{
			SetActiveAction(null);
		}


		for (int index = 0; index < previewPath.Count-1; index++)
		{
			var path = previewPath[index];
			if (path.X < 0 || path.Y < 0) break;
			var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));
			var nextpos = Utility.GridToWorldPos((Vector2)  previewPath[index+1] + new Vector2(0.5f, 0.5f));

			Color c = Color.Green;
			float mul = (float)WorldManager.Instance.GetTileAtGrid(previewPath[index+1]).TraverseCostFrom(path);
			if (mul > 1.5f)
			{
				c = Color.Yellow;
				
			}

			spriteBatch.DrawLine(pos, nextpos, c, 8f);
		}

		try
		{
			PathFinding.PathFindResult result = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, target);
			int moveUse = 1;
			while (result.Cost > actor.GetMoveRange() * moveUse)
			{
				moveUse++;
			}

			for (int i = 0; i < moveUse; i++)
			{
				spriteBatch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"), Utility.GridToWorldPos(target) + new Vector2(-20 * moveUse, -30) + new Vector2(45, 0) * i, null, Color.White, 0f, Vector2.Zero, 4.5f, SpriteEffects.None, 0f);

			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}

#endif
}