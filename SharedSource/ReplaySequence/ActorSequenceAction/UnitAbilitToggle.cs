using System.Threading.Tasks;
using DefconNull.World.WorldActions;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;

public class UnitAbilitToggle : UnitSequenceAction
{
	
	int abilityID;
	public override bool CanBatch => true;
	public UnitAbilitToggle(int actorID, int abilityID) : base(actorID, SequenceType.AbilityToggle)
	{
		this.abilityID = abilityID;
	}
	
	public UnitAbilitToggle(int actorID, Message msg) : base(actorID, SequenceType.AbilityToggle)
	{
		abilityID = msg.GetInt();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			ToggleAbility t = (ToggleAbility) Actor.Abilities[abilityID];
			t.Toggle();
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(abilityID);
	}
}