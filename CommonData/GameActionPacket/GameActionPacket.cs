using System;
using Network.Packets;

namespace CommonData
{
	public class GameActionPacket : Packet
	{
		public ActionType Type { get; set; }
		public int ID { get; set; }

		public GameActionPacket()
		{
			
		}

		public override void BeforeReceive()
		{
		
		}

		public override void BeforeSend()
		{

		}
		
	}

	public enum ActionType
	{
		EndTurn=0,
		Attack=1,
		Move=2,
		Turn=3,
		Crouch=4,
	}





}