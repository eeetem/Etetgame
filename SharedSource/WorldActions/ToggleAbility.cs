using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public class ToggleAbility : IUnitAbility
{
	private readonly UnitAbility on;
	private readonly UnitAbility off;
	private bool isOn;
	private int index;

	public string Tooltip
	{
		get {
			if (isOn)
			{
				return off.tooltip;
			}

			return on.tooltip;
		}
	}

	public void Toggle()
	{
		isOn = !isOn;
	}

	public float GetOptimalRangeAI()
	{
		return isOn ? on.GetOptimalRangeAI() : off.GetOptimalRangeAI();
	}


	public ToggleAbility(UnitAbility on, UnitAbility off,int index)
	{
		this.on = on;
		this.off = off;
		this.index = index;
	}


	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target)
	{
		if (isOn)
		{
			return off.CanPerform(actor, target);
		}
		return on.CanPerform(actor, target);
	}

	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor)
	{
		if (isOn)
		{
			return off.HasEnoughPointsToPerform(actor);
		}
		return on.HasEnoughPointsToPerform(actor);
	}


	public List<string> MakePacketArgs()
	{
		if (isOn)
		{
			return off.MakePacketArgs();
		}
		return on.MakePacketArgs();
	}

	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target)
	{
		List<SequenceAction> r = new List<SequenceAction>();
		if (isOn)
		{
			r =	off.GetConsequences(actor,target);
		}else
		{
			r = on.GetConsequences(actor,target);
		}
			
		//todo make this a world change
		r.Add(new UnitAbilitToggle(actor.WorldObject.ID,index));

		return r;
	}


	public List<Effect> Effects	{
		get {
			if (isOn)
			{
				return on.Effects;
			}

			return off.Effects;
		}
	}

	public Tuple<int,int,int> GetCost(Unit c)
	{
		if (isOn)
		{
			return off.GetCost(c);
		}

		return on.GetCost(c);
	}

	public bool ImmideateActivation => true;
#if CLIENT
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (isOn)
		{
			off.Preview(actor,actor.WorldObject.TileLocation.Position,spriteBatch);
		}
		else
		{
			on.Preview(actor, actor.WorldObject.TileLocation.Position, spriteBatch);
		}
	}


	public Texture2D Icon
	{
		get {
			if (isOn)
			{
				return on.Icon;
			}

			return off.Icon;
		}
	}

#endif
	public object Clone()
	{
		return new ToggleAbility((UnitAbility)on.Clone(),(UnitAbility)off.Clone(),index);
	}
}