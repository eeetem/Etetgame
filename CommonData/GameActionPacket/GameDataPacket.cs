using Network.Packets;

namespace CommonData
{
	
		public class GameDataPacket : Packet
		{
			public bool IsPlayerOne { get; set; }
			public bool IsPlayer1Turn{ get; set; }

		

		}
	
}