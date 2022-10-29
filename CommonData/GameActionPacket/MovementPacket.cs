using System.Collections.Generic;
using System.IO;

namespace CommonData
{
	public class MovementPacket : GameActionPacket
	{
		public MovementPacket(int id, List<Vector2Int> path, int movePointsUsed)
		{
			Path = path;
			MovePointsUsed = movePointsUsed;
			this.Type = ActionType.Move;
			ID = id;
			
		}

		public int MovePointsUsed { get; set; }
		public int ID { get; set; }
		public List<Vector2Int> Path { get; set; }
	}
}