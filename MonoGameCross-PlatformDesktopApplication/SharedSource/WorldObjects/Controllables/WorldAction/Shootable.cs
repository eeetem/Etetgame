using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno.Items;

public class Shootable : DeliveryMethod
{
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

	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		if (target == actor.worldObject.TileLocation.Position)
		{
			return new Tuple<bool, string>(false, "You can't shoot yourself!");
		}
#if CLIENT

		if (!freeFire)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(target);
			if (tile.ControllableAtLocation == null || !tile.ControllableAtLocation.IsVisible() || tile.ControllableAtLocation.ControllableComponent.IsMyTeam())
			{
				return new Tuple<bool, string>(false, "Invalid target, hold ctrl for free fire");
			}
		}		
#endif
	
		return new Tuple<bool, string>(true, "");
	}
	public Projectile MakeProjectile(Controllable actor,Vector2Int target)
	{
		//target = actor.worldObject.TileLocation.Position + new Vector2(-10,0);
		bool lowShot =false;


		WorldTile tile = WorldManager.Instance.GetTileAtGrid(target);
		if (targeting == TargetingType.Auto)
		{
		if (tile.ControllableAtLocation != null  && tile.ControllableAtLocation.ControllableComponent.Crouching) 
		{
				lowShot = true;
#if CLIENT
				if (!tile.ControllableAtLocation.IsVisible())
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
		
		Vector2 shotDir = Vector2.Normalize(target -actor.worldObject.TileLocation.Position);
		Projectile projectile = new Projectile(actor.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),target+new Vector2(0.5f,0.5f),dmg,dropOffRange,lowShot,actor.Crouching,detResistance,supressionRange,supression);

		

		return projectile;
	}

	public override Vector2Int ExectuteAndProcessLocationChild(Controllable actor, Vector2Int target)
	{
		actor.ClearOverWatch();

			
		//client shouldnt be allowed to judge what got hit
		//fire packet just makes the unit "shoot"
		//actual damage and projectile is handled elsewhere

			Projectile p = MakeProjectile(actor, target);
#if SERVER
			p.Fire();
			Networking.DoAction(new ProjectilePacket(p.result,p.covercast,p.originalDmg,p.dropoffRange,p.determinationResistanceCoefficient,p.supressionRange,p.supressionStrenght,p.shooterLow,p.targetLow));
#endif			

		
		actor.worldObject.Face(Utility.GetDirection(actor.worldObject.TileLocation.Position,target));
		if (p.result.hit)
		{
			return WorldManager.Instance.GetObject(p.result.hitObjID).TileLocation.Position;
		}
	
		return target;
		
		
	}
#if CLIENT
	protected Projectile? previewShot;

	private Vector2Int lastTarget = new Vector2Int(0,0);
	private TargetingType lastTargetingType = TargetingType.Auto;
	public static bool freeFire = false;

	public override Vector2Int? PreviewChild(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (!freeFire)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(target);
			if (tile.ControllableAtLocation == null || !tile.ControllableAtLocation.IsVisible() || tile.ControllableAtLocation.ControllableComponent.IsMyTeam())
			{
				return null;
			}
		}

		if (actor.worldObject.TileLocation.Position == target)
		{
			return target;
		}

		if (target != lastTarget || targeting != lastTargetingType)
		{
			previewShot = MakeProjectile(actor, target);
			lastTarget = target;
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

				
		spriteBatch.DrawText("X:"+target.X+" Y:"+target.Y+" Target Height: "+targetHeight,  Camera.GetMouseWorldPos(), 2/(Camera.GetZoom()),Color.Wheat);



		foreach (var tile in previewShot.SupressedTiles())
		{

			if (tile.Surface == null) continue;

			Texture2D sprite = tile.Surface.GetTexture();

			spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, Color.DarkBlue * 0.45f);

			if (tile.ControllableAtLocation != null)
			{
				tile.ControllableAtLocation.PreviewData.detDmg += previewShot.supressionStrenght;
			}

		}


		var startPoint = Utility.GridToWorldPos(previewShot.result.StartPoint);
		var endPoint = Utility.GridToWorldPos(previewShot.result.EndPoint);

		Vector2 point1 = startPoint;
		Vector2 point2;
		int k = 0;
		var dmg = previewShot.originalDmg;
		foreach (var dropOff in previewShot.dropOffPoints)
		{
			if (dropOff == previewShot.dropOffPoints.Last())
			{
				point2 = Utility.GridToWorldPos(previewShot.result.CollisionPointLong);

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
		if (previewShot.result.hitObjID != -1)
		{
			hitobj = WorldManager.Instance.GetObject(previewShot.result.hitObjID);
		}


		if (previewShot.covercast != null && previewShot.covercast.hit)
		{
			Color c = Color.Green;
			string hint = "";
			var coverPoint = Utility.GridToWorldPos(previewShot.covercast.CollisionPointLong);
			Cover cover = WorldManager.Instance.GetObject(previewShot.covercast.hitObjID).GetCover();
			if (hitobj?.ControllableComponent != null && hitobj.ControllableComponent.Crouching)
			{
				if (cover != Cover.Full)
				{
					cover++;
				}
			}

			switch (cover)
			{
				case Cover.None:
					c = Color.Green;
					Console.WriteLine("How: Cover object has no cover");
					break;
				case Cover.Low:
					c = Color.Gray;
					coverModifier = 2;
					hint = "Cover: -2 DMG";
					break;
				case Cover.High:
					c = Color.Black;
					coverModifier = 4;
					hint = "Cover: -4 DMG";
					break;
				case Cover.Full:
					c = Color.Black;
					coverModifier = 10;
					hint = "Full Cover: -10 DMG";
					break;
				default:

					break;

			}

			//spriteBatch.DrawString(Game1.SpriteFont, hint, coverPoint + new Vector2(2f, 2f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			var coverobj = WorldManager.Instance.GetObject(previewShot.covercast.hitObjID);
			var coverobjtransform = coverobj.Type.Transform;
			Texture2D yellowsprite = coverobj.GetTexture();

			spriteBatch.Draw(yellowsprite, coverobjtransform.Position + Utility.GridToWorldPos(coverobj.TileLocation.Position), Color.Yellow);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.CollisionPointLong), 15, 10, Color.Yellow, 25f);
			//spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.StartPoint), 15, 10, Color.Green, 25f);
			//spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.EndPoint), 40, 10, Color.Pink, 25f);

		}



		if (hitobj != null)
		{
			var transform = hitobj.Type.Transform;
			Texture2D redSprite = hitobj.GetTexture();


			spriteBatch.Draw(redSprite, transform.Position + Utility.GridToWorldPos(hitobj.TileLocation.Position), Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.result.CollisionPointLong), 15, 10, Color.Red, 25f);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);

				var data = hitobj.PreviewData;
				data.totalDmg = previewShot.originalDmg;
				data.distanceBlock = previewShot.originalDmg - previewShot.dmg;
				data.finalDmg += previewShot.dmg - coverModifier;
				data.coverBlock = coverModifier;
				if (hitobj.ControllableComponent != null && hitobj.ControllableComponent.Determination > 0)
				{
					data.finalDmg -= previewShot.determinationResistanceCoefficient;
					data.determinationBlock = previewShot.determinationResistanceCoefficient;
				}

				hitobj.PreviewData = data;
		}
		
		
		if (previewShot.result.hit)
		{
			return previewShot.result.CollisionPointLong;
		}
	
		return target;


	}

	public override void InitPreview()
	{
		previewShot = null;
		lastTarget = new Vector2Int(0,0);
		//targeting = TargetingType.Auto;
	}

	public override void AnimateChild(Controllable actor, Vector2Int target)
	{
	
	}
#endif
}