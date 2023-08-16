using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class UpdateTile : SequenceAction
{
	public override bool CanBatch => true;
	public WorldTile.WorldTileData Data;
	public UpdateTile(WorldTile.WorldTileData data) : base(SequenceType.UpdateTile)
	{
		Data = data;
	}

	
	public override Task GenerateTask()
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
	
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}