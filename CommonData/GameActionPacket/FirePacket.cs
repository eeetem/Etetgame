using System.Collections.Generic;
using System.IO;

namespace CommonData
{
	public class FirePacket : GameActionPacket
	{
		public FirePacket(int id, Vector2Int target)
		{
			this.Type = ActionType.Attack;
			ID = id;
			this.Target = target;
		}
		
		public int ID { get; set; }
		public Vector2Int Target { get; set; }
	}
}