using System;
using System.Threading.Tasks;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class UpdateTile : SequenceAction
{

	public WorldTile.WorldTileData Data;
	public UpdateTile(WorldTile.WorldTileData data) : base(-1, SequenceType.UpdateTile)
	{
		Data = data;
	}


	public override Task Do()
	{
		return GenerateTask();
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			WorldManager.Instance.LoadWorldTile(Data);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		throw new Exception("This sequence action does not support serialization, it is generated from tile updates");
	}
}