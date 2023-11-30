using System.Linq;
using System.Threading.Tasks;
using DefconNull.WorldActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class UnitShoot : UnitSequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.UnitShoot;
	}
	
	public int OriginalDamage;
	public int CoverBlock;
	public int RangeBlock;
	public Shootable.Projectile Projectile;

	public static UnitShoot Make(int actorID, Shootable.Projectile p, int originalDamage, int coverBlock, int rangeBlock) 
	{
		UnitShoot t = (GetAction(SequenceType.UnitShoot) as UnitShoot)!;
		t.Requirements = new TargetingRequirements(actorID);
		t.OriginalDamage = originalDamage;
		t.CoverBlock = coverBlock;
		t.RangeBlock = rangeBlock;
		t.Projectile = p;
		return t;
	}


	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{

		});
		return t;
	}
#if CLIENT
	public override void DrawDesc(Vector2 pos, SpriteBatch batch)
	{
		batch.DrawText("Shot:" + OriginalDamage +" -" +CoverBlock +"C -"+RangeBlock+"R", pos, Color.White);
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

			spriteBatch.Draw(yellowsprite, coverobjtransform.Position + Utility.GridToWorldPos(coverObj.TileLocation.Position), Color.Yellow);
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
		base.DeserializeArgs(message);
		OriginalDamage = message.GetInt();
		CoverBlock = message.GetInt();
		RangeBlock = message.GetInt();
		Projectile = message.GetSerializable<Shootable.Projectile>();
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(OriginalDamage);
		message.Add(CoverBlock);
		message.Add(RangeBlock);
		message.Add(Projectile);
	}
}