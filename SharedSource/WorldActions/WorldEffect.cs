using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.WorldActions.DeliveryMethods;
using DefconNull.WorldObjects;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using Microsoft.Xna.Framework;
#endif


namespace DefconNull.WorldActions;

public class WorldEffect : Effect
{
	public readonly DeliveryMethod DeliveryMethod;
	public readonly WorldConseqences Conseqences;

	public WorldEffect(DeliveryMethod deliveryMethod, WorldConseqences conseqences)
	{
		DeliveryMethod = deliveryMethod;
		Conseqences = conseqences;
	}

	public override float GetOptimalRangeAI()
	{
		return DeliveryMethod.GetOptimalRangeAI(Conseqences.Range-1);
	}

	protected override List<SequenceAction> GetConsequencesChild(Unit actor, WorldObject target,int dimension = -1)
	{
		//Console.WriteLine("getting consequences on "+target+" by "+actor.WorldObject.ID);
		var changes = new List<SequenceAction>();
		WorldObject? tgt = target;
		if(!DeliveryMethod.CanPerform(actor, target,dimension).Item1) return changes;
		var t = DeliveryMethod.ExectuteAndProcessLocation(actor,ref tgt);
		if(tgt == null) return changes;
		foreach (var change in t)
		{
			changes.Add(change);
		}
		foreach (var change in Conseqences.GetApplyConsiqunces(tgt))
		{
			changes.Add(change);
		}

	
		
		return changes;

	}
	
	public Tuple<WorldObject,HashSet<IWorldTile>> GetAffectedTiles(Unit actor, WorldObject target)
	{
		var tiles = new HashSet<IWorldTile>();

	
		if (!CanPerformChild(actor, target).Item1)
		{
			return new Tuple<WorldObject, HashSet<IWorldTile>>(target,tiles);
		}


		foreach (var tile in Conseqences.GetAffectedTiles(target.TileLocation.Position))
		{
			tiles.Add(tile);
		}
			
		
		return new Tuple<WorldObject, HashSet<IWorldTile>>(target,tiles);
	}




	protected override Tuple<bool, string> CanPerformChild(Unit actor, WorldObject target, int dimension = -1)
	{
		return DeliveryMethod.CanPerform(actor, target,dimension);
	}


}