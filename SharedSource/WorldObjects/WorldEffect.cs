using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace MultiplayerXeno;

public class WorldEffect : IMessageSerializable
{
	public int Dmg;
	public int Det ;
	public ValueChange Move ;
	public ValueChange Act ;
	public int Range = 0;
	public string? Sfx = "";
	public string? PlaceItemPrefab = null;
	public readonly List<Tuple<string, int>> AddStatus = new List<Tuple<string?, int>>();
	public readonly List<string> RemoveStatus = new List<string>();
	public bool noPanic = false;
	
	public List<Tuple<string,string,string>> Effects = new List<Tuple<string, string, string>>();
	public bool Visible;
	public ValueChange MoveRange ;
	public bool Los;
	public int ExRange;
	public bool TargetFoe = false;
	public bool TargetFriend =false;
	public bool TargetSelf =false;
	public List<string> Ignores = new List<string>();
	public VariableValue? GiveItem;
	
	public void Serialize(Message message)
	{
		message.AddInt(Dmg);
		message.AddInt(Det);
		message.AddSerializable(Move);
		message.AddSerializable(Act);
		message.Add(Range);
		message.AddNullableString(Sfx);
		message.AddNullableString(PlaceItemPrefab);

		message.Add(AddStatus.Count);
		foreach (var tuple in AddStatus)
		{
			message.Add(tuple.Item1);
			message.Add(tuple.Item2);
		}

		message.AddStrings(RemoveStatus.ToArray());
		
		message.Add(Effects.Count);
		foreach (var tuple in Effects)
		{
			message.Add(tuple.Item1);
			message.Add(tuple.Item2);
			message.Add(tuple.Item3);
		}

		
		
		message.Add(noPanic);
		message.Add(Visible);
		message.Add(MoveRange);
		message.Add(Los);
		message.Add(ExRange);
		message.Add(TargetFoe);
		message.Add(TargetFriend);
		message.Add(TargetSelf);
		message.AddStrings(Ignores.ToArray());


		message.AddBool(GiveItem != null);
		if(GiveItem != null) message.Add(GiveItem);
	}

	public void Deserialize(Message message)
	{
		Dmg = message.GetInt();
		Det = message.GetInt();
		Move = message.GetSerializable<ValueChange>();
		Act = message.GetSerializable<ValueChange>();
		Range = message.GetInt();
		Sfx = message.GetNullableString();
		PlaceItemPrefab = message.GetNullableString();

		AddStatus.Clear();
		for (int i = 0; i < message.GetInt(); i++)
		{
			AddStatus.Add(new Tuple<string, int>(message.GetString(), message.GetInt()));
		}
		RemoveStatus.Clear();
		RemoveStatus.AddRange(message.GetStrings());
		
		Effects.Clear();
		for (int i = 0; i < message.GetInt(); i++)
		{
			Effects.Add(new Tuple<string, string,string>(message.GetString(), message.GetString(), message.GetString()));
		}
		
		noPanic = message.GetBool();
		Visible = message.GetBool();
		MoveRange = message.GetSerializable<ValueChange>();
		Los = message.GetBool();
		ExRange = message.GetInt();
		TargetFoe = message.GetBool();
		TargetFriend = message.GetBool();
		TargetSelf = message.GetBool();
		Ignores = message.GetStrings().ToList();

		if (message.GetBool())
		{
			GiveItem = message.GetSerializable<VariableValue>();
		}
	}

	private List<WorldTile> GetAffectedTiles(Vector2Int target,WorldObject? user)
	{
			var hitt = WorldManager.Instance.GetTilesAround(target, Range, Cover.Low);
		var excl = WorldManager.Instance.GetTilesAround(target, ExRange, Cover.Low);
		var list = hitt.Except(excl).ToList();
		if (Los)
		{
			if (user != null && user.UnitComponent!=null)
			{
				list.RemoveAll(x => Visibility.None == WorldManager.Instance.CanSee(user.UnitComponent, x.Position, true));
			}
			else
			{
				list.RemoveAll(x => Visibility.None == WorldManager.Instance.CanSee(target, x.Position,99,false));
			}
			
		}

		return list;

	}

	List<WorldObject> _ignoreList = new List<WorldObject>();
	public void Apply(Vector2Int target, WorldObject? user = null)
	{
		_ignoreList = new List<WorldObject>();
		foreach (var tile in GetAffectedTiles(target,user))
		{
			ApplyOnTile(tile,user);
		}

	}
	protected void ApplyOnTile(WorldTile tile,WorldObject? user = null)
	{

		if (tile.EastEdge != null && !_ignoreList.Contains(tile.EastEdge))
		{
			tile.EastEdge?.TakeDamage(Dmg, 0);
			_ignoreList.Add(tile.EastEdge!);
		}
		if (tile.WestEdge != null && !_ignoreList.Contains(tile.WestEdge))
		{
			tile.WestEdge?.TakeDamage(Dmg, 0);
			_ignoreList.Add(tile.WestEdge!);
		}
		if (tile.NorthEdge != null && !_ignoreList.Contains(tile.NorthEdge))
		{
			tile.NorthEdge?.TakeDamage(Dmg, 0);
			_ignoreList.Add(tile.NorthEdge!);
		}
		if (tile.SouthEdge != null && !_ignoreList.Contains(tile.SouthEdge))
		{
			tile.SouthEdge?.TakeDamage(Dmg, 0);
			_ignoreList.Add(tile.SouthEdge!);
		}


		foreach (var item in tile.ObjectsAtLocation)
		{
			item.TakeDamage(Dmg,0);
		}
		if (PlaceItemPrefab!=null)
		{
#if SERVER
			WorldManager.Instance.MakeWorldObject(PlaceItemPrefab, tile.Position, user?.Facing ?? Direction.North);
#endif
		}
		
		


		if (tile.UnitAtLocation != null && !Ignores.Contains(tile.UnitAtLocation.Type.Name))
		{
			
			Unit ctr = tile.UnitAtLocation;
			if (user != null && user.UnitComponent!=null)
			{
				if(ctr.IsPlayerOneTeam == user.UnitComponent.IsPlayerOneTeam && !TargetFriend) return;
				if(ctr.IsPlayerOneTeam != user.UnitComponent.IsPlayerOneTeam && !TargetFoe) return;
				if(Equals(tile.UnitAtLocation, user) && !TargetSelf) return;
			
			}
			
			tile.UnitAtLocation.TakeDamage(Dmg, 0);
			Act.Apply(ref ctr.ActionPoints);
			Move.Apply(ref ctr.MovePoints);
			ctr.Suppress(Det,noPanic);
			MoveRange.Apply(ref ctr.MoveRangeEffect);
			foreach (var status in RemoveStatus)
			{
				tile.UnitAtLocation?.RemoveStatus(status);
			}
			foreach (var status in AddStatus)
			{
				tile.UnitAtLocation?.ApplyStatus(status.Item1,status.Item2);
			}
			
			if(GiveItem!=null)
			{
				ctr.AddItem(PrefabManager.UseItems[GiveItem.GetValue(user.UnitComponent,ctr)]);
			}
			
		}



	}
#if CLIENT
	

	protected void PreviewOnTile(WorldTile tile, SpriteBatch spriteBatch, WorldObject? user, int previewRecursiveDepth=0)
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

		if (tile.WestEdge != null && tile.WestEdge.PreviewData.finalDmg < Dmg)
		{
			tile.WestEdge.PreviewData.finalDmg = Dmg;
		}
		if (tile.NorthEdge != null&& tile.NorthEdge.PreviewData.finalDmg < Dmg)
		{
			tile.NorthEdge.PreviewData.finalDmg = Dmg;
		}
		if (tile.EastEdge != null&& tile.EastEdge.PreviewData.finalDmg < Dmg)
		{
			tile.EastEdge.PreviewData.finalDmg = Dmg;
		}
		if (tile.SouthEdge != null&& tile.SouthEdge.PreviewData.finalDmg < Dmg)
		{
			tile.SouthEdge.PreviewData.finalDmg = Dmg;
		}
		
		if (tile.UnitAtLocation != null )
		{
			WorldObject Wo = tile.UnitAtLocation.WorldObject;
			Wo.PreviewData.detDmg += Det;
			Wo.PreviewData.finalDmg += Dmg;
			Wo.PreviewData.totalDmg += Dmg;
		}

		if (PlaceItemPrefab != null)
		{
			var prefab = PrefabManager.WorldObjectPrefabs[PlaceItemPrefab];
			if(prefab.DesturctionEffect!=null){
				prefab.DesturctionEffect.Preview(tile.Position,spriteBatch,user,previewRecursiveDepth+1);
			}
		}

		
	}
	

	public void Preview(Vector2Int target, SpriteBatch spriteBatch, WorldObject? user,int previewRecursiveDepth=0)
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
			PostPorcessing.AddTweenReturnTask(effect.Item1, float.Parse(effect.Item2, CultureInfo.InvariantCulture), float.Parse(effect.Item3), true, 10f);
		}
	}
#endif

}