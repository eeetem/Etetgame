using System;
using System.Threading.Tasks;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.SharedSource.Units.ReplaySequence;

public class ChangeUnitValues : UnitSequenceAction
{
	private int actChange;
	private int moveChange;
	private int detChange;
	private int moveRangeEffect;
	public ChangeUnitValues(int actorID, int actChange=0, int moveChange=0, int detChange=0, int moveRangeEffect =0) : base(actorID,  SequenceType.ChangeUnitValues)
	{
		this.actChange = actChange;
		this.moveChange = moveChange;
		this.detChange = detChange;
		this.moveRangeEffect = moveRangeEffect;
	}
	public ChangeUnitValues(int actorID, Message msg) : base(actorID, SequenceType.ChangeUnitValues)
	{
		actChange = msg.GetInt();
		moveChange = msg.GetInt();
		detChange = msg.GetInt();
		moveRangeEffect = msg.GetInt();
	}
	

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.ActionPoints += actChange;
			Actor.MovePoints += moveChange;
			Actor.Determination += detChange;
			Actor.MoveRangeEffect += moveRangeEffect;
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(actChange);
		message.Add(moveChange);
		message.Add(detChange);
		message.Add(moveRangeEffect);
	}

#if CLIENT
	override public void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}
#endif
}