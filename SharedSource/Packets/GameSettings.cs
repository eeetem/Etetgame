using Network.Packets;

namespace MultiplayerXeno;

public class GameSettings : Packet
{
	public int TimeLimit { get; set; } = -1;
}