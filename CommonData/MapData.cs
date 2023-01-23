using Network.Packets;

namespace CommonData;

public class MapData : Packet
{
	public string Name { get; set; }
	//todo unit count
	public byte[] ByteData { get; set; }
	
}