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
	public ushort Det;
	public ValueChange Move;
	public ValueChange Act;
	public ValueChange MoveRange;
	public ValueChange Determination;
	public bool ChangeValues = false;
	public int Range = 1;
	public Cover LosCheckCover = Cover.Low;
	public string? Sfx = "";
	public string? PLaceItemConsequence = null;
	public readonly List<Tuple<string, int>> AddStatus = new List<Tuple<string, int>>();
	public readonly List<string> RemoveStatus = new List<string>();

	public List<Tuple<string,string,string>> Effects = new List<Tuple<string, string, string>>();
	public readonly List<SpawnParticle.ParticleParams> ParticleParamsList = new List<SpawnParticle.ParticleParams>();

	
	public int ExRange;
	public List<string> Ignores = new List<string>();

	public bool FogOfWarSpot { get; set; }
	public int FogOfWarSpotScatter { get; set; }
	public int DetRes { get; set; }
	public int EnvRes { get; set; }
	public bool TargetFriend { get; set; }
	public bool TargetEnv { get; set; }
	public bool TargetEnemy { get; set; }
	public bool TargetSelf { get; set; }
	private object _lockObj = new object();



	public List<IWorldTile> GetAffectedTiles(Vector2Int target)
	{
		var hitt = WorldManager.Instance.GetTilesAround(target, Range, -1,LosCheckCover);
		var excl = WorldManager.Instance.GetTilesAround(target, ExRange, -1,null);
		var list = hitt.Except(excl).ToList();
		return list;
	}

	List<WorldObject> _ignoreList = new List<WorldObject>();
	public List<SequenceAction> GetApplyConsequnces(WorldObject user, WorldObject target)
	{
		
		var list = new List<SequenceAction>();
		if (FogOfWarSpot)
		{
			MoveCamera m = MoveCamera.Make(target.TileLocation.Position, true, FogOfWarSpotScatter);
			list.Add(m);
		}
		lock (_lockObj)
		{
			_ignoreList.Clear();

			foreach (var tile in GetAffectedTiles(target.TileLocation.Position))
			{
				foreach (var sqc in ConsequencesOnTile(user, tile))
				{
					list.Add(sqc);
				}
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
			PostProcessingEffect peffect = PostProcessingEffect.Make(effect.Item1, float.Parse(effect.Item2, CultureInfo.InvariantCulture), float.Parse(effect.Item3, CultureInfo.InvariantCulture), true);
			list.Add(peffect);
		}
		

		return list;
	}

	protected List<SequenceAction> ConsequencesOnObject(WorldObject user,WorldObject obj)
	{
		var consequences = new List<SequenceAction>();
		if (_ignoreList.Contains(obj)) return consequences;
		_ignoreList.Add(obj);
		bool shouldDo = TargetSelf && obj.ID == user.ID;
		
		
		if (obj.UnitComponent is not null && user.UnitComponent is not null)
		{
			if(TargetFriend && obj.UnitComponent.IsPlayer1Team == user.UnitComponent.IsPlayer1Team) shouldDo = true;
			if(TargetEnemy && obj.UnitComponent.IsPlayer1Team != user.UnitComponent.IsPlayer1Team) shouldDo = true;
		}
		else
		{
			if(TargetEnv && obj.UnitComponent is null) shouldDo = true;
		}
		if(Ignores.Contains(obj.Type.Name)) shouldDo = false;

		if (!shouldDo) return consequences;

		if (Dmg != 0)
		{
			consequences.Add(WorldObjectManager.TakeDamage.Make(Dmg, DetRes,  obj.ID,EnvRes));
		}

		if (obj.UnitComponent is not null)
		{
			//if(obj.UnitComponent.)
			UnitSequenceAction.TargetingRequirements req = new UnitSequenceAction.TargetingRequirements(obj.ID);


			if (Det != 0)
			{
				consequences.Add(Suppress.Make(Det, req));//purely for preview reasons
			}
			if (ChangeValues)
			{
				consequences.Add(ChangeUnitValues.Make(req,Act,Move,Determination, MoveRange));
			}
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

		}
		

		return consequences;
	}



	protected List<SequenceAction> ConsequencesOnTile(WorldObject user, IWorldTile tile)
	{
		var consequences = new List<SequenceAction>();


		foreach (var p in ParticleParamsList)
		{
			consequences.Add(SpawnParticle.Make(p,tile.Position));
		}
		
		foreach (var edge in tile.GetAllEdges())
		{
			consequences.AddRange(ConsequencesOnObject(user,edge));
		}

		foreach (var item in tile.ObjectsAtLocation)
		{
			consequences.AddRange(ConsequencesOnObject(user,item));
		}

		if (tile.UnitAtLocation != null)
		{
			consequences.AddRange(ConsequencesOnObject(user,tile.UnitAtLocation.WorldObject));
		}
		else
		{
			if (Det != 0)
			{
				consequences.Add(Suppress.Make(Det, tile.Position));//purely for preview reasons
			}
			if (ChangeValues)
			{
				consequences.Add(ChangeUnitValues.Make(tile.Position,Act,Move,Determination, MoveRange));
			}
		}

		
		

		if (PLaceItemConsequence!=null)
		{
			consequences.Add(WorldObjectManager.MakeWorldObject.Make(PLaceItemConsequence, tile.Position, user.Facing,user.Fliped)); //fix this
		}



	


		


		return consequences;
	}

}