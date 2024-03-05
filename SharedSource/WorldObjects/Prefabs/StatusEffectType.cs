using DefconNull.ReplaySequence;
using DefconNull.WorldActions;

namespace DefconNull.WorldObjects;

public class StatusEffectType
{
	public readonly string Name;
	public readonly string Tip;
	public readonly WorldConseqences Conseqences;

	public StatusEffectType(string name, string tip, WorldConseqences itm)
	{
		Name = name;
		Tip = tip;
		Conseqences = itm;
	}

	public void Apply(Unit actor)
	{
		SequenceManager.AddSequence(Conseqences.GetApplyConsiqunces(actor.WorldObject));
	}
}