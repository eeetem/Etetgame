using System.Threading.Tasks;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class SelectItem : SequenceAction
{
	public int itemIndex;
	public SelectItem(int actorID, int itemIndex) : base(actorID, SequenceType.SelectItem)
	{
		this.itemIndex = itemIndex;
	}
	public SelectItem(int actorID, Message args) : base(actorID, SequenceType.SelectItem)
	{
		this.itemIndex = args.GetInt();
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
		message.Add(itemIndex);
	}
}
