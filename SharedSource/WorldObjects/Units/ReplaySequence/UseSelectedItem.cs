using System;
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

	public override Task Do()
	{
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.Visible==Visibility.None)
			{
				if (Actor.SelectedItem.Visible)
				{
					Camera.SetPos(target + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
				}
			}
			else
			{
				Camera.SetPos(Actor.WorldObject.TileLocation.Position);
			}
#endif
			GenerateTask().RunSynchronously();
		});


		return t;
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