using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class PlayAnimation : UnitSequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.PlayAnimation;
	}

	protected override Task GenerateSpecificTask()
	{
		throw new NotImplementedException();
	}

	protected override void SerializeArgs(Message message)
	{
		throw new NotImplementedException();
	}

#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
	
}