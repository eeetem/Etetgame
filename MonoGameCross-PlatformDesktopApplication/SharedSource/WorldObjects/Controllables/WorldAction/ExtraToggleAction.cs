using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

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


	public Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		if (isOn)
		{
			return off.CanPerform(actor, target);
		}
		return on.CanPerform(actor, target);
	}

	public List<string> MakePacketArgs()
	{
		if (isOn)
		{
			return off.MakePacketArgs();
		}
		return on.MakePacketArgs();
	}

	public void Execute(Controllable actor, Vector2Int target)
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

	public int[]  GetConsiquences(Controllable c)
	{
		if (isOn)
		{
			return off.GetConsiquences(c);
		}

		return on.GetConsiquences(c);
	}

	public bool ImmideateActivation { get=> true; }
#if CLIENT
	public void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (isOn)
		{
			off.Preview(actor,actor.worldObject.TileLocation.Position,spriteBatch);
		}
		else
		{
			on.Preview(actor, actor.worldObject.TileLocation.Position, spriteBatch);
		}
	}

	public void Animate(Controllable actor, Vector2Int target)
	{
		if (isOn)
		{
			on.Animate(actor,actor.worldObject.TileLocation.Position);
		}
		else
		{
			off.Animate(actor,actor.worldObject.TileLocation.Position);
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