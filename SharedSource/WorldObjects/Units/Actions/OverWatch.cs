using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldActions.UnitAbility;
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
		queue.Enqueue(new UnitOverWatch(actor.WorldObject.ID,target));
		return queue;
	}
#endif



#if CLIENT


	public override Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, List<string> args)
	{
		var abilityIndex= int.Parse(args[0]);
		IUnitAbility action = actor.Abilities[(abilityIndex)];
		return action.CanPerform(actor, target,false,false);
	}

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch,List<string> args)
	{
		var abilityIndex= int.Parse(args[0]);
		IUnitAbility action = actor.Abilities[(abilityIndex)];
		
		var tiles = WorldManager.Instance.GetTilesAround(target,action.over);
		HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
		/*	foreach (var endTile in tiles)
			{
				WorldManager.RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(WorldObject.TileLocation.Position,endTile.Position,Cover.Full);
				foreach (var pos in outcome.Path)
				{
					positions.Add(pos);
				}

			}*/

		HashSet<Tuple<Vector2Int,bool>> result = new HashSet<Tuple<Vector2Int, bool>>();
		foreach (var position in positions)
		{
			if (Abilities[0].CanPerform(this,position,false,false).Item1)
			{
				result.Add(new Tuple<Vector2Int, bool>(position,true));
			}
		
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

			if (actor.Abilities[0].Effects[0].GetType() == typeof(Shootable))
			{
				var shoot = (Shootable) actor.Abilities[0].Effects[0];
				var projectile = shoot.GenerateProjectile(actor,shot.Item1,-1);
				spriteBatch.DrawText(""+projectile.Dmg,   Utility.GridToWorldPos(tile.Position),4,Color.White);
			}
            
		}

	}

#endif
}

