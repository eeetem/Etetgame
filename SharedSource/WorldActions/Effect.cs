using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace DefconNull.WorldActions;

public abstract class Effect
{
	public Vector2Int Offset = new Vector2Int(0, 0);
	public abstract float GetOptimalRangeAI();
    
	public Tuple<bool, string> CanPerform(Unit actor, WorldObject target, int dimension =-1)
	{
		var tile = GetTargetTile(actor, target);
		if (tile == null)
		{
			return new Tuple<bool, string> (false,"Can't target empty tile");
		}
		return CanPerformChild(actor,tile,dimension);
	}
	protected abstract Tuple<bool, string>  CanPerformChild(Unit actor, WorldObject target,int dimension = -1);
	
	public List<SequenceAction> GetConsequences(Unit actor, WorldObject target,int dimension = -1)
	{
		var tile = GetTargetTile(actor, target);
		if (tile == null)
		{
			return new List<SequenceAction>();
		}
		return GetConsequencesChild(actor,tile,dimension);
	}
	
	private WorldObject? GetTargetTile(Unit actor, WorldObject target)
	{
		if (Offset != new Vector2Int(0,0))
		{
			var sfc = WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position + Offset).Surface;
			return sfc;
		}

		return target;
	}
	
	protected abstract List<SequenceAction> GetConsequencesChild(Unit actor, WorldObject target,int dimension = -1);


	public Tuple<bool, string> IsRecommendedToPerform(Unit actor, WorldObject target, int dimension)
	{
		var tile = GetTargetTile(actor, target);
		if (tile == null)
		{
			return new Tuple<bool, string> (false,"Can't target empty tile");
		}
		
		return IsRecommendedToPerformChild(actor,tile,dimension);
	}

	protected abstract Tuple<bool, string> IsRecommendedToPerformChild(Unit actor, WorldObject target, int dimension);
}