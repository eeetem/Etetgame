using System;
using System.Collections.Generic;
using MultiplayerXeno;
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
	
	public override Tuple<bool,string> CanPerform(Controllable actor, ref Vector2Int position)
	{
		
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.worldObject.TileLocation.Position, position);
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

		if (Controllable.moving)
		{
			return new Tuple<bool, string>(false, "Can't move multiple units at once");
		}
		
		return new Tuple<bool, string>(true, "");
	}

	public override void Execute(Controllable actor,Vector2Int target)
	{
		PathFinding.PathFindResult result = PathFinding.GetPath(actor.worldObject.TileLocation.Position, target);
		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange()*moveUse)
		{
			moveUse++;
		}

		actor.MovePoints -= moveUse;
		actor.canTurn = true;
		actor.MoveAnimation(result.Path);

	}

	

#if CLIENT
	
	private static List<Vector2Int>? previewPath = new List<Vector2Int>();

	private Vector2Int lastTarget = new Vector2Int(0,0);

	public override void InitAction()
	{

		lastTarget = new Vector2Int(0, 0);
		base.InitAction();
	}

	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
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
			SetActiveAction(null);
		}



		foreach (var path in previewPath)
		{

			if (path.X < 0 || path.Y < 0) break;
			var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

			spriteBatch.DrawCircle(pos, 20, 10, Color.Green, 20f);


		}

		PathFinding.PathFindResult result = PathFinding.GetPath(actor.worldObject.TileLocation.Position, target);
		int moveUse = 1;
		while (result.Cost > actor.GetMoveRange() * moveUse)
		{
			moveUse++;
		}
		
		for (int i = 0; i < moveUse; i++)
		{
			spriteBatch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn"),Utility.GridToWorldPos((Vector2)target)+new Vector2(-20*moveUse,-30)+new Vector2(45,0)*i,null,Color.White,0f,Vector2.Zero, 2.5f,SpriteEffects.None,0f);
		
		}
	}

	public override void Animate(Controllable actor, Vector2Int target)
	{
		return;
	}
#endif
}