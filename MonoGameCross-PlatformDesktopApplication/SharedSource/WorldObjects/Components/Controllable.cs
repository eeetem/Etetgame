#nullable enable
using MultiplayerXeno.Structs;

namespace MultiplayerXeno
{
	public  class Controllable
	{
		private WorldObject Parent;
		private ControllableType Type;
		public Controllable(bool isPlayerOneTeam, WorldObject parent, ControllableType type)
		{
			Parent = parent;
			Type = type;
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
			Parent.Position = pos;
			hasMoved = true;

		}

		public void EndTurn()
		{
			
			
		}
		public void Update()
		{
			
		}
	}
}