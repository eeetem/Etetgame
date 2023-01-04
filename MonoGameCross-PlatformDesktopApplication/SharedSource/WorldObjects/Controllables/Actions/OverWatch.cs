using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace MultiplayerXeno;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}

	
	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int position)
	{
	
		if (actor.TurnPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough turn points");
		}
		if (actor.MovePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points");
		}
		if (actor.FirePoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough fire points");
		}
	

		return new Tuple<bool, string>(true, "");
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.TurnPoints=0;
		actor.FirePoints=0;
		actor.MovePoints=0;
		var positions = GetOverWatchPositions(actor, target);
		foreach (var position in positions)
		{
			WorldManager.Instance.GetTileAtGrid(position).Watch(actor);
			actor.overWatchedTiles.Add(position);
		}

		actor.overWatch = true;
		actor.worldObject.Face(Utility.ToClampedDirection( actor.worldObject.TileLocation.Position-target));

	}

	private HashSet<Vector2Int> GetOverWatchPositions(Controllable actor,Vector2Int target)
	{
		var tiles = WorldManager.Instance.GetTilesAround(target,actor.Type.OverWatchSize);
		HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
		foreach (var endTile in tiles)
		{
			
			RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(actor.worldObject.TileLocation.Position,endTile.Position,Cover.Full,true);
			foreach (var pos in outcome.Path)
			{
				positions.Add(pos);
			}

		}

		foreach (var position in new List<Vector2Int>(positions))
		{
			if (!actor.CanHit(position))
			{
				positions.Remove(position);
			}
		}

		return positions;
	}
#if CLIENT
	

	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		

		foreach (var pos in GetOverWatchPositions(actor,target))
		{

			var tile = WorldManager.Instance.GetTileAtGrid(pos);
			if (tile.Surface == null) continue;
			
			Color c = Color.Yellow * 0.45f;
			
			if (actor.CanHit(pos,true))
			{
				c = Color.Green * 0.45f;
			}
							
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position,c );
		}

	}
#endif
}

