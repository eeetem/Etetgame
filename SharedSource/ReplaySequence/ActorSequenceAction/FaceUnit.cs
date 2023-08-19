using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class FaceUnit : UnitSequenceAction
{
	public Vector2Int target;
	public FaceUnit(int actorID, Vector2Int target) : base(new TargetingRequirements(actorID), SequenceType.Face)
	{
		this.target = target;
	}
	public FaceUnit(TargetingRequirements actorID, Message args) : base(actorID, SequenceType.Face)
	{
		target = args.GetSerializable<Vector2Int>();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			var targetDir = Utility.GetDirection(Actor.WorldObject.TileLocation.Position, target);
			Actor.canTurn = false;
			Actor.WorldObject.Face(targetDir);
		});
		return t;

	}
	

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.AddSerializable(target);
	}
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}