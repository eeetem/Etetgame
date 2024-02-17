

namespace DefconNull.WorldObjects.Units;

public class StatusEffectInstance
{
	public StatusEffectInstance(StatusEffectType type, int duration)
	{
		this.type = type;
		this.duration = duration;
	}
	public readonly StatusEffectType type;
	public int duration;

	public void Apply(Unit unit)
	{
		type.Apply(unit);
		Log.Message("UNITS", "Applying status effect: " + type.name + " to " + unit.WorldObject.ID);
		duration--;
	}
}