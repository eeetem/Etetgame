﻿using CommonData;

namespace MultiplayerXeno;

public class Face : Action
{
	public Face() :base(ActionType.Face)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{
		
		
		var targetDir = Utility.ToClampedDirection(actor.worldObject.TileLocation.Position - position);
		if (targetDir == actor.worldObject.Facing)
		{
			return false;
		}//dont let the action happen if the player is already facing that direction 
		
		if (actor.TurnPoints > 0)
		{
			return true;
		}
	

		return false;
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		var targetDir = Utility.ToClampedDirection(actor.worldObject.TileLocation.Position - target);
		actor.TurnPoints--;
		actor.worldObject.Face(targetDir);
	}




}

