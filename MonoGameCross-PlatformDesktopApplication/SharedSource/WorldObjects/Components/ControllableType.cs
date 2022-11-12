using CommonData;

namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int MoveRange = 4;
		public int SightRange = 8;


		public int MaxHealth = 5;
		public int MaxAwareness = 1;

		public Controllable Instantiate(WorldObject parent,ControllableData data)
		{
			
			Controllable obj = new Controllable(data.Team1,parent,this,data.MovePoints,data.TurnPoints,data.ActionPoints);
			obj.Health = data.Health;
			obj.Awareness = data.Awareness;

			return obj;
		}
	}
}