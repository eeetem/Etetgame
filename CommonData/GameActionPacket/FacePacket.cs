using System.Collections.Generic;
using System.IO;

namespace CommonData
{
	public class FacePacket : GameActionPacket
	{
		public FacePacket(int id, Direction dir)
		{
			this.Type = ActionType.Turn;
			ID = id;
			Dir =dir;

		}
		
		public int ID { get; set; }
		public Direction Dir { get; set; }
	}
}