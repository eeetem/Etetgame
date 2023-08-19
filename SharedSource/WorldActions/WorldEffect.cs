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

public class WorldEffect : IWorldEffect
{
	public readonly DeliveryMethod DeliveryMethod;
	public readonly WorldConseqences Conseqences;
#if CLIENT
	public TargetAid targetAid;
#endif

	public enum TargetAid
	{
		None,
		Unit,
		Enemy
		
	}
	
	public WorldEffect(DeliveryMethod deliveryMethod, WorldConseqences conseqences)
	{
		DeliveryMethod = deliveryMethod;
		Conseqences = conseqences;
	}

	public float GetOptimalRangeAI()
	{
		return DeliveryMethod.GetOptimalRangeAI(Conseqences.Range-1);
	}

	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target)
	{
		Console.WriteLine("getting consequences on "+target+" by "+actor.WorldObject.ID);
		var changes = new List<SequenceAction>();

		Vector2Int? nullTarget = target;
		var t = DeliveryMethod.ExectuteAndProcessLocation(actor,ref nullTarget);
		if (nullTarget.HasValue)
		{
			foreach (var change in t)
			{
				changes.Add(change);
			}
			foreach (var change in Conseqences.GetApplyConsiqunces(nullTarget.Value, actor.WorldObject))
			{
				changes.Add(change);
			}

				
		}
		
		return changes;

	}
	
	public Tuple<Vector2Int?,HashSet<WorldTile>> GetAffectedTiles(Unit actor, Vector2Int target)
	{
		var tiles = new HashSet<WorldTile>();

		Vector2Int? nullTarget = target;
		if (!CanPerform(actor, target).Item1)
		{
			return new Tuple<Vector2Int?, HashSet<WorldTile>>(nullTarget,tiles);
		}

		var t = DeliveryMethod.ExectuteAndProcessLocation(actor,ref nullTarget);

		if (nullTarget.HasValue)
		{
			foreach (var tile in Conseqences.GetAffectedTiles(nullTarget.Value, actor.WorldObject))
			{
				tiles.Add(tile);
			}

				
		}
		
		return new Tuple<Vector2Int?, HashSet<WorldTile>>(nullTarget,tiles);
	}




	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{
#if CLIENT
		if (!FreeFire && targetAid != TargetAid.None)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(target);
			if (tile.UnitAtLocation == null || !tile.UnitAtLocation.WorldObject.IsVisible())
			{
				return new Tuple<bool, string>(false, "Invalid target, hold ctrl for free fire");
			}
			if (targetAid == TargetAid.Enemy && tile.UnitAtLocation.IsMyTeam())
			{
				return new Tuple<bool, string>(false, "Invalid target, hold ctrl for free fire");
			}
		}	
#endif
		return DeliveryMethod.CanPerform(actor, target);
	}
#if CLIENT
	public static bool FreeFire = false;


	List<SequenceAction> previewCache = new List<SequenceAction>();
	int perivewActorID = -1;
	Vector2Int previewTarget = new Vector2Int(-1,-1);
	private Tuple<Vector2Int?, HashSet<WorldTile>> previewArea = new Tuple<Vector2Int?, HashSet<WorldTile>>(null,new HashSet<WorldTile>());

	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
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