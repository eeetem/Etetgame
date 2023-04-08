﻿using System;
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
		public int determinationResistanceCoefficient = 1;
		public int supressionRange;
		public int supressionStrenght;
		public bool shooterLow;
		public bool targetLow;
		public Projectile(ProjectilePacket packet)
		{
			this.result = packet.result;
			this.covercast = packet.covercast;
			this.dmg = packet.dmg;
			this.originalDmg = packet.dmg;
			this.dropoffRange = packet.dropoffRange;
			this.determinationResistanceCoefficient = packet.determinationResistanceCoefficient;
			this.supressionRange = packet.suppresionRange;
			this.supressionStrenght = packet.supressionStrenght;
			this.shooterLow = packet.shooterLow;
			this.targetLow = packet.targetLow;
			CalculateDetails();
			Fire();
		}

	
		public Projectile(Vector2 from, Vector2 to, int dmg, int dropoffRange, bool targetLow = false, bool shooterLow = false, int determinationResistanceCoefficient = 1, int supressionRange = 2,int supressionStrenght=1)
		{
			this.dmg = dmg;
			this.originalDmg = dmg;
			this.dropoffRange = dropoffRange;
			this.determinationResistanceCoefficient = determinationResistanceCoefficient;
			this.supressionRange = supressionRange;
			this.supressionStrenght = supressionStrenght;
			this.shooterLow = shooterLow;
			this.targetLow = targetLow;
				
		/*	if (Vector2.Distance(from, to) <= 1.5)
			{
				result = WorldManager.Instance.Raycast(from , to, Cover.High,false,Cover.Full);

				Vector2 dir = Vector2.Normalize(to - from);
					to = result.CollisionPointLong + Vector2.Normalize(to - from);
					RayCastOutcome cast;
					cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 2f, to, Cover.Full, true); //ignore cover pointblank
					if (cast.hit && result.hitObjID != cast.hitObjID)
					{
						covercast = cast;
					}
				
			}
			else
			{*/
		if (shooterLow)
		{
			result = WorldManager.Instance.Raycast(from, to, Cover.High, false, true,Cover.High);
		}
		else if (targetLow)
		{
			result = WorldManager.Instance.Raycast(from, to, Cover.High, false, true,Cover.Full);
		}
		else
		{
			result = WorldManager.Instance.Raycast(from, to, Cover.Full);
		}

		if (!result.hit) {
			var tile = WorldManager.Instance.GetTileAtGrid(to);
			var obj = tile.ObjectAtLocation;
			if (obj != null) {
#if CLIENT
				if (obj.IsVisible()) {
#endif
					var controllable = obj.ControllableComponent;
					if (controllable != null && controllable.Crouching && this.targetLow == false) {
						// Do nothing if targetLow is false
					} else {
						result = new RayCastOutcome(from, to) {
							hit = true,
							hitObjID = obj.Id,
							CollisionPointLong = to,
						};
					}
#if CLIENT
				}
#endif
			}
		}

		if (result.hit)
		{

			Vector2 dir = Vector2.Normalize(from - to);
			to = result.CollisionPointLong + Vector2.Normalize(to - from)/5f;
			RayCastOutcome cast;


			cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 1.25f, to, Cover.High, true);
			if (cast.hit && result.hitObjID != cast.hitObjID)
			{
				covercast = cast;
			}
			else
			{
				cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 1.25f, to, Cover.Low, true);
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
			

			CalculateDetails();
		}

		public void CalculateDetails()
		{
			float range = Math.Min( Vector2.Distance(result.StartPoint, result.CollisionPointLong), Vector2.Distance(result.StartPoint, result.EndPoint));
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
					dmg=(int)Math.Ceiling(dmg/1.8f);
				}

				dropOffPoints[i] = result.StartPoint + (Vector2.Normalize(result.EndPoint - result.StartPoint)* dropoffRange *(i+1));
			}
		}

		public List<WorldTile> SupressedTiles()
		{
			var pos = new Vector2Int((int) result.CollisionPointLong.X, (int) result.CollisionPointLong.Y);
			var worldTile = WorldManager.Instance.GetTileAtGrid(pos);
			if (result.CollisionPointLong != result.EndPoint)
			{
				try
				{
					var dir = Utility.GetDirectionToSideWithPoint(pos, result.CollisionPointLong);
				

					Cover passCover = Cover.High;
					if (this.shooterLow)
					{
						passCover = Cover.Low;
					}

					if (worldTile.GetCover(dir,true)>passCover)
					{
						pos = new Vector2Int((int) result.CollisionPointShort.X, (int) result.CollisionPointShort.Y);
		
					}
				}
				catch (Exception e)
				{
					pos = new Vector2Int((int) result.CollisionPointShort.X, (int) result.CollisionPointShort.Y);
				}
			}
			var tiles = WorldManager.Instance.GetTilesAround(pos,supressionRange,Cover.High);
			return tiles;
		}

		public void Fire()
		{
			//play animation
			
			if (result.hit)
			{
				if (covercast != null)
				{
					var hitobj = WorldManager.Instance.GetObject(result.hitObjID);
					Cover cover = WorldManager.Instance.GetObject(covercast.hitObjID)!.GetCover();
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

				
				
				WorldManager.Instance.GetObject(result.hitObjID)?.TakeDamage(this);
			
			}
			else
			{
				Console.WriteLine("MISS");
				//nothing is hit
			}

			Console.WriteLine("starting to supress");
			List<WorldTile> tiles = SupressedTiles();
			Console.WriteLine("checking " + tiles.Count + " tiles");
			foreach (var tile in tiles)
			{
				if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null)
				{
					tile.ObjectAtLocation.ControllableComponent.Suppress(supressionStrenght, true);
					Console.WriteLine("supressed: determination="+tile.ObjectAtLocation.ControllableComponent.Determination);
				}
			}
			
		}

	}
}