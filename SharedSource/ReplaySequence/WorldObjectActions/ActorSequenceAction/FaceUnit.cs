using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class FaceUnit : UnitSequenceAction
{
	public Vector2Int Target;
	private bool usePoint;

	protected bool Equals(FaceUnit other)
	{
		return base.Equals(other) && Target.Equals(other.Target);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((FaceUnit) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (base.GetHashCode() * 397) ^ Target.GetHashCode();
		}
	}


	public override string ToString()
	{
		return  "Face: "+ base.ToString()+ $"{nameof(Target)}: {Target}";
	}

	public static FaceUnit Make(int actorID, Vector2Int target,bool usePoint) 
	{
		FaceUnit t = (GetAction(SequenceType.Face) as FaceUnit)!;
		t.Target = target;
		t.Requirements = new TargetingRequirements(actorID);
		t.usePoint = usePoint;

		return t;
	}

	public override SequenceType GetSequenceType()
	{
		return SequenceType.Face;
	}

	protected override void RunSequenceAction()
	{
		
			if(Actor.WorldObject.TileLocation.Position == Target) return;
			var targetDir = Utility.GetDirection(Actor.WorldObject.TileLocation.Position, Target);
			if (targetDir != Actor.WorldObject.Facing)
			{
				if(usePoint)
					Actor.CanTurn = false;
				Actor.WorldObject.Face(targetDir);
			}

	}
	

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.AddSerializable(Target);
		message.AddBool(usePoint);
	}

	protected override void DeserializeArgs(Message message)
	{
		base.DeserializeArgs(message);
		Target = message.GetSerializable<Vector2Int>();
		usePoint = message.GetBool();
	}
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}