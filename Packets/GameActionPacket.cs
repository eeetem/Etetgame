using System;
using Network.Packets;

namespace Packets
{
	public class GameActionPacket : Packet
	{
		public ActionType Type { get; set; }
		
		
		public override void BeforeReceive()
		{
		
		}

		public override void BeforeSend()
		{

		}
	}

	public enum ActionType
	{
		Move=0,
		Attack=1,
		EndTurn=2
	}
	
	
	
}