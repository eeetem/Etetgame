using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.ReplaySequence;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.World.WorldActions;

public class Shootable : Effect
{
	
	public enum TargetingType
	{
		Auto,
		High,
		Low
	}
	readonly int preDropOffDmg;
	readonly int detResistance;
	readonly int supressionStrenght;
	readonly int supressionRange;
	readonly int dropOffRange;
	
	public static TargetingType targeting = TargetingType.Auto;
	public Shootable(int preDropOffDmg, int detResistance, int supressionStrenght, int supressionRange, int dropOffRange)
	{
		this.preDropOffDmg = preDropOffDmg;
		this.detResistance = detResistance;
		this.supressionStrenght = supressionStrenght;
		this.supressionRange = supressionRange;
		this.dropOffRange = dropOffRange;
	}

    


	public override float GetOptimalRangeAI()
	{
		return dropOffRange + supressionRange;
	}

	public struct Projectile
	{
		public WorldManager.RayCastOutcome Result { get; set; }
		public WorldManager.RayCastOutcome? CoverCast { get; set; } = null; //tallest cover on the way
		public int Dmg = 0;
		public Vector2[] DropOffPoints = new Vector2[0];
		public readonly List<int> SupressionIgnores = new List<int>();
		public bool shooterLow;
		public bool targetLow;

		public Projectile(WorldManager.RayCastOutcome result, WorldManager.RayCastOutcome? coverCast)
		{
			Result = result;
			CoverCast = coverCast;
		}
	}

	public Projectile GenerateProjectile(Unit actor, Vector2Int target, int dimension)
	{
		
		bool shooterLow = actor.Crouching;
		Vector2 shotDir = Vector2.Normalize(target -actor.WorldObject.TileLocation.Position);
		Vector2 from = actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f) + shotDir / new Vector2(2.5f, 2.5f);
		Vector2 to = target + new Vector2(0.5f, 0.5f);
		bool targetLow = false;//move this outside
		var targetTile = WorldManager.Instance.GetTileAtGrid(target, dimension);
		if(targetTile.UnitAtLocation!=null&&targetTile.UnitAtLocation.Crouching)
		{
			targetLow = true;
		}
		else
		{
			targetLow = false;
		}

		WorldManager.RayCastOutcome result;
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

		//if we reached the end tile but didnt hit anything, autolock onto the unit on the tile
		if (!result.hit) {
			var tile = WorldManager.Instance.GetTileAtGrid(to,dimension);
			var obj = tile.UnitAtLocation;
			if (obj != null) {
				var controllable = obj;
				if (controllable.Crouching && targetLow == false) {
					// Do nothing if targetLow is false
				} else {
					result = new WorldManager.RayCastOutcome(from, to) {
						hit = true,
						HitObjId = obj.WorldObject.ID,
						CollisionPointLong = to,
						CollisionPointShort = to,
					};
				}

				
			}
		}
		WorldManager.RayCastOutcome? coverCast = null;

		Vector2 dir = Vector2.Normalize(from - to);
		to = result.CollisionPointLong + Vector2.Normalize(to - from)/5f;
		WorldManager.RayCastOutcome cast;

		cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 1.4f, to, Cover.High, false,true,pseudoLayer:dimension);
		if (cast.hit && result.HitObjId != cast.HitObjId)
		{
			coverCast = cast;
		}
		else
		{
			cast = WorldManager.Instance.Raycast(to + Vector2.Normalize(dir) * 1.4f, to, Cover.Low, false,true,pseudoLayer:dimension);
			if (cast.hit && result.HitObjId != cast.HitObjId)
			{
				coverCast = cast;
			}
			else
			{
				coverCast = null;
			}
		}



		
		Projectile p = new Projectile(result,coverCast);
		p.targetLow = targetLow;
		p.SupressionIgnores.Add(actor.WorldObject.ID);

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


	protected override Tuple<bool, string> CanPerformChild(Unit actor, Vector2Int target, int dimension = -1)
	{
		if(actor.WorldObject.TileLocation.Position == target)
		{
			return new Tuple<bool, string>(false,"You can't shoot yourself!");
		}
		var p = GenerateProjectile(actor, target,dimension);

		if (p.Result.hit)
		{
			var hitobj = WorldManager.Instance.GetObject(p.Result.HitObjId,dimension);
			if (hitobj!.Type.Edge || ( new Vector2Int((int)float.Round(p.Result.CollisionPointLong.X),(int)float.Round(p.Result.CollisionPointLong.Y)) != target && new Vector2Int((int)float.Round(p.Result.CollisionPointShort.X),(int)float.Round(p.Result.CollisionPointShort.Y)) != target))
			{
				return new Tuple<bool, string>(false,"Can't hit target");
			}
		}


		return new Tuple<bool, string>(true,"");
	}

	protected override List<SequenceAction> GetConsequencesChild(Unit actor, Vector2Int target,int dimension = -1)
	{
		
		/*
		 * 		bool lowShot =false;


		WorldTile tile = WorldManager.Instance.GetTileAtGrid(target);
		if (targeting == TargetingType.Auto)
		{
			if (tile.UnitAtLocation != null  && tile.UnitAtLocation.Crouching)
			{
				lowShot = true;

				if (clientPreview && !tile.UnitAtLocation.WorldObject.IsVisible())
				{
					lowShot = false;
				}

			}
		}else if(targeting == TargetingType.Low)
		{
			lowShot = true;
		}
		else if(targeting == TargetingType.High)
		{
			lowShot = false;
		}
		 */
		var p = GenerateProjectile(actor, target,dimension);
		var retrunList = new List<SequenceAction>();
		var m = new MoveCamera(p.Result.CollisionPointLong, true, 3);
		retrunList.Add(m);
		var turnact = new FaceUnit(actor.WorldObject.ID, target);
		retrunList.Add(turnact);
		if (p.CoverCast.HasValue)
		{
			var coverObj = WorldManager.Instance.GetObject(p.CoverCast.Value.HitObjId,dimension);
			var hitObj = WorldManager.Instance.GetObject(p.Result.HitObjId,dimension);
			Cover cover = coverObj!.GetCover();
			if (hitObj?.UnitComponent != null && hitObj.UnitComponent.Crouching)
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

			var act = new TakeDamage(coverBlock, 0, coverObj!.ID);
			retrunList.Add(act);
		}

		if (p.Result.hit)
		{
			var hitObj = WorldManager.Instance.GetObject(p.Result.HitObjId,dimension);
			if (hitObj != null)
			{

				if (hitObj.UnitComponent is not null)
				{
					var act = new FaceUnit(hitObj.ID, p.Result.StartPoint);
					retrunList.Add(act);
				}
				var act2 = new TakeDamage(p.Dmg, detResistance, hitObj.ID);
				retrunList.Add(act2);

			}
			else
			{
				Console.WriteLine("hitobj is null");
			}
		}
		else
		{
			//we missed so add taking damage on this tile as consiquences
			var act2 = new TakeDamage(p.Dmg, detResistance, p.Result.EndPoint, new List<string>());
			retrunList.Add(act2);
			
		}
		List<IWorldTile> tiles = SupressedTiles(p,dimension);
		retrunList.EnsureCapacity(retrunList.Count+tiles.Count);
		foreach (var tile in tiles)
		{

            var act2 = new Suppress(supressionStrenght, tile.Position);
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
			if (p.shooterLow)
			{
				passCover = Cover.Low;
			}
			
			if (WorldManager.Instance.GetCover(pos,dir,true)>passCover)
			{
				pos = new Vector2Int((int) p.Result.CollisionPointShort.X, (int) p.Result.CollisionPointShort.Y);
		
			}
			
		}
		var tiles = WorldManager.Instance.GetTilesAround(pos,supressionRange,dimension,Cover.High);
		return tiles;
	}

#if CLIENT
	
	Vector2Int previewTarget = new Vector2Int(-1,-1);
	int perivewActorID = -1;
	List<SequenceAction> previewCache = new List<SequenceAction>();
	private Projectile? previewShot;
	protected override void PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		if((previewTarget != target || perivewActorID != actor.WorldObject.ID) && CanPerform(actor,target,-1).Item1)	
		{
			previewCache = GetConsequences(actor, target);
			perivewActorID = actor.WorldObject.ID;
			previewTarget = target;
			previewShot = GenerateProjectile(actor, target,-1);
		}
		if(previewShot==null)
		{
			return;
		}
		
		spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"),  Utility.GridToWorldPos(previewTarget+new Vector2(-1.5f,-0.5f)), Color.Red);
		var area = SupressedTiles(previewShot.Value);
		spriteBatch.DrawOutline(area, Color.Blue, 5);
		string targetHeight = "fix this shit";
/*

		switch (targeting)
		{
			case TargetingType.Auto:
				targetHeight = "Auto(";
				if (previewShot.targetLow || previewShot.shooterLow)
				{
					targetHeight += "Low)";
				}
				else
				{
					targetHeight+= "High)";
				}

				break;
			case TargetingType.High:
				targetHeight = "High";
				break;
			case TargetingType.Low:
				targetHeight = "Low";
				break;
		}

		if (actor.Crouching)
		{
			targetHeight = "Low(Crouching)";
		}
*/

		spriteBatch.DrawText("X:"+previewTarget.X+" Y:"+previewTarget.Y+" Target Height: "+targetHeight,  Camera.GetMouseWorldPos(), 2/Camera.GetZoom(),Color.Wheat);



		foreach (var act in previewCache)
		{
			act.PreviewIfShould(spriteBatch);
		}


		var startPoint = Utility.GridToWorldPos(previewShot.Value.Result.StartPoint);
		var endPoint = Utility.GridToWorldPos(previewShot.Value.Result.EndPoint);

		Vector2 point1 = startPoint;
		Vector2 point2;
		int k = 0;
		var dmg = preDropOffDmg;
		foreach (var dropOff in previewShot.Value.DropOffPoints)
		{
			if (dropOff == previewShot.Value.DropOffPoints.Last())
			{
				point2 = Utility.GridToWorldPos(previewShot.Value.Result.CollisionPointLong);

			}
			else
			{
				point2 = Utility.GridToWorldPos(dropOff);
			}

			Color c;
			switch (k)
			{
				case 0:
					c = Color.DarkGreen;
					break;
				case 1:
					c = Color.Orange;
					break;
				case 2:
					c = Color.DarkRed;
					break;
				default:
					c = Color.Purple;
					break;

			}


			spriteBatch.DrawLine(point1.X, point1.Y, point2.X, point2.Y, c, 25);
			spriteBatch.DrawText(""+dmg,   Utility.GridToWorldPos(dropOff),4,Color.White);

			dmg = (int) Math.Ceiling(dmg / 1.8f);
			k++;
			point1 = point2;
		}


		spriteBatch.DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, Color.White, 5);
		WorldObject? hitobj = null;
		if (previewShot.Value.Result.HitObjId != -1)
		{
			hitobj = WorldManager.Instance.GetObject(previewShot.Value.Result.HitObjId);
		}

		WorldObject? coverObj = null;
		if (previewShot.Value.CoverCast.HasValue && previewShot.Value.CoverCast.Value.hit)
		{
			coverObj = WorldManager.Instance.GetObject(previewShot.Value.CoverCast.Value.HitObjId);
		}
		if(coverObj!= null){
			//crash here?
			var coverCast = previewShot.Value.CoverCast!.Value;
			

			//spriteBatch.DrawString(Game1.SpriteFont, hint, coverPoint + new Vector2(2f, 2f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			var coverobjtransform = coverObj.Type.Transform;
			Texture2D yellowsprite = coverObj.GetTexture();

			spriteBatch.Draw(yellowsprite, coverobjtransform.Position + Utility.GridToWorldPos(coverObj.TileLocation.Position), Color.Yellow);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(coverCast.CollisionPointLong), 5, 10, Color.Yellow, 25f);


		}



		if (hitobj != null)
		{
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.Value.Result.CollisionPointLong), 5, 10, Color.Red, 25f);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);

//			var data = hitobj.PreviewData;
//			data.totalDmg = previewShot.OriginalDmg;
//			data.distanceBlock = previewShot.OriginalDmg - previewShot.Dmg;
//			data.finalDmg += previewShot.Dmg - coverModifier;
//			data.coverBlock = coverModifier;
//			if (hitobj.UnitComponent == null)
//			{
//				data.finalDmg -= previewShot.DeterminationResistanceCoefficient;
//				data.determinationBlock = previewShot.DeterminationResistanceCoefficient;
//			}
//			else if (hitobj.UnitComponent.Determination > 0)
//			{
//				data.finalDmg -= previewShot.DeterminationResistanceCoefficient;
//				data.determinationBlock = previewShot.DeterminationResistanceCoefficient;
//			}
//
			//	GameLayout.ScreenData = data;
			//todo new UI
		}
		

	}
#endif
	
}