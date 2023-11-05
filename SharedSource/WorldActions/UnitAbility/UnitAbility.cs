using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;


#if CLIENT
using Microsoft.Xna.Framework.Graphics;
using DefconNull.Rendering;
#endif

namespace DefconNull.World.WorldActions;

public class UnitAbility 
{
	public readonly string Name = null!;
	public readonly string Tooltip = null!;
	public readonly ushort DetCost;
	public readonly ushort MoveCost;
	public readonly ushort ActCost;
	public readonly bool ImmideateActivation;
	public readonly bool AIExempt;
	public readonly ushort OverWatchRange;
	public readonly ushort Index;
	public List<Effect> Effects { get; }

	public AbilityCost GetCost()
	{
		return new AbilityCost(DetCost,ActCost,MoveCost);
	}


	readonly public List<string> TargetAids;
	
	public bool CanOverWatch => OverWatchRange > 0;
	

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


	public UnitAbility(string name, string tooltip, ushort determinationCost, ushort movePointCost, ushort actionPointCost, ushort overWatchRange, List<Effect> effects, bool immideaateActivation, ushort index, bool aiExempt, List<string> targetAids)
	{
		this.Name = name;
		this.Tooltip = tooltip;
		DetCost = determinationCost;
		MoveCost = movePointCost;
		ActCost  = actionPointCost;
		Effects = effects;
		this.ImmideateActivation = immideaateActivation;
		this.Index = index;
		this.AIExempt = aiExempt;
		
		this.TargetAids = targetAids;
#if CLIENT

		Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		
#endif

		OverWatchRange = overWatchRange;
	}



	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, bool consultTargetAids, bool nextTurn, int dimension =-1)
	{
		if(ImmideateActivation && target != actor.WorldObject.TileLocation.Position) return new Tuple<bool, string>(false, "Target is not self");
		Tuple<bool, string>? res;
		if (consultTargetAids)
		{
			var aids = IsValidTarget(actor, target, dimension);
			if (!aids.Item1)
			{
				return aids;
			}
				
			res = IsPlausibleToPerform(actor,target,dimension);
			if (!res.Item1)
			{
				return res;
			}

		}

		
		
		res = HasEnoughPointsToPerform(actor,nextTurn);
		if (!res.Item1)
		{
			return res;
		}

		return new Tuple<bool, string>(true, "");
	
	}

	public Tuple<bool,string> IsValidTarget(Unit actor, Vector2Int target, int dimension)
	{
		if (TargetAids.Count == 0) return new Tuple<bool, string>(true, "");
		foreach (var t in TargetAids)
		{
			var str = t;
			bool negate = false;
			if (str.StartsWith("!"))
			{
				negate = true;
				str = str.Substring(1);
			}

			switch (str)
			{
				case "enemy":
					if (negate)
					{
						throw new Exception("Cannot negate enemy target aid");
					}

					if (WorldManager.Instance.GetTileAtGrid(target,dimension).UnitAtLocation?.IsPlayer1Team == actor.IsPlayer1Team)
					{
						return new Tuple<bool, string>(false, "Target is not an enemy");
					}
					break;
				case "friend":
					if (negate)
					{
						throw new Exception("Cannot negate friend target aid");
					}

					if (WorldManager.Instance.GetTileAtGrid(target,dimension).UnitAtLocation?.IsPlayer1Team != actor.IsPlayer1Team)
					{
						return new Tuple<bool, string>(false, "Target is not an friend");
					}
					break;
				default:
					var u = WorldManager.Instance.GetTileAtGrid(target,dimension).UnitAtLocation;
					
					if(u is null && !negate) return new Tuple<bool, string>(false, "Target is not a "+str);
					bool match = u!.Type.Name == str;

					if (negate)
					{
						if (match)return new Tuple<bool, string>(false, "Target is a "+str);
					}
					else
					{
						if(!match) return new Tuple<bool, string>(false, "Target is not a "+str);
					}
					break;
			}
		}
		return new Tuple<bool, string>(true, "");
	}

	public Tuple<bool, string> IsPlausibleToPerform(Unit actor, Vector2Int target,int dimension = -1)
	{
		
		var result = Effects[0].CanPerform(actor, target,dimension);
		if (!result.Item1)
		{
			return result;
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

	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target, int dimension = -1)
	{
		if (ImmideateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}
		var consequences = new List<SequenceAction>();
		ChangeUnitValues ch = ChangeUnitValues.Make(actor.WorldObject.ID,-ActCost,-MoveCost,-DetCost);
		consequences.Add(ch);
		
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
		if (ImmideateActivation)
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
		return new UnitAbility(Name, Tooltip, DetCost, MoveCost, ActCost, OverWatchRange, Effects, ImmideateActivation,Index,AIExempt,TargetAids);	
		
	}
    


}