using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class PlayAnimation : UnitSequenceAction
{

	public override Task GenerateTask()
	{
		throw new NotImplementedException();
	}

	protected override void SerializeArgs(Message message)
	{
		throw new NotImplementedException();
	}

	public PlayAnimation(int actorID) : base(new TargetingRequirements(actorID), SequenceType.PlayAnimation)
	{
	}
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
	
}