using System;
using System.Collections.Generic;
using MultiplayerXeno;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Face : Action
{
	public Face() :base(ActionType.Face)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Unit actor, ref Vector2Int position)
	{
		
		
		var targetDir =  Utility.GetDirection(actor.WorldObject.TileLocation.Position, position);
		if (targetDir == actor.WorldObject.Facing)
		{
			return new Tuple<bool, string>(false, "Already facing that direction");
		}//dont let the action happen if the player is already facing that direction 

		if (Unit.moving)
		{
			return new Tuple<bool, string>(false, "Can't face while moving");
		}

		if (!actor.canTurn)
		{
			return new Tuple<bool, string>(false, "Can't turn");
		}
	

		return new Tuple<bool, string>(true, "");
	}

	public override void Execute(Unit actor,Vector2Int target)
	{
		var targetDir = Utility.GetDirection(actor.WorldObject.TileLocation.Position, target);
		actor.canTurn = false;
		actor.WorldObject.Face(targetDir);
	}
#if CLIENT

	private Vector2Int lastTarget;
	private Dictionary<Vector2Int,Visibility> previewTiles = new Dictionary<Vector2Int, Visibility>();
	public override void InitAction()
	{
		lastTarget = new Vector2Int(0, 0);
		base.InitAction();
	}
	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (lastTarget == new Vector2Int(0, 0))
		{
			var targetDir =  Utility.GetDirection(actor.WorldObject.TileLocation.Position, target);
			previewTiles = WorldManager.Instance.GetVisibleTiles(actor.WorldObject.TileLocation.Position, targetDir, actor.GetSightRange(),actor.Crouching);
			lastTarget = target;
		}
		if (lastTarget != target)
		{
			SetActiveAction(null);
			
		}
		
		foreach (var visTuple in previewTiles)
		{
			WorldTile tile = WorldManager.Instance.GetTileAtGrid(visTuple.Key);
			if (tile.Surface == null) continue;

			Texture2D sprite = tile.Surface.GetTexture();
			Color c = Color.Pink;
			if (visTuple.Value == Visibility.Full)
			{
				c = Color.Brown * 0.45f;
			}else if (visTuple.Value == Visibility.Partial)
			{
				c = Color.RosyBrown * 0.45f;
			}

			spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, c);
			
		}
	}
	public override void Animate(Unit actor, Vector2Int target)
	{
	//	base.Animate(actor,target);
		return;
	}
#endif




}

