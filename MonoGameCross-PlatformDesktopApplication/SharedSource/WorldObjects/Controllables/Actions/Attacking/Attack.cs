﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno;

public abstract class Attack : Action
{
	public Attack(ActionType actionType) : base(actionType)
	{
		
	}

	protected Projectile MakeProjectile(Controllable actor,Vector2Int target)
	{
		
			bool lowShot = false;
			if (actor.Crouching)
			{
				lowShot = true;
			}else
			{
				WorldTile tile = WorldManager.Instance.GetTileAtGrid(target);
				if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null && tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent.Crouching)
				{
					lowShot = true;
				}
			}

			Vector2 shotDir = Vector2.Normalize(target -actor.worldObject.TileLocation.Position);
			Projectile projectile = new Projectile(actor.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),target+new Vector2(0.5f,0.5f),GetDamage(actor),actor.Type.WeaponRange,lowShot,GetAwarenessResistanceEffect(actor),GetSupressionRange(actor),GetSupressionStrenght(actor));

		

		return projectile;
	}

	protected override void Execute(Controllable actor, Vector2Int target)
	{
		actor.ClearOverWatch();
			
		//client shouldnt be allowed to judge what got hit
		//fire packet just makes the unit "shoot"
		//actual damage and projectile is handled elsewhere
#if SERVER
			Projectile p = MakeProjectile(actor, target);
			p.Fire();
			Networking.DoAction(new ProjectilePacket(p.result,p.covercast,p.originalDmg,p.dropoffRange,p.awarenessResistanceCoefficient,p.supressionRange,p.supressionStrenght));

#endif
		actor.worldObject.Face(Utility.ToClampedDirection( actor.worldObject.TileLocation.Position-target));

	}

	protected abstract int GetDamage(Controllable actor);
	protected abstract int GetSupressionRange(Controllable actor);
	protected abstract int GetAwarenessResistanceEffect(Controllable actor);
	protected virtual int GetSupressionStrenght(Controllable actor)
	{
		return 1;
	}


#if CLIENT

	private static Projectile previewShot;

	private Vector2Int lastTarget = new Vector2Int(0,0);
	
	
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{

		if (target != lastTarget)
		{
			previewShot = MakeProjectile(actor, target);
			lastTarget = target;
		}	
		var tiles = WorldManager.Instance.GetTilesAround(new Vector2Int((int)previewShot.result.CollisionPoint.X, (int)previewShot.result.CollisionPoint.Y),previewShot.supressionRange);
		foreach (var tile in tiles)
		{
			if (tile.Surface == null) continue;
							
			Texture2D sprite = tile.Surface.GetTexture();

			spriteBatch.Draw(sprite, tile.Surface.GetDrawTransform().Position, Color.Cyan*0.3f);
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
				point2 = Utility.GridToWorldPos(previewShot.result.CollisionPoint);
							
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
						
			
			spriteBatch.DrawLine(point1.X,point1.Y,point2.X,point2.Y,c,25);
			spriteBatch.DrawString(Game1.SpriteFont,"Damage: "+dmg,  point1,c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			dmg = (int)Math.Ceiling(dmg/2f);
			k++;
			point1 = point2;
					
							
						
		}
					
				
		spriteBatch.DrawLine(startPoint.X,startPoint.Y,endPoint.X,endPoint.Y,Color.White,5);
		int coverModifier = 0;
					
		var hitobj = WorldManager.Instance.GetObject(previewShot.result.hitObjID);
		if (previewShot.covercast != null && previewShot.covercast.hit)
		{
			Color c = Color.Green;
			string hint = "";
			var coverPoint = Utility.GridToWorldPos(previewShot.covercast.CollisionPoint);
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
			spriteBatch.DrawString(Game1.SpriteFont,hint, coverPoint+new Vector2(2f,2f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.StartPoint), 15, 10, Color.Red, 25f);
						
			var coverobj = WorldManager.Instance.GetObject(previewShot.covercast.hitObjID);
			var coverobjtransform = coverobj.Type.Transform;
			Texture2D yellowsprite = coverobj.GetTexture();

			spriteBatch.Draw(yellowsprite, coverobjtransform.Position + Utility.GridToWorldPos(coverobj.TileLocation.Position), Color.Yellow);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.covercast.CollisionPoint), 15, 10, Color.Yellow, 25f);

		}
				
					
						
		if (hitobj != null)
		{
			var transform = hitobj.Type.Transform;
			Texture2D redSprite = hitobj.GetTexture();
						

			spriteBatch.Draw(redSprite, transform.Position + Utility.GridToWorldPos(hitobj.TileLocation.Position), Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(previewShot.result.CollisionPoint), 15, 10, Color.Red, 25f);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
			if (hitobj.ControllableComponent != null)
			{
				if (hitobj.ControllableComponent.Awareness > 0)
				{
					spriteBatch.DrawString(Game1.SpriteFont, "Final Damage: " + ((previewShot.dmg - coverModifier) - previewShot.awarenessResistanceCoefficient) + ("  (-"+previewShot.awarenessResistanceCoefficient+" due to awareness)"), Utility.GridToWorldPos(previewShot.result.CollisionPoint + new Vector2(-0.5f, -0.5f)), Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
				}
				else
				{
					spriteBatch.DrawString(Game1.SpriteFont, "Final Damage: " + (previewShot.dmg - coverModifier), Utility.GridToWorldPos(previewShot.result.CollisionPoint + new Vector2(-0.5f, -0.5f)), Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
				}
			}
		}
	}
#endif


}
