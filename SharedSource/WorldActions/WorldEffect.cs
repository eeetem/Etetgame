using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using Microsoft.Xna.Framework;
#endif


namespace DefconNull.World.WorldActions;

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

	
		var t = DeliveryMethod.ExectuteAndProcessLocation(actor,ref target);
	
		foreach (var change in t)
		{
			changes.Add(change);
		}
		foreach (var change in Conseqences.GetApplyConsiqunces(target))
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
#if CLIENT
	public static bool FreeFire = false;


	List<SequenceAction> previewCache = new List<SequenceAction>();
	Vector2Int previewActor = new Vector2Int(-1,-1);
	private WorldObject? previewTarget;
	private Tuple<WorldObject, HashSet<IWorldTile>> previewArea = new(null,new HashSet<IWorldTile>());

	protected override List<OwnedPreviewData> PreviewChild(Unit actor, WorldObject target, SpriteBatch spriteBatch)
	{
		
		if((!Equals(previewTarget, target) || previewActor != actor.WorldObject.TileLocation.Position) )	
		{
			previewCache = GetConsequences(actor, target);
			previewActor = actor.WorldObject.TileLocation.Position;
			previewTarget = target;
			previewArea = GetAffectedTiles(actor, target);
		}
        
		spriteBatch.DrawOutline(previewArea.Item2, Color.Red, 5);
		
		foreach (var act in previewCache)
		{
			act.PreviewIfShould(spriteBatch);
		}

		
			spriteBatch.DrawLine(Utility.GridToWorldPos(actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(previewArea.Item1.TileLocation.Position + new Vector2(0.5f, 0.5f)), Color.Red, 2);


		return new List<OwnedPreviewData>();
	}
#endif
}