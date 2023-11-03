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

	public override Task GenerateTask()
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
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}