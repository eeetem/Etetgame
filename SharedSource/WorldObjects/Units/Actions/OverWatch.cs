using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects.Units.ReplaySequence;

using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using DefconNull.World.WorldActions;
using FontStashSharp;
#endif
namespace DefconNull.World.WorldObjects.Units.Actions;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}


#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor,Vector2Int target, List<string>? args)
	{
		var queue = new Queue<SequenceAction>();
	//	var m = new MoveCamera(actor.WorldObject.TileLocation.Position, false, 1);
	//		queue.Enqueue(m);
		queue.Enqueue(new UnitOverWatch(actor.WorldObject.ID,target,int.Parse(args![0])));
		return queue;
	}
#endif




	public override Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, List<string> args)
	{
		var abilityIndex= int.Parse(args[0]);
		UnitAbility action = actor.Abilities[(abilityIndex)];
		return action.CanPerform(actor, target,false,false);
	}

#if CLIENT

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch, List<string> args)
	{
		

		foreach (var loc in actor.GetOverWatchPositions(target,int.Parse(args[0])))
		{
			var tile = WorldManager.Instance.GetTileAtGrid(loc);
			if (tile.Surface == null) continue;
			Color c = Color.Green * 0.45f;
			
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position, c);
			
			
		}
	}

#endif
}