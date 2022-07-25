using MultiplayerXeno.Prefabs;

namespace MultiplayerXeno
{
	public class Controllable : WorldObject
	{
		public Controllable(Vector2Int position, ControllableType type, int id, bool isPlayerOneTeam) : base(position, type, id)
		{
			IsPlayerOneTeam = isPlayerOneTeam;
		}

		public bool IsPlayerOneTeam { get; private set;}

		private bool hasMoved = false;

		public void StartTurn()
		{
			hasMoved = false;

		}

		public void Move(Vector2Int pos)
		{
			this.Position = pos;
			hasMoved = true;

		}

		public void EndTurn()
		{
			
			
		}

		

	}
}