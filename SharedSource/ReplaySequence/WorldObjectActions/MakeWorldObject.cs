using System;
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

		public override BatchingMode Batching => BatchingMode.Always;
		
		Vector2Int position;
		public WorldObject.WorldObjectData data;
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
		public static MakeWorldObject Make(WorldObject.WorldObjectData data, WorldTile position)
		{
			var t = GetAction(SequenceType.MakeWorldObject) as MakeWorldObject;
			t.data = data;
			t.position = position.Position;
			return t;
		}


		protected override void RunSequenceAction()
		{

				if (data.ID != -1 ) //if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
				{
					DeleteWorldObject.Make(data.ID).GenerateTask().RunSynchronously();
				}
				else
				{
					data.ID = GetNextId();
				}

				WorldObjectType type = PrefabManager.WorldObjectPrefabs[data.Prefab];
				var tile = WorldManager.Instance.GetTileAtGrid(position);
				WorldObject wo = new WorldObject(type, tile, data);
				wo.fliped = data.Fliped;

				type.Place(wo, tile, data);

				if(wo is null) throw new Exception("Created a null worldobject");
				lock (WoLock)
				{
					if (!WorldObjects.ContainsKey(wo.ID))
					{
						WorldObjects[wo.ID] = wo;
					}
					else
					{
						WorldObjects.Add(wo.ID,wo);
					}

					
				}
				

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