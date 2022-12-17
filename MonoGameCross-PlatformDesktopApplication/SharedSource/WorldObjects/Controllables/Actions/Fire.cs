using System;
using System.Collections.Generic;
using System.Linq;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno;

public class Fire : Action
{
	public Fire() :base(ActionType.Attack)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{
		
		if (position == actor.worldObject.TileLocation.Position)
		{
			return false;
		}

		if (actor.overWatch)
		{
			return true;//can overwatch fire without points
		}

		if (actor.ActionPoints <= 0)
		{
			return false;
		}
		if (actor.MovePoints <= 0)
		{
			return false;
		}

		return true;

	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{

			actor.ActionPoints--;
			actor.Awareness--;
			actor.MovePoints--;
			actor.ClearOverWatch();
			
		//client shouldnt be allowed to judge what got hit
		//fire packet just makes the unit "shoot"
		//actual damage and projectile is handled elsewhere
#if SERVER
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
			Vector2 shotDir = Vector2.Normalize(target - actor.worldObject.TileLocation.Position);
			Projectile p = new Projectile(actor.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),target+new Vector2(0.5f,0.5f),actor.Type.WeaponDmg,actor.Type.WeaponRange,lowShot);
			p.Fire();
			Networking.DoAction(new ProjectilePacket(p.result,p.covercast,actor.Type.WeaponDmg,p.dropoffRange));

#endif
#if CLIENT
		Camera.SetPos(target);
		if (actor.Type.WeaponRange > 6)
		{
			ObjectSpawner.Burst(actor.worldObject.TileLocation.Position, target);
		}
		else
		{
			ObjectSpawner.ShotGun(actor.worldObject.TileLocation.Position,target);	
		}
#endif
		actor.worldObject.Face(Utility.ToClampedDirection( actor.worldObject.TileLocation.Position-target));

	}
#if CLIENT

	private static Projectile previewShot;

	private Vector2Int lastTarget = new Vector2Int(0,0);
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (target != lastTarget)
		{
			bool lowShot = false;
			if (UI.SelectedControllable.Crouching)
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

			Vector2 shotDir = Vector2.Normalize(target - UI.SelectedControllable.worldObject.TileLocation.Position);
			previewShot = new Projectile(UI.SelectedControllable.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),target+new Vector2(0.5f,0.5f),UI.SelectedControllable.Type.WeaponDmg,UI.SelectedControllable.Type.WeaponRange,lowShot);

			lastTarget = target;
		}

				
		var tiles = WorldManager.Instance.GetTilesAround(new Vector2Int((int)previewShot.result.EndPoint.X, (int)previewShot.result.EndPoint.Y));
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
		var dmg = UI.SelectedControllable.Type.WeaponDmg;
		foreach (var dropOff in previewShot.dropOffPoints)
		{
			if (dropOff == previewShot.dropOffPoints.Last())
			{
				point2 = Utility.GridToWorldPos(previewShot.result.EndPoint);
							
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
						
			spriteBatch.DrawString(Game1.SpriteFont,"Damage: "+dmg,  point1,c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			spriteBatch.DrawLine(point1.X,point1.Y,point2.X,point2.Y,c,25);
			dmg = (int)Math.Ceiling(dmg/2f);
			k++;
			point1 = point2;
					
							
						
		}
					
				
		spriteBatch.DrawLine(startPoint.X,startPoint.Y,endPoint.X,endPoint.Y,Color.White,15);
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
					coverModifier = 1;
					hint = "Cover: -1 DMG";
					break;
				case Cover.High:
					c = Color.Black;
					coverModifier = 2;
					hint = "Cover: -2 DMG";
					break;
				case Cover.Full:
					c = Color.Black;
					coverModifier = 10;
					hint = "Full Cover: -10 DMG";
					break;
				default:
							
					break;

			}
			spriteBatch.DrawString(Game1.SpriteFont,hint, coverPoint+new Vector2(1f,1f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			spriteBatch.DrawLine(coverPoint.X,coverPoint.Y,endPoint.X,endPoint.Y,c,9);
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
					spriteBatch.DrawString(Game1.SpriteFont, "Final Damage: " + (previewShot.dmg - coverModifier) / 2 + "(Saved By Awareness)", Utility.GridToWorldPos(previewShot.result.CollisionPoint + new Vector2(-0.5f, -0.5f)), Color.Black, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
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

