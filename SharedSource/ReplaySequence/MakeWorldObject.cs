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
	public override bool CanBatch => true;
	
	string prefab;
	Vector2Int Position;
	Direction facing;
	int id;
	
	public MakeWorldObject(string prefab, Vector2Int Position, Direction facing) : base(SequenceType.MakeWorldObject)
	{
		this.prefab = prefab;
		this.Position = Position;
		this.facing = facing;
		id = WorldManager.Instance.GetNextId();//could cause issues if an object is deleted while IDs of a batch are being generated
	}
	public MakeWorldObject(Message msg) : base(SequenceType.MakeWorldObject)
	{
		prefab = msg.GetString();
		Position = msg.GetSerializable<Vector2Int>();
		facing = (Direction)msg.GetInt();
		id = msg.GetInt();
	}


	public override Task GenerateTask()
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
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		spriteBatch.DrawPrefab(Utility.GridToWorldPos(Position), prefab, facing);
		if (PrefabManager.WorldObjectPrefabs[prefab].DestructionConseqences != null)
		{
			spriteBatch.DrawOutline(PrefabManager.WorldObjectPrefabs[prefab].DestructionConseqences.GetAffectedTiles(Position, null), Color.Yellow, 4);
			PrefabManager.WorldObjectPrefabs[prefab].DestructionConseqences!.GetApplyConsiqunces(Position, null).ForEach(x => x.Preview(spriteBatch));
		}
	}
#endif
}