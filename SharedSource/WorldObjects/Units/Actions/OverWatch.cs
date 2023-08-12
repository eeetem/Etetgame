﻿using System;


using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using FontStashSharp;
#endif
namespace DefconNull.World.WorldObjects.Units.Actions;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}

	
	public override Tuple<bool, string> CanPerform(Unit actor,  Vector2Int position)
	{
	

		if (actor.MovePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points");
		}
		if (actor.ActionPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough fire points");
		}
	

		return new Tuple<bool, string>(true, "");
	}
#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor,Vector2Int target)
	{
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new UnitOverWatch(actor.WorldObject.ID,target));
		return queue;
	}
#endif



#if CLIENT
	

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		//todo mod support, make this not only for shootables
		foreach (var shot in actor.GetOverWatchPositions(target))
		{

			var tile = WorldManager.Instance.GetTileAtGrid(shot.Item1);
			if (tile.Surface == null) continue;
			
			Color c = Color.Yellow * 0.45f;
			
			if (shot.Item2)
			{
				c = Color.Green * 0.45f;
			}
							
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position,c );
			//spriteBatch.DrawText(""+shot.Dmg,   Utility.GridToWorldPos(tile.Position),4,Color.White);
		}

	}

#endif
}

