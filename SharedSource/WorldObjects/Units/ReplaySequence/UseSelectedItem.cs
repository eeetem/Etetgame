using System;
using System.Threading;
using Riptide;
using System.Threading.Tasks;

namespace MultiplayerXeno.ReplaySequence;

public class UseSelectedItem : SequenceAction
{
	public Vector2Int Target;

	public UseSelectedItem(int actorID, Vector2Int target) : base(actorID, SequenceType.UseItem)
	{
		this.Target = target;
	}
	public UseSelectedItem(int actorID, Message args) : base(actorID, SequenceType.UseItem)
	{
		this.Target = args.GetSerializable<Vector2Int>();
	}

	public override Task Do()
	{
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.TileVisibility == Visibility.None)
			{
				if (Actor.SelectedItem.Visible)
				{
					Camera.SetPos(Actor.WorldObject.TileLocation.Position + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
		
				}
			}
			else
			{
				Camera.SetPos(Actor.WorldObject.TileLocation.Position);
			}

			Thread.Sleep(600);
			if (WorldManager.Instance.GetTileAtGrid(Target).TileVisibility == Visibility.None)
			{
				if (Actor.SelectedItem.Visible)
				{
					Camera.SetPos(Target + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
		
				}
			}
			else
			{
				Camera.SetPos(Target);
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
			item.Execute(Actor, Target);
			Actor.LastItem = Actor.SelectedItem;
			Actor.RemoveItem(Actor.SelectedItemIndex);
			if (Actor.WorldObject.TileLocation.Position != Target)
			{
				Actor.WorldObject.Face(Utility.GetDirection(Actor.WorldObject.TileLocation.Position, Target));
			}
		});
		return t;

	}
	

	protected override void SerializeArgs(Message message)
	{
		message.AddSerializable(Target);
	}
}