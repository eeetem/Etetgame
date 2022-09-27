namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int moveRange = 4;
		public int sightRange = 10;

		public Controllable Instantiate(WorldObject parent,bool team1)
		{
			Controllable obj = new Controllable(team1,parent,this);

			return obj;
		}
	}
}