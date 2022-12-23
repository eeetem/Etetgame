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

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{

		if (actor.TurnPoints <= 0)
		{
			return false;
		}
		if (actor.MovePoints <= 0)
		{
			return false;
		}
		if (actor.ActionPoints <= 0)
		{
			return false;
		}
	

		return true;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		actor.TurnPoints=0;
		actor.ActionPoints=0;
		actor.MovePoints=0;
		var positions = GetOverWatchPositions(actor, target);
		foreach (var position in positions)
		{
			WorldManager.Instance.GetTileAtGrid(position).Watch(actor);
			actor.overWatchedTiles.Add(position);
		}

		actor.overWatch = true;


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

		return positions;
	}
#if CLIENT
	

	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		

		foreach (var pos in GetOverWatchPositions(actor,target))
		{

			var tile = WorldManager.Instance.GetTileAtGrid(pos);
			if (tile.Surface == null) continue;
							
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position, Color.Orange*0.3f);
		}

	}
#endif
}

