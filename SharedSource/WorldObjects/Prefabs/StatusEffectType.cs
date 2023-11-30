using DefconNull.ReplaySequence;
using DefconNull.WorldActions;

namespace DefconNull.WorldObjects;

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
		SequenceManager.AddSequence(Conseqences.GetApplyConsiqunces(actor.WorldObject));
	}
}