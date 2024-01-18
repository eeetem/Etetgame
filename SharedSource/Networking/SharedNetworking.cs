using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{

    public enum NetworkMessageID : ushort
    {

        MapDataInitiate =5,
        MapDataFinish =6,
        GameData =7,
        GameAction =8,
        StartGame =9,
        EndTurn =10,
        SquadComp =11,
        MapUpload =12,
        TileUpdate =13,
        MapDataInitiateConfirm = 14,
        ReplaySequence = 15,
        Notify = 16,
        Chat = 17,
        PreGameData = 18,
        Kick = 19,
        AddAI = 20,
        PracticeMode = 21,
        DoAI = 22,
        MapReaload = 23,
        UnitUpdate = 24,
    }

    public enum ReplaySequenceTarget : ushort
    {
        Player1 = 0,
        Player2 = 1,
        All =2,
    }
    
    public struct UnitUpdate : IMessageSerializable
    {
        public WorldObject.WorldObjectData Data;
        public Vector2Int? Position;

        public UnitUpdate(WorldObject.WorldObjectData data, Vector2Int? position)
        {
            this.Data = data;
            Position = position;
        }

        public void Serialize(Message message)
        {
            message.Add(Data);
            message.Add(Position.HasValue);
            if (Position.HasValue)
            {
                message.Add(Position.Value);
            }
        }

        public void Deserialize(Message message)
        {
            Data = message.GetSerializable<WorldObject.WorldObjectData>();
            if (message.GetBool())
            {
                Position = message.GetSerializable<Vector2Int>();
            }
            else
            {
                Position = null;
            }
        }
    }
	
    private static void LogNetCode(string msg)
    {
        DefconNull.Log.Message("RIPTIDE",msg);
    }

}
