using System;
using DefconNull.Networking;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldObjects.Units.Actions;

public class UseAbility : Action
{
	
	
	public UseAbility() : base(ActionType.UseAbility)
	{
	}

	public override Tuple<bool, string> CanPerform(Unit actor,ref  Vector2Int target)
	{
			
		IExtraAction action = actor.GetAction(AbilityIndex);
		return action.CanPerform(actor, ref target);

	}

	public static bool abilityLock = false;
	private static int _abilityIndex = -1;

	public static int AbilityIndex
	{
		get => _abilityIndex;
		set {
			if (abilityLock)
			{
				return;
			}

			_abilityIndex = value;
		}
	}

#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, Vector2Int target)
	{

		IExtraAction action = actor.GetAction(AbilityIndex);
		var res = action.ExecutionResult(actor, target);
		var queue = new Queue<SequenceAction>();
		foreach (var sequenceAction in res)
		{
			queue.Enqueue(sequenceAction);
		}
		return queue;


	}
#endif
	



#if CLIENT
		public override void SendToServer(Unit actor, Vector2Int target)
		{
			IExtraAction action = actor.GetAction(AbilityIndex);
			var packet = new GameActionPacket(actor.WorldObject.ID,target,Type);
			packet.Args.Add(AbilityIndex.ToString());
			foreach (var a in action.MakePacketArgs())
			{
				packet.Args.Add(a);
			}

			NetworkingManager.SendGameAction(packet);
		
		}
		public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
		{
			IExtraAction action = actor.GetAction(AbilityIndex);
			action.Preview(actor, target,spriteBatch);
		}


#endif

	
}