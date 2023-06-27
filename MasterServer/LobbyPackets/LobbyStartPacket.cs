namespace MultiplayerXeno;

public class LobbyStartPacket : Packet
{
	public string LobbyName { get; set; } = "";
	public string Password { get; set; }= "";
}