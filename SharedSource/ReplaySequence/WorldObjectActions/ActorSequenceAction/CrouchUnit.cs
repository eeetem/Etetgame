using System.Threading;
using System.Threading.Tasks;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class CrouchUnit : UnitSequenceAction
{

	
	public override SequenceType GetSequenceType()
	{
		return SequenceType.Crouch;
	}

	public override BatchingMode Batching => ShouldBatch();

	private BatchingMode ShouldBatch()
	{
		if(Actor.Crouching)
		{
			return BatchingMode.SyncAlone;//uncrouch and animation start should happen on the same tick
		}

		return BatchingMode.AsycnBatchAlways;
		
	}

	protected override void RunSequenceAction()
	{
		Actor.CanTurn = true;
#if CLIENT
		//animation should be always played in standing state so you can see units stand up behind cover
		if (Actor.Crouching)
		{
			Actor.Crouching = !Actor.Crouching;
			Actor.WorldObject.StartAnimation("StandUp");
		}
		else
		{
			Actor.WorldObject.StartAnimation("CrouchDown");
			while (Actor.WorldObject.IsAnimating)
			{
				Thread.Sleep(100);
			}
			Actor.WorldObject.CurrentAnimation = null;
			Actor.Crouching = !Actor.Crouching;
		}


#else
Actor.Crouching = !Actor.Crouching;
#endif
		WorldManager.Instance.MakeFovDirty();


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