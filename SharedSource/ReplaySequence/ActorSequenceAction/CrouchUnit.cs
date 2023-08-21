using System.Threading.Tasks;
using DefconNull.World;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class CrouchUnit : UnitSequenceAction
{
	
	public CrouchUnit(int actorID) : base(new TargetingRequirements(actorID), SequenceType.Crouch)
	{
	}
	public CrouchUnit(TargetingRequirements actorID, Message args) : base(actorID, SequenceType.Crouch)
	{
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.canTurn = true;
			Actor.Crouching = !Actor.Crouching;
#if CLIENT
					WorldManager.Instance.MakeFovDirty();
#endif
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		return;
	}



#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}