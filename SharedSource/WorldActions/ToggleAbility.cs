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
	public int index;

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
	public string Name
	{
		get {
			if (isOn)
			{
				return off.name;
			}

			return on.name;
		}
	}

	public int Index => index;


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


	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, bool nextturn = false, int dimension=-1)
	{
		if(_disabled) return new Tuple<bool, string>(false, "Ability is disabled");
		if (isOn)
		{
			return off.CanPerform(actor, target,nextturn,dimension);
		}
		return on.CanPerform(actor, target,nextturn,dimension);
	}

	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor,bool nextTurn = false)
	{
		if (isOn)
		{
			return off.HasEnoughPointsToPerform(actor,nextTurn);
		}
		return on.HasEnoughPointsToPerform(actor,nextTurn);
	}

	public Tuple<bool, string> IsPlausibleToPerform(Unit actor, Vector2Int target, int dimension = -1)
	{
		if (isOn)
		{
			return off.IsPlausibleToPerform(actor,target,dimension);
		}
		return on.IsPlausibleToPerform(actor,target,dimension);
	}

	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target, int dimension = -1)
	{
		if (isOn)
		{
			return off.GetConsequences(actor,target,dimension);
		}
		return on.GetConsequences(actor,target,dimension);
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
	public bool Disabled => _disabled;
	private bool _disabled = false;
	public void Disable()
	{
		_disabled = true;
	}
	public object Clone()
	{
		if (_disabled)
		{
			var ab = new ToggleAbility((UnitAbility)on.Clone(),(UnitAbility)off.Clone(),index);
			ab.Disable();
			return ab;
		}

		return new ToggleAbility((UnitAbility)on.Clone(),(UnitAbility)off.Clone(),index);
	}
}