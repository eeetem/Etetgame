using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;
using MultiplayerXeno.ReplaySequence;
#if CLIENT
using FontStashSharp;
#endif
namespace MultiplayerXeno;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}

	
	public override Tuple<bool, string> CanPerform(Unit actor,ref  Vector2Int position)
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
	public override Queue<SequenceAction> ExecuteServerSide(Unit actor,Vector2Int target)
	{
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ReplaySequence.OverWatch(actor.WorldObject.ID,target));
		return queue;
	}
#endif



#if CLIENT
	

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		//todo mod support, make this not only for shootables
		foreach (var shot in actor.GetOverWatchPositions(target))
		{

			var tile = WorldManager.Instance.GetTileAtGrid(shot.Result.EndPoint);
			if (tile.Surface == null) continue;
			
			Color c = Color.Yellow * 0.45f;
			
			if (actor.CanHit(shot.Result.EndPoint,true))
			{
				c = Color.Green * 0.45f;
			}
							
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position,c );
			spriteBatch.DrawText(""+shot.Dmg,   Utility.GridToWorldPos(tile.Position),4,Color.White);
		}

	}

#endif
}

