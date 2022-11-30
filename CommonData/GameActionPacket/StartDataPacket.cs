using Network.Packets;

namespace CommonData
{
	
		public class StartDataPacket : Packet
		{
			public int Soldiers { get; set; }
			public int Scouts { get; set; }
		}
	
}