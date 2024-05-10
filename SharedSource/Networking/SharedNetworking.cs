using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{

    public enum NetworkMessageID : ushort
    {

        MapDataInitiate =6,
        GameData =7,
        GameAction =8,
        StartGame =9,
        EndTurn =10,
        SquadComp =11,
        //MapUpload =12,
        //TileUpdate =13,
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
        StartTutorial = 24,
        SequenceFinished = 25,
        
    }

    public enum ReplaySequenceTarget : ushort
    {
        Player1 = 0,
        Player2 = 1,
        All =2,
    }
    

	
    private static void LogNetCode(string msg)
    {
        Log.Message("RIPTIDE",msg);
    }

}
