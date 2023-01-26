using Network.Packets;

namespace CommonData;

public class LobbyStartPacket : Packet
{
	public string LobbyName { get; set; }
	public string Password { get; set; }
}