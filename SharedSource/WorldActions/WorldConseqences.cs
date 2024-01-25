using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldObjects;
#if CLIENT
using DefconNull.Rendering.PostProcessing;
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.WorldActions;

public class WorldConseqences 
{
	public int Dmg;
	public int Det ;
	public ValueChange Move ;
	public ValueChange Act ;
	public int Range = 1;
	public string? Sfx = "";
	public string? PLaceItemConsequence = null;
	public readonly List<Tuple<string, int>> AddStatus = new List<Tuple<string?, int>>();
	public readonly List<string> RemoveStatus = new List<string>();

	public List<Tuple<string,string,string>> Effects = new List<Tuple<string, string, string>>();
	public ValueChange MoveRange ;
	public int ExRange;
	public bool TargetFoe = false;
	public bool TargetFriend =false;
	public bool TargetSelf =false;
	public List<string> Ignores = new List<string>();

	public bool FogOfWarSpot { get; set; }
	public int FogOfWarSpotScatter { get; set; }
	public int DetRes { get; set; }
	public int EnvRes { get; set; }


	public List<IWorldTile> GetAffectedTiles(Vector2Int target)
	{
		var hitt = WorldManager.Instance.GetTilesAround(target, Range, -1,Cover.Low);
		var excl = WorldManager.Instance.GetTilesAround(target, ExRange, -1,Cover.Low);
		var list = hitt.Except(excl).ToList();
		return list;
	}

	List<WorldObject> _ignoreList = new List<WorldObject>();
	public List<SequenceAction> GetApplyConsiqunces(WorldObject target)//rn only for placing prefabs
	{
		_ignoreList = new List<WorldObject>();
		
		var list = new List<SequenceAction>();
		if (FogOfWarSpot)
		{
			MoveCamera m = MoveCamera.Make(target.TileLocation.Position,true,FogOfWarSpotScatter);
			list.Add(m);
		}

		foreach (var tile in GetAffectedTiles(target.TileLocation.Position))
		{
			foreach (var sqc in ConsequencesOnTile(tile,target))
			{
				list.Add(sqc);
			}
		}

		if (Sfx != "" && Sfx != null)
		{
			PlaySound playSound = PlaySound.Make(Sfx, target.TileLocation.Position);
			list.Add(playSound);
			//Audio.PlaySound(Sfx, target);
		}

		foreach (var effect in Effects)
		{
			PostProcessingEffect peffect = PostProcessingEffect.Make(effect.Item1,float.Parse(effect.Item2, CultureInfo.InvariantCulture),float.Parse(effect.Item3, CultureInfo.InvariantCulture),true);
			list.Add(peffect);
		}

		return list;
	}
	protected List<SequenceAction> ConsequencesOnTile(IWorldTile tile, WorldObject originalTarget)
	{
		var consequences = new List<SequenceAction>();
		if (Dmg != 0)
		{
			if (tile.EastEdge != null && !_ignoreList.Contains(tile.EastEdge))
			{
				//tile.EastEdge?.TakeDamage(Dmg, 0);

				consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg, DetRes,  tile.EastEdge!.ID,EnvRes));
				_ignoreList.Add(tile.EastEdge!);
			}

			if (tile.WestEdge != null && !_ignoreList.Contains(tile.WestEdge))
			{

				consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg, DetRes,  tile.WestEdge!.ID,EnvRes));
				_ignoreList.Add(tile.WestEdge!);
				

			}

			if (tile.NorthEdge != null && !_ignoreList.Contains(tile.NorthEdge))
			{
				//tile.NorthEdge?.TakeDamage(Dmg, 0);
				consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg, DetRes,  tile.NorthEdge!.ID,EnvRes));
				_ignoreList.Add(tile.NorthEdge!);
			}

			if (tile.SouthEdge != null && !_ignoreList.Contains(tile.SouthEdge))
			{
				//tile.SouthEdge?.TakeDamage(Dmg, 0);
				consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg, DetRes, tile.SouthEdge!.ID,EnvRes));
				_ignoreList.Add(tile.SouthEdge!);
			}


			foreach (var item in tile.ObjectsAtLocation)
			{
				consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg, DetRes,item!.ID,EnvRes));
			}
			consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg,DetRes, EnvRes,tile.Position,Ignores));
		}
		

		if (PLaceItemConsequence!=null)
		{
			consequences.Add(WorldObjectManager.MakeWorldObject.Make(PLaceItemConsequence, tile.Position, originalTarget.Facing)); //fix this
		}

		UnitSequenceAction.TargetingRequirements req = new UnitSequenceAction.TargetingRequirements(tile.Position);
		req.TypesToIgnore = new List<string>();
		req.TypesToIgnore.AddRange(Ignores);


		if (Det != 0)
		{
			consequences.Add(Suppress.Make(Det, req));
		}

		consequences.Add(ChangeUnitValues.Make(req,Act,Move,null, MoveRange));

		foreach (var status in RemoveStatus)
		{
			consequences.Add(UnitStatusEffect.Make(req,false,status));
			//	tile.UnitAtLocation?.RemoveStatus(status);
		}
		foreach (var status in AddStatus)
		{
			consequences.Add(UnitStatusEffect.Make(req,true,status.Item1,status.Item2));
			//tile.UnitAtLocation?.ApplyStatus(status.Item1,status.Item2);
		}

		


		return consequences;
	}

}