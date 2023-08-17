using System;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.ReplaySequence;

public class MoveCamera : SequenceAction
{
	
	Vector2Int location;
	bool doAlways;
	int scatter;
	public MoveCamera(Vector2Int location,bool doAlways, int scatter) : base(SequenceType.MoveCamera)
	{
		this.location = location;
		this.doAlways = doAlways;
		this.scatter = scatter;
	}
	public MoveCamera(Message msg) : base(SequenceType.MoveCamera)
	{
		location = msg.GetSerializable<Vector2Int>();
		doAlways = msg.GetBool();
		scatter = msg.GetInt();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
#if CLIENT
			
			if (WorldManager.Instance.GetTileAtGrid(location).TileVisibility==Visibility.None)
			{
				if (doAlways)
				{
					Camera.SetPos(location + new Vector2Int(Random.Shared.Next(-scatter, scatter), Random.Shared.Next(-scatter, scatter)));
				}
			}
			else
			{
				Camera.SetPos(location);
			}

			while (Camera.ForceMoving)
			{
				Thread.Sleep(50);
			}
#endif
			
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(location);
		message.Add(doAlways);
		message.Add(scatter);
	}
}