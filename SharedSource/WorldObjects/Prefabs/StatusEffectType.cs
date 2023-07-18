namespace DefconNull.World.WorldObjects;

public class StatusEffectType
{
	public readonly string name;
	readonly WorldEffect effect;

	public StatusEffectType(string name, WorldEffect itm)
	{
		this.name = name;
		effect = itm;
	}

	public void Apply(Unit actor)
	{
		effect.Apply(actor.WorldObject.TileLocation.Position);
	}
}