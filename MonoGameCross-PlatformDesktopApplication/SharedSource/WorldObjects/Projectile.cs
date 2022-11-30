using System;
using CommonData;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno
{
	public class Projectile
	{
		public RayCastOutcome result { get; private set; }
		public RayCastOutcome? covercast { get; private set; }//tallest cover on the way
		public int dmg;
		public int dropoffRange;
		public Vector2[] dropOffPoints;

		public Projectile(ProjectilePacket packet)
		{
			this.result = packet.result;
			this.covercast = packet.covercast;
			this.dmg = packet.dmg;
			this.dropoffRange = packet.dropoffRange;
			CalculateDetails();
			Fire();
		}

		
		public Projectile(Vector2 from, Vector2 to, int dmg,int dropoffRange)
		{
			this.dmg = dmg;
			this.dropoffRange = dropoffRange;

			result = WorldManager.Instance.Raycast(from , to, Cover.Full);
			
			var cast = WorldManager.Instance.Raycast(from, to, Cover.High,true,true);
			if (cast.hit && result.hitObjID != cast.hitObjID)
			{
				covercast = cast;
			}
			else
			{
				cast = WorldManager.Instance.Raycast(from, to, Cover.Low,true,true);
				if (cast.hit && result.hitObjID != cast.hitObjID)
				{
					covercast = cast;
				}
				else
				{
					covercast = null;
				}
			}
	
			
			CalculateDetails();
		}

		public void CalculateDetails()
		{
			float range = Math.Min( Vector2.Distance(result.StartPoint, result.CollisionPoint), Vector2.Distance(result.StartPoint, result.EndPoint));
			int dropOffs = 0;
			while (range > dropoffRange)
			{
				range -= dropoffRange;
				dropOffs++;
			}
			
			dropOffPoints = new Vector2[dropOffs+1];
			for (int i = 0; i < dropOffs+1; i++)
			{
				if (i != 0)
				{
					dmg=(int)Math.Ceiling(dmg/2f);
				}

				dropOffPoints[i] = result.StartPoint + (Vector2.Normalize(result.EndPoint - result.StartPoint)* dropoffRange *(i+1));
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