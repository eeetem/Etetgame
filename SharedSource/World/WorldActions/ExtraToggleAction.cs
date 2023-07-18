using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public class ExtraToggleAction : IExtraAction
{
	private readonly ExtraAction on;
	private readonly ExtraAction off;
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
	public ExtraToggleAction(ExtraAction on, ExtraAction off)
	{
		this.on = on;
		this.off = off;
	}


	public Tuple<bool, string> CanPerform(Unit actor,ref Vector2Int target)
	{
		if (isOn)
		{
			return off.CanPerform(actor,ref target);
		}
		return on.CanPerform(actor,ref target);
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

	public void Execute(Unit actor, Vector2Int target)
	{
		if (isOn)
		{
			off.Execute(actor,target);
		}
		else
		{
			on.Execute(actor,target);
		}

		isOn = !isOn;

	}
	
	public WorldAction WorldAction	{
		get {
			if (isOn)
			{
				return on.WorldAction;
			}

			return off.WorldAction;
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
		return new ExtraToggleAction((ExtraAction)on.Clone(),(ExtraAction)off.Clone());
	}
}