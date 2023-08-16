
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
#if CLIENT
using DefconNull.Rendering.PostProcessing;
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.World.WorldActions;

public class WorldConseqences : IMessageSerializable
{
	public int Dmg;
	public int Det ;
	public ValueChange Move ;
	public ValueChange Act ;
	public int Range = 1;
	public string? Sfx = "";
	public string? PlaceItemPrefab = null;
	public readonly List<Tuple<string, int>> AddStatus = new List<Tuple<string?, int>>();
	public readonly List<string> RemoveStatus = new List<string>();

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

	public List<WorldTile> GetAffectedTiles(Vector2Int target,WorldObject? user)
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
	public List<SequenceAction> GetApplyConsiqunces(Vector2Int target, WorldObject? user = null)
	{
		_ignoreList = new List<WorldObject>();
		
		var list = new List<SequenceAction>();
		foreach (var tile in GetAffectedTiles(target,user))
		{
			foreach (var sqc in ConsequencesOnTile(tile,user))
			{
				list.Add(sqc);
			}

		}

		if (Sfx != "" && Sfx != null)
		{
			list.Add(new PlaySound(Sfx, target));
			//Audio.PlaySound(Sfx, target);
		}
		foreach (var effect in Effects)
		{
			list.Add(new PostProcessingEffect(effect.Item1, float.Parse(effect.Item2, CultureInfo.InvariantCulture), float.Parse(effect.Item3), true, 10f));
	//		PostPorcessing.AddTweenReturnTask(effect.Item1, float.Parse(effect.Item2, CultureInfo.InvariantCulture), float.Parse(effect.Item3), true, 10f);
		}

		return list;
	}
	protected List<SequenceAction> ConsequencesOnTile(WorldTile tile,WorldObject? user = null)
	{
		var consequences = new List<SequenceAction>();
		if (tile.EastEdge != null && !_ignoreList.Contains(tile.EastEdge))
		{
			//tile.EastEdge?.TakeDamage(Dmg, 0);
			consequences.Add(new TakeDamage(Dmg,0, tile.EastEdge!.ID));
			_ignoreList.Add(tile.EastEdge!);
		}
		if (tile.WestEdge != null && !_ignoreList.Contains(tile.WestEdge))
		{
			//tile.WestEdge?.TakeDamage(Dmg, 0);
			consequences.Add(new TakeDamage(Dmg,0, tile.WestEdge!.ID));
			_ignoreList.Add(tile.WestEdge!);
		}
		if (tile.NorthEdge != null && !_ignoreList.Contains(tile.NorthEdge))
		{
			//tile.NorthEdge?.TakeDamage(Dmg, 0);
			consequences.Add(new TakeDamage(Dmg,0, tile.NorthEdge!.ID));
			_ignoreList.Add(tile.NorthEdge!);
		}
		if (tile.SouthEdge != null && !_ignoreList.Contains(tile.SouthEdge))
		{
			//tile.SouthEdge?.TakeDamage(Dmg, 0);
			consequences.Add(new TakeDamage(Dmg,0, tile.SouthEdge!.ID));
			_ignoreList.Add(tile.SouthEdge!);
		}


		foreach (var item in tile.ObjectsAtLocation)
		{
			item.TakeDamage(Dmg,0);
			consequences.Add(new TakeDamage(Dmg,0, item!.ID));
		}
		
		if (PlaceItemPrefab!=null)
		{
			consequences.Add(new MakeWorldObject(PlaceItemPrefab, tile.Position, user?.Facing ?? Direction.North));
		}
		
		if (tile.UnitAtLocation != null && !Ignores.Contains(tile.UnitAtLocation.Type.Name))
		{
			
			Unit ctr = tile.UnitAtLocation;
			if (user != null && user.UnitComponent!=null)
			{

				if (Equals(tile.UnitAtLocation, user.UnitComponent))
				{
					if(!TargetSelf) return consequences;
				}
				else
				{
					if (ctr.IsPlayerOneTeam == user.UnitComponent.IsPlayerOneTeam && !TargetFriend) return consequences;
					if (ctr.IsPlayerOneTeam != user.UnitComponent.IsPlayerOneTeam && !TargetFoe) return consequences;
				}
				
			}

			consequences.Add(new TakeDamage(Dmg,0,ctr.WorldObject.ID));
			consequences.Add(new Suppress(Det,ctr.WorldObject.ID));
			consequences.Add(new ChangeUnitValues(ctr.WorldObject.ID,Act.GetChange(ctr.ActionPoints),Move.GetChange(ctr.MovePoints),0, MoveRange.GetChange(ctr.MoveRangeEffect)));
		//	Act.Apply(ref ctr.ActionPoints);
		//	Move.Apply(ref ctr.MovePoints);
		//	ctr.Suppress(Det);
			//MoveRange.Apply(ref ctr.MoveRangeEffect);
			foreach (var status in RemoveStatus)
			{
				consequences.Add(new UnitStatusEffect(ctr.WorldObject.ID,false,status));
			//	tile.UnitAtLocation?.RemoveStatus(status);
			}
			foreach (var status in AddStatus)
			{
				consequences.Add(new UnitStatusEffect(ctr.WorldObject.ID,true,status.Item1,status.Item2));
				//tile.UnitAtLocation?.ApplyStatus(status.Item1,status.Item2);
			}
			
			if(GiveItem!=null)
			{
				consequences.Add(new GiveItem(ctr.WorldObject.ID,GiveItem.GetValue(user.UnitComponent,ctr)));
			//	ctr.AddItem(PrefabManager.UseItems[GiveItem.GetValue(user.UnitComponent,ctr)]);
			}
			
		}


		return consequences;
	}

}