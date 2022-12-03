using CommonData;

namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int MoveRange = 4;
		public int SightRange = 16;
		
		public int MaxMovePoints = 2;
		

		public int WeaponDmg = 4;
		public int WeaponRange = 10;
		
		public int MaxHealth = 10;
		public int MaxAwareness = 1;

		public bool RunAndGun = false;
		public Controllable Instantiate(WorldObject parent,ControllableData data)
		{
			
			Controllable obj = new Controllable(data.Team1,parent,this,data);


			return obj;
		}
	}
}