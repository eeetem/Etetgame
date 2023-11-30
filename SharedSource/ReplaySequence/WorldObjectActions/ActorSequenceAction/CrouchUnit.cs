﻿using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class CrouchUnit : UnitSequenceAction
{

	
	public override SequenceType GetSequenceType()
	{
		return SequenceType.Crouch;
	}

	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			Actor.canTurn = true;
			Actor.Crouching = !Actor.Crouching;

			WorldManager.Instance.MakeFovDirty();

		});
		return t;
	}
	


#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
	public static CrouchUnit Make(int actorID)
	{
		CrouchUnit t = (GetAction(SequenceType.Crouch) as CrouchUnit)!;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
}