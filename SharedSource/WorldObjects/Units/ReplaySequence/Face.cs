using System.Threading.Tasks;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class Face : SequenceAction
{
	public Vector2Int target;
	public Face(int actorID, Vector2Int target) : base(actorID, SequenceType.Face)
	{
		this.target = target;
	}
	public Face(int actorID, Message args) : base(actorID, SequenceType.Face)
	{
		this.target = args.GetSerializable<Vector2Int>();
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			var targetDir = Utility.GetDirection(Actor.WorldObject.TileLocation.Position, target);
			Actor.canTurn = false;
			Actor.WorldObject.Face(targetDir);
		});
		return t;

	}
	

	protected override void SerializeArgs(Message message)
	{
		message.AddSerializable(target);
	}
}