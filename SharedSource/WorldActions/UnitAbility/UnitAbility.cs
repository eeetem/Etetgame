using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
#if CLIENT
using Microsoft.Xna.Framework.Graphics;
using DefconNull.Rendering;
#endif

namespace DefconNull.WorldActions.UnitAbility;

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
#if CLIENT
	public Texture2D Icon { get; set; }
#endif

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
		Name = name;
		tooltip = tooltip.Replace("\\n", "\n");//replace escape characters
		Tooltip = tooltip;
		DetCost = determinationCost;
		MoveCost = movePointCost;
		ActCost  = actionPointCost;
		Effects = effects;
		ImmideateActivation = immideaateActivation;
		Index = index;
		AIExempt = aiExempt;
		
		TargetAids = targetAids;
#if CLIENT

		Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		
#endif

		OverWatchRange = overWatchRange;
	}



	public Tuple<bool, string> CanPerform(Unit actor, WorldObject target, bool consultTargetAids, bool nextTurn, int dimension =-1)
	{

		Tuple<bool, string>? res;
		res = HasEnoughPointsToPerform(actor,nextTurn);
		if (!res.Item1)
		{
			return res;
		}

		
		if (consultTargetAids)
		{
			var aids = IsValidTarget(actor, target);
			if (!aids.Item1)
			{
				return aids;
			}
			
		}
		var res2 = IsPlausibleToPerform(actor,target,dimension);
		if (!res2.Item1)//if impossible return
		{
			return new Tuple<bool, string>(res2.Item1, res2.Item3);
		}
		if(consultTargetAids && !res2.Item2)//unadvisable execution
		{
			return new Tuple<bool, string>(res2.Item2, res2.Item3);
		}
		
		
	

		return new Tuple<bool, string>(true, "");
	
	}

	public Tuple<bool,string> IsValidTarget(Unit actor, WorldObject target)
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

					if (target.UnitComponent?.IsPlayer1Team == actor.IsPlayer1Team)
					{
						return new Tuple<bool, string>(false, "Target is not an enemy");
					}
					break;
				case "friend":
					if (negate)
					{
						throw new Exception("Cannot negate friend target aid");
					}

					if (target.UnitComponent?.IsPlayer1Team != actor.IsPlayer1Team)
					{
						return new Tuple<bool, string>(false, "Target is not an friend");
					}
					break;
				default:
					var u = target.UnitComponent;
					
					//if(u is null && !negate) return new Tuple<bool, string>(false, "Target is not a "+str);
					if(u is null)continue;
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

	public Tuple<bool,bool, string>  IsPlausibleToPerform(Unit actor, WorldObject target,int dimension = -1)
	{
		
		return Effects[0].CanPerform(actor, target,dimension);

	}


	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor, bool nextTurn = false)
	{
		var nextTurnPoints = actor.GetPointsNextTurn();
		if (DetCost > 0)
		{
			int determination = actor.Determination.Current;
			if (nextTurn)
			{
				determination = nextTurnPoints.Item3;
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
				movePoints = nextTurnPoints.Item1;
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
				actPoints = nextTurnPoints.Item2;
			}
			
			if (actPoints - ActCost < 0)
			{
				return new Tuple<bool, string>(false, "Not enough action points");
			}
		}


		return new Tuple<bool, string>(true, "");
	}

	public List<SequenceAction> GetConsequences(Unit actor, WorldObject target, int dimension = -1)
	{
		if (ImmideateActivation)
		{
			target = actor.WorldObject;
		}
		var consequences = new List<SequenceAction>(Effects.Count*3);
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

	

	public object Clone()
	{
		return new UnitAbility(Name, Tooltip, DetCost, MoveCost, ActCost, OverWatchRange, Effects, ImmideateActivation,Index,AIExempt,TargetAids);	
		
	}



}