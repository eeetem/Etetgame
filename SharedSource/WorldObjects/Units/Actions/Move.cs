using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MultiplayerXeno.Pathfinding;
using MultiplayerXeno.ReplaySequence;

namespace MultiplayerXeno;

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

		WorldEffect w = new WorldEffect();
		w.Move.Value = -moveUse;
		w.TargetFriend = true;
		w.TargetSelf = true;
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ReplaySequence.WorldChange(actor.WorldObject.ID,actor.WorldObject.TileLocation.Position,w));
		queue.Enqueue(new ReplaySequence.Move(actor.WorldObject.ID,result.Path!));
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

		PathFinding.PathFindResult result = PathFinding.GetPath(actor.WorldObject.TileLocation.Position, target);
		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange() * moveUse)
		{
			moveUse++;
		}
		
		for (int i = 0; i < moveUse; i++)
		{
			spriteBatch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"),Utility.GridToWorldPos(target)+new Vector2(-20*moveUse,-30)+new Vector2(45,0)*i,null,Color.White,0f,Vector2.Zero, 4.5f,SpriteEffects.None,0f);
		
		}
	}

#endif
}