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

	protected override List<SequenceAction> GetConsequencesChild(Unit actor, Vector2Int target,int dimension = -1)
	{
		//Console.WriteLine("getting consequences on "+target+" by "+actor.WorldObject.ID);
		var changes = new List<SequenceAction>();

		Vector2Int? nullTarget = target;
		var t = DeliveryMethod.ExectuteAndProcessLocation(actor,ref nullTarget);
		if (nullTarget.HasValue)
		{
			foreach (var change in t)
			{
				changes.Add(change);
			}
			foreach (var change in Conseqences.GetApplyConsiqunces(nullTarget.Value))
			{
				changes.Add(change);
			}

				
		}
		
		return changes;

	}
	
	public Tuple<Vector2Int?,HashSet<IWorldTile>> GetAffectedTiles(Unit actor, Vector2Int target)
	{
		var tiles = new HashSet<IWorldTile>();

		Vector2Int? nullTarget = target;
		if (!CanPerformChild(actor, target).Item1)
		{
			return new Tuple<Vector2Int?, HashSet<IWorldTile>>(nullTarget,tiles);
		}

		var t = DeliveryMethod.ExectuteAndProcessLocation(actor,ref nullTarget);

		if (nullTarget.HasValue)
		{
			foreach (var tile in Conseqences.GetAffectedTiles(nullTarget.Value))
			{
				tiles.Add(tile);
			}

				
		}
		
		return new Tuple<Vector2Int?, HashSet<IWorldTile>>(nullTarget,tiles);
	}




	protected override Tuple<bool, string> CanPerformChild(Unit actor, Vector2Int target, int dimension = -1)
	{
		return DeliveryMethod.CanPerform(actor, target,dimension);
	}
#if CLIENT
	public static bool FreeFire = false;


	List<SequenceAction> previewCache = new List<SequenceAction>();
	int perivewActorID = -1;
	Vector2Int previewTarget = new Vector2Int(-1,-1);
	private Tuple<Vector2Int?, HashSet<IWorldTile>> previewArea = new(null,new HashSet<IWorldTile>());

	protected override void PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		if((previewTarget != target || perivewActorID != actor.WorldObject.ID) && CanPerform(actor,target).Item1)	
		{
			previewCache = GetConsequences(actor, target);
			perivewActorID = actor.WorldObject.ID;
			previewTarget = target;
			previewArea = GetAffectedTiles(actor, target);
		}
        
		spriteBatch.DrawOutline(previewArea.Item2, Color.Red, 5);
		
		foreach (var act in previewCache)
		{
			act.PreviewIfShould(spriteBatch);
		}

		if (previewArea.Item1.HasValue)
		{
			spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"), Utility.GridToWorldPos(previewArea.Item1.Value + new Vector2(-1.5f, -0.5f)), Color.Red);
			spriteBatch.DrawLine(Utility.GridToWorldPos(actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(previewArea.Item1.Value + new Vector2(0.5f, 0.5f)), Color.Red, 2);


		}

	
	}
#endif
}