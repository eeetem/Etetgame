using System;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class UseUpSelectedItem : UnitSequenceAction
{
	public Vector2Int Target;

	public UseUpSelectedItem(int actorID, Vector2Int target) : base(actorID, SequenceType.UseItem)
	{
		Target = target;
	}
	public UseUpSelectedItem(int actorID, Message args) : base(actorID, SequenceType.UseItem)
	{
		Target = args.GetSerializable<Vector2Int>();
	}

	public override Task Do()
	{
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.TileVisibility == Visibility.None)
			{
				if (Actor.SelectedItem!.Visible)
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
			Actor.LastItem = Actor.SelectedItem;
			Actor.RemoveItem(Actor.SelectedItemIndex);
		});
		return t;

	}
	

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.AddSerializable(Target);
	}
}