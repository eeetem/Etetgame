using System.Threading.Tasks;
using Riptide;
#if CLIENT
using DefconNull.Rendering.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace DefconNull.ReplaySequence;

public class PostProcessingEffect : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.PostProcessingEffect;
	}
	
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return true;
	}
#endif
	protected bool Equals(PostProcessingEffect other)
	{
		return Parameter == other.Parameter && Target.Equals(other.Target) && Speed.Equals(other.Speed) && WipeQueue == other.WipeQueue && ReturnSpeed.Equals(other.ReturnSpeed);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((PostProcessingEffect) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Parameter.GetHashCode();
			hashCode = (hashCode * 397) ^ Target.GetHashCode();
			hashCode = (hashCode * 397) ^ Speed.GetHashCode();
			hashCode = (hashCode * 397) ^ WipeQueue.GetHashCode();
			hashCode = (hashCode * 397) ^ ReturnSpeed.GetHashCode();
			return hashCode;
		}
	}

	public override BatchingMode Batching => BatchingMode.Always;
	public string Parameter = "";
	public float Target;
	public float Speed;
	public bool WipeQueue;
	public float ReturnSpeed;
	public static PostProcessingEffect Make(string parameter,float target, float speed, bool wipeQueue = false, float returnSpeed = 10f)
	{
		PostProcessingEffect t = (GetAction(SequenceType.PostProcessingEffect) as PostProcessingEffect)!;
		t.Parameter = parameter;
		t.Target = target;
		t.Speed = speed;
		t.WipeQueue = wipeQueue;
		t.ReturnSpeed = returnSpeed;
		return t;
	}


	protected override void RunSequenceAction()
	{
		
#if CLIENT
			PostProcessing.AddTweenReturnTask(Parameter, Target, Speed, WipeQueue, ReturnSpeed);
#endif

	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(Parameter);
		message.Add(Target);
		message.Add(Speed);
		message.Add(WipeQueue);
		message.Add(ReturnSpeed);
	}

	protected override void DeserializeArgs(Message msg)
	{
		Parameter = msg.GetString();
		Target = msg.GetFloat();
		Speed = msg.GetFloat();
		WipeQueue = msg.GetBool();
		ReturnSpeed = msg.GetFloat();
	}

#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif

}