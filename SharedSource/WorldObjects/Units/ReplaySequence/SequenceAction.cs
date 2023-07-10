using System;
using System.Threading.Tasks;
using System.Threading;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public abstract class SequenceAction : IMessageSerializable
{
	public enum SequenceType
	{
		Action=1,
		Move=2,
		Face=3,
		Crouch=4,
		UseItem = 5,
		SelectItem = 6,
		WorldEffect = 7,
		PlayAnimation =8,
		UpdateTile =9,
		Overwatch = 10
	}
	public readonly SequenceType SqcType;

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

	public SequenceAction(int actorID,SequenceType tp)
	{
		SqcType = tp;
		ActorID = actorID;
	}

	public virtual bool ShouldDo()
	{
		return true;
	}

	public virtual Task Do()
	{
		
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.Visible==Visibility.None)
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

	protected abstract Task GenerateTask();


	protected abstract void SerializeArgs(Message message);	

	public void Serialize(Message message)
	{
		message.Add(ActorID);
		SerializeArgs(message);
	}

	public void Deserialize(Message message)
	{
		throw new Exception("cannot deserialize abstract SequenceAction");
	}
}