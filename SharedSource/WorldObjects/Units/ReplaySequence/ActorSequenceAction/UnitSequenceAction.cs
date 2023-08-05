using System;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public abstract class UnitSequenceAction : SequenceAction
{
	
	public UnitSequenceAction(int actorID, SequenceAction.SequenceType type) : base(type)
	{
		ActorID = actorID;
	}
	public readonly int ActorID;
	protected Unit Actor
	{
		get
		{
			var obj = WorldManager.Instance.GetObject(ActorID);
			int attempts = 0;
			while(obj == null)
			{
				Thread.Sleep(1000);
				obj = WorldManager.Instance.GetObject(ActorID);
				attempts++;
				if (attempts>10)
				{
					throw new Exception("Sequence Actor not found");
				}
			}

			return obj.UnitComponent;
		}
	}

	public override Task Do()
	{
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.TileVisibility==Visibility.None)
			{
				// if (Visible)
				// {
				// Camera<>.SetPos(target + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
				// }
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



	protected override void SerializeArgs(Message message)
	{
		message.Add(ActorID);
	}
}