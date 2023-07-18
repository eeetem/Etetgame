using System.Threading.Tasks;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class OverWatch : SequenceAction
{
	public Vector2Int Target;

	public OverWatch(int actorID, Vector2Int tg) : base(actorID, SequenceType.Overwatch)
	{
		Target = tg;
	}

	public OverWatch(int actorID, Message msg) : base(actorID, SequenceType.Overwatch)
	{
		Target = msg.GetSerializable<Vector2Int>();
	}
	
	public override Task Do()
	{
		var t = new Task(delegate
		{
			GenerateTask().RunSynchronously();
		});
		return t;
	}
	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.ActionPoints.Current=0;
			Actor.MovePoints.Current=0;
			var positions = Actor.GetOverWatchPositions(Target);
			foreach (var shot in positions)
			{
				WorldManager.Instance.GetTileAtGrid(shot.Result.EndPoint).Watch(Actor);
				Actor.overWatchedTiles.Add(shot.Result.EndPoint);
			}

			Actor.overWatch = true;
			Actor.WorldObject.Face(Utility.GetDirection(Actor.WorldObject.TileLocation.Position, Target));

		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(Target);
	}
}