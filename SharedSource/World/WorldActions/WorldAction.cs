using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.World.WorldActions;

public class WorldAction
{
	public readonly string Name;
	public readonly string Description;
	public readonly List<DeliveryMethod> DeliveryMethods = new List<DeliveryMethod>();
	public readonly WorldEffect Effect;
#if CLIENT
	public readonly Texture2D? Icon;
	public TargetAid targetAid;
#endif

	public enum TargetAid
	{
		None,
		Unit,
		Enemy
		
	}
	
	public WorldAction(string name,  string description, List<DeliveryMethod> deliveryMethods, WorldEffect effect)
	{
		Name = name;
		DeliveryMethods = deliveryMethods;
		Effect = effect;
		Description = description;
#if CLIENT
		if (name != "")
		{
			Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		}
#endif
	}

	public float GetOptimalRangeAI()
	{
		return DeliveryMethods[0].GetOptimalRangeAI(Effect.Range-1);
	}

	public List<SequenceAction> GetConsiquences(Unit actor, Vector2Int target)
	{
		Console.WriteLine("Executing Action "+Name+" on "+target+" by "+actor.WorldObject.ID);
		var changes = new List<SequenceAction>();
		foreach (var method in DeliveryMethods)
		{
			Vector2Int? nullTarget = target;
			var t = method.ExectuteAndProcessLocation(actor,ref nullTarget);
			if (nullTarget.HasValue)
			{
				foreach (var change in t)
				{
					changes.Add(change);
				}
				foreach (var change in Effect.ApplyConsiqunces(nullTarget.Value, actor.WorldObject))
				{
					changes.Add(change);
				}

				
			}
		}
		return changes;

	}




	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
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
		return DeliveryMethods[0].CanPerform(actor,ref target);
	}
#if CLIENT
	public static bool FreeFire = false;
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
	
		if (!FreeFire && targetAid != TargetAid.None)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(target);
			if (tile.UnitAtLocation == null || !tile.UnitAtLocation.WorldObject.IsVisible())
			{
				return;
			}

			if (targetAid == TargetAid.Enemy && tile.UnitAtLocation.IsMyTeam())
			{
				return;
			}
		}
		

		foreach (var method in DeliveryMethods)
		{
			//todo fix this
			//var result = method.Preview(actor, target, spriteBatch);
			//if(result == null) continue;
		//	Effect.Preview(result.Value, spriteBatch,actor.WorldObject);
		}
	
	}
	

#endif



}