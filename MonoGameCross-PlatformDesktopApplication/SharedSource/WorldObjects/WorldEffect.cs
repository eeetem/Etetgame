using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class WorldEffect
{
	public int Dmg;
	public int Det ;
	public ValueChange Move ;
	public ValueChange Act ;
	public int Range = 0;
	public string? Sfx = "";
	public string? PlaceItemPrefab = null;
	public readonly List<Tuple<string,int>> AddStatus = new List<Tuple<string, int>>();
	public readonly List<string> RemoveStatus = new List<string>();

	
	public List<Tuple<string,string,string>> Effects = new List<Tuple<string, string, string>>();
	public bool Visible;
	public ValueChange MoveRange ;
	public bool Los;
	public int ExRange;
	public bool TargetFoe = false;
	public bool TargetFriend =false;
	public bool TargetSelf =false;


	private List<WorldTile> GetAffectedTiles(Vector2Int target,Controllable? user)
	{
			var hitt = WorldManager.Instance.GetTilesAround(target, Range, Cover.Low);
		var excl = WorldManager.Instance.GetTilesAround(target, ExRange, Cover.Low);
		var list = hitt.Except(excl).ToList();
		if (Los)
		{
			if (user != null)
			{
				list.RemoveAll(x => Visibility.None == WorldManager.Instance.CanSee(user, x.Position, true));
			}
			else
			{
				list.RemoveAll(x => Visibility.None == WorldManager.Instance.CanSee(target, x.Position,99,false));
			}
			
		}

		return list;

	}

	public void Apply(Vector2Int target, Controllable? user = null)
	{
		foreach (var tile in GetAffectedTiles(target,user))
		{
			ApplyOnTile(tile,user);
		}

	}
	protected void ApplyOnTile(WorldTile tile,Controllable? user = null)
	{
		
		tile.EastEdge?.TakeDamage(Dmg,0);
		tile.WestEdge?.TakeDamage(Dmg,0);
		tile.NorthEdge?.TakeDamage(Dmg,0);
		tile.SouthEdge?.TakeDamage(Dmg,0);
		foreach (var item in tile.ObjectsAtLocation)
		{
			item.TakeDamage(Dmg,0);
		}
		if (PlaceItemPrefab!=null)
		{
#if SERVER
			WorldManager.Instance.MakeWorldObject(PlaceItemPrefab, tile.Position);
#endif
		}
		
		


		if (tile.ControllableAtLocation != null)
		{
			Controllable ctr = tile.ControllableAtLocation.ControllableComponent;
			if (user != null)
			{
				if(ctr.IsPlayerOneTeam == user.IsPlayerOneTeam && !TargetFriend) return;
				if(ctr.IsPlayerOneTeam != user.IsPlayerOneTeam && !TargetFoe) return;
				if(Equals(tile.ControllableAtLocation.ControllableComponent, user) && !TargetSelf) return;
			
			}
			
			tile.ControllableAtLocation.TakeDamage(Dmg, 0);
			Act.Apply(ref ctr.ActionPoints);
			Move.Apply(ref ctr.MovePoints);
			ctr.Suppress(Det,true);
			MoveRange.Apply(ref ctr.MoveRangeEffect);
			foreach (var status in RemoveStatus)
			{
				tile.ControllableAtLocation.ControllableComponent?.RemoveStatus(status);
			}
			foreach (var status in AddStatus)
			{
				tile.ControllableAtLocation.ControllableComponent?.ApplyStatus(status.Item1,status.Item2);
			}
			
		}



	}
#if CLIENT
	

	protected void PreviewOnTile(WorldTile tile, SpriteBatch spriteBatch, Controllable? user, int previewRecursiveDepth=0)
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
			WorldObject Wo = tile.ControllableAtLocation;
			Wo.PreviewData.detDmg += Det;
			Wo.PreviewData.finalDmg += Dmg;
			tile.ControllableAtLocation.PreviewData.finalDmg += Dmg;
		}

		if (PlaceItemPrefab != null)
		{
			var prefab = PrefabManager.WorldObjectPrefabs[PlaceItemPrefab];
			if(prefab.desturctionEffect!=null){
				prefab.desturctionEffect.Preview(tile.Position,spriteBatch,user,previewRecursiveDepth+1);
			}
		}

		
	}
	

	public void Preview(Vector2Int target, SpriteBatch spriteBatch, Controllable? user,int previewRecursiveDepth=0)
	{
		foreach (var tile in GetAffectedTiles(target,user))
		{
			PreviewOnTile(tile,spriteBatch,user,previewRecursiveDepth);
		}
	}

	public void Animate(Vector2Int target)
	{
		
		if (WorldManager.Instance.GetTileAtGrid(target).Visible==Visibility.None)
		{
			if (Visible)
			{
				Camera.SetPos(target + new Vector2Int(Random.Shared.Next(-3, 3), Random.Shared.Next(-3, 3)));
			}
		}
		else
		{
			Camera.SetPos(target);
		}


		if (Sfx != null)
		{
			Audio.PlaySound(Sfx, target);
		}
		
		foreach (var effect in Effects)
		{
			PostPorcessing.AddTweenReturnTask(effect.Item1, float.Parse(effect.Item2), float.Parse(effect.Item3), true, 10f);
		}
	}
#endif
}