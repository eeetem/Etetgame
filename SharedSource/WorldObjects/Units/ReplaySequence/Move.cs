using System.Collections.Generic;

namespace MultiplayerXeno.ReplaySequence;

public class Move : SequenceAction
{
	private List<Vector2Int> Path;
	private int MovePoints;
	public Move(int actorID,List<Vector2Int> path, int movePoints) : base(actorID)
	{
		Path = path;
		MovePoints = movePoints;
	}
	public override void Do()
	{
		Unit act = WorldManager.Instance.GetObject(ActorID).UnitComponent;
		act.MovePoints -= MovePoints;
		act.MoveAnimation(Path);
		act.canTurn = true;
	}
}