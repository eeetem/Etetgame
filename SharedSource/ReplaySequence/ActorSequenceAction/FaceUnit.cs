using System;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class FaceUnit : UnitSequenceAction
{
	public Vector2Int target;
	
	public static FaceUnit Make(int actorID, Vector2Int target) 
	{
		FaceUnit t = (GetAction(SequenceType.Face) as FaceUnit)!;
		t.target = target;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}

	public override SequenceType GetSequenceType()
	{
		return SequenceType.Face;
	}

	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			if(Actor.WorldObject.TileLocation.Position == target) return;
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

	protected override void DeserializeArgs(Message message)
	{
		base.DeserializeArgs(message);
		target = message.GetSerializable<Vector2Int>();
	}
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}