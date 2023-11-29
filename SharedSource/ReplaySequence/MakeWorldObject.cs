using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class MakeWorldObject : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.MakeWorldObject;
	}

	public override bool CanBatch => true;
	
	string prefab;
	Vector2Int Position;
	Direction facing;
	int id;
	
	public static MakeWorldObject Make(string prefab, Vector2Int Position, Direction facing)
	{
		var t = GetAction(SequenceType.MakeWorldObject) as MakeWorldObject;
		t.prefab = prefab;
		t.Position = Position;
		t.facing = facing;
		t.id = WorldManager.Instance.GetNextId();//could cause issues if an object is deleted while IDs of a batch are being generated
		return t;
	}
	


	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			WorldManager.Instance.MakeWorldObject(prefab, Position, facing, id);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(prefab);
		message.Add(Position);
		message.Add((int)facing);
		message.Add(id);
	}

	protected override void DeserializeArgs(Message msg)
	{
		prefab = msg.GetString();
		Position = msg.GetSerializable<Vector2Int>();
		facing = (Direction)msg.GetInt();
		id = msg.GetInt();
	}
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		spriteBatch.DrawPrefab(Utility.GridToWorldPos(Position), prefab, facing);
		//var loadedPrefab = PrefabManager.WorldObjectPrefabs[prefab];
		//if (loadedPrefab.DestructionConseqences != null)
		//{
		//	spriteBatch.DrawOutline(loadedPrefab.DestructionConseqences!.GetAffectedTiles(Position), Color.Yellow, 4);
		//	//loadedPrefab.DestructionConseqences!.GetApplyConsiqunces(WorldManager.Instance.GetTileAtGrid(Position).Surface!).ForEach(x => x.PreviewIfShould(spriteBatch));
		//}
	}
#endif
}