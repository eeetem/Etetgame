using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;
using MultiplayerXeno.ReplaySequence;

namespace MultiplayerXeno;

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
	public override Queue<SequenceAction> ExecuteServerSide(Unit actor, Vector2Int target)
	{
		var queue = new Queue<SequenceAction>();
		queue.Enqueue(new ReplaySequence.DoAction(actor.WorldObject.ID,target,AbilityIndex));
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

			Networking.SendGameAction(packet);
		
		}
		public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
		{
			IExtraAction action = actor.GetAction(AbilityIndex);
			action.Preview(actor, target,spriteBatch);
		}


#endif

	
}