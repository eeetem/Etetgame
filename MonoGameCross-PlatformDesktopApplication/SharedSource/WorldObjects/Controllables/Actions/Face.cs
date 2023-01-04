using System;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Face : Action
{
	public Face() :base(ActionType.Face)
	{
	}

	
	public override Tuple<bool,string> CanPerform(Controllable actor, Vector2Int position)
	{
		
		
		var targetDir = Utility.ToClampedDirection(actor.worldObject.TileLocation.Position - position);
		if (targetDir == actor.worldObject.Facing)
		{
			return new Tuple<bool, string>(false, "Already facing that direction");
		}//dont let the action happen if the player is already facing that direction 

		if (Controllable.moving)
		{
			return new Tuple<bool, string>(false, "Can't face while moving");
		}

		if (actor.TurnPoints <= 0)
		{
			return new Tuple<bool, string>(false, "Not enough turn points");
		}
	

		return new Tuple<bool, string>(true, "");
	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{
		var targetDir = Utility.ToClampedDirection(actor.worldObject.TileLocation.Position - target);
		actor.TurnPoints--;
		actor.worldObject.Face(targetDir);
	}
#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new System.NotImplementedException();
	}
#endif




}

