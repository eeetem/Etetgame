using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public class ExtraToggleAction : IUnitAbility
{
	private readonly UnitAbility on;
	private readonly UnitAbility off;
	private bool isOn;


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

	public float GetOptimalRangeAI()
	{
		return isOn ? on.GetOptimalRangeAI() : off.GetOptimalRangeAI();
	}


	public ExtraToggleAction(UnitAbility on, UnitAbility off)
	{
		this.on = on;
		this.off = off;
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

	public bool CanHit(Unit actor, Vector2Int target, bool lowTarget = false)
	{
		if (isOn)
		{
			return off.CanHit(actor,target,lowTarget);
		}
		return on.CanHit(actor,target,lowTarget);
		

		
	}

	public List<string> MakePacketArgs()
	{
		if (isOn)
		{
			return off.MakePacketArgs();
		}
		return on.MakePacketArgs();
	}

	public List<SequenceAction> ExecutionResult(Unit actor, Vector2Int target)
	{
		List<SequenceAction> r = new List<SequenceAction>();
		if (isOn)
		{
			r =	off.ExecutionResult(actor,target);
		}else
		{
			r = on.ExecutionResult(actor,target);
		}
		//todo make this a world change
		isOn = !isOn;

		return r;
	}


	public List<IWorldEffect> Effects	{
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
		return new ExtraToggleAction((UnitAbility)on.Clone(),(UnitAbility)off.Clone());
	}
}