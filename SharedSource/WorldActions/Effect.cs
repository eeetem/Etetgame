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
    
	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{
		return CanPerformChild(actor, target+Offset);
	}
	protected abstract Tuple<bool, string> CanPerformChild(Unit actor, Vector2Int target);
	
	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target)
	{
		return GetConsequencesChild(actor, target+Offset);
	}
	
	protected abstract List<SequenceAction> GetConsequencesChild(Unit actor, Vector2Int target);

#if CLIENT
	
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		PreviewChild(actor, target+Offset, spriteBatch);
	}
	
	protected abstract void PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
#endif


}