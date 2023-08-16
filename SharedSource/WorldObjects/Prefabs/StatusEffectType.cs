using DefconNull.World.WorldActions;

namespace DefconNull.World.WorldObjects;

public class StatusEffectType
{
	public readonly string name;
	readonly WorldConseqences _conseqences;

	public StatusEffectType(string name, WorldConseqences itm)
	{
		this.name = name;
		_conseqences = itm;
	}

	public void Apply(Unit actor)
	{
		WorldManager.Instance.AddSequence(_conseqences.GetApplyConsiqunces(actor.WorldObject.TileLocation.Position));
	}
}