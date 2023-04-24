using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class WorldEffect
{
	public int dmg = 0;
	public int detDmg = 0;
	public int range = 0;
	public string? sfx = "";
	public string? placeItemPrefab = null;
	
	public List<Tuple<string,string,string>> effects = new List<Tuple<string, string, string>>();

	public void Apply(Vector2Int target)
	{

		var hitt = WorldManager.Instance.GetTilesAround(target, range, Cover.Low);

		foreach (var tile in hitt)
		{
			ApplyOnTile(tile);
		}

	}
	protected void ApplyOnTile(WorldTile tile)
	{
		
		tile.EastEdge?.TakeDamage(dmg,0);
		tile.WestEdge?.TakeDamage(dmg,0);
		tile.NorthEdge?.TakeDamage(dmg,0);
		tile.SouthEdge?.TakeDamage(dmg,0);
		tile.ControllableAtLocation?.TakeDamage(dmg,0);
		tile.ControllableAtLocation?.ControllableComponent?.Suppress(detDmg);
		foreach (var item in tile.ObjectsAtLocation)
		{
			item.TakeDamage(dmg);
		}
		if (placeItemPrefab!=null)
		{
#if SERVER
			WorldManager.Instance.MakeWorldObject(placeItemPrefab, tile.Position);
#endif
		}
	}
#if CLIENT
	

	protected void PreviewOnTile(WorldTile tile, SpriteBatch spriteBatch, int previewRecursiveDepth=0)
	{
		if (tile.Surface == null)return;

		Texture2D sprite = tile.Surface.GetTexture();
		
		Color c = Color.DarkRed;
		switch (previewRecursiveDepth)
		{
			case 0:
				c = Color.DarkRed;
				break;
			case 1:
				c = Color.Red;
				break;
			case 2:
				c = Color.Orange;
				break;
			case 3:
				c = Color.Yellow;
				break;
			case 4:
				c = Color.Green;
				break;
			case 5:
				c = Color.Blue;
				break;
			case 6:
				c = Color.Indigo;
				break;
			case 7:
				c = Color.Violet;
				break;
			case 8:
				c = Color.White;
				break;
			default:
				c = Color.White;
				break;
			
		}

		spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, c * 0.45f);

		if (tile.ControllableAtLocation != null )
		{
			tile.ControllableAtLocation.PreviewData.detDmg += detDmg;
			tile.ControllableAtLocation.PreviewData.finalDmg += dmg;
		}

		if (placeItemPrefab != null)
		{
			var prefab = PrefabManager.WorldObjectPrefabs[placeItemPrefab];
			if(prefab.desturctionEffect!=null){
				prefab.desturctionEffect.Preview(tile.Position,spriteBatch,previewRecursiveDepth+1);
			}
		}

		
	}
	

	public void Preview(Vector2Int target, SpriteBatch spriteBatch,int previewRecursiveDepth=0)
	{
		var hitt = WorldManager.Instance.GetTilesAround(target, range, Cover.Low);

		foreach (var tile in hitt)
		{
			PreviewOnTile(tile,spriteBatch,previewRecursiveDepth);
		}
	}

	public void Animate(Vector2Int target)
	{
		if (WorldManager.Instance.GetTileAtGrid(target).Visible==Visibility.None)
		{
			Camera.SetPos(target + new Vector2Int(Random.Shared.Next(-5, 5), Random.Shared.Next(-5, 5)));
		}
		else
		{
			Camera.SetPos(target);
		}

		if (sfx != null)
		{
			Audio.PlaySound(sfx, target);
		}
		
		foreach (var effect in effects)
		{
			PostPorcessing.AddTweenReturnTask(effect.Item1, float.Parse(effect.Item2), float.Parse(effect.Item3), true, 10f);
		}
	}
#endif
}