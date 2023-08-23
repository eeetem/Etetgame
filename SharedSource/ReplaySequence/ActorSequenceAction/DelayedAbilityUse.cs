using System.Threading.Tasks;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.ReplaySequence.ActorSequenceAction;

public class DelayedAbilityUse  : UnitSequenceAction
{
	int abilityIndex;
	Vector2Int target;
	public DelayedAbilityUse(int actorID, int abilityIndex, Vector2Int target) : base(new TargetingRequirements(actorID), SequenceType.DelayedAbilityUse)
	{
		this.abilityIndex = abilityIndex;
		this.target = target;
	}
	
	public DelayedAbilityUse(TargetingRequirements actorID, Message msg) : base(actorID, SequenceType.DelayedAbilityUse)
	{
		abilityIndex = msg.GetInt();
		target = msg.GetSerializable<Vector2Int>();
	}

	public override Task GenerateTask()
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
}