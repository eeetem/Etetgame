using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Riptide;


namespace DefconNull.ReplaySequence;

public class ProjectileAction : SequenceAction
{
	
	public Vector2Int From;
	public Vector2Int To;

	protected bool Equals(ProjectileAction other)
	{
		return From.Equals(other.From) && To.Equals(other.To);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((MoveCamera) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = From.GetHashCode();
			hashCode = (hashCode * 397) ^ To.GetHashCode();
			return hashCode;
		}
	}

	public static ProjectileAction Make(Vector2Int from,Vector2Int to) 
	{
		ProjectileAction t = (GetAction(SequenceType.Projectile) as ProjectileAction)!;
		t.From = from;
		t.To = to;
		return t;
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return false;
	}
#endif


	public override BatchingMode Batching => BatchingMode.Always;

	public override SequenceType GetSequenceType()
	{
		return SequenceType.Projectile;
	}

	
	//todo serverside scattering
	protected override void RunSequenceAction()
	{

	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(From);
		message.Add(To);
		
	}

	protected override void DeserializeArgs(Message message)
	{
		From = message.GetSerializable<Vector2Int>();
		To = message.GetSerializable<Vector2Int>();

	}
#if CLIENT
	override public void Preview(SpriteBatch spriteBatch)
	{
		spriteBatch.DrawLine(Utility.GridToWorldPos(From + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(To+ new Vector2(0.5f, 0.5f)), Color.Red, 2);
	}
#endif
}