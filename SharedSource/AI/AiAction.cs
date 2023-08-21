using DefconNull.World.WorldObjects;

namespace DefconNull.AI;

public abstract class AIAction
{

	public readonly AIActionType Type;


	public AIAction(AIActionType? type)
	{
		if(type==null) return;
		Type = (AIActionType)type;
		AI.AiActions.Add((AIActionType)type, this);
	}



	public enum AIActionType
	{
		FinaliseMovement=1,
		Move=2,
		OverWatch = 5,
		UseAbility = 7,

	}
	public abstract void Execute(Unit unit);
}