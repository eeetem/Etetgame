﻿namespace MultiplayerXeno;

public class StatusEffectType
{
	public readonly string name;
	readonly WorldEffect effect;

	public StatusEffectType(string name, WorldEffect itm)
	{
		this.name = name;
		effect = itm;
	}

	public void Apply(Controllable actor)
	{
		effect.Apply(actor.worldObject.TileLocation.Position);
	}
}