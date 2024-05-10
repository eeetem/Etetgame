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
    
	public Tuple<bool,bool, string> CanPerform(Unit actor, WorldObject target, int dimension =-1)
	{
		if (Offset != new Vector2Int(0,0))
		{
			var sfc = WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position + Offset).Surface;
			if (sfc != null)
			{
				return CanPerformChild(actor,sfc, dimension);
			}

			return new Tuple<bool,bool, string> (false,false,"Can't target empty tile");
		}

		return CanPerformChild(actor,target,dimension);
	}
	protected abstract Tuple<bool,bool, string>  CanPerformChild(Unit actor, WorldObject target,int dimension = -1);
	
	public List<SequenceAction> GetConsequences(Unit actor, WorldObject target,int dimension = -1)
	{
		if (Offset != new Vector2Int(0,0))
		{
			var sfc = WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position + Offset).Surface;
			if (sfc != null)
			{
				return GetConsequencesChild(actor,sfc, dimension);
			}

			return new List<SequenceAction>();
		}

		return GetConsequencesChild(actor,target,dimension);
	}
	
	protected abstract List<SequenceAction> GetConsequencesChild(Unit actor, WorldObject target,int dimension = -1);


}