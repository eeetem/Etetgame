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

#if CLIENT
	
	List<SequenceAction> previewCache = new List<SequenceAction>();
	Vector2Int previewActor = new Vector2Int(-1,-1);
	private WorldObject? previewTarget;

	public List<SequenceAction>  Preview(Unit actor, WorldObject target, SpriteBatch spriteBatch)
	{
		if (!Equals(previewTarget, target) || previewActor != actor.WorldObject.TileLocation.Position)
		{
		//	if (CanPerform(actor, target).Item1)
			//{
				previewCache = GetConsequences(actor, target);
				previewActor = actor.WorldObject.TileLocation.Position;
				previewTarget = target;
			//}
		}
		spriteBatch.DrawLine(Utility.GridToWorldPos(actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(target.TileLocation.Position+ new Vector2(0.5f, 0.5f)), Color.Red, 2);

		foreach (var act in previewCache)
		{
			act.Preview(spriteBatch);
		}

		
		if (Offset != new Vector2Int(0,0))
		{
		//	PreviewChild(actor, WorldManager.Instance.GetTileAtGrid(target.TileLocation.Position+Offset).Surface,spriteBatch);
			return previewCache;
		}

	//	PreviewChild(actor,target,spriteBatch);
		return previewCache;
	}
	
	//protected abstract void  PreviewChild(Unit actor, WorldObject target, SpriteBatch spriteBatch);
#endif


}