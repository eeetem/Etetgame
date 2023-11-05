using System.Threading.Tasks;
using Riptide;

#if CLIENT
using DefconNull.Rendering.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class PostProcessingEffect : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.PostProcessingEffect;
	}

	public override bool CanBatch => true;
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


	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
#if CLIENT
			PostProcessing.AddTweenReturnTask(Parameter, Target, Speed, WipeQueue, ReturnSpeed);
#endif
		});
		return t;
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
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif

}