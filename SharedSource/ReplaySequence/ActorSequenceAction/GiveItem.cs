using System;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;

public class GiveItem : UnitSequenceAction
{
	private readonly VariableValue val;
	readonly int UserID = -1;
	public GiveItem(int actorID, VariableValue value, int UserID = -1) : base(new TargetingRequirements(actorID), SequenceType.GiveItem)
	{
		this.val = value;
		this.UserID = UserID;
	}
	public GiveItem(TargetingRequirements actorID, VariableValue value, int UserID = -1) : base(actorID, SequenceType.GiveItem)
	{
		this.val = value;
		this.UserID = UserID;
	}
	public GiveItem(TargetingRequirements actorID, Message args) : base(actorID, SequenceType.GiveItem)
	{
		val = args.GetSerializable<VariableValue>();
		UserID = args.GetInt();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			string itm;
			if (UserID == -1)
			{
				itm = val.GetValue(null, Actor);
			}else
			{
				itm = val.GetValue(WorldManager.Instance.GetObject(UserID).UnitComponent, Actor);
			}

			Actor.AddItem(PrefabManager.UseItems[itm]);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(val);
		message.Add(UserID);
	}
	
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//todo UI rework
	}
#endif
}