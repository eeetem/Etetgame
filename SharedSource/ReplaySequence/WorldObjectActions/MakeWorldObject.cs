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
		public override bool ShouldSendToPlayerServerCheck(bool player1)
		{
			var wtile = WorldManager.Instance.GetTileAtGrid(position);
			return wtile.IsVisible(team1: player1);
		}
#endif

		public override BatchingMode Batching => BatchingMode.Sequential;
		
		Vector2Int position = new Vector2Int(23,11);
		public WorldObject.WorldObjectData data = new WorldObject.WorldObjectData("basicFloor");
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

		public override string ToString()
		{
			return "Making world object: " + data.ID + " " + data.Prefab + " " + position + data.Facing;
		}

		protected override void RunSequenceAction()
		{

			Console.WriteLine("DOING:"+ this);
			if (data.ID != -1 ) //if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
			{
				Console.WriteLine("deleting object with same if if exists");
				DeleteWorldObject.Make(data.ID).GenerateTask().RunSynchronously();
			}
			else
			{
				data.ID = GetNextId();
				Console.WriteLine("Generated new id: " + data.ID);
			}

			WorldObjectType type = PrefabManager.WorldObjectPrefabs[data.Prefab];
			var tile = WorldManager.Instance.GetTileAtGrid(position);
			WorldObject wo = new WorldObject(type, tile, data);
			wo.Fliped = data.Fliped;

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

		protected bool Equals(MakeWorldObject other)
		{
			return position.Equals(other.position) && data.Equals(other.data);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MakeWorldObject) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (position.GetHashCode() * 397) ^ data.GetHashCode();
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

		public override Message? MakeTestingMessage()
		{
			position = new Vector2Int(12, 5);
			data = new WorldObject.WorldObjectData("Scout");
			data.Lifetime = 100;
			data.Facing = Direction.SouthEast;
			data.JustSpawned = true;
			Message m = Message.Create();
			SerializeArgs(m);
			return m;
		}
	}

}