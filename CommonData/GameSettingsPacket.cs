using Network.Packets;

namespace CommonData;

public class GameSettingsPacket : Packet
{
	public string MapName { get; set; }
	public TimeLimitType TimeLimitType { get; set; }

	
}

public enum TimeLimitType
{
	None,
	PerTurn,
	Chess
}