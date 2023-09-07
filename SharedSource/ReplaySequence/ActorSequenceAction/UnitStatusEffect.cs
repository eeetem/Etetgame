using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;

public class UnitStatusEffect  : UnitSequenceAction
{
	public readonly bool addNotRemove;
	public readonly string effectName;
	public readonly int duration;
	public override bool CanBatch => true;

	public UnitStatusEffect(Vector2Int actorID, bool addNotRemove, string effectName, int duration = 0) : base(new TargetingRequirements(actorID), SequenceType.UnitStatusEffect)
	{
		this.addNotRemove = addNotRemove;
		this.effectName = effectName;
		this.duration = duration;
	}
	public UnitStatusEffect(TargetingRequirements actorID, bool addNotRemove, string effectName, int duration = 0) : base(actorID, SequenceType.UnitStatusEffect)
	{
		this.addNotRemove = addNotRemove;
		this.effectName = effectName;
		this.duration = duration;
	}
	public UnitStatusEffect(TargetingRequirements actorID, Message msg) : base(actorID, SequenceType.UnitStatusEffect)
	{
		addNotRemove = msg.GetBool();
		effectName = msg.GetString();
		duration = msg.GetInt();
	}

	public override Task GenerateTask()
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
	
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{

		Texture2D sprite = Actor.WorldObject.GetTexture();
		spriteBatch.Draw(sprite, Actor.WorldObject.GetDrawTransform().Position, Color.Yellow * 0.8f);

		
		//todo UI rework
	}
#endif
}