using System.Threading.Tasks;
using DefconNull.World;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

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