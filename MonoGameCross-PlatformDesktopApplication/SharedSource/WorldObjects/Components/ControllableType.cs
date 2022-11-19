using CommonData;

namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int MoveRange = 4;
		public int SightRange = 9;


		public int MaxHealth = 5;
		public int MaxAwareness = 1;

		public Controllable Instantiate(WorldObject parent,ControllableData data)
		{
			
			Controllable obj = new Controllable(data.Team1,parent,this,data);


			return obj;
		}
	}
}