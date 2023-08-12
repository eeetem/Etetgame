using System;
using System.Collections.Generic;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;



#if CLIENT
using Microsoft.Xna.Framework.Graphics;
using DefconNull.Rendering;
#endif

namespace DefconNull.World.WorldActions;

public class UnitAbility : IUnitAbility
{
	public readonly string name = null!;
	public readonly string tooltip = null!;
	public readonly int DetCost;
	public readonly int MoveCost;
	public readonly int ActCost;
	public readonly bool immideaateActivation;

	public List<IWorldEffect> Effects { get; }

	public Tuple<int,int,int> GetCost(Unit c)
	{
		return new Tuple<int, int, int>(DetCost,ActCost,MoveCost);
	}

	public bool ImmideateActivation => immideaateActivation;

	public string Tooltip => tooltip;


	public float GetOptimalRangeAI()
	{
		//get average
		var total = 0f;
		foreach (var effect in Effects)
		{
			total += effect.GetOptimalRangeAI();
		}

		return total / Effects.Count;
	}


	public UnitAbility(string name, string tooltip, int determinationCost, int movePointCost, int actionPointCost, List<IWorldEffect> effects, bool immideaateActivation)
	{
		this.name = name;
		this.tooltip = tooltip;
		DetCost = determinationCost;
		MoveCost = movePointCost;
		ActCost  = actionPointCost;
		Effects = effects;
		this.immideaateActivation = immideaateActivation;
#if CLIENT
		if (name != "")
		{
			Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		}
#endif
	}

	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{

		var res = HasEnoughPointsToPerform(actor);
		if (!res.Item1)
		{
			return res;
		}

		foreach (var effect in Effects)
		{
			var result = effect.CanPerform(actor, target);
			if (!result.Item1)
			{
				return result;
			}
		}
		return new Tuple<bool, string>(true, "");
	
	}

	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor)
	{
		if (actor.Determination - DetCost < 0)
		{
			return new Tuple<bool, string>(false, "Not enough determination");
		}
		
		if (actor.MovePoints - MoveCost < 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points");
		}
		

		if (actor.ActionPoints - MoveCost < 0)
		{
			return new Tuple<bool, string>(false, "Not enough action points");
		}
		

		return new Tuple<bool, string>(true, "");
	}

	public List<string> MakePacketArgs()
	{
		return new List<string>();
	}

	public List<SequenceAction> ExecutionResult(Unit actor, Vector2Int target)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}
		var consiquences = new List<SequenceAction>();
		consiquences.Add(new ChangeUnitValues(actor.WorldObject.ID,-ActCost,-MoveCost,-DetCost));
		
		foreach (var effect in Effects)
		{
			var actConsiquences =  effect.GetConsiquences( actor,  target);

			foreach (var c in actConsiquences)
			{
				consiquences.Add(c);
			}
		}

		return consiquences;
	}

	
#if CLIENT
	

	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}

		foreach (var eff in Effects)
		{
			eff.Preview(actor, target, spriteBatch);
		}
		
		
	}


	public Texture2D Icon { get; set; }
	

#endif

	public object Clone()
	{
		return new UnitAbility(name, tooltip, DetCost, MoveCost, ActCost,  Effects, immideaateActivation);	
	}


	public bool CanHit(Unit actor, Vector2Int target, bool lowTarget)
	{
		foreach (var effect in Effects)
		{
			if (effect.CanPerform(actor, target).Item1)
				return true;
		}

		return false;
	}
}