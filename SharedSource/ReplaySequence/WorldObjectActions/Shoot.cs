using System.Linq;
using System.Threading.Tasks;

using DefconNull.WorldActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;

#if CLIENT
using System;
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
	public Shootable.Projectile Projectile = new Shootable.Projectile();
	public static Shoot Make(Shootable.Projectile p, int originalDamage, int coverBlock, int rangeBlock) 
	{
		Shoot t = (GetAction(SequenceType.Shoot) as Shoot)!;
		t.OriginalDamage = originalDamage;
		t.CoverBlock = coverBlock;
		t.RangeBlock = rangeBlock;
		t.Projectile = p;
		return t;
	}

	
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
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return true;
	}

#endif

	protected override void RunSequenceAction()
	{
		
	}

	protected override void SerializeArgs(Message message)
	{
	
		message.Add(OriginalDamage);
		message.Add(CoverBlock);
		message.Add(RangeBlock);
		message.Add(Projectile);
	}
}