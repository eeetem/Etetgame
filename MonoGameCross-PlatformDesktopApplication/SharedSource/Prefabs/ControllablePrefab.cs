namespace MultiplayerXeno.Prefabs
{
	public class ControllableType : WorldObjectType
	{
		public Controllable InitialisePrefab(int id, Vector2Int position, WorldObject.Direction facing, bool team1)
		{
			Controllable obj = new Controllable(position,this,id,team1);
		}
		
		
		public int moveRange;
	}
}