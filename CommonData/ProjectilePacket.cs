using Microsoft.Xna.Framework;
using Network.Packets;

namespace CommonData;

public class ProjectilePacket : Packet
{

	public override void BeforeReceive()
	{
		result.CollisionPoint = new Vector2(RCollisionPointX, RCollisionPointY);
		result.StartPoint = new Vector2(RStartPointX, RStartPointY);
		result.EndPoint = new Vector2(REndPointX, REndPointY);

		if (covercast != null)
		{
			
			covercast.CollisionPoint = new Vector2(CCollisionPointX, CCollisionPointY);
			covercast.StartPoint = new Vector2(CStartPointX, CStartPointY);
			covercast.EndPoint = new Vector2(CEndPointX, CEndPointY);

			
		}

	}

	public override void BeforeSend()
	{
		RCollisionPointX = result.CollisionPoint.X;
		RCollisionPointY = result.CollisionPoint.Y;
		
		RStartPointX = result.StartPoint.X;
		RStartPointY = result.StartPoint.Y;
		
		REndPointX = result.EndPoint.X;
		REndPointY = result.EndPoint.Y;

		if (covercast != null)
		{


			CCollisionPointX = covercast.CollisionPoint.X;
			CCollisionPointY = covercast.CollisionPoint.Y;

			CStartPointX = covercast.StartPoint.X;
			CStartPointY = covercast.StartPoint.Y;

			CEndPointX = covercast.EndPoint.X;
			CEndPointY = covercast.EndPoint.Y;
		}
	}
	public float RCollisionPointX{get;set;}
	public float RCollisionPointY{get;set;}
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
	public int awarenessResistanceCoefficient { get;  set; }
	public int suppresionRange { get;  set; }
	public int supressionStrenght { get;  set; }

	public ProjectilePacket(RayCastOutcome result, RayCastOutcome? covercast, int dmg, int dropoffRange, int awarenessResistanceCoefficient,int suppressionRange,int supressionStrenght)
	{
		this.result = result;
		this.covercast = covercast;
		this.dmg = dmg;
		this.dropoffRange = dropoffRange;
		this.awarenessResistanceCoefficient = awarenessResistanceCoefficient;
		this.suppresionRange = suppressionRange;
		this.supressionStrenght = supressionStrenght;

	}


}