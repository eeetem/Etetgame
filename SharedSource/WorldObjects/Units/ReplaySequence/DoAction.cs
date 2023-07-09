using System;
using System.Threading;
using System.Threading.Tasks;
using MultiplayerXeno.Items;
using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class DoAction : SequenceAction
{
	public Vector2Int Target;
	public int ActionID;

	public DoAction(int actorID, Vector2Int tg, int action ) : base(actorID, SequenceType.Action)
	{
		Target = tg;
		ActionID = action;
	}

	public DoAction(int actorID, Message msg) : base(actorID, SequenceType.Action)
	{

		Target = msg.GetSerializable<Vector2Int>();
		ActionID = msg.GetInt();
	}


	public override bool ShouldDo()
	{
		if (Actor.overWatch)
		{
			Console.WriteLine("Considering shooting on overwatch");
			bool res = WorldManager.Instance.GetTileAtGrid(Target).UnitAtLocation != null;
			Console.WriteLine("result: "+res);
			return res;
		}

		return true;
	}

	public override Task Do()
	{
		var t = new Task(delegate
		{
#if CLIENT
			if (Actor.WorldObject.TileLocation.Visible == Visibility.None)
			{
				if (Actor.GetAction(ActionID).WorldAction.Effect.Visible)
				{
					Camera.SetPos(Actor.WorldObject.TileLocation.Position + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
		
				}
			}
			else
			{
				Camera.SetPos(Actor.WorldObject.TileLocation.Position);
			}

			Thread.Sleep(600);
			if (WorldManager.Instance.GetTileAtGrid(Target).Visible == Visibility.None)
			{
				if (Actor.GetAction(ActionID).WorldAction.Effect.Visible)
				{
					Camera.SetPos(Target + new Vector2Int(Random.Shared.Next(-4, 4), Random.Shared.Next(-4, 4)));
		
				}
			}
			else
			{
				Camera.SetPos(Target);
			}
#endif
			GenerateTask().RunSynchronously();
		});
		return t;
	}
	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			IExtraAction action = Actor.GetAction(ActionID);
			action.Execute(Actor, Target);
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(Target);
		message.Add(ActionID);
	}

}