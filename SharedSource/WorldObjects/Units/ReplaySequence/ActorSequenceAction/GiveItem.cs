using System.Threading.Tasks;
using DefconNull.World.WorldObjects;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;

public class GiveItem : UnitSequenceAction
{
	private readonly string _name;
	public GiveItem(int actorID, string name) : base(actorID, SequenceType.GiveItem)
	{
		_name = name;
	}
	public GiveItem(int actorID, Message args) : base(actorID, SequenceType.GiveItem)
	{
		_name = args.GetString();
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.AddItem(PrefabManager.UseItems[_name]);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(_name);
	}
}