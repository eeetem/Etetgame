using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public class ExtraAction : IExtraAction
{
	public readonly string name;
	public readonly string tooltip;
	public readonly int DeterminationChange;
	public readonly ValueChange MovePointChange;
	public readonly ValueChange ActionPointChange;
	public readonly WorldAction act;
	public WorldAction WorldAction { get => act; }
	public readonly bool immideaateActivation;


	public int[] GetConsiquences(Controllable c)
	{
		return new[] {DeterminationChange, ActionPointChange.GetChange(c.ActionPoints), MovePointChange.GetChange(c.MovePoints)};
	}

	public bool ImmideateActivation { get => immideaateActivation; }
	public string Tooltip
	{
		get => tooltip;
	}

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

	public Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
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

		return WorldAction.CanPerform(actor, target);
	}

	public List<string> MakePacketArgs()
	{
		if(WorldAction.DeliveryMethods.Find(x => x is Shootable)!= null)
		{
			return new List<string>() {Shootable.targeting.ToString()};
		}

		return new List<string>();
	}

	public void Execute(Controllable actor, Vector2Int target)
	{
		if (immideaateActivation)
		{
			target = actor.worldObject.TileLocation.Position;
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
	public void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (immideaateActivation)
		{
			target = actor.worldObject.TileLocation.Position;
		}

		WorldAction.Preview(actor, target, spriteBatch);
	}

	public void Animate(Controllable actor, Vector2Int target)
	{
		if (immideaateActivation)
		{
			target = actor.worldObject.TileLocation.Position;
		}
		WorldAction.Animate(actor, target);
	}

	public Texture2D Icon { get; set; }
	

#endif

	public object Clone()
	{
		return new ExtraAction(name, tooltip, DeterminationChange, MovePointChange, ActionPointChange, act, immideaateActivation);	
	}
}