using DefconNull.World.WorldActions;

namespace DefconNull.World.WorldObjects;

public class StatusEffectType
{
	public readonly string name;
	readonly WorldConsiqences _consiqences;

	public StatusEffectType(string name, WorldConsiqences itm)
	{
		this.name = name;
		_consiqences = itm;
	}

	public void Apply(Unit actor)
	{
		WorldManager.Instance.AddSequence(_consiqences.GetApplyConsiqunces(actor.WorldObject.TileLocation.Position));
	}
}