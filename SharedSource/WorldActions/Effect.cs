using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public abstract class Effect
{
	public Vector2Int Offset = new Vector2Int(0, 0);
	public abstract float GetOptimalRangeAI();
    
	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, int dimension =-1)
	{
		return CanPerformChild(actor, target+Offset,dimension);
	}
	protected abstract Tuple<bool, string> CanPerformChild(Unit actor, Vector2Int target,int dimension = -1);
	
	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target,int dimension = -1)
	{
		return GetConsequencesChild(actor, target+Offset,dimension);
	}
	
	protected abstract List<SequenceAction> GetConsequencesChild(Unit actor, Vector2Int target,int dimension = -1);

#if CLIENT
	
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		PreviewChild(actor, target+Offset, spriteBatch);
	}
	
	protected abstract void PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
#endif


}