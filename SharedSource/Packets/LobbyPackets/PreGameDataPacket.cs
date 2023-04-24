using Network.Packets;

namespace MultiplayerXeno;

public class PreGameDataPacket : Packet
{
	public List<string> MapList { get; set; }
	public List<string> CustomMapList { get; set; }
	public string HostName { get; set; }
	public string Player2Name { get; set; }
	public List<string> Spectators { get; set; } = new List<string>();
	public string SelectedMap { get; set; }
	public int TurnTime { get; set; }

}
