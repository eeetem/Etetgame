using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DefconNull.World.WorldObjects;

public class Projectile
{
	public RayCastOutcome Result { get; private set; }
	public RayCastOutcome? CoverCast { get; private set; }//tallest cover on the way
	public int Dmg;
	public readonly int OriginalDmg;
	public readonly int DropoffRange;
	public Vector2[] DropOffPoints = null!;
	public readonly int DeterminationResistanceCoefficient = 1;
	public readonly int SupressionRange;
	public readonly int SupressionStrenght;
	public bool ShooterLow;
	public bool TargetLow;
	public List<int> SupressionIgnores = new List<int>();


	
	public Projectile(Vector2 from, Vector2 to, int dmg, int dropoffRange, bool targetLow = false, bool shooterLow = false, int determinationResistanceCoefficient = 1, int supressionRange = 2,int supressionStrenght=1, bool clientPreview = false)
	{
		Dmg = dmg;
		OriginalDmg = dmg;
		DropoffRange = dropoffRange;
		DeterminationResistanceCoefficient = determinationResistanceCoefficient;
		SupressionRange = supressionRange;
		SupressionStrenght = supressionStrenght;
		ShooterLow = shooterLow;
		TargetLow = targetLow;

		if (shooterLow)
		{
			Result = WorldManager.Instance.Raycast(from, to, Cover.High, false,false,Cover.High);
		}
		else if (targetLow)
		{
			Result = WorldManager.Instance.Raycast(from, to, Cover.High, false,false,Cover.Full);
		}
		else
		{
			Result = WorldManager.Instance.Raycast(from, to, Cover.Full,false);
		}

		if (!Result.hit) {
			var tile = WorldManager.Instance.GetTileAtGrid(to);
			var obj = tile.UnitAtLocation;
			if (obj != null && (!clientPreview || obj.WorldObject.IsVisible())) {
				var controllable = obj;
				if (controllable.Crouching && TargetLow == false) {
					// Do nothing if targetLow is false
				} else {
					Result = new RayCastOutcome(from, to) {
						hit = true,
						HitObjId = obj.WorldObject.ID,
						CollisionPointLong = to,
					};
				}

				
			}
		}

	
		if (Result.hit)
		{

			Vector2 dir = Vector2.Normalize(from - to);
			to = Result.CollisionPointLong + Vector2.Normalize(to - from)/5f;
			RayCastOutcome cast;

			cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 1.4f, to, Cover.High, false,true);
			if (cast.hit && Result.HitObjId != cast.HitObjId)
			{
				CoverCast = cast;
			}
			else
			{
				cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 1.4f, to, Cover.Low, false,true);
				if (cast.hit && Result.HitObjId != cast.HitObjId)
				{
					CoverCast = cast;
				}
				else
				{
					CoverCast = null;
				}
			}



		}
			

		CalculateDetails();
	}

	public void CalculateDetails()
	{
		float range = Math.Min( Vector2.Distance(Result.StartPoint, Result.CollisionPointLong), Vector2.Distance(Result.StartPoint, Result.EndPoint));
		int dropOffs = 0;
		while (range > DropoffRange)
		{
			range -= DropoffRange;
			dropOffs++;
		}
			
		DropOffPoints = new Vector2[dropOffs+1];
		for (int i = 0; i < dropOffs+1; i++)
		{
			if (i != 0)
			{
				Dmg=(int)Math.Ceiling(Dmg/1.8f);
			}

			DropOffPoints[i] = Result.StartPoint + Vector2.Normalize(Result.EndPoint - Result.StartPoint)* DropoffRange *(i+1);
		}
	}

	public List<WorldTile> SupressedTiles()
	{
		var pos = new Vector2Int((int) Result.CollisionPointLong.X, (int) Result.CollisionPointLong.Y);
		var worldTile = WorldManager.Instance.GetTileAtGrid(pos);
		if (Result.CollisionPointLong != Result.EndPoint)
		{
			try
			{
				var dir = Utility.GetDirectionToSideWithPoint(pos, Result.CollisionPointLong);
				

				Cover passCover = Cover.High;
				if (ShooterLow)
				{
					passCover = Cover.Low;
				}

				if (worldTile.GetCover(dir,true)>passCover)
				{
					pos = new Vector2Int((int) Result.CollisionPointShort.X, (int) Result.CollisionPointShort.Y);
		
				}
			}
			catch (Exception)
			{
				pos = new Vector2Int((int) Result.CollisionPointShort.X, (int) Result.CollisionPointShort.Y);
			}
		}
		var tiles = WorldManager.Instance.GetTilesAround(pos,SupressionRange,Cover.High);
		return tiles;
	}

	public void Fire()
	{
		
		if (Result.hit)
		{
			var hitObj = WorldManager.Instance.GetObject(Result.HitObjId);
			if (hitObj != null)
			{
							

				if (CoverCast != null)
				{
					var coverObj = WorldManager.Instance.GetObject(CoverCast.HitObjId);
					Cover cover = coverObj!.GetCover();
					if (coverObj?.UnitComponent != null && coverObj.UnitComponent.Crouching)
					{
						if (cover != Cover.Full)
						{ 
							cover++;
						}
					}

					int coverBlock = 0;
					switch (cover)
					{
						case Cover.Full:
							coverBlock = 20;
							break;
						case Cover.High:
							coverBlock =4;
							break;
						case Cover.Low:
							coverBlock =2;
							break;
						case Cover.None:
							Console.Write("coverless object hit, this shouldnt happen");
							//this shouldnt happen
							break;
					}
					if(Dmg>coverBlock)
					{
						Dmg -= coverBlock;
					}
					else
					{
						coverBlock = Dmg;
						Dmg = 0;
					}
					coverObj!.TakeDamage(coverBlock,0);
				}

				
				
				hitObj.TakeDamage(this);
				if (!hitObj.destroyed && hitObj.UnitComponent is not null)
				{
					hitObj.Face( Utility.GetDirection(hitObj.TileLocation.Position, Result.StartPoint));
				}

			}
			else
			{
				Console.WriteLine("hitobj is null");
			}
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
			if (tile.UnitAtLocation != null && !SupressionIgnores.Contains(tile.UnitAtLocation.WorldObject.ID))
			{
				tile.UnitAtLocation.Suppress(SupressionStrenght);
				Console.WriteLine("supressed: determination="+tile.UnitAtLocation.Determination);
			}
	
		}
			
	}

}