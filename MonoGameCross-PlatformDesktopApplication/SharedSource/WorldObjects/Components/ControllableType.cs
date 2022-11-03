using CommonData;

namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int MoveRange = 4;
		public int SightRange = 10;


		public int MaxHealth = 5;
		public int MaxAwareness = 2;

		public Controllable Instantiate(WorldObject parent,ControllableData data)
		{
			
			Controllable obj = new Controllable(data.Team1,parent,this,data.MovePoints,data.TurnPoints,data.ActionPoints);

			return obj;
		}
	}
}