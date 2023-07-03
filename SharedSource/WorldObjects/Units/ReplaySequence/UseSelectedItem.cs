using Riptide;
using System.Threading.Tasks;

namespace MultiplayerXeno.ReplaySequence;

public class UseSelectedItem : SequenceAction
{
	public Vector2Int target;

	public UseSelectedItem(int actorID, Vector2Int target) : base(actorID, SequenceType.UseItem)
	{
		this.target = target;
	}
	public UseSelectedItem(int actorID, Message args) : base(actorID, SequenceType.UseItem)
	{
		this.target = args.GetSerializable<Vector2Int>();
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			var item = Actor.SelectedItem;
			item.Execute(Actor, target);
			Actor.LastItem = Actor.SelectedItem;
			Actor.RemoveItem(Actor.SelectedItemIndex);
			Actor.WorldObject.Face(Utility.GetDirection(Actor.WorldObject.TileLocation.Position,target));
		});
		return t;

	}
	

	protected override void SerializeArgs(Message message)
	{
		message.AddSerializable(target);
	}
}