using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno
{
	public class Projectile
	{
		public RayCastOutcome result { get; private set; }
		public RayCastOutcome? covercast { get; private set; }//tallest cover on the way
		public int dmg;
		public int originalDmg;
		public int dropoffRange;
		public Vector2[] dropOffPoints;
		public int awarenessResistanceCoefficient = 1;
		public int supressionRange;
		public int supressionStrenght;
		public Projectile(ProjectilePacket packet)
		{
			this.result = packet.result;
			this.covercast = packet.covercast;
			this.dmg = packet.dmg;
			this.originalDmg = packet.dmg;
			this.dropoffRange = packet.dropoffRange;
			this.awarenessResistanceCoefficient = packet.awarenessResistanceCoefficient;
			this.supressionRange = packet.suppresionRange;
			this.supressionStrenght = packet.supressionStrenght;
			CalculateDetails();
			Fire();
		}


		public Projectile(Vector2 from, Vector2 to, int dmg, int dropoffRange, bool lowShot = false, int awarenessResistanceCoefficient = 1, int supressionRange = 2,int supressionStrenght=1)
		{
			this.dmg = dmg;
			this.originalDmg = dmg;
			this.dropoffRange = dropoffRange;
			this.awarenessResistanceCoefficient = awarenessResistanceCoefficient;
			this.supressionRange = supressionRange;
			this.supressionStrenght = supressionStrenght;

			if (lowShot)
			{
				result = WorldManager.Instance.Raycast(from , to, Cover.High, false, Cover.Full);
			}
			else
			{
				result = WorldManager.Instance.Raycast(from , to, Cover.Full);
			}


			Vector2 dir = Vector2.Normalize(from - to);
			to = result.CollisionPoint+Vector2.Normalize(to-from);

			
			RayCastOutcome cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 2.5f, to, Cover.High, true);
				if (cast.hit && result.hitObjID != cast.hitObjID)
				{
					covercast = cast;
				}
				else
				{
					cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 2.5f, to, Cover.Low, true);
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
				if (covercast != null)
				{
					var hitobj = WorldManager.Instance.GetObject(result.hitObjID);
					Cover cover = WorldManager.Instance.GetObject(covercast.hitObjID).GetCover();
						if (hitobj?.ControllableComponent != null && hitobj.ControllableComponent.Crouching)
						{
							if (cover != Cover.Full)
							{ 
								cover++;
							}
						}

					switch (cover)
					{
						case Cover.Full:
							dmg -= 10;
							break;
						case Cover.High:
							dmg-=4;
							break;
						case Cover.Low:
							dmg-=2;
							break;
						case Cover.None:
							Console.Write("coverless object hit, this shouldnt happen");
							//this shouldnt happen
							break;
					}
				}

				
				
				WorldManager.Instance.GetObject(result.hitObjID).TakeDamage(this);
			}
			else
			{
				Console.WriteLine("MISS");
				//nothing is hit
			}
			List<WorldTile> tiles = WorldManager.Instance.GetTilesAround(result.EndPoint,supressionRange);
			foreach (var tile in tiles)
			{
				if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null)
				{
					tile.ObjectAtLocation.ControllableComponent.Awareness -= supressionStrenght;
					if (tile.ObjectAtLocation.ControllableComponent.Awareness <= 0)
					{
						tile.ObjectAtLocation.ControllableComponent.Panic();
					}
				}
			}
			
		}

	}
}