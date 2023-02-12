using Network.Packets;

namespace CommonData
{
	
		public class UnitStartDataPacket : Packet
		{
			public int Soldiers { get; set; }
			public int Scouts { get; set; }
			public int Heavies { get; set; }
		}
	
}