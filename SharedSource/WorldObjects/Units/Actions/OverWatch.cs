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
		throw new NotImplementedException();
		actor.ActionPoints.Current=0;
		actor.MovePoints.Current=0;
		var positions = GetOverWatchPositions(actor, target);
		foreach (var shot in positions)
		{
			WorldManager.Instance.GetTileAtGrid(shot.Result.EndPoint).Watch(actor);
			actor.overWatchedTiles.Add(shot.Result.EndPoint);
		}

		actor.overWatch = true;
		actor.WorldObject.Face(Utility.GetDirection(actor.WorldObject.TileLocation.Position, target));

	}
#endif


	private HashSet<Projectile> GetOverWatchPositions(Unit actor,Vector2Int target)
	{
		var tiles = WorldManager.Instance.GetTilesAround(target,actor.Type.OverWatchSize);
		HashSet<Projectile> possibleShots = new HashSet<Projectile>();
		HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
		foreach (var endTile in tiles)
		{
			
			RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(actor.WorldObject.TileLocation.Position,endTile.Position,Cover.Full);
			foreach (var pos in outcome.Path)
			{
				positions.Add(pos);
			}

		}

		foreach (var position in positions)
		{
			if (actor.CanHit(position))
			{
				foreach (var method in actor.Type.DefaultAttack.WorldAction.DeliveryMethods)
				{
					if (method is Shootable)
					{
						var proj = ((Shootable)method).MakeProjectile(actor, position);
						possibleShots.Add(proj);
						break;
					}

				}

				
			}
		}

		return possibleShots;
	}
#if CLIENT
	

	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		//todo mod support, make this not only for shootables
		foreach (var shot in GetOverWatchPositions(actor,target))
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
	public override void Animate(Unit actor, Vector2Int target)
	{
		base.Animate(actor,target);
		return;
	}
#endif
}

