﻿using System;
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
			if(data.UnitData.HasValue)
			{
				if (data.UnitData.Value.Team1 == player1)
				{
					return true;//always get updates for your own team
				}
			}

			if (_position == null) return false;//dont send off map objects
			var wtile = WorldManager.Instance.GetTileAtGrid((Vector2Int)_position);
			var vis = wtile.GetVisibility(player1);
			if(PrefabManager.WorldObjectPrefabs[data.Prefab].Edge)
			{
				Visibility vis2;
				WorldTile t;
				switch (data.Facing)
				{
					case Direction.North:
						t = WorldManager.Instance.GetTileAtGrid((Vector2Int)_position + new Vector2Int(0, -1));
						vis2 = t.GetVisibility(player1);
						if(vis2>vis) vis = vis2;
						break;
					
					case Direction.West:
						t = WorldManager.Instance.GetTileAtGrid((Vector2Int)_position + new Vector2Int(-1, 0));
						vis2 = t.GetVisibility(player1);
						if(vis2>vis) vis = vis2;
						break;
				}
			}

			return vis > Visibility.None;
		}
#endif

        public override BatchingMode Batching => BatchingMode.Sequential;

        private Vector2Int _position = new Vector2Int(0, 0);
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
            t._position = Position;
            return t;
        }
	
        public static MakeWorldObject Make(WorldObject.WorldObjectData data, Vector2Int position)
        {
            return Make(data,WorldManager.Instance.GetTileAtGrid(position));
        }

        public static MakeWorldObject Make(WorldObject.WorldObjectData data, WorldTile position)
        {
            var t = GetAction(SequenceType.MakeWorldObject) as MakeWorldObject;
            t.data = data;
            t._position = position.Position;
            return t;
        }

        public override string ToString()
        {
            return "Making world object: " + data.ID + " " + data.Prefab + " " + _position + data.Facing;
        }

        protected override void RunSequenceAction()
        {

            Log.Message("WORLD OBJECT MANAGER","DOING:"+ this);
            if (data.ID != -1 ) //if it has a pre defined id - delete the old obj - otherwise we can handle other id stuff when creatng it
            {
                Log.Message("WORLD OBJECT MANAGER","deleting object with same if if exists");
                DeleteWorldObject.Make(data.ID).GenerateTask().RunTaskSynchronously();

            }
            else
            {
                data.ID = GetNextId();
                Log.Message("WORLD OBJECT MANAGER","Generated new id: " + data.ID);
            }

		
            WorldObjectType type = PrefabManager.WorldObjectPrefabs[data.Prefab];

            WorldObject wo;
            WorldTile? tile = null;
		
		
            tile = WorldManager.Instance.GetTileAtGrid((Vector2Int)_position);
            wo = new WorldObject(type, tile, data);
				
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
            return _position.Equals(other._position) && data.Equals(other.data);
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
                return (_position.GetHashCode() * 397) ^ data.GetHashCode();
            }
        }

        protected override void SerializeArgs(Message message)
        {
            if(_position== null) throw new Exception("serialising world object creation with no position");
            message.Add((Vector2Int)_position);
            message.Add(data);

        }

        protected override void DeserializeArgs(Message msg)
        {
            _position = msg.GetSerializable<Vector2Int>();
            data = msg.GetSerializable<WorldObject.WorldObjectData>();
        }

        public override Message? MakeTestingMessage()
        {
            _position = new Vector2Int(12, 5);
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