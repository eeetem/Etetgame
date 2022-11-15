using System;
using CommonData;

namespace MultiplayerXeno
{
	public class Projectile
	{
		public RayCastOutcome result { get; private set; }
		public RayCastOutcome? covercast { get; private set; }//tallest cover on the way
		public int dmg;

		public Projectile(ProjectilePacket packet)
		{
			this.result = packet.result;
			this.covercast = packet.covercast;
			this.dmg = packet.dmg;
			Fire();
		}

		public Projectile(Vector2Int from, Vector2Int to, int dmg)
		{
			this.dmg = dmg;

			result = WorldManager.Instance.Raycast(from, to, Cover.Full);
			
			var cast = WorldManager.Instance.Raycast(from, to, Cover.High);
			if (cast.hit && result.hitObjID != cast.hitObjID)
			{
				covercast = cast;
			}
			else
			{
				cast = WorldManager.Instance.Raycast(from, to, Cover.Low);
				if (cast.hit && result.hitObjID != cast.hitObjID)
				{
					covercast = cast;
				}
				else
				{
					covercast = null;
				}
			}
		}

		public void Fire()
		{
			//play animation
			
			if (result.hit)
			{
				int finalDmg = dmg;
				if (covercast != null)
				{
					switch (WorldManager.Instance.GetObject(covercast.hitObjID).GetCover())
					{
						case Cover.Full:
							return;
						case Cover.High:
							finalDmg-=2;
							break;
						case Cover.Low:
							finalDmg-=1;
							break;
						case Cover.None:
							Console.Write("coverless object hit, this shouldnt happen");
							//this shouldnt happen
							break;
					}
				}
				

				WorldManager.Instance.GetObject(result.hitObjID).TakeDamage(finalDmg);
			}
			else
			{
				Console.WriteLine("MISS");
				//nothing is hit
			}
			
			
		}

	}
}