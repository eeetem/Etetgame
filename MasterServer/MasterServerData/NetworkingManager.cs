namespace DefconNull.Networking;

public static partial class NetworkingManager
{
    public enum MasterServerNetworkMessageID : ushort
    {
        //masterserver
        PlayerList = 0,
        LobbyCreated = 1,
        LobbyStart = 2,
        LobbyList = 3,
        Refresh = 4,
        Chat =5,
    }


}