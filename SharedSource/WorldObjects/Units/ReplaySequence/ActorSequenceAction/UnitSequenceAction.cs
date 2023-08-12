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

			return obj.UnitComponent!;
		}
	}
	



	protected override void SerializeArgs(Message message)
	{
		message.Add(ActorID);
	}
}