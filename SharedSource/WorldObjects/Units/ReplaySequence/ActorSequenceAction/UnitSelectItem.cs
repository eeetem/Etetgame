using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class UnitSelectItem : UnitSequenceAction
{
	public int itemIndex;
	public UnitSelectItem(int actorID, int itemIndex) : base(actorID, SequenceType.SelectItem)
	{
		this.itemIndex = itemIndex;
	}
	public UnitSelectItem(int actorID, Message args) : base(actorID, SequenceType.SelectItem)
	{
		itemIndex = args.GetInt();
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.SelectedItemIndex = itemIndex;
		});
		return t;

	}
	
	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(itemIndex);
	}
}
