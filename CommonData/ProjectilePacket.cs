using Microsoft.Xna.Framework;
using Network.Packets;

namespace CommonData;

public class ProjectilePacket : Packet
{

	public override void BeforeReceive()
	{
		result.CollisionPointLong = new Vector2(RCollisionPointX, RCollisionPointY);
		result.CollisionPointShort = new Vector2(RCollisionPointshortX, RCollisionPointshortY);
		result.StartPoint = new Vector2(RStartPointX, RStartPointY);
		result.EndPoint = new Vector2(REndPointX, REndPointY);

		if (covercast != null)
		{
			
			covercast.CollisionPointLong = new Vector2(CCollisionPointX, CCollisionPointY);
			covercast.StartPoint = new Vector2(CStartPointX, CStartPointY);
			covercast.EndPoint = new Vector2(CEndPointX, CEndPointY);

			
		}

	}

	public override void BeforeSend()
	{
		RCollisionPointX = result.CollisionPointLong.X;
		RCollisionPointY = result.CollisionPointLong.Y;
		
		RCollisionPointshortX = result.CollisionPointShort.Y;
		RCollisionPointshortY = result.CollisionPointShort.Y;
		
		RStartPointX = result.StartPoint.X;
		RStartPointY = result.StartPoint.Y;
		
		REndPointX = result.EndPoint.X;
		REndPointY = result.EndPoint.Y;

		if (covercast != null)
		{


			CCollisionPointX = covercast.CollisionPointLong.X;
			CCollisionPointY = covercast.CollisionPointLong.Y;

			CStartPointX = covercast.StartPoint.X;
			CStartPointY = covercast.StartPoint.Y;

			CEndPointX = covercast.EndPoint.X;
			CEndPointY = covercast.EndPoint.Y;
		}
	}
	public float RCollisionPointX{get;set;}
	public float RCollisionPointY{get;set;}
	public float RCollisionPointshortX{get;set;}
	public float RCollisionPointshortY{get;set;}
	public float RStartPointX{get;set;}
	public float RStartPointY{get;set;}
	public float REndPointX{get;set;}
	public float REndPointY{get;set;}
	
	public float CCollisionPointX{get;set;}
	public float CCollisionPointY{get;set;}
	public float CStartPointX{get;set;}
	public float CStartPointY{get;set;}
	public float CEndPointX{get;set;}
	public float CEndPointY{get;set;}

	public RayCastOutcome result { get;  set; }
	public RayCastOutcome? covercast { get;  set; }//tallest cover on the way
	public int dmg { get;  set; }
	public int dropoffRange { get;  set; }
	public int determinationResistanceCoefficient { get;  set; }
	public int suppresionRange { get;  set; }
	public int supressionStrenght { get;  set; }

	public ProjectilePacket(RayCastOutcome result, RayCastOutcome? covercast, int dmg, int dropoffRange, int determinationResistanceCoefficient,int suppresionRange,int supressionStrenght)
	{
		this.result = result;
		this.covercast = covercast;
		this.dmg = dmg;
		this.dropoffRange = dropoffRange;
		this.determinationResistanceCoefficient = determinationResistanceCoefficient;
		this.suppresionRange = suppresionRange;
		this.supressionStrenght = supressionStrenght;

	}


}