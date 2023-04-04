using Network.Packets;

namespace CommonData;

public class GameSettings : Packet
{
	public int TimeLimit { get; set; } = -1;
}