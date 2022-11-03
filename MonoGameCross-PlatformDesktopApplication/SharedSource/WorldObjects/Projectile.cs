using System;
using CommonData;

namespace MultiplayerXeno
{
	public class Projectile
	{
		private WorldManager.RayCastOutcome result;
		private int dmg;
		public Projectile(Vector2Int from, Vector2Int to, int dmg)
		{
			this.dmg = dmg;
			result = WorldManager.Instance.Raycast(from, to, Cover.Low);
		}

		public void Fire()
		{
			//play animation
			
			if (result.hit)
			{
				var tile = WorldManager.Instance.GetTileAtGrid(result.EndPoint);
				
				result.hitObj.TakeDamage(dmg);
			}
			else
			{
				Console.WriteLine("MISS");
				//nothing is hit
			}
			
			
		}

	}
}