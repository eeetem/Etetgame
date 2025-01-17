﻿using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Riptide;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.WorldActions;

public class Shootable : Effect
{
	
	readonly int preDropOffDmg;
	readonly int detResistance;
	readonly ushort supressionStrenght;
	readonly int supressionRange;
	readonly int dropOffRange;
	readonly int shotCount;
	readonly int shotDelay;
	readonly float shotSpread;
	private readonly string sound;
	


	public Shootable(int dmg, int detRes, ushort supression, int suppresionRange, int dropoff, int shotCount, int shotDelay, float shotSpread, string sound)
	{
		preDropOffDmg = dmg;
		detResistance = detRes;
		supressionStrenght = supression;
		supressionRange = suppresionRange;
		dropOffRange = dropoff;
		this.shotCount = shotCount;
		this.shotDelay = shotDelay;
		this.shotSpread = shotSpread;
		this.sound = sound;
		
	}


	public override float GetOptimalRangeAI()
	{
		return dropOffRange + supressionRange;
	}

	public struct Projectile : IMessageSerializable
	{
		public WorldManager.RayCastOutcome Result { get; set; }
		public WorldManager.RayCastOutcome? CoverCast { get; set; } = null; //tallest cover on the way
		public int Dmg = 0;
		public Vector2[] DropOffPoints = new Vector2[0];//NOT SERIALISED WARNING
		public readonly List<int> SuppressionIgnores = new List<int>();
		public bool ShooterLow;
		public bool TargetLow;

		public bool Equals(Projectile other)
		{
			return Dmg == other.Dmg && DropOffPoints.SequenceEqual(other.DropOffPoints) && SuppressionIgnores.SequenceEqual(other.SuppressionIgnores) && ShooterLow == other.ShooterLow && TargetLow == other.TargetLow && Result.Equals(other.Result) && Nullable.Equals(CoverCast, other.CoverCast);
		}

		public override bool Equals(object? obj)
		{
			return obj is Projectile other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Dmg;
				hashCode = (hashCode * 397) ^ DropOffPoints.GetHashCode();
				hashCode = (hashCode * 397) ^ SuppressionIgnores.GetHashCode();
				hashCode = (hashCode * 397) ^ ShooterLow.GetHashCode();
				hashCode = (hashCode * 397) ^ TargetLow.GetHashCode();
				hashCode = (hashCode * 397) ^ Result.GetHashCode();
				hashCode = (hashCode * 397) ^ CoverCast.GetHashCode();
				return hashCode;
			}
		}

		public Projectile(WorldManager.RayCastOutcome result, WorldManager.RayCastOutcome? coverCast)
		{
			Result = result;
			CoverCast = coverCast;
			SuppressionIgnores = new List<int>();
		}

		public Projectile()
		{
			SuppressionIgnores = new List<int>();
		}
		public void Serialize(Message message)
		{
			message.Add(Result);
			message.Add(Dmg);
			message.AddInts(SuppressionIgnores.ToArray());
			message.Add(ShooterLow);
			message.Add(TargetLow);
		}

		public void Deserialize(Message message)
		{
			Result = message.GetSerializable<WorldManager.RayCastOutcome>();
			Dmg = message.GetInt();
			SuppressionIgnores.AddRange(message.GetInts());
			ShooterLow = message.GetBool();
			TargetLow = message.GetBool();
		}
	}

	public Projectile GenerateProjectile(Unit actor, WorldObject targetObj, int dimension)
	{

		Vector2Int target = targetObj.TileLocation.Position;
		int targetId = targetObj.ID;
		
		
		bool shooterLow = actor.Crouching;
		Vector2 shotDir = Vector2.Normalize(target - actor.WorldObject.TileLocation.Position);
		Vector2 from = actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f) ;
		
		bool targetLow = false;//move this outside

		if(targetObj.UnitComponent!= null && targetObj.UnitComponent.Crouching )
		{
			targetLow = true;
		}
		else
		{
			targetLow = false;
		}
		
		
		WorldManager.RayCastOutcome? result = null;
		Vector2 baseShot = target + new Vector2(0.5f, 0.5f);
		List<Vector2> potentialTargets = new List<Vector2>(){};
		if (targetObj.Type.Edge)
		{
			switch (targetObj.Facing)
			{
				case Direction.North:
					baseShot = target + new Vector2(0.5f, 0.00f);
					break;
				case Direction.West:
					baseShot = target + new Vector2(0.00f, 0.5f);
					break;
			}
			potentialTargets.Add(baseShot);
		}
		else
		{
			potentialTargets.Add(baseShot);
			potentialTargets.Add(target + new Vector2(0.25f, 0.25f));
			potentialTargets.Add(target + new Vector2(0.25f, 0.75f));
			potentialTargets.Add(target + new Vector2(0.75f, 0.25f));
			potentialTargets.Add(target + new Vector2(0.75f, 0.75f));
			potentialTargets.Add(baseShot);
		}

		//we insert target + new Vector2(0.5f, 0.5f) twice since it's the fisrt thing we should try but also last thing to return if we dont hit anything
		
		int j = 0;
		Vector2 to = default;
		while (!result.HasValue || !result.Value.hit || result.Value.HitObjId != targetId)
		{
			if(j>=potentialTargets.Count) break;
			to = potentialTargets[j];
			j++;
			
			if (shooterLow)
			{
				//we are crouched so we hit High cover at all distances
				result = WorldManager.Instance.Raycast(from, to, Cover.High, false,false,Cover.High, pseudoLayer:dimension);
			}
			else if (targetLow)
			{
				//we are standing, the target is crouched, point blank we are blocked only by full walls while the rest of the way we'll hit high cover
				result = WorldManager.Instance.Raycast(from, to, Cover.High, false,false,Cover.Full,pseudoLayer:dimension);
			}
			else
			{
				//we both are standing, only full blocks
				result = WorldManager.Instance.Raycast(from, to, Cover.Full,false,pseudoLayer:dimension);
			}

			

		
		}
		WorldManager.RayCastOutcome? coverCast = null;
		if (result.Value.hit && Vector2.Distance(result.Value.StartPoint, result.Value.CollisionPointLong) > 1.6f)
		{
			Vector2 dir = Vector2.Normalize(from - to);
			to = result.Value.CollisionPointLong;
			WorldManager.RayCastOutcome cast;

			cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 0.5f, to, Cover.High, false, true, pseudoLayer: dimension);
			if (cast.hit && result.Value.HitObjId != cast.HitObjId)
			{
				coverCast = cast;
			}
			else
			{
				cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir)  * 0.5f, to, Cover.Low, false, true, pseudoLayer: dimension);
				if (cast.hit && result.Value.HitObjId != cast.HitObjId)
				{
					coverCast = cast;
				}
				else
				{
					coverCast = null;
				}
			}


		}

		Projectile p = new Projectile(result.Value,coverCast);
		p.TargetLow = targetLow;
		p.SuppressionIgnores.Add(actor.WorldObject.ID);

		float range = Math.Min( Vector2.Distance(p.Result.StartPoint, p.Result.CollisionPointLong), Vector2.Distance(p.Result.StartPoint, p.Result.EndPoint));
		int dropOffs = 0;
		while (range > dropOffRange)
		{
			range -= dropOffRange;
			dropOffs++;
		}
			
		p.DropOffPoints = new Vector2[dropOffs+1];
		p.Dmg = preDropOffDmg;
		for (int i = 0; i < dropOffs+1; i++)
		{
			if (i != 0)
			{
				p.Dmg=(int)Math.Ceiling(p.Dmg/1.8f);
			}

			p.DropOffPoints[i] = p.Result.StartPoint + Vector2.Normalize(p.Result.EndPoint - p.Result.StartPoint)* dropOffRange *(i+1);
		}

		return p;
	}
	
	protected override Tuple<bool, string>  CanPerformChild(Unit actor, WorldObject target, int dimension = -1)
	{
		if(Equals(actor.WorldObject, target))
		{
			return new Tuple<bool, string> (false,"You can't shoot yourself!");
		}
		return new Tuple<bool, string> (true,"");
	}
	protected override Tuple<bool, string> IsRecommendedToPerformChild(Unit actor, WorldObject target, int dimension = -1)
	{
		var canPerform = CanPerformChild(actor, target, dimension);
		if (!canPerform.Item1)
		{
			return canPerform;
		}
		var p = GenerateProjectile(actor, target,dimension);
		if (p.Result.hit)
		{
			var hitobj = PseudoWorldManager.GetObject(p.Result.HitObjId,dimension);
			if (hitobj == null || hitobj!.Type.Edge || hitobj.TileLocation.Position != (Vector2Int)p.Result.EndPoint)
			{
				return new Tuple<bool, string> (false,"Can't hit target");
			}
		}
		return new Tuple<bool, string> (true,"");
	}

	protected override List<SequenceAction> GetConsequencesChild(Unit actor, WorldObject target,int dimension = -1)
	{
		if (actor.WorldObject.TileLocation.Position == target.TileLocation.Position) return new List<SequenceAction>();
		var p = GenerateProjectile(actor, target,dimension);
		var retrunList = new List<SequenceAction>();
		var m = MoveCamera.Make(actor.WorldObject.TileLocation.Position, true, 3);
		retrunList.Add(m);
		m = MoveCamera.Make(p.Result.CollisionPointLong, true, 3);
		retrunList.Add(m);


		int rangeBlock = preDropOffDmg - p.Dmg;
		int coverBlock = 0;
		if (p.CoverCast.HasValue)
		{
			var coverObj =PseudoWorldManager.GetObject(p.CoverCast.Value.HitObjId,dimension);
			var hitObj =PseudoWorldManager.GetObject(p.Result.HitObjId,dimension);
			Cover cover = coverObj!.GetCover();
			if (hitObj?.UnitComponent != null && hitObj.UnitComponent.Crouching)
			{
				if (cover != Cover.Full)
				{
					cover++;
				}
			}

			
			switch (cover)
			{
				case Cover.Full:
					coverBlock = 20;
					break;
				case Cover.High:
					coverBlock = 4;
					break;
				case Cover.Low:
					coverBlock = 2;
					break;
				case Cover.None:
					Console.Write("coverless object hit, this shouldnt happen");
					//this shouldnt happen
					break;
			}

			if (p.Dmg > coverBlock)
			{
				p.Dmg -= coverBlock;
			}
			else
			{
				coverBlock = p.Dmg;
				p.Dmg = 0;
			}

			var act = WorldObjectManager.TakeDamage.Make(coverBlock, 0, coverObj!.ID);
			retrunList.Add(act);
		}

		
		
		var shoot = Shoot.Make(p, preDropOffDmg, coverBlock, rangeBlock, shotCount, shotDelay, shotSpread, sound);
		retrunList.Add(shoot);
		if (p.Result.hit)
		{
			var hitObj =PseudoWorldManager.GetObject(p.Result.HitObjId,dimension);
			if (hitObj != null)
			{

				if (hitObj.UnitComponent is not null)
				{
					var lookAtAttacker = FaceUnit.Make(hitObj.ID, p.Result.StartPoint,false);
					retrunList.Add(lookAtAttacker);
				}
				var act2 = WorldObjectManager.TakeDamage.Make(p.Dmg, detResistance, hitObj.ID);
				retrunList.Add(act2);

			}
		}

		List<IWorldTile> tiles = SupressedTiles(p,dimension);
		retrunList.EnsureCapacity(retrunList.Count+tiles.Count);
		foreach (var tile in tiles)
		{
			if(tile.UnitAtLocation != null && p.SuppressionIgnores.Contains(tile.UnitAtLocation.WorldObject.ID))
			{
				continue;
			}
			var act2 = Suppress.Make(supressionStrenght, tile.Position);
			retrunList.Add(act2);
	
		}

		return retrunList;
		
	}
	public List<IWorldTile> SupressedTiles(Projectile p, int dimension = -1)
	{
		var pos = new Vector2Int((int) p.Result.CollisionPointLong.X, (int) p.Result.CollisionPointLong.Y);
	
		if (p.Result.CollisionPointLong != p.Result.EndPoint)
		{
			var dir = Utility.GetDirectionToSideWithPoint(pos, p.Result.CollisionPointLong);
			
			Cover passCover = Cover.High;//i dont remember why this is here
			if (p.ShooterLow)
			{
				passCover = Cover.Low;
			}
			
			if (WorldManager.Instance.GetCover(pos,dir,false,true)>passCover)
			{
				pos = new Vector2Int((int) p.Result.CollisionPointShort.X, (int) p.Result.CollisionPointShort.Y);
		
			}
			
		}
		var tiles = WorldManager.Instance.GetTilesAround(pos,supressionRange,dimension,Cover.High);
		return tiles;
	}

	
}