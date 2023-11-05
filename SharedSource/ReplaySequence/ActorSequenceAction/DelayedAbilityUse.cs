using System.Threading.Tasks;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.ReplaySequence.ActorSequenceAction;

public class DelayedAbilityUse  : UnitSequenceAction
{
	int abilityIndex;
	Vector2Int target;
	
	public static DelayedAbilityUse Make(int actorID, int abilityIndex, Vector2Int target) 
	{
		DelayedAbilityUse t = GetAction(SequenceType.DelayedAbilityUse) as DelayedAbilityUse;
		t.abilityIndex = abilityIndex;
		t.target = target;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	


	public override SequenceType GetSequenceType()
	{
		return SequenceType.DelayedAbilityUse;
	}

	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			//clientside execution of ability
			//really should be avoided since it'll likely cause desyncs
			//but we'll see
			Actor.Abilities[abilityIndex].GetConsequences(Actor, target).ForEach(x => x.GenerateTask().RunSynchronously());
		});
		return t;
	}
	
	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(abilityIndex);
		message.Add(target);
	}

	protected override void DeserializeArgs(Message message)
	{
		base.DeserializeArgs(message);
		abilityIndex = message.GetInt();
		target = message.GetSerializable<Vector2Int>();
	}
}