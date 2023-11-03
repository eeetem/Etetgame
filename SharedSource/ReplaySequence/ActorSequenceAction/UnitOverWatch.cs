using System;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class UnitOverWatch : UnitSequenceAction
{
	public Vector2Int Target;
	public int abilityIndex;

	public static UnitOverWatch Make(int actorID, Vector2Int tg, int abilityIndex)
	{
		UnitOverWatch t = GetAction(SequenceType.Overwatch) as UnitOverWatch;
		t.Target = tg;
		t.abilityIndex = abilityIndex;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	public override SequenceType GetSequenceType()
	{
		return SequenceType.Overwatch;
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.ActionPoints.Current=0;
			Actor.MovePoints.Current=0;
			Actor.Overwatch = new  Tuple<bool, int>(true,abilityIndex);
			var positions = Actor.GetOverWatchPositions(Target,abilityIndex);
			foreach (var shot in positions)
			{
				WorldManager.Instance.GetTileAtGrid(shot).Watch(Actor);
				Actor.overWatchedTiles.Add(shot);
			}
			Actor.WorldObject.Face(Utility.GetDirection(Actor.WorldObject.TileLocation.Position, Target));

		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(Target);
		message.Add(abilityIndex);
	}

	protected override void DeserializeArgs(Message message)
	{
		base.DeserializeArgs(message);
		Target = message.GetSerializable<Vector2Int>();
		abilityIndex = message.GetInt();
	}

#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif

}