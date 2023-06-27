﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

#if CLIENT
using MultiplayerXeno.UILayouts;
#endif


namespace MultiplayerXeno.Items;

public class Shootable : DeliveryMethod
{
	
	public enum TargetingType
	{
		Auto,
		High,
		Low
	}
	readonly int dmg;
	readonly int detResistance;
	readonly int supression;
	readonly int supressionRange;
	readonly int dropOffRange;
	
	public static TargetingType targeting = TargetingType.Auto;
	public Shootable(int dmg, int detResistance, int supression, int supressionRange, int dropOffRange)
	{
		this.dmg = dmg;
		this.detResistance = detResistance;
		this.supression = supression;
		this.supressionRange = supressionRange;
		this.dropOffRange = dropOffRange;
	}

	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		if (target == actor.WorldObject.TileLocation.Position)
		{
			return new Tuple<bool, string>(false, "You can't shoot yourself!");
		}
#if CLIENT
		
		if (target != _lastTarget || targeting != lastTargetingType)
		{
			previewShot = MakeProjectile(actor, target);
			_lastTarget = target;
		}

		if (!WorldAction.FreeFire)
		{
			if (previewShot.Result.hit && WorldManager.Instance.GetObject(previewShot.Result.HitObjId).TileLocation.Position != (Vector2Int) previewShot.Result.EndPoint)
			{
				return new Tuple<bool, string>(false, "Invalid target, hold ctrl for free fire");
			}
		}
#endif
	
		return new Tuple<bool, string>(true, "");
	}


	public Projectile MakeProjectile(Unit actor,Vector2Int target)
	{
		//target = actor.WorldObject.TileLocation.Position + new Vector2(-10,0);
		bool lowShot =false;


		WorldTile tile = WorldManager.Instance.GetTileAtGrid(target);
		if (targeting == TargetingType.Auto)
		{
			if (tile.UnitAtLocation != null  && tile.UnitAtLocation.Crouching) 
			{
				lowShot = true;
#if CLIENT
				if (!tile.UnitAtLocation.WorldObject.IsVisible())
				{
					lowShot = false;
				}
#endif
			}
		}else if(targeting == TargetingType.Low)
		{
			lowShot = true;
		}
		else if(targeting == TargetingType.High)
		{
			lowShot = false;
		}
		
		Vector2 shotDir = Vector2.Normalize(target -actor.WorldObject.TileLocation.Position);
		Projectile projectile = new Projectile(actor.WorldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+shotDir/new Vector2(2.5f,2.5f),target+new Vector2(0.5f,0.5f),dmg,dropOffRange,lowShot,actor.Crouching,detResistance,supressionRange,supression);
		projectile.SupressionIgnores.Add(actor.WorldObject.ID);
		

		return projectile;
	}

	public override Vector2Int? ExectuteAndProcessLocationChild(Unit actor, Vector2Int target)
	{
		//client shouldnt be allowed to judge what got hit
		//fire packet just makes the unit "shoot"
		//actual damage and projectile is handled elsewhere

		Projectile p = MakeProjectile(actor, target);
		
#if SERVER
			p.Fire();
		//	Networking.DoAction(new ProjectilePacket(p.Result,p.CoverCast,p.OriginalDmg,p.DropoffRange,p.DeterminationResistanceCoefficient,p.SupressionRange,p.SupressionStrenght,p.ShooterLow,p.TargetLow,p.SupressionIgnores));
#endif			

		
		actor.WorldObject.Face(Utility.GetDirection(actor.WorldObject.TileLocation.Position,target));
		if (p.Result.hit)
		{
			var obj = WorldManager.Instance.GetObject(p.Result.HitObjId);
			if (obj == null)
			{
				return p.Result.CollisionPointShort;
			}
			return obj.TileLocation.Position;
		}
	
		return target;
		
		
	}
#if CLIENT
	protected Projectile? previewShot;

	private Vector2Int _lastTarget = new Vector2Int(0,0);
	private TargetingType lastTargetingType = TargetingType.Auto;


	public override Vector2Int? PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		if (actor.WorldObject.TileLocation.Position == target)
		{
			return target;
		}
		
		if (target != _lastTarget || targeting != lastTargetingType)
		{
			previewShot = MakeProjectile(actor, target);
			_lastTarget = target;
		}

		spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"),  Utility.GridToWorldPos(target+new Vector2(-1.5f,-0.5f)), Color.Red);
		if (previewShot == null)
		{
			return target;
		}

		string targetHeight = "";
		switch (targeting)
		{
			case TargetingType.Auto:
				targetHeight = "Auto(";
				if (previewShot.TargetLow || previewShot.ShooterLow)
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


		spriteBatch.DrawText("X:"+target.X+" Y:"+target.Y+" Target Height: "+targetHeight,  Camera.GetMouseWorldPos(), 2/Camera.GetZoom(),Color.Wheat);



		foreach (var tile in previewShot.SupressedTiles())
		{

			if (tile.Surface == null) continue;

			Texture2D sprite = tile.Surface.GetTexture();

			spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, Color.DarkBlue * 0.45f);

			if (tile.UnitAtLocation != null && !previewShot.SupressionIgnores.Contains(tile.UnitAtLocation.WorldObject.ID))
			{
				tile.UnitAtLocation.WorldObject.PreviewData.detDmg += previewShot.SupressionStrenght;
			}

		}


		var startPoint = Utility.GridToWorldPos(previewShot.Result.StartPoint);
		var endPoint = Utility.GridToWorldPos(previewShot.Result.EndPoint);

		Vector2 point1 = startPoint;
		Vector2 point2;
		int k = 0;
		var dmg = previewShot.OriginalDmg;
		foreach (var dropOff in previewShot.DropOffPoints)
		{
			if (dropOff == previewShot.DropOffPoints.Last())
			{
				point2 = Utility.GridToWorldPos(previewShot.Result.CollisionPointLong);

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
		int coverModifier = 0;
		WorldObject? hitobj = null;
		if (previewShot.Result.HitObjId != -1)
		{
			hitobj = WorldManager.Instance.GetObject(previewShot.Result.HitObjId);
		}

		WorldObject? coverObj = null;
		if (previewShot.CoverCast != null && previewShot.CoverCast.hit)
		{
			coverObj = WorldManager.Instance.GetObject(previewShot.CoverCast.HitObjId);
		}
		if(coverObj!= null){
			//crash here?
			var coverCast = previewShot.CoverCast;

			Cover cover = coverObj.GetCover();
			if (hitobj?.UnitComponent != null && hitobj.UnitComponent.Crouching)
			{
				if (cover != Cover.Full)
				{
					cover++;
				}
			}

			switch (cover)
			{
				case Cover.None:
					Console.WriteLine("How: Cover object has no cover");
					break;
				case Cover.Low:
					coverModifier = 2;
					break;
				case Cover.High:
					coverModifier = 4;
					break;
				case Cover.Full:
					coverModifier = 10;
					break;
			}

			//spriteBatch.DrawString(Game1.SpriteFont, hint, coverPoint + new Vector2(2f, 2f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			var coverobjtransform = coverObj.Type.Transform;
			Texture2D yellowsprite = coverObj.GetTexture();

			spriteBatch.Draw(yellowsprite, coverobjtransform.Position + Utility.GridToWorldPos(coverObj.TileLocation.Position), Color.Yellow);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(coverCast.CollisionPointLong), 15, 10, Color.Yellow, 25f);
			coverObj.PreviewData.finalDmg += coverModifier;
			Console.WriteLine(coverObj.PreviewData.finalDmg);

		}



		if (hitobj != null)
		{
			var transform = hitobj.Type.Transform;
			Texture2D redSprite = hitobj.GetTexture();


			spriteBatch.Draw(redSprite, transform.Position + Utility.GridToWorldPos(hitobj.TileLocation.Position), Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.Result.CollisionPointLong), 15, 10, Color.Red, 25f);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);

			var data = hitobj.PreviewData;
			data.totalDmg = previewShot.OriginalDmg;
			data.distanceBlock = previewShot.OriginalDmg - previewShot.Dmg;
			data.finalDmg += previewShot.Dmg - coverModifier;
			data.coverBlock = coverModifier;
			if (hitobj.UnitComponent == null)
			{
				data.finalDmg -= previewShot.DeterminationResistanceCoefficient;
				data.determinationBlock = previewShot.DeterminationResistanceCoefficient;
			}
			else if (hitobj.UnitComponent.Determination > 0)
			{
				data.finalDmg -= previewShot.DeterminationResistanceCoefficient;
				data.determinationBlock = previewShot.DeterminationResistanceCoefficient;
			}

			hitobj.PreviewData = data;
			GameLayout.ScreenData = data;
		}
		
		
		if (previewShot.Result.hit)
		{
			return previewShot.Result.CollisionPointLong;
		}
	
		return target;


	}

	public override void InitPreview()
	{
		previewShot = null;
		_lastTarget = new Vector2Int(0,0);
		//targeting = TargetingType.Auto;
	}

	public override void AnimateChild(Unit actor, Vector2Int target)
	{
	
	}
#endif
}