namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int MoveRange = 4;
		public int SightRange = 10;


		public int MaxHealth = 5;
		public int MaxAwareness = 5;

		public Controllable Instantiate(WorldObject parent,bool team1)
		{
			Controllable obj = new Controllable(team1,parent,this);

			return obj;
		}
	}
}