using System;
using System.Threading.Tasks;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class WorldChange : SequenceAction
{
	private WorldEffect effect;
	private Vector2Int target;
	public WorldChange(int actorID, Vector2Int target, WorldEffect effect) : base(actorID, SequenceType.WorldEffect)
	{
		this.target = target;
		this.effect = effect;
	}
	public WorldChange(int actorID, Message args) : base(actorID, SequenceType.WorldEffect)
	{
		target = args.GetSerializable<Vector2Int>();
		effect = args.GetSerializable<WorldEffect>();
	}

	public override Task Do()
	{
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.TileVisibility==Visibility.None)
			{
				 if (effect.Visible)
				 {
				 Camera.SetPos(target + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
				 }
			}
			else
			{
				Camera.SetPos(Actor.WorldObject.TileLocation.Position);
			}
#endif
			GenerateTask().RunSynchronously();
		});


		return t;
	}

	protected override Task GenerateTask()
	{	
		var t = new Task(delegate { effect.Apply(target, Actor.WorldObject); });
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(target);
		message.Add(effect);
	}

	
}