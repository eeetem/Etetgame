using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using FontStashSharp;
#endif
namespace MultiplayerXeno;

public class OverWatch : Action
{
	public OverWatch() :base(ActionType.OverWatch)
	{
	}

	
	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int position)
	{
	

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

	public override void Execute(Controllable actor,Vector2Int target)
	{
		actor.FirePoints=0;
		actor.MovePoints=0;
		var positions = GetOverWatchPositions(actor, target);
		foreach (var shot in positions)
		{
			WorldManager.Instance.GetTileAtGrid(shot.result.EndPoint).Watch(actor);
			actor.overWatchedTiles.Add(shot.result.EndPoint);
		}

		actor.overWatch = true;
		actor.worldObject.Face(Utility.GetDirection(actor.worldObject.TileLocation.Position, target));

	}

	private HashSet<Projectile> GetOverWatchPositions(Controllable actor,Vector2Int target)
	{
		var tiles = WorldManager.Instance.GetTilesAround(target,actor.Type.OverWatchSize);
		HashSet<Projectile> possibleShots = new HashSet<Projectile>();
		HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
		foreach (var endTile in tiles)
		{
			
			RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(actor.worldObject.TileLocation.Position,endTile.Position,Cover.Full,true);
			foreach (var pos in outcome.Path)
			{
				positions.Add(pos);
			}

		}

		foreach (var position in positions)
		{
			if (actor.CanHit(position))
			{

				var proj = ((Fire) Actions[ActionType.Attack]).MakeProjectile(actor, position);
				possibleShots.Add(proj);
			}
		}

		return possibleShots;
	}
#if CLIENT
	

	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		

		foreach (var shot in GetOverWatchPositions(actor,target))
		{

			var tile = WorldManager.Instance.GetTileAtGrid(shot.result.EndPoint);
			if (tile.Surface == null) continue;
			
			Color c = Color.Yellow * 0.45f;
			
			if (actor.CanHit(shot.result.EndPoint,true))
			{
				c = Color.Green * 0.45f;
			}
							
			Texture2D texture = tile.Surface.GetTexture();

			spriteBatch.Draw(texture, tile.Surface.GetDrawTransform().Position,c );
			spriteBatch.DrawString(Game1.SpriteFont,""+shot.dmg,   Utility.GridToWorldPos(tile.Position),Color.White, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
		}

	}
	public override void Animate(Controllable actor, Vector2Int target)
	{
		return;
	}
#endif
}

