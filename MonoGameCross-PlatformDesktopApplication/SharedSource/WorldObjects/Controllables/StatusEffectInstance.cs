﻿namespace MultiplayerXeno;

public class StatusEffectInstance
{
	public StatusEffectInstance(StatusEffectType type, int duration)
	{
		this.type = type;
		this.duration = duration;
	}
	public readonly StatusEffectType type;
	public int duration;

	public void Apply(Controllable controllable)
	{
		type.Apply(controllable);
		duration--;
	}
}