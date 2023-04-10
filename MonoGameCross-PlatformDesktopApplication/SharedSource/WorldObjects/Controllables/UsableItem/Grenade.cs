using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno.Items;

public class Grenade : UsableItem
{
	
	readonly int throwRange;
	readonly int range;
	readonly int detDmg;
	readonly int dmg;
	readonly int smoke;
	public Grenade(string? name, int throwRange,int range,int detDmg,int dmg,int smoke):base(name)
	{
		this.throwRange = throwRange;
		this.range = range;
		this.detDmg = detDmg;
		this.dmg = dmg;
		this.smoke = smoke;
	}

	
	public override void Execute(Controllable actor, Vector2Int target)
	{

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.worldObject.TileLocation.Position, target, Cover.Full,true);
		var hitt = WorldManager.Instance.GetTilesAround(outcome.CollisionPointShort, 3, Cover.Low);

		foreach (var tile in hitt)
		{
			ApplyOnTile(tile);
		}

		actor.RemoveItem(this);
	}


	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		if (Vector2.Distance(actor.worldObject.TileLocation.Position, target) >= throwRange)
		{
			return new Tuple<bool, string>(false, "Too Far");
		}
		
		return new Tuple<bool, string>(true, "");
	}

	protected void ApplyOnTile(WorldTile tile)
	{
		tile.ObjectAtLocation?.TakeDamage(dmg,0);
		tile.ObjectAtLocation?.ControllableComponent?.Suppress(detDmg);
		if (smoke>0)
		{
			tile.ApplySmoke(smoke);
		}
	}

#if CLIENT

	protected void PreviewOnTile(WorldTile tile, SpriteBatch spriteBatch)
	{
		if (tile.Surface == null)return;

		Texture2D sprite = tile.Surface.GetTexture();

		spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, Color.DarkRed * 0.45f);

		if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null)
		{
			tile.ObjectAtLocation.ControllableComponent.PreviewData.detDmg += detDmg;
			tile.ObjectAtLocation.ControllableComponent.PreviewData.finalDmg += dmg;
		}
	}

	private Vector2Int lastTarget;
	private List<WorldTile> hitTiles = new List<WorldTile>();
	private Vector2Int hit = new(0,0);
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (actor.worldObject.TileLocation.Position == target)
		{
			return;
		}
		if(lastTarget == null || (target!=lastTarget && Vector2.Distance(actor.worldObject.TileLocation.Position,target)<throwRange))
		{
			var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.worldObject.TileLocation.Position, target, Cover.Full,true);
			hit = outcome.CollisionPointShort;
			hitTiles = WorldManager.Instance.GetTilesAround(hit, range, Cover.Low);
			lastTarget = target;
		}

		spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"),  Utility.GridToWorldPos(hit+new Vector2(-1.5f,-0.5f)), Color.Red);
		spriteBatch.DrawLine(Utility.GridToWorldPos(actor.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)) , Utility.GridToWorldPos(hit+new Vector2(0.5f,0.5f)), Color.Red, 2);
		

		foreach (var tile in hitTiles)
		{
			PreviewOnTile(tile, spriteBatch);
		}
	}
	
#endif
	
}