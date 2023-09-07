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
	public readonly ushort DetCost;
	public readonly ushort MoveCost;
	public readonly ushort ActCost;
	public readonly bool immideaateActivation;

	public List<Effect> Effects { get; }

	public Tuple<int,int,int> GetCost(Unit c)
	{
		return new Tuple<int, int, int>(DetCost,ActCost,MoveCost);
	}

	public bool ImmideateActivation => immideaateActivation;

	public string Tooltip => tooltip;
	public string Name => name;
	public int Index => index;
	public int index;
	public bool Disabled => _disabled;


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


	public UnitAbility(string name, string tooltip, ushort determinationCost, ushort movePointCost, ushort actionPointCost, List<Effect> effects, bool immideaateActivation, int index)
	{
		this.name = name;
		this.tooltip = tooltip;
		DetCost = determinationCost;
		MoveCost = movePointCost;
		ActCost  = actionPointCost;
		Effects = effects;
		this.immideaateActivation = immideaateActivation;
		this.index = index;
#if CLIENT

		Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		
#endif
	}

	
	private bool _disabled = false;
	public void Disable()
	{
		_disabled = true;
	}
	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, bool NextTurn = false, int dimension =-1)
	{
		if(_disabled) return new Tuple<bool, string>(false, "Ability is disabled");
		var res = HasEnoughPointsToPerform(actor,NextTurn);
		if (!res.Item1)
		{
			return res;
		}
	
		res = IsPlausibleToPerform(actor,target,dimension);
		if (!res.Item1)
		{
			return res;
		}
 
		return new Tuple<bool, string>(true, "");
	
	}

	public Tuple<bool, string> IsPlausibleToPerform(Unit actor, Vector2Int target,int dimension = -1)
	{
		foreach (var effect in Effects)
		{
			var result = effect.CanPerform(actor, target,dimension);
			if (!result.Item1)
			{
				return result;
			}
		}
		return new Tuple<bool, string>(true, "");
	}


	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor, bool nextTurn = false)
	{
		if (DetCost > 0)
		{
			int determination = actor.Determination.Current;
			if (nextTurn && !actor.Paniced)
			{
				determination++;
				if (actor.Determination > actor.Determination.Max)
				{
					determination = actor.Determination.Max;
				}
			}

			if (determination - DetCost < 0)
			{
				return new Tuple<bool, string>(false, "Not enough determination");
			}
			
		}

	

		if(MoveCost>0){
			
			int movePoints = actor.MovePoints.Current;
			if (nextTurn)
			{
				movePoints = actor.MovePoints.Max;
			}

			if (movePoints- MoveCost < 0)
			{
				return new Tuple<bool, string>(false, "Not enough move points");
			}
			
		}

		if (ActCost > 0)
		{
						
			int actPoints = actor.ActionPoints.Current;
			if (nextTurn)
			{
				actPoints = actor.ActionPoints.Max;
			}
			
			if (actPoints - ActCost < 0)
			{
				return new Tuple<bool, string>(false, "Not enough action points");
			}
		}


		return new Tuple<bool, string>(true, "");
	}

	public List<string> MakePacketArgs()
	{
		return new List<string>();
	}

	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target, int dimension = -1)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}
		var consequences = new List<SequenceAction>();
		consequences.Add(new ChangeUnitValues(actor.WorldObject.ID,-ActCost,-MoveCost,-DetCost));
		
		foreach (var effect in Effects)
		{
			var actConsequences =  effect.GetConsequences( actor,  target,dimension);

			foreach (var c in actConsequences)
			{
				consequences.Add(c);
			}
		}

		return consequences;
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
		if (_disabled)
		{
			var ab = new UnitAbility(name, tooltip, DetCost, MoveCost, ActCost,  Effects, immideaateActivation,index);	
			ab.Disable();
			return ab;
		}
		
		return new UnitAbility(name, tooltip, DetCost, MoveCost, ActCost,  Effects, immideaateActivation,index);	
		

		
	}
    


}