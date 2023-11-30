using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence;

public class UpdateTile : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.UpdateTile;
	}

	public override bool CanBatch => true;
	public WorldTile.WorldTileData Data;
	
	public UpdateTile(WorldTile.WorldTileData data) 
	{
		Data = data;
	}
#if SERVER
	public override bool ShouldDoServerCheck(bool player1)
	{
		return true;
	}
#endif

	
	protected override Task GenerateSpecificTask()
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
	protected override void DeserializeArgs(Message message)
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