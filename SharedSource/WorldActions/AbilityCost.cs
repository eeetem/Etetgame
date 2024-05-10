namespace DefconNull.WorldActions;

public struct AbilityCost
{
	public readonly int Determination;
	public readonly int ActionPoints;
	public readonly int MovePoints;

	public AbilityCost(int determination, int actionPoints, int movePoints)
	{
		Determination = determination;
		ActionPoints = actionPoints;
		MovePoints = movePoints;
	}

}