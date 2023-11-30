using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class UnitStatusEffect  : UnitSequenceAction
{
	public bool addNotRemove;
	public string effectName;
	public int duration;
	public override SequenceType GetSequenceType()
	{
		return SequenceType.UnitStatusEffect;
	}

	public override BatchingMode Batching => BatchingMode.Always;

	public static UnitStatusEffect Make(TargetingRequirements actorID, bool addNotRemove, string effectName, int duration = 0)
	{
		UnitStatusEffect t = GetAction(SequenceType.UnitStatusEffect) as UnitStatusEffect;
		t.addNotRemove = addNotRemove;
		t.effectName = effectName;
		t.duration = duration;
		t.Requirements = actorID;
		return t;
	}

	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			if (addNotRemove)
			{
				Actor.ApplyStatus(effectName, duration);
			}
			else
			{
				Actor.RemoveStatus(effectName);
			}
		});
		return t;
	}
	
	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(addNotRemove);
		message.Add(effectName);
		message.Add(duration);
	}

	protected override void DeserializeArgs(Message msg)
	{
		base.DeserializeArgs(msg);
		addNotRemove = msg.GetBool();
		effectName = msg.GetString();
		duration = msg.GetInt();
	}

#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{

		Texture2D sprite = Actor.WorldObject.GetTexture();
		spriteBatch.Draw(sprite, Actor.WorldObject.GetDrawTransform().Position, Color.Yellow * 0.8f);

		
		//todo UI rework
	}
#endif
}