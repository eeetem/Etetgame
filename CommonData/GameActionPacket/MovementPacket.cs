using System.Collections.Generic;
using System.IO;

namespace CommonData
{
	public class MovementPacket : GameActionPacket
	{
		public MovementPacket(int id, List<Vector2Int> path)
		{
			Path = path;
			this.Type = ActionType.Move;
			ID = id;
		}

		public int ID { get; set; }
		public List<Vector2Int> Path { get; set; }
	}
}