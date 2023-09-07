using DefconNull.World.WorldActions;

namespace DefconNull.World.WorldObjects;

public class StatusEffectType
{
	public readonly string name;
	public readonly WorldConseqences Conseqences;

	public StatusEffectType(string name, WorldConseqences itm)
	{
		this.name = name;
		Conseqences = itm;
	}

	public void Apply(Unit actor)
	{
		WorldManager.Instance.AddSequence(Conseqences.GetApplyConsiqunces(actor.WorldObject.TileLocation.Position));
	}
}