using System.Threading.Tasks;
using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions;

public static partial class WorldObjectManager
{
	public class MakeWorldObject : SequenceAction
	{
		public override SequenceType GetSequenceType()
		{
			return SequenceType.MakeWorldObject;
		}
#if SERVER
		public override bool ShouldDoServerCheck(bool player1)
		{
			var wtile = WorldManager.Instance.GetTileAtGrid(position);
			return wtile.IsVisible(team1: player1);
		}
#endif

		public override bool CanBatch => true;
		
		Vector2Int position;
		private WorldObject.WorldObjectData data;
		public static MakeWorldObject Make(string prefab, Vector2Int Position, Direction facing, Unit.UnitData? unitData = null)
		{
			var data = new WorldObject.WorldObjectData(prefab);
			data.UnitData = unitData;
			data.Facing = facing;
			data.Facing = facing;
			data.ID = GetNextId();
			var t = GetAction(SequenceType.MakeWorldObject) as MakeWorldObject;
			t.data = data;
			t.position = Position;
			return t;
		}
		public static SequenceAction Make(WorldObject.WorldObjectData data, WorldTile position)
		{
			var t = GetAction(SequenceType.MakeWorldObject) as MakeWorldObject;
			t.data = data;
			t.position = position.Position;
			return t;
		}


		protected override Task GenerateSpecificTask()
		{
			var t = new Task(delegate
			{
				if (data.ID != -1) //if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
				{
					DeleteWorldObject(data.ID); //delete existing object with same id, most likely caused by server updateing a specific entity
				}
				else
				{
					data.ID = WorldObjectManager.GetNextId();
				}

				WorldObjectType type = PrefabManager.WorldObjectPrefabs[data.Prefab];
				var tile = WorldManager.Instance.GetTileAtGrid(position);
				WorldObject WO = new WorldObject(type, tile, data);
				WO.fliped = data.Fliped;

				type.Place(WO, tile, data);

				WorldObjects.EnsureCapacity(WO.ID + 1);
				WorldObjects[WO.ID] = WO;
			});
			return t;
		}



		protected override void SerializeArgs(Message message)
		{

			message.Add(position);
			message.Add(data);

		}

		protected override void DeserializeArgs(Message msg)
		{
			position = msg.GetSerializable<Vector2Int>();
			data = msg.GetSerializable<WorldObject.WorldObjectData>();
		}

	
	}

}