using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class UnitOverWatch : UnitSequenceAction
{
	public Vector2Int Target;
	public int abilityIndex;

	public UnitOverWatch(int actorID, Vector2Int tg, int abilityIndex) : base(new TargetingRequirements(actorID), SequenceType.Overwatch)
	{
		Target = tg;
		this.abilityIndex = abilityIndex;
	}

	public UnitOverWatch(TargetingRequirements actorID, Message msg) : base(actorID, SequenceType.Overwatch)
	{
		Target = msg.GetSerializable<Vector2Int>();
		abilityIndex = msg.GetInt();
	}
	

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.ActionPoints.Current=0;
			Actor.MovePoints.Current=0;
			Actor.overWatch = true;
			var positions = Actor.GetOverWatchPositions(Target,abilityIndex);
			foreach (var shot in positions)
			{
				WorldManager.Instance.GetTileAtGrid(shot).Watch(Actor,abilityIndex);
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
	
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}