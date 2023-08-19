using System;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class UseUpSelectedItem : UnitSequenceAction
{
	public Vector2Int Target;

	public UseUpSelectedItem(int actorID, Vector2Int target) : base(new TargetingRequirements(actorID), SequenceType.UseItem)
	{
		Target = target;
	}
	public UseUpSelectedItem(TargetingRequirements actorID, Message args) : base(actorID, SequenceType.UseItem)
	{
		Target = args.GetSerializable<Vector2Int>();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.LastItem = Actor.SelectedItem;
			Actor.RemoveItem(Actor.SelectedItemIndex);
		});
		return t;

	}
	

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.AddSerializable(Target);
	}
	
	
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}