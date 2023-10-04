using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
	public readonly bool aiExempt;
	public readonly ushort OverWatchRange;
	public List<Effect> Effects { get; }

	public Tuple<int,int,int> GetCost(Unit c)
	{
		return new Tuple<int, int, int>(DetCost,ActCost,MoveCost);
	}

	public bool ImmideateActivation => immideaateActivation;

	public string Tooltip => tooltip;
	public string Name => name;
	public int Index => index;
	public bool AIExempt => aiExempt;
	public readonly int index;
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


	public UnitAbility(string name, string tooltip, ushort determinationCost, ushort movePointCost, ushort actionPointCost, ushort overWatchRange, List<Effect> effects, bool immideaateActivation, int index, bool aiExempt, List<string> targetAids)
	{
		this.name = name;
		this.tooltip = tooltip;
		DetCost = determinationCost;
		MoveCost = movePointCost;
		ActCost  = actionPointCost;
		Effects = effects;
		this.immideaateActivation = immideaateActivation;
		this.index = index;
		this.aiExempt = aiExempt;
		
		this.TargetAids = targetAids;
#if CLIENT

		Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		
#endif

		OverWatchRange = overWatchRange;
	}
	


	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, bool consultTargetAids, bool nextTurn, int dimension =-1)
	{
		if (consultTargetAids)
		{
			var aids = IsValidTarget(actor, target, dimension);
			if (!aids.Item1)
			{
				return aids;
			}
		}
		
		
		var res = HasEnoughPointsToPerform(actor,nextTurn);
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

	private Tuple<bool,string> IsValidTarget(Unit actor, Vector2Int target, int dimension)
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
					if(negate) match = !match;
					if(!match) return new Tuple<bool, string>(false, "Target is not a "+str);
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
			consequences.EnsureCapacity(actConsequences.Count + consequences.Count);//resiize to not loose performace to memory allocation stuff
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
		if(!CanPerform(actor,target,true,false).Item1) return;
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
		return new UnitAbility(name, tooltip, DetCost, MoveCost, ActCost, OverWatchRange, Effects, immideaateActivation,index,aiExempt,TargetAids);	
		
	}
    


}