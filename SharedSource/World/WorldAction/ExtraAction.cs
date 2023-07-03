using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace MultiplayerXeno.Items;

public class ExtraAction : IExtraAction
{
	public readonly string name = null!;
	public readonly string tooltip = null!;
	public readonly int DeterminationChange;
	public readonly ValueChange MovePointChange;
	public readonly ValueChange ActionPointChange;
	public readonly WorldAction act = null!;
	public WorldAction WorldAction => act;
	public readonly bool immideaateActivation;



	public int[] GetConsiquences(Unit c)
	{
		return new[] {DeterminationChange, ActionPointChange.GetChange(c.ActionPoints), MovePointChange.GetChange(c.MovePoints)};
	}

	public bool ImmideateActivation => immideaateActivation;

	public string Tooltip => tooltip;

	public ExtraAction(string name, string tooltip, int determinationCost, ValueChange movePointCost, ValueChange actionPointCost, WorldAction action, bool immideaateActivation)
	{
		this.name = name;
		this.tooltip = tooltip;
		DeterminationChange = determinationCost;
		MovePointChange = movePointCost;
		ActionPointChange  = actionPointCost;
		act = action;
		this.immideaateActivation = immideaateActivation;
#if CLIENT
		
		Icon = act.Icon;
#endif
	}

	public ExtraAction()
	{
		
	}

	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{

		var res = HasEnoughPointsToPerform(actor);
		if (!res.Item1)
		{
			return res;
		}

		return WorldAction.CanPerform(actor, ref target);
	}

	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor)
	{
		if (actor.Determination + DeterminationChange < 0)
		{
			return new Tuple<bool, string>(false, "Not enough determination");
		}
		

		if (!MovePointChange.Set)
		{
			if (actor.MovePoints + MovePointChange.Value < 0)
			{
				return new Tuple<bool, string>(false, "Not enough move points");
			}
		}

		if (!MovePointChange.Set)
		{
			if (actor.ActionPoints + ActionPointChange.Value < 0)
			{
				return new Tuple<bool, string>(false, "Not enough action points");
			}
		}

		return new Tuple<bool, string>(true, "");
	}

	public List<string> MakePacketArgs()
	{
		if(WorldAction.DeliveryMethods.Find(x => x is Shootable)!= null)
		{
			return new List<string>() {Shootable.targeting.ToString()};
		}

		return new List<string>();
	}

	public void Execute(Unit actor, Vector2Int target)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}
		
		actor.Suppress(-DeterminationChange,true);
		MovePointChange.Apply(ref actor.MovePoints);
		ActionPointChange.Apply(ref actor.ActionPoints);
		WorldAction.Execute( actor,  target);
	}
#if CLIENT
	
	public void InitPreview()
	{
		WorldAction.InitPreview();
	}
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}

		WorldAction.Preview(actor, target, spriteBatch);
	}


	public Texture2D Icon { get; set; }
	

#endif

	public object Clone()
	{
		return new ExtraAction(name, tooltip, DeterminationChange, MovePointChange, ActionPointChange, act, immideaateActivation);	
	}


}