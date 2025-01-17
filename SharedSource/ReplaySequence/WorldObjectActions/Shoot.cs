﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DefconNull.WorldActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;

#if CLIENT
using DefconNull.LocalObjects;
using System;
using System.Threading;
using DefconNull.Rendering;
#endif
namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class Shoot : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.Shoot;
	}
	
	protected bool Equals(Shoot other)
	{
		return OriginalDamage == other.OriginalDamage && CoverBlock == other.CoverBlock && RangeBlock == other.RangeBlock && Projectile.Equals(other.Projectile);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Shoot) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = base.GetHashCode();
			hashCode = (hashCode * 397) ^ OriginalDamage;
			hashCode = (hashCode * 397) ^ CoverBlock;
			hashCode = (hashCode * 397) ^ RangeBlock;
			hashCode = (hashCode * 397) ^ Projectile.GetHashCode();
			return hashCode;
		}
	}

	public int OriginalDamage;
	public int CoverBlock;
	public int RangeBlock;
	public int ShotCount = 5;
	public int ShotDelay = 300;
	public float ShotSpread = 0.5f;
	public string ShotSound = "";
	public Shootable.Projectile Projectile = new Shootable.Projectile();
	public static Shoot Make(Shootable.Projectile p, int originalDamage, int coverBlock, int rangeBlock, int shotCount, int shotDelay, float shotSpread, string sound)
	{
		Shoot t = (GetAction(SequenceType.Shoot) as Shoot)!;
		t.OriginalDamage = originalDamage;
		t.CoverBlock = coverBlock;
		t.RangeBlock = rangeBlock;
		t.Projectile = p;
		t.ShotCount = shotCount;
		t.ShotDelay = shotDelay;
		t.ShotSpread = shotSpread;
		t.ShotSound = sound;
		return t;
	}

	public override bool ShouldDo()
	{
#if CLIENT
		return true;
#elif SERVER
		return false;
#endif
	}


	public override BatchingMode Batching => BatchingMode.NonBlockingAlone;
#if CLIENT

	public override void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
	{
		batch.DrawText("" +
		               "           Shooting:\n" +
		               "  [Green]Base damage[-] done by the weapon\n" +
		               "  Damage [Red]absorbed[-] by cover\n" +
		               "  Damage [Red]lost[-] to weapons range\n" +
		               "  [Green]Final Damage[-] done by the weapon", pos, scale, Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/shoot"), pos + new Vector2(0, 5)*scale,scale/2f,Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/cover"), pos + new Vector2(0, 16)*scale,scale/2f,Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/rangeicon"), pos + new Vector2(0, 28)*scale,scale/2f,Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/genericDamage"), pos + new Vector2(0, 40)*scale,scale/2f,Color.White);
		
		
	}

	public override void DrawConsequence(Vector2 pos, SpriteBatch batch)
	{
		pos += new Vector2(0, 0);
		Vector2 offset = new Vector2(TextureManager.GetTexture("HoverHud/Consequences/shoot").Width-2,0);
		
		batch.DrawNumberedIcon(OriginalDamage.ToString(), TextureManager.GetTexture("HoverHud/Consequences/shoot"), pos, Color.White);
		
		batch.DrawNumberedIcon(CoverBlock.ToString(), TextureManager.GetTexture("HoverHud/Consequences/cover"), pos+offset, Color.Red);
		
		batch.DrawNumberedIcon(RangeBlock.ToString(), TextureManager.GetTexture("HoverHud/Consequences/rangeicon"), pos+offset*2, Color.Red);
		
		batch.DrawText("=", pos+offset*2+new Vector2(26,7),2f, Color.White);

		batch.DrawNumberedIcon(Math.Max(0,OriginalDamage-CoverBlock-RangeBlock).ToString(), TextureManager.GetTexture("HoverHud/Consequences/genericDamage"), pos+offset*3+new Vector2(8,0), Color.White);

		
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/line"), new Vector2(44,25), Color.White);
	}


	public override void Preview(SpriteBatch spriteBatch)
	{
		base.Preview(spriteBatch);
		
		var startPoint = Utility.GridToWorldPos(Projectile.Result.StartPoint);
		var endPoint = Utility.GridToWorldPos(Projectile.Result.EndPoint);

		Vector2 point1 = startPoint;
		Vector2 point2;
		int k = 0;
		foreach (var dropOff in Projectile.DropOffPoints)
		{
			if (dropOff == Projectile.DropOffPoints.Last())
			{
				point2 = Utility.GridToWorldPos(Projectile.Result.CollisionPointLong);

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


			spriteBatch.DrawLine(point1.X, point1.Y, point2.X, point2.Y, c, 5);
			
			k++;
			point1 = point2;
		}


		//spriteBatch.DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, Color.White, 1);
		WorldObject? hitobj = null;
		if (Projectile.Result.HitObjId != -1)
		{
			hitobj =WorldObjectManager.GetObject(Projectile.Result.HitObjId);
		}

		WorldObject? coverObj = null;
		if (Projectile.CoverCast.HasValue && Projectile.CoverCast.Value.hit)
		{
			coverObj =WorldObjectManager.GetObject(Projectile.CoverCast.Value.HitObjId);
		}
		if(coverObj!= null){
			//crash here?
			var coverCast = Projectile.CoverCast!.Value;
			

			//spriteBatch.DrawString(Game1.SpriteFont, hint, coverPoint + new Vector2(2f, 2f), c, 0, Vector2.Zero, 4, new SpriteEffects(), 0);
			var coverobjtransform = coverObj.Type.Transform;
			Texture2D yellowsprite = coverObj.GetTexture();
			var pos = coverobjtransform.Position + Utility.GridToWorldPos(coverObj.TileLocation.Position);
			spriteBatch.Draw(yellowsprite, pos,null,Color.Yellow,0,Vector2.Zero, 1f,SpriteEffects.None,0);
			//spriteBatch.Draw(obj.GetSprite().TextureRegion.Texture, transform.Position + Utility.GridToWorldPos(obj.TileLocation.Position),Color.Red);
			spriteBatch.DrawCircle(Utility.GridToWorldPos(coverCast.CollisionPointLong), 5, 10, Color.Yellow, 15f);
			
		}



		if (hitobj != null && hitobj.IsVisible())
		{
			spriteBatch.DrawCircle(Utility.GridToWorldPos(Projectile.Result.CollisionPointLong), 5, 10, Color.Red, 15f);
		}
	}
#endif

	protected override void DeserializeArgs(Message message)
	{
		
		OriginalDamage = message.GetInt();
		CoverBlock = message.GetInt();
		RangeBlock = message.GetInt();
		Projectile = message.GetSerializable<Shootable.Projectile>();
		ShotSound = message.GetString();
		ShotCount = message.GetInt();
		ShotDelay = message.GetInt();
		ShotSpread = message.GetFloat();
		
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return true;
	}
	public override List<SequenceAction> SendPrerequesteInfoToPlayer(bool player1)
	{
		if (Projectile.Result.hit)
		{
			var hitobj = WorldObjectManager.GetObject(Projectile.Result.HitObjId);
			if(hitobj != null && hitobj.UnitComponent != null && hitobj.UnitComponent.IsPlayer1Team == player1)
			{
				Vector2Int tile = Projectile.Result.StartPoint;
				var attacker = WorldManager.Instance.GetTileAtGrid(tile).UnitAtLocation;
				if (attacker == null)
				{
					Log.Message("ERROR","Attacker not found for shooting reveal "+tile);
					return base.SendPrerequesteInfoToPlayer(player1);
				}
				return new List<SequenceAction>(){SpotUnit.Make(attacker.WorldObject.ID,player1)};
			}
		}
		return base.SendPrerequesteInfoToPlayer(player1);
	}

#endif

	protected override void RunSequenceAction()
	{
#if CLIENT
		
		for (int i = 0; i < ShotCount; i++)
		{
			new Tracer(Utility.GridToWorldPos(Projectile.Result.StartPoint), Utility.GridToWorldPos(Projectile.Result.CollisionPointShort+new Vector2(Random.Shared.NextSingle(-0.5f,0.5f),Random.Shared.NextSingle(-0.5f,0.5f))));
			Thread.Sleep(ShotDelay);
			if (ShotSound != "")
			{
				Audio.PlaySound(ShotSound,Utility.GridToWorldPos(Projectile.Result.StartPoint),1f,0.7f,5f);
			}
		}
#endif
	}

	protected override void SerializeArgs(Message message)
	{
	
		message.Add(OriginalDamage);
		message.Add(CoverBlock);
		message.Add(RangeBlock);
		message.Add(Projectile);
		message.Add(ShotSound);
		message.Add(ShotCount);
		message.Add(ShotDelay);
		message.Add(ShotSpread);
		
		
		
	}
}