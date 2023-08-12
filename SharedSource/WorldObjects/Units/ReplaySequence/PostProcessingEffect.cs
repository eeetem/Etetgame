using System.Threading.Tasks;
using Riptide;

#if CLIENT
using DefconNull.Rendering.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class PostProcessingEffect : SequenceAction
{
	public override bool CanBatch => true;
	private string parameter;
	private float target;
	private float speed;
	private bool wipeQueue;
	private float returnSpeed;

	public PostProcessingEffect(string parameter,float target, float speed, bool wipeQueue = false, float returnSpeed = 10f) : base(SequenceType.PostProcessingEffect)
	{
		this.parameter = parameter;
		this.target = target;
		this.speed = speed;
		this.wipeQueue = wipeQueue;
		this.returnSpeed = returnSpeed;
	}
	
	public PostProcessingEffect(Message msg) : base(SequenceType.PostProcessingEffect)
	{
		parameter = msg.GetString();
		target = msg.GetFloat();
		speed = msg.GetFloat();
		wipeQueue = msg.GetBool();
		returnSpeed = msg.GetFloat();
	}


	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
#if CLIENT
			PostProcessing.AddTweenReturnTask(parameter, target, speed, wipeQueue, returnSpeed);
#endif
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(parameter);
		message.Add(target);
		message.Add(speed);
		message.Add(wipeQueue);
		message.Add(returnSpeed);
	}
	
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}