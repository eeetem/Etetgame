using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldObjects.Units.Actions;

public class Face : Action
{
	public Face() :base(ActionType.Face)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Unit actor, ActionExecutionParamters args)
	{
		
		
		var targetDir =  Utility.GetDirection(actor.WorldObject.TileLocation.Position, args.Target!.Value);
		if (targetDir == actor.WorldObject.Facing)
		{
			return new Tuple<bool, string>(false, "Already facing that direction");
		}//dont let the action happen if the player is already facing that direction 


		if (!actor.canTurn)
		{
			return new Tuple<bool, string>(false, "Can't turn");
		}
	

		return new Tuple<bool, string>(true, "");
	}

#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, ActionExecutionParamters args)
	{

		var queue = new Queue<SequenceAction>();
		queue.Enqueue(FaceUnit.Make(actor.WorldObject.ID,args.Target!.Value));
		return queue;

	}
#endif

#if CLIENT

	private Vector2Int lastTarget;
	private IDictionary<Vector2Int,Visibility> previewTiles = new Dictionary<Vector2Int, Visibility>();

	public override List<SequenceAction> Preview(Unit actor, ActionExecutionParamters args,SpriteBatch spriteBatch)
	{
			var targetDir =  Utility.GetDirection(actor.WorldObject.TileLocation.Position, args.Target!.Value);
			previewTiles = WorldManager.Instance.GetVisibleTiles(actor.WorldObject.TileLocation.Position, targetDir, actor.GetSightRange(),actor.Crouching);
		
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
return new List<SequenceAction>();

	}

#endif




}

