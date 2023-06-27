using System.Collections.Generic;
using System.Linq;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class Move : SequenceAction
{
	private List<Vector2Int> Path;
	public Move(int actorID,List<Vector2Int> path) : base(actorID,SequenceType.Move)
	{
		Path = path;
	}
	public Move(int actorID,Message args) : base(actorID,SequenceType.Move)
	{
		Path = args.GetSerializables<Vector2Int>().ToList();
	}
	public override void Do()
	{
		
		Actor.MoveAnimation(Path);
		Actor.canTurn = true;
	}

	protected override void SerializeArgs(Message message)
	{
		message.AddSerializables(Path.ToArray());
	}


}