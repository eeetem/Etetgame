using Network.Packets;

namespace CommonData;

public class PreGameDataPacket : Packet
{
	public List<string> MapList { get; set; }
	public string HostName { get; set; }
	public string Player2Name { get; set; }
	public List<string> Spectators { get; set; }
	public int SelectedIndex { get; set; }

}
