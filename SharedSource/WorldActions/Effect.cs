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
    
	public Tuple<bool, string> CanPerform(Unit actor, WorldObject target, int dimension =-1)
	{
		if (Offset != new Vector2Int(0,0))
		{
			return CanPerformChild(actor, WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position+Offset).Surface,dimension);
		}

		return CanPerformChild(actor,target,dimension);
	}
	protected abstract Tuple<bool, string> CanPerformChild(Unit actor, WorldObject target,int dimension = -1);
	
	public List<SequenceAction> GetConsequences(Unit actor, WorldObject target,int dimension = -1)
	{
		if (Offset != new Vector2Int(0,0))
		{
			return GetConsequencesChild(actor, WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position+Offset).Surface,dimension);
		}

		return GetConsequencesChild(actor,target,dimension);
	}
	
	protected abstract List<SequenceAction> GetConsequencesChild(Unit actor, WorldObject target,int dimension = -1);

#if CLIENT
	
	public List<OwnedPreviewData>  Preview(Unit actor, WorldObject target, SpriteBatch spriteBatch)
	{
		if (Offset != new Vector2Int(0,0))
		{
			return PreviewChild(actor, WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position+Offset).Surface,spriteBatch);
		}

		return PreviewChild(actor,target,spriteBatch);
	}
	
	protected abstract List<OwnedPreviewData>  PreviewChild(Unit actor, WorldObject target, SpriteBatch spriteBatch);
#endif


}